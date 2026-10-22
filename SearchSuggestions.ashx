<%@ WebHandler Language="C#" Class="SearchSuggestions" %>

using System;
using System.Configuration;
using System.Globalization;
using System.Data.SqlClient;
using System.Text;
using System.Web;

public class SearchSuggestions : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        string term = context.Request.QueryString["term"];
        if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < 2)
        {
            context.Response.Write("[]");
            return;
        }

        string connStr = ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;
        string query = @"SELECT TOP 6
                            p.Product_ID, p.Product_Name, p.Product_SellingPrice,
                            p.Discount_ID AS ProductDiscountId, cat.Discount_ID AS CategoryDiscountId,
                            pd.Discount_Type AS ProductDiscountType, pd.Discount_Value AS ProductDiscountValue,
                            pd.Discount_Status AS ProductDiscountStatus, pd.Discount_StartDate AS ProductDiscountStart, pd.Discount_EndDate AS ProductDiscountEnd,
                            cd.Discount_Type AS CategoryDiscountType, cd.Discount_Value AS CategoryDiscountValue,
                            cd.Discount_Status AS CategoryDiscountStatus, cd.Discount_StartDate AS CategoryDiscountStart, cd.Discount_EndDate AS CategoryDiscountEnd
                          FROM Product p
                          INNER JOIN Category cat ON p.Category_ID = cat.Category_ID
                          LEFT JOIN Discount pd ON p.Discount_ID = pd.Discount_ID
                          LEFT JOIN Discount cd ON cat.Discount_ID = cd.Discount_ID
                          WHERE p.Product_Status = 'Active' AND p.Product_Name LIKE @Term
                          ORDER BY p.Product_Name";

        var sb = new StringBuilder("[");
        bool first = true;

        using (SqlConnection conn = new SqlConnection(connStr))
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@Term", term.Trim() + "%");
            conn.Open();
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (!first) sb.Append(",");
                    first = false;
                    decimal basePrice = Convert.ToDecimal(reader["Product_SellingPrice"]);
                    decimal finalPrice = basePrice;

                    if (reader["ProductDiscountId"] != DBNull.Value && IsDiscountActive(reader, "Product"))
                    {
                        finalPrice = ApplyDiscount(basePrice, reader["ProductDiscountType"].ToString(), Convert.ToDecimal(reader["ProductDiscountValue"]));
                    }
                    else if (reader["CategoryDiscountId"] != DBNull.Value && IsDiscountActive(reader, "Category"))
                    {
                        finalPrice = ApplyDiscount(basePrice, reader["CategoryDiscountType"].ToString(), Convert.ToDecimal(reader["CategoryDiscountValue"]));
                    }

                    string priceText = "R" + finalPrice.ToString("N2");
                    string oldPriceText = finalPrice < basePrice ? "R" + basePrice.ToString("N2") : "";
                    string imageUrl = "/ProductImage.ashx?id=" + reader["Product_ID"];

                    sb.Append("{\"name\":\"").Append(JsonEscape(reader["Product_Name"].ToString()))
                      .Append("\",\"price\":\"").Append(JsonEscape(priceText))
                      .Append("\",\"oldPrice\":\"").Append(JsonEscape(oldPriceText))
                      .Append("\",\"image\":\"").Append(JsonEscape(imageUrl))
                      .Append("\"}");
                }
            }
        }

        sb.Append("]");
        context.Response.Write(sb.ToString());
    }

    private bool IsDiscountActive(SqlDataReader r, string prefix)
    {
        object statusValue = r[prefix + "DiscountStatus"];
        if (statusValue == DBNull.Value) return false;

        string statusStr = statusValue.ToString().Trim();
        bool isActive = statusStr == "1"
            || statusStr.Equals("true", StringComparison.OrdinalIgnoreCase)
            || statusStr.Equals("Active", StringComparison.OrdinalIgnoreCase);
        if (!isActive) return false;

        DateTime start = Convert.ToDateTime(r[prefix + "DiscountStart"]);
        DateTime end = Convert.ToDateTime(r[prefix + "DiscountEnd"]);
        DateTime now = DateTime.Now;
        return now >= start && now <= end;
    }

    private decimal ApplyDiscount(decimal basePrice, string discountType, decimal discountValue)
    {
        if (discountType == "Percentage")
        {
            return Math.Round(basePrice - (basePrice * discountValue / 100m), 2);
        }
        decimal result = basePrice - discountValue;
        return result < 0 ? 0 : result;
    }

    private string JsonEscape(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    public bool IsReusable => false;
}
