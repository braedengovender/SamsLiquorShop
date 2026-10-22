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

namespace SamsLiquorShop.Employee
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected string MonthLabel = "";

        protected string DailySalesChartJson = "{}";
        protected string PaymentChartJson = "{}";
        protected string HourlyChartJson = "{}";

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
            if (!AuthHelper.RequireRole(this, "Cashier")) return;

            if (!IsPostBack)
            {
                litGreeting.Text = "Welcome back, " + Server.HtmlEncode(Session["UserName"].ToString());

                int employeeId = Convert.ToInt32(Session["EmployeeID"]);
                DateTime now = DateTime.Now;
                DateTime monthStart = new DateTime(now.Year, now.Month, 1);
                MonthLabel = monthStart.ToString("MMMM yyyy");

                LoadSummary(employeeId, monthStart, now);
                LoadBestCustomer(monthStart);
                LoadDailySalesChart(employeeId, monthStart, now);
                LoadPaymentChart(employeeId, monthStart);
                LoadHourlyChart(employeeId, monthStart);
                LoadMyTransactions(employeeId);
                LoadLowStockItems();
            }
        }

        private static string Serialize(ChartData data)
        {
            return new JavaScriptSerializer().Serialize(data);
        }

        // "▲ 12.4% vs this point last month" style badge under a KPI value
        private static string DeltaBadge(decimal current, decimal previous)
        {
            if (previous == 0)
            {
                return "<span class=\"dash-delta is-flat\">no sales by this point last month</span>";
            }

            decimal change = (current - previous) / previous * 100;
            string css = change > 0 ? "is-up" : change < 0 ? "is-down" : "is-flat";
            string arrow = change > 0 ? "&#9650;" : change < 0 ? "&#9660;" : "&#8212;";
            return "<span class=\"dash-delta " + css + "\">" + arrow + " " + Math.Abs(change).ToString("0.0") + "% vs this point last month</span>";
        }

        // This month so far, compared with the same number of days at the start of last month
        private void LoadSummary(int employeeId, DateTime monthStart, DateTime now)
        {
            DateTime prevMonthStart = monthStart.AddMonths(-1);
            DateTime prevMonthSamePoint = prevMonthStart + (now - monthStart);
            if (prevMonthSamePoint > monthStart) prevMonthSamePoint = monthStart;

            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                string myQuery = @"
                    SELECT
                        ISNULL(SUM(CASE WHEN Sale_DateTime >= @MonthStart THEN Sale_TotalAmount END), 0) AS MyRevenue,
                        COUNT(CASE WHEN Sale_DateTime >= @MonthStart THEN 1 END) AS MyCount,
                        ISNULL(SUM(CASE WHEN Sale_DateTime < @PrevSamePoint THEN Sale_TotalAmount END), 0) AS PrevRevenue,
                        COUNT(CASE WHEN Sale_DateTime < @PrevSamePoint THEN 1 END) AS PrevCount
                    FROM Sale
                    WHERE Employee_ID = @EmpId
                      AND Sale_DateTime >= @PrevMonthStart";

                decimal revenue = 0, prevRevenue = 0;
                int count = 0, prevCount = 0;

                using (SqlCommand cmd = new SqlCommand(myQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpId", employeeId);
                    cmd.Parameters.AddWithValue("@MonthStart", monthStart);
                    cmd.Parameters.AddWithValue("@PrevMonthStart", prevMonthStart);
                    cmd.Parameters.AddWithValue("@PrevSamePoint", prevMonthSamePoint);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            revenue = Convert.ToDecimal(reader["MyRevenue"]);
                            count = Convert.ToInt32(reader["MyCount"]);
                            prevRevenue = Convert.ToDecimal(reader["PrevRevenue"]);
                            prevCount = Convert.ToInt32(reader["PrevCount"]);
                        }
                    }
                }

                decimal avgSale = count > 0 ? revenue / count : 0;
                decimal prevAvgSale = prevCount > 0 ? prevRevenue / prevCount : 0;

                litMyRevenue.Text = revenue.ToString("C");
                litMyTxCount.Text = count.ToString();
                litMyAvgSale.Text = avgSale.ToString("C");

                litMyRevenueDelta.Text = DeltaBadge(revenue, prevRevenue);
                litMyTxDelta.Text = DeltaBadge(count, prevCount);
                litMyAvgSaleDelta.Text = DeltaBadge(avgSale, prevAvgSale);

                string lowStockCountQuery = @"
                    SELECT COUNT(*)
                    FROM Product
                    WHERE Product_QuantityInStock <= Product_ReorderQuantity
                      AND Product_Status = 'Active'";

                using (SqlCommand cmd = new SqlCommand(lowStockCountQuery, conn))
                {
                    litLowStockCount.Text = cmd.ExecuteScalar().ToString();
                }
            }
        }

        private void LoadBestCustomer(DateTime monthStart)
        {
            string query = @"
                SELECT TOP 1
                    CONCAT(c.Customer_FirstName, ' ', c.Customer_LastName) AS CustomerName,
                    SUM(s.Sale_TotalAmount) AS TotalSpent,
                    COUNT(s.Sale_ID) AS OrderCount
                FROM Sale s
                INNER JOIN Customer c ON s.Customer_ID = c.Customer_ID
                WHERE s.Sale_DateTime >= @MonthStart
                  AND s.Customer_ID <> 999999
                GROUP BY c.Customer_FirstName, c.Customer_LastName
                ORDER BY TotalSpent DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MonthStart", monthStart);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        litBestCustomerName.Text = Server.HtmlEncode(reader["CustomerName"].ToString());
                        litBestCustomerSpend.Text = Convert.ToDecimal(reader["TotalSpent"]).ToString("C");
                        litBestCustomerOrders.Text = reader["OrderCount"].ToString();
                        phBestCustomer.Visible = true;
                        phNoBestCustomer.Visible = false;
                    }
                    else
                    {
                        phBestCustomer.Visible = false;
                        phNoBestCustomer.Visible = true;
                    }
                }
            }
        }

        // One bar per day from the 1st to today; days with no sales show as an empty bar
        private void LoadDailySalesChart(int employeeId, DateTime monthStart, DateTime now)
        {
            var daily = new Dictionary<DateTime, decimal>();

            string query = @"
                SELECT CAST(Sale_DateTime AS DATE) AS SaleDay, SUM(Sale_TotalAmount) AS Revenue
                FROM Sale
                WHERE Employee_ID = @EmpId
                  AND Sale_DateTime >= @MonthStart
                GROUP BY CAST(Sale_DateTime AS DATE)";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@EmpId", employeeId);
                cmd.Parameters.AddWithValue("@MonthStart", monthStart);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        daily[Convert.ToDateTime(reader["SaleDay"]).Date] = Convert.ToDecimal(reader["Revenue"]);
                    }
                }
            }

            var labels = new List<string>();
            var values = new List<decimal>();
            for (DateTime day = monthStart; day <= now.Date; day = day.AddDays(1))
            {
                labels.Add(day.ToString("d MMM"));
                values.Add(daily.ContainsKey(day) ? daily[day] : 0);
            }

            DailySalesChartJson = Serialize(new ChartData { type = "bar", labels = labels.ToArray(), values = values.ToArray(), money = true });
        }

        private void LoadPaymentChart(int employeeId, DateTime monthStart)
        {
            var labels = new List<string>();
            var values = new List<decimal>();

            string query = @"
                SELECT
                    pm.PaymentMethod_Name,
                    SUM(pay.Payment_AmountPaid) AS TotalPaid
                FROM Payment pay
                INNER JOIN PaymentMethod pm ON pay.PaymentMethod_ID = pm.PaymentMethod_ID
                INNER JOIN Sale s ON pay.Sale_ID = s.Sale_ID
                WHERE s.Employee_ID = @EmpId
                  AND s.Sale_DateTime >= @MonthStart
                GROUP BY pm.PaymentMethod_Name
                ORDER BY TotalPaid DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@EmpId", employeeId);
                cmd.Parameters.AddWithValue("@MonthStart", monthStart);
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

            PaymentChartJson = Serialize(new ChartData { type = "doughnut", labels = labels.ToArray(), values = values.ToArray(), money = true });
        }

        // Transactions per hour of day; hours between the first and last trading hour
        // with no sales are filled in as zero so the shape of the day reads correctly
        private void LoadHourlyChart(int employeeId, DateTime monthStart)
        {
            var byHour = new Dictionary<int, decimal>();

            string query = @"
                SELECT DATEPART(HOUR, Sale_DateTime) AS SaleHour, COUNT(*) AS Transactions
                FROM Sale
                WHERE Employee_ID = @EmpId
                  AND Sale_DateTime >= @MonthStart
                GROUP BY DATEPART(HOUR, Sale_DateTime)";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@EmpId", employeeId);
                cmd.Parameters.AddWithValue("@MonthStart", monthStart);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        byHour[Convert.ToInt32(reader["SaleHour"])] = Convert.ToDecimal(reader["Transactions"]);
                    }
                }
            }

            var labels = new List<string>();
            var values = new List<decimal>();
            if (byHour.Count > 0)
            {
                for (int hour = byHour.Keys.Min(); hour <= byHour.Keys.Max(); hour++)
                {
                    labels.Add(hour.ToString("00") + ":00");
                    values.Add(byHour.ContainsKey(hour) ? byHour[hour] : 0);
                }
            }

            HourlyChartJson = Serialize(new ChartData { type = "bar", labels = labels.ToArray(), values = values.ToArray(), money = false });
        }

        // Table cell for a customer name; walk-in sales (no named customer) get a small grey tag
        protected string CustomerCell(object customerName)
        {
            if (customerName == null || customerName == DBNull.Value)
            {
                return "<span class=\"dash-tag\">Walk-in</span>";
            }
            return Server.HtmlEncode(customerName.ToString());
        }

        private void LoadMyTransactions(int employeeId)
        {
            // CONCAT turns NULL names into ' ', so blank names and the walk-in placeholder (999999) become NULL here
            string query = @"
                SELECT TOP 10
                    'INV-' + RIGHT('000000' + CAST(s.Sale_ID AS VARCHAR(6)), 6) AS InvoiceNumber,
                    s.Sale_DateTime,
                    CASE WHEN s.Customer_ID IS NULL OR s.Customer_ID = 999999 THEN NULL
                         ELSE NULLIF(LTRIM(RTRIM(CONCAT(c.Customer_FirstName, ' ', c.Customer_LastName))), '')
                    END AS CustomerName,
                    s.Sale_TotalAmount
                FROM Sale s
                LEFT JOIN Customer c ON s.Customer_ID = c.Customer_ID
                WHERE s.Employee_ID = @EmpId
                ORDER BY s.Sale_DateTime DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@EmpId", employeeId);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvMyTransactions.DataSource = dt;
                    gvMyTransactions.DataBind();
                }
            }
        }

        private void LoadLowStockItems()
        {
            string query = @"
                SELECT Product_ID, Product_Name, Product_QuantityInStock, Product_ReorderQuantity
                FROM Product
                WHERE Product_QuantityInStock <= Product_ReorderQuantity
                  AND Product_Status = 'Active'
                ORDER BY Product_QuantityInStock ASC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptLowStock.DataSource = dt;
                rptLowStock.DataBind();
                rptLowStock.Visible = dt.Rows.Count > 0;
                phNoLowStock.Visible = dt.Rows.Count == 0;
            }
        }

        // Stock as a percentage of its reorder level, for the low-stock meter (capped at 100)
        protected int StockPercent(object quantity, object reorderLevel)
        {
            decimal qty = quantity == DBNull.Value ? 0 : Convert.ToDecimal(quantity);
            decimal reorder = reorderLevel == DBNull.Value ? 0 : Convert.ToDecimal(reorderLevel);
            if (reorder <= 0) return qty > 0 ? 100 : 0;
            return (int)Math.Min(100, Math.Round(qty / reorder * 100));
        }

        protected string StockLevelClass(object quantity, object reorderLevel)
        {
            return StockPercent(quantity, reorderLevel) < 40 ? "is-critical" : "is-warning";
        }
    }
}
