using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;

namespace SamsLiquorShop.Customer
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected string SpendingChartJson = "{}";
        protected string CategoryChartJson = "{}";
        protected string FavouritesChartJson = "{}";

        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        // Simple holder used only to serialize chart data safely.
        // type: "bar" (columns), "hbar" (horizontal bars) or "doughnut" — see the chart script in Site.Master
        private class ChartData
        {
            public string type { get; set; }
            public string[] labels { get; set; }
            public decimal[] values { get; set; }
            public bool money { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Customer")) return;

            if (!IsPostBack)
            {
                litGreeting.Text = "Welcome back, " + Server.HtmlEncode(Session["UserName"].ToString());

                int customerId = Convert.ToInt32(Session["CustomerID"]);

                LoadSpendingSummary(customerId);
                LoadLoyaltyPoints(customerId);
                LoadSpendingChart(customerId);
                LoadCategoryChart(customerId);
                LoadFavouritesChart(customerId);
                LoadRecentOrders(customerId);
            }
        }

        private static string Serialize(ChartData data)
        {
            return new JavaScriptSerializer().Serialize(data);
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

        // One bar per month for the last 12 months; months with no orders show as an empty bar
        private void LoadSpendingChart(int customerId)
        {
            DateTime now = DateTime.Now;
            DateTime firstMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-11);
            var monthly = new Dictionary<DateTime, decimal>();

            string query = @"
                SELECT YEAR(Sale_DateTime) AS Yr, MONTH(Sale_DateTime) AS Mo, SUM(Sale_TotalAmount) AS Spent
                FROM Sale
                WHERE Customer_ID = @CustomerId
                  AND Sale_DateTime >= @FirstMonth
                GROUP BY YEAR(Sale_DateTime), MONTH(Sale_DateTime)";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                cmd.Parameters.AddWithValue("@FirstMonth", firstMonth);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var month = new DateTime(Convert.ToInt32(reader["Yr"]), Convert.ToInt32(reader["Mo"]), 1);
                        monthly[month] = Convert.ToDecimal(reader["Spent"]);
                    }
                }
            }

            var labels = new List<string>();
            var values = new List<decimal>();
            for (DateTime month = firstMonth; month <= now; month = month.AddMonths(1))
            {
                labels.Add(month.ToString("MMM yy"));
                values.Add(monthly.ContainsKey(month) ? monthly[month] : 0);
            }

            SpendingChartJson = Serialize(new ChartData { type = "bar", labels = labels.ToArray(), values = values.ToArray(), money = true });
        }

        // Runs a two-column (label, value) query for this customer and returns it as chart data
        private ChartData LoadLabelValueChart(string query, int customerId, string type, bool money)
        {
            var labels = new List<string>();
            var values = new List<decimal>();

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        labels.Add(reader[0].ToString());
                        values.Add(Convert.ToDecimal(reader[1]));
                    }
                }
            }

            return new ChartData { type = type, labels = labels.ToArray(), values = values.ToArray(), money = money };
        }

        private void LoadCategoryChart(int customerId)
        {
            string query = @"
                SELECT c.Category_Name, SUM(sl.SaleLine_Subtotal) AS Spent
                FROM SaleLine sl
                INNER JOIN Sale s ON sl.Sale_ID = s.Sale_ID
                INNER JOIN Product p ON sl.Product_ID = p.Product_ID
                INNER JOIN Category c ON p.Category_ID = c.Category_ID
                WHERE s.Customer_ID = @CustomerId
                GROUP BY c.Category_Name
                ORDER BY Spent DESC";

            CategoryChartJson = Serialize(LoadLabelValueChart(query, customerId, "doughnut", true));
        }

        private void LoadFavouritesChart(int customerId)
        {
            string query = @"
                SELECT TOP 5 p.Product_Name, SUM(sl.SaleLine_Quantity) AS UnitsBought
                FROM SaleLine sl
                INNER JOIN Sale s ON sl.Sale_ID = s.Sale_ID
                INNER JOIN Product p ON sl.Product_ID = p.Product_ID
                WHERE s.Customer_ID = @CustomerId
                GROUP BY p.Product_Name
                ORDER BY UnitsBought DESC";

            FavouritesChartJson = Serialize(LoadLabelValueChart(query, customerId, "hbar", false));
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
