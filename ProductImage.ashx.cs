using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;

namespace SamsLiquorShop
{
    /// <summary>
    /// Summary description for ProductImage
    /// </summary>
    public class ProductImage : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            int productId;
            if (!int.TryParse(context.Request.QueryString["id"], out productId))
            {
                context.Response.StatusCode = 400;
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;
            string query = "SELECT Product_Image FROM Product WHERE Product_ID = @Id";

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", productId);
                conn.Open();
                object result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    byte[] imageBytes = (byte[])result;
                    context.Response.ContentType = "image/jpeg";
                    context.Response.BinaryWrite(imageBytes);
                }
                else
                {
                    // Fallback placeholder if no image is stored
                    context.Response.Redirect("~/Images/product-placeholder.png");
                }
            }
        }

        public bool IsReusable => false;
    }
}