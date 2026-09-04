using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
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
                LoadRecentOrder(customerId);
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
                        litTotalSpent.Text = Convert.ToDecimal(reader["TotalSpent"]).ToString("C");
                        litOrderCount.Text = reader["OrderCount"].ToString();
                    }
                }
            }
        }

        private void LoadLoyaltyPoints(int customerId)
        {
            string query = "SELECT Customer_LoyaltyPointsBalance FROM Customer WHERE Customer_ID = @CustomerId";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                litLoyaltyPoints.Text = result != null ? result.ToString() : "0";
            }
        }

        private void LoadRecentOrder(int customerId)
        {
            string query = @"
                SELECT TOP 1
                    s.Sale_ID,
                    s.Sale_DateTime,
                    s.Sale_TotalAmount,
                    (SELECT ISNULL(SUM(SaleLine_Quantity), 0) FROM SaleLine WHERE Sale_ID = s.Sale_ID) AS ItemCount
                FROM Sale s
                WHERE s.Customer_ID = @CustomerId
                ORDER BY s.Sale_DateTime DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        litRecentOrderDate.Text = Convert.ToDateTime(reader["Sale_DateTime"]).ToString("d MMM yyyy");
                        litRecentOrderTotal.Text = Convert.ToDecimal(reader["Sale_TotalAmount"]).ToString("C");
                        litRecentOrderItems.Text = reader["ItemCount"].ToString();
                        phRecentOrder.Visible = true;
                        litNoOrders.Visible = false;
                    }
                    else
                    {
                        phRecentOrder.Visible = false;
                        litNoOrders.Visible = true;
                    }
                }
            }
        }
    }
}