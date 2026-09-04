<%@ WebHandler Language="C#" Class="SearchSuggestions" %>

using System;
using System.Configuration;
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
        string query = @"SELECT TOP 6 Product_Name 
                          FROM Product 
                          WHERE Product_Status = 'Active' AND Product_Name LIKE @Term
                          ORDER BY Product_Name";

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
                    sb.Append("{\"name\":\"").Append(JsonEscape(reader["Product_Name"].ToString())).Append("\"}");
                }
            }
        }

        sb.Append("]");
        context.Response.Write(sb.ToString());
    }

    private string JsonEscape(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    public bool IsReusable => false;
}
