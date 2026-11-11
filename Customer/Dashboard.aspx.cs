using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SamsLiquorShop.Customer
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Customer")) return;

            if (!IsPostBack)
            {
                litGreeting.Text = "Welcome back, " + Server.HtmlEncode(Session["UserName"].ToString());

                int customerId = Convert.ToInt32(Session["CustomerID"]);

                LoadSpendingSummary(customerId);
                LoadLoyaltyPoints(customerId);
                LoadRecentOrders(customerId);
            }
        }

        private void LoadSpendingSummary(int customerId)
        {
            string query = @"
                SELECT
                    ISNULL(SUM(Sale_TotalAmount), 0) AS TotalSpent,
                    COUNT(*) AS OrderCount
                FROM Sale
                WHERE Customer_ID = @CustomerId";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        decimal totalSpent = Convert.ToDecimal(reader["TotalSpent"]);
                        int orderCount = Convert.ToInt32(reader["OrderCount"]);

                        litTotalSpent.Text = totalSpent.ToString("C");
                        litOrderCount.Text = orderCount.ToString();
                        litAvgOrder.Text = (orderCount > 0 ? totalSpent / orderCount : 0).ToString("C");
                    }
                }
            }
        }

        // Points redeem at 1 point = R1 (mirrors the POS loyalty rules)
        private void LoadLoyaltyPoints(int customerId)
        {
            string query = "SELECT Customer_LoyaltyPointsBalance FROM Customer WHERE Customer_ID = @CustomerId";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                int points = result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);

                litLoyaltyPoints.Text = points.ToString("N0");
                litPointsValue.Text = ((decimal)points).ToString("C");
            }
        }

        private void LoadRecentOrders(int customerId)
        {
            string query = @"
                SELECT TOP 5
                    'INV-' + RIGHT('000000' + CAST(s.Sale_ID AS VARCHAR(6)), 6) AS InvoiceNumber,
                    s.Sale_DateTime,
                    (SELECT ISNULL(SUM(SaleLine_Quantity), 0) FROM SaleLine WHERE Sale_ID = s.Sale_ID) AS ItemCount,
                    s.Sale_TotalAmount
                FROM Sale s
                WHERE s.Customer_ID = @CustomerId
                ORDER BY s.Sale_DateTime DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvRecentOrders.DataSource = dt;
                    gvRecentOrders.DataBind();
                }
            }
        }
    }
}
