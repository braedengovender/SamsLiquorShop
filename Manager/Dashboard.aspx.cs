using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;

namespace SamsLiquorShop.Manager
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected const int MonthsBack = 3;

        protected string RevenueChartJson = "{}";
        protected string TopProductsChartJson = "{}";
        protected string CategoryChartJson = "{}";
        protected string PaymentChartJson = "{}";

        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        // Simple holder used only to serialize chart data safely
        private class ChartData
        {
            public string[] labels { get; set; }
            public decimal[] values { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Manager")) return;

            if (!IsPostBack)
            {
                litGreeting.Text = "Welcome back, " + Server.HtmlEncode(Session["UserName"].ToString());

                DateTime cutoff = DateTime.Now.AddMonths(-MonthsBack);

                LoadKpis(cutoff);
                LoadRevenueChart(cutoff);
                LoadTopProductsChart(cutoff);
                LoadCategoryChart(cutoff);
                LoadPaymentChart(cutoff);
                LoadRecentSales();
                LoadTopCustomers(cutoff);
                LoadBestCustomerSpotlight(cutoff);
                LoadActiveDiscounts();
            }
        }

        private void LoadKpis(DateTime cutoff)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                string kpiQuery = @"
                    SELECT 
                        ISNULL(SUM(Sale_TotalAmount), 0) AS TotalRevenue,
                        COUNT(*) AS TotalTransactions
                    FROM Sale
                    WHERE Sale_DateTime >= @Cutoff";

                decimal totalRevenue = 0;
                int totalTransactions = 0;

                using (SqlCommand cmd = new SqlCommand(kpiQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Cutoff", cutoff);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            totalRevenue = reader.GetDecimal(0);
                            totalTransactions = reader.GetInt32(1);
                        }
                    }
                }

                litRevenue.Text = totalRevenue.ToString("C");
                litTransactions.Text = totalTransactions.ToString();
                litAvgBasket.Text = totalTransactions > 0
                    ? (totalRevenue / totalTransactions).ToString("C")
                    : "R0.00";

                string lowStockQuery = @"
                    SELECT COUNT(*) 
                    FROM Product 
                    WHERE Product_QuantityInStock <= Product_ReorderQuantity 
                      AND Product_Status = 'Active'";

                using (SqlCommand cmd = new SqlCommand(lowStockQuery, conn))
                {
                    litLowStock.Text = cmd.ExecuteScalar().ToString();
                }
            }
        }

        private void LoadRevenueChart(DateTime cutoff)
        {
            var labels = new System.Collections.Generic.List<string>();
            var values = new System.Collections.Generic.List<decimal>();

            string query = @"
                SELECT 
                    FORMAT(Sale_DateTime, 'MMM yyyy') AS MonthLabel,
                    YEAR(Sale_DateTime) AS Yr,
                    MONTH(Sale_DateTime) AS Mo,
                    SUM(Sale_TotalAmount) AS Revenue
                FROM Sale
                WHERE Sale_DateTime >= @Cutoff
                GROUP BY FORMAT(Sale_DateTime, 'MMM yyyy'), YEAR(Sale_DateTime), MONTH(Sale_DateTime)
                ORDER BY Yr, Mo";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Cutoff", cutoff);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        labels.Add(reader["MonthLabel"].ToString());
                        values.Add(Convert.ToDecimal(reader["Revenue"]));
                    }
                }
            }

            var serializer = new JavaScriptSerializer();
            RevenueChartJson = serializer.Serialize(new ChartData { labels = labels.ToArray(), values = values.ToArray() });
        }

        private void LoadTopProductsChart(DateTime cutoff)
        {
            var labels = new System.Collections.Generic.List<string>();
            var values = new System.Collections.Generic.List<decimal>();

            string query = @"
                SELECT TOP 5 
                    p.Product_Name, 
                    SUM(sl.SaleLine_Quantity) AS UnitsSold
                FROM SaleLine sl
                INNER JOIN Product p ON sl.Product_ID = p.Product_ID
                INNER JOIN Sale s ON sl.Sale_ID = s.Sale_ID
                WHERE s.Sale_DateTime >= @Cutoff
                GROUP BY p.Product_Name
                ORDER BY UnitsSold DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Cutoff", cutoff);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        labels.Add(reader["Product_Name"].ToString());
                        values.Add(Convert.ToDecimal(reader["UnitsSold"]));
                    }
                }
            }

            var serializer = new JavaScriptSerializer();
            TopProductsChartJson = serializer.Serialize(new ChartData { labels = labels.ToArray(), values = values.ToArray() });
        }

        private void LoadCategoryChart(DateTime cutoff)
        {
            var labels = new System.Collections.Generic.List<string>();
            var values = new System.Collections.Generic.List<decimal>();

            string query = @"
                SELECT 
                    c.Category_Name, 
                    SUM(sl.SaleLine_Subtotal) AS CategoryRevenue
                FROM SaleLine sl
                INNER JOIN Product p ON sl.Product_ID = p.Product_ID
                INNER JOIN Category c ON p.Category_ID = c.Category_ID
                INNER JOIN Sale s ON sl.Sale_ID = s.Sale_ID
                WHERE s.Sale_DateTime >= @Cutoff
                GROUP BY c.Category_Name
                ORDER BY CategoryRevenue DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Cutoff", cutoff);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        labels.Add(reader["Category_Name"].ToString());
                        values.Add(Convert.ToDecimal(reader["CategoryRevenue"]));
                    }
                }
            }

            var serializer = new JavaScriptSerializer();
            CategoryChartJson = serializer.Serialize(new ChartData { labels = labels.ToArray(), values = values.ToArray() });
        }

        private void LoadPaymentChart(DateTime cutoff)
        {
            var labels = new System.Collections.Generic.List<string>();
            var values = new System.Collections.Generic.List<decimal>();

            string query = @"
                SELECT 
                    pm.PaymentMethod_Name, 
                    SUM(pay.Payment_AmountPaid) AS TotalPaid
                FROM Payment pay
                INNER JOIN PaymentMethod pm ON pay.PaymentMethod_ID = pm.PaymentMethod_ID
                INNER JOIN Sale s ON pay.Sale_ID = s.Sale_ID
                WHERE s.Sale_DateTime >= @Cutoff
                GROUP BY pm.PaymentMethod_Name
                ORDER BY TotalPaid DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Cutoff", cutoff);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        labels.Add(reader["PaymentMethod_Name"].ToString());
                        values.Add(Convert.ToDecimal(reader["TotalPaid"]));
                    }
                }
            }

            var serializer = new JavaScriptSerializer();
            PaymentChartJson = serializer.Serialize(new ChartData { labels = labels.ToArray(), values = values.ToArray() });
        }

        private void LoadRecentSales()
        {
            string query = @"
                SELECT TOP 15
                    s.Sale_ID,
                    s.Sale_DateTime,
                    ISNULL(CONCAT(c.Customer_FirstName, ' ', c.Customer_LastName), 'Walk-in Customer') AS CustomerName,
                    CONCAT(e.Employee_FirstName, ' ', e.Employee_LastName) AS EmployeeName,
                    s.Sale_TotalAmount
                FROM Sale s
                LEFT JOIN Customer c ON s.Customer_ID = c.Customer_ID
                LEFT JOIN Employee e ON s.Employee_ID = e.Employee_ID
                ORDER BY s.Sale_DateTime DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvRecentSales.DataSource = dt;
                gvRecentSales.DataBind();
            }
        }

        private void LoadTopCustomers(DateTime cutoff)
        {
            string query = @"
                SELECT TOP 5
                    CONCAT(c.Customer_FirstName, ' ', c.Customer_LastName) AS CustomerName,
                    SUM(s.Sale_TotalAmount) AS TotalSpent,
                    COUNT(s.Sale_ID) AS OrderCount
                FROM Sale s
                INNER JOIN Customer c ON s.Customer_ID = c.Customer_ID
                WHERE s.Sale_DateTime >= @Cutoff
                GROUP BY c.Customer_FirstName, c.Customer_LastName
                ORDER BY TotalSpent DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Cutoff", cutoff);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvTopCustomers.DataSource = dt;
                    gvTopCustomers.DataBind();
                }
            }
        }

        private void LoadBestCustomerSpotlight(DateTime cutoff)
        {
            string query = @"
                SELECT TOP 1
                    CONCAT(c.Customer_FirstName, ' ', c.Customer_LastName) AS CustomerName,
                    SUM(s.Sale_TotalAmount) AS TotalSpent,
                    COUNT(s.Sale_ID) AS OrderCount
                FROM Sale s
                INNER JOIN Customer c ON s.Customer_ID = c.Customer_ID
                WHERE s.Sale_DateTime >= @Cutoff
                  AND s.Customer_ID <> 999999
                GROUP BY c.Customer_FirstName, c.Customer_LastName
                ORDER BY TotalSpent DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Cutoff", cutoff);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        litBestCustomerName.Text = Server.HtmlEncode(reader["CustomerName"].ToString());
                        litBestCustomerSpend.Text = Convert.ToDecimal(reader["TotalSpent"]).ToString("C");
                        litBestCustomerOrders.Text = reader["OrderCount"].ToString();
                    }
                    else
                    {
                        litBestCustomerName.Text = "No customer activity yet";
                        litBestCustomerSpend.Text = "R0.00";
                        litBestCustomerOrders.Text = "0";
                    }
                }
            }
        }

        private void LoadActiveDiscounts()
        {
            string query = @"
                SELECT Discount_Name, Discount_Type, Discount_Value, Discount_EndDate
                FROM Discount
                WHERE Discount_Status = 1
                  AND Discount_StartDate <= GETDATE()
                  AND Discount_EndDate >= GETDATE()
                ORDER BY Discount_EndDate ASC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvActiveDiscounts.DataSource = dt;
                gvActiveDiscounts.DataBind();
            }
        }
    }
}