using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;

namespace SamsLiquorShop
{
    /// <summary>
    /// Serves a product's picture from the database (ProductImage.ashx?id=123).
    /// </summary>
    public class ProductImage : IHttpHandler
    {
        // Shown for products that have no picture yet
        private const string PlaceholderSvg =
            "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 120 120\">" +
            "<rect width=\"120\" height=\"120\" fill=\"#eef3f8\"/>" +
            "<path d=\"M52 18h16v14l8 14v56H44V46l8-14z\" fill=\"none\" stroke=\"#9db0c3\" stroke-width=\"3\" stroke-linejoin=\"round\"/>" +
            "</svg>";

        public void ProcessRequest(HttpContext context)
        {
            int productId;
            if (!int.TryParse(context.Request.QueryString["id"], out productId))
            {
                context.Response.StatusCode = 400;
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("SELECT Product_Image FROM Product WHERE Product_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", productId);
                conn.Open();
                object result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    byte[] imageBytes = (byte[])result;
                    context.Response.ContentType = DetectContentType(imageBytes);
                    context.Response.BinaryWrite(imageBytes);
                }
                else
                {
                    context.Response.ContentType = "image/svg+xml";
                    context.Response.Write(PlaceholderSvg);
                }
            }
        }

        private static string DetectContentType(byte[] b)
        {
            if (b.Length > 3 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47) return "image/png";
            if (b.Length > 3 && b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x38) return "image/gif";
            if (b.Length > 11 && b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46
                && b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50) return "image/webp";
            return "image/jpeg";
        }

        public bool IsReusable => false;
    }
}