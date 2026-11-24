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
        // Walk-in sales on the POS are recorded against this placeholder customer
        private const int WalkInCustomerId = 999999;

        // Selected date range, from ?range=7d|30d|3m|12m (defaults to 3 months)
        protected string RangeKey = "3m";
        protected string RangeLabel = "3 months";
        protected string RevenueBucketLabel = "by Week";

        protected string RevenueChartJson = "{}";
        protected string TopProductsChartJson = "{}";
        protected string CategoryChartJson = "{}";
        protected string PaymentChartJson = "{}";
        protected string CashierChartJson = "{}";
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
            System.Diagnostics.Debug.WriteLine("DASH sid=" + Session.SessionID + " new=" + Session.IsNewSession + " role=" + Session["UserRole"]);

            if (!AuthHelper.RequireRole(this, "Manager")) return;

            if (!IsPostBack)
            {
                litGreeting.Text = "Welcome back, " + Server.HtmlEncode(Session["UserName"].ToString());
                LoadNewOrdersNotice();

                DateTime now = DateTime.Now;
                DateTime cutoff = ResolveRange(now);
                // The previous period is the same length, immediately before the current one
                DateTime prevCutoff = cutoff - (now - cutoff);

                if (RangeKey == "12m" && UseSampleDataFor12Months)
                {
                    // 12 months only: presentable sample figures (see Dashboard.Sample12m.cs)
                    LoadSample12Months(cutoff, now);
                }
                else
                {
                    LoadKpis(cutoff, prevCutoff);
                    LoadRevenueChart(cutoff, now);
                    LoadTopProductsChart(cutoff);
                    LoadCategoryChart(cutoff);
                    LoadPaymentChart(cutoff);
                    LoadCashierChart(cutoff);
                    LoadHourlyChart(cutoff);
                    LoadTopCustomers(cutoff);
                }
                LoadLowStock();
                LoadRecentSales();
                LoadActiveDiscounts();

                // Low-stock pop-up: once per login (logout clears the session, so it shows again next time)
                if (Session["LowStockToastShown"] == null)
                {
                    Session["LowStockToastShown"] = true;
                    LoadStockToast();
                }
            }
        }

        // Banner at the top of the dashboard (the manager's landing page after login) listing online orders
        // that are still "Placed", i.e. nobody has started packing them. Hidden when there are none.
        private void LoadNewOrdersNotice()
        {
            int total;
            var orders = Helpers.OrderHelper.GetNewOrders(5, out total);
            if (total == 0) return;

            var sb = new StringBuilder();
            sb.Append("<div class=\"dash-notice is-orders\" role=\"status\">")
              .Append("<div class=\"dash-notice-body\"><strong>")
              .Append(total == 1 ? "1 new online order is" : total + " new online orders are")
              .Append(" waiting to be packed</strong><ul>");
            foreach (var o in orders)
            {
                sb.Append("<li><span>INV-").Append(o.SaleId.ToString("000000")).Append(" &middot; ")
                  .Append(Server.HtmlEncode(o.CustomerName)).Append(" &middot; ").Append(Server.HtmlEncode(o.Method))
                  .Append(" &middot; ").Append(o.Items).Append(o.Items == 1 ? " item" : " items")
                  .Append("</span><span class=\"dash-notice-meta\">").Append(o.Total.ToString("C"))
                  .Append(", placed ").Append(Server.HtmlEncode(o.PlacedAt.ToString("d MMM HH:mm"))).Append("</span></li>");
            }
            if (total > orders.Count) sb.Append("<li class=\"dash-notice-meta\">and ").Append(total - orders.Count).Append(" more</li>");
            sb.Append("</ul></div><a href=\"").Append(ResolveUrl("~/Manager/OnlineOrders.aspx"))
              .Append("\" class=\"btn-primary\">View online orders</a></div>");

            litNewOrders.Text = sb.ToString();
        }

        // Count of active products at or below their reorder level, plus the five lowest for the pop-up.
        // Stays hidden when nothing is low.
        private void LoadStockToast()
        {
            string query = @"
                SELECT Product_Name, Product_QuantityInStock, Product_ReorderQuantity,
                       COUNT(*) OVER () AS LowCount
                FROM Product
                WHERE Product_QuantityInStock <= Product_ReorderQuantity
                  AND Product_Status = 'Active'
                ORDER BY Product_QuantityInStock ASC, Product_Name";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count == 0) return;

                int lowCount = Convert.ToInt32(dt.Rows[0]["LowCount"]);
                litToastTitle.Text = lowCount + (lowCount == 1 ? " item is" : " items are") + " low on stock";

                DataTable top = dt.Clone();
                for (int i = 0; i < Math.Min(5, dt.Rows.Count); i++) top.ImportRow(dt.Rows[i]);
                rptToastItems.DataSource = top;
                rptToastItems.DataBind();

                phStockToast.Visible = true;
            }
        }

        private DateTime ResolveRange(DateTime now)
        {
            switch (Request.QueryString["range"])
            {
                case "7d":
                    RangeKey = "7d";
                    RangeLabel = "7 days";
                    RevenueBucketLabel = "by Day";
                    return now.Date.AddDays(-6);
                case "30d":
                    RangeKey = "30d";
                    RangeLabel = "30 days";
                    RevenueBucketLabel = "by Day";
                    return now.Date.AddDays(-29);
                case "12m":
                    RangeKey = "12m";
                    RangeLabel = "12 months";
                    RevenueBucketLabel = "by Month";
                    return new DateTime(now.Year, now.Month, 1).AddMonths(-11);
                default:
                    RangeKey = "3m";
                    RangeLabel = "3 months";
                    RevenueBucketLabel = "by Week";
                    return now.Date.AddDays(-7 * 13 + 1);
            }
        }

        private static string Serialize(ChartData data)
        {
            return new JavaScriptSerializer().Serialize(data);
        }

        // "▲ 12.4% vs previous 3 months" style badge under a KPI value
        private string DeltaBadge(decimal current, decimal previous)
        {
            if (previous == 0)
            {
                return "<span class=\"dash-delta is-flat\">no data for previous " + RangeLabel + "</span>";
            }

            decimal change = (current - previous) / previous * 100;
            string css = change > 0 ? "is-up" : change < 0 ? "is-down" : "is-flat";
            string arrow = change > 0 ? "&#9650;" : change < 0 ? "&#9660;" : "&#8212;";
            return "<span class=\"dash-delta " + css + "\">" + arrow + " " + Math.Abs(change).ToString("0.0") + "% vs previous " + RangeLabel + "</span>";
        }

        private void LoadKpis(DateTime cutoff, DateTime prevCutoff)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                string kpiQuery = @"
                    SELECT
                        ISNULL(SUM(CASE WHEN Sale_DateTime >= @Cutoff THEN Sale_TotalAmount END), 0) AS Revenue,
                        COUNT(CASE WHEN Sale_DateTime >= @Cutoff THEN 1 END) AS Transactions,
                        ISNULL(SUM(CASE WHEN Sale_DateTime < @Cutoff THEN Sale_TotalAmount END), 0) AS PrevRevenue,
                        COUNT(CASE WHEN Sale_DateTime < @Cutoff THEN 1 END) AS PrevTransactions
                    FROM Sale
                    WHERE Sale_DateTime >= @PrevCutoff";

                decimal revenue = 0, prevRevenue = 0;
                int transactions = 0, prevTransactions = 0;

                using (SqlCommand cmd = new SqlCommand(kpiQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Cutoff", cutoff);
                    cmd.Parameters.AddWithValue("@PrevCutoff", prevCutoff);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            revenue = Convert.ToDecimal(reader["Revenue"]);
                            transactions = Convert.ToInt32(reader["Transactions"]);
                            prevRevenue = Convert.ToDecimal(reader["PrevRevenue"]);
                            prevTransactions = Convert.ToInt32(reader["PrevTransactions"]);
                        }
                    }
                }

                decimal avgBasket = transactions > 0 ? revenue / transactions : 0;
                decimal prevAvgBasket = prevTransactions > 0 ? prevRevenue / prevTransactions : 0;

                litRevenue.Text = revenue.ToString("C");
                litTransactions.Text = transactions.ToString();
                litAvgBasket.Text = avgBasket.ToString("C");

                litRevenueDelta.Text = DeltaBadge(revenue, prevRevenue);
                litTransactionsDelta.Text = DeltaBadge(transactions, prevTransactions);
                litAvgBasketDelta.Text = DeltaBadge(avgBasket, prevAvgBasket);

            }

            LoadLowStockCount();
        }

        // The "Low Stock Items" KPI is always real, whatever the range
        private void LoadLowStockCount()
        {
            string lowStockQuery = @"
                SELECT COUNT(*)
                FROM Product
                WHERE Product_QuantityInStock <= Product_ReorderQuantity
                  AND Product_Status = 'Active'";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(lowStockQuery, conn))
            {
                conn.Open();
                litLowStock.Text = cmd.ExecuteScalar().ToString();
            }
        }

        // Daily totals from SQL, grouped into days / weeks / months here so that
        // periods with no sales still show up as an empty bar instead of disappearing
        private void LoadRevenueChart(DateTime cutoff, DateTime now)
        {
            var daily = new Dictionary<DateTime, decimal>();

            string query = @"
                SELECT CAST(Sale_DateTime AS DATE) AS SaleDay, SUM(Sale_TotalAmount) AS Revenue
                FROM Sale
                WHERE Sale_DateTime >= @Cutoff
                GROUP BY CAST(Sale_DateTime AS DATE)";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Cutoff", cutoff);
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

            Func<DateTime, DateTime, decimal> sumBetween = (from, to) =>
                daily.Where(d => d.Key >= from && d.Key < to).Sum(d => d.Value);

            if (RangeKey == "7d" || RangeKey == "30d")
            {
                for (DateTime day = cutoff.Date; day <= now.Date; day = day.AddDays(1))
                {
                    labels.Add(RangeKey == "7d" ? day.ToString("ddd d MMM") : day.ToString("d MMM"));
                    values.Add(sumBetween(day, day.AddDays(1)));
                }
            }
            else if (RangeKey == "12m")
            {
                for (DateTime month = cutoff; month <= now; month = month.AddMonths(1))
                {
                    labels.Add(month.ToString("MMM yy"));
                    values.Add(sumBetween(month, month.AddMonths(1)));
                }
            }
            else
            {
                for (DateTime week = cutoff.Date; week <= now.Date; week = week.AddDays(7))
                {
                    labels.Add(week.ToString("d MMM"));
                    values.Add(sumBetween(week, week.AddDays(7)));
                }
            }

            RevenueChartJson = Serialize(new ChartData { type = "bar", labels = labels.ToArray(), values = values.ToArray(), money = true });
        }

        // Runs a two-column (label, value) query for the selected range and returns it as chart data
        private ChartData LoadLabelValueChart(string query, DateTime cutoff, string type, bool money)
        {
            var labels = new List<string>();
            var values = new List<decimal>();

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Cutoff", cutoff);
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

        private void LoadTopProductsChart(DateTime cutoff)
        {
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

            TopProductsChartJson = Serialize(LoadLabelValueChart(query, cutoff, "hbar", false));
        }

        private void LoadCategoryChart(DateTime cutoff)
        {
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

            CategoryChartJson = Serialize(LoadLabelValueChart(query, cutoff, "hbar", true));
        }

        private void LoadPaymentChart(DateTime cutoff)
        {
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

            PaymentChartJson = Serialize(LoadLabelValueChart(query, cutoff, "doughnut", true));
        }

        private void LoadCashierChart(DateTime cutoff)
        {
            string query = @"
                SELECT TOP 8
                    CONCAT(e.Employee_FirstName, ' ', e.Employee_LastName) AS EmployeeName,
                    SUM(s.Sale_TotalAmount) AS Revenue
                FROM Sale s
                INNER JOIN Employee e ON s.Employee_ID = e.Employee_ID
                WHERE s.Sale_DateTime >= @Cutoff
                GROUP BY e.Employee_ID, e.Employee_FirstName, e.Employee_LastName
                ORDER BY Revenue DESC";

            CashierChartJson = Serialize(LoadLabelValueChart(query, cutoff, "hbar", true));
        }

        // Transactions per hour of day; hours between the first and last trading hour
        // with no sales are filled in as zero so the shape of the day reads correctly
        private void LoadHourlyChart(DateTime cutoff)
        {
            var byHour = new Dictionary<int, decimal>();

            string query = @"
                SELECT DATEPART(HOUR, Sale_DateTime) AS SaleHour, COUNT(*) AS Transactions
                FROM Sale
                WHERE Sale_DateTime >= @Cutoff
                GROUP BY DATEPART(HOUR, Sale_DateTime)";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Cutoff", cutoff);
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

        private void LoadLowStock()
        {
            string query = @"
                SELECT TOP 8 Product_ID, Product_Name, Product_QuantityInStock, Product_ReorderQuantity
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

        // Table cell for a customer name; walk-in sales (no named customer) get a small grey tag
        protected string CustomerCell(object customerName)
        {
            if (customerName == null || customerName == DBNull.Value)
            {
                return "<span class=\"dash-tag\">Walk-in</span>";
            }
            return Server.HtmlEncode(customerName.ToString());
        }

        // Percentage discounts read "10%", fixed-amount ones read as Rand
        protected string DiscountValue(object discountType, object discountValue)
        {
            decimal value = discountValue == DBNull.Value ? 0 : Convert.ToDecimal(discountValue);
            string type = discountType == DBNull.Value ? "" : discountType.ToString();
            return type.IndexOf("percent", StringComparison.OrdinalIgnoreCase) >= 0
                ? value.ToString("0.##") + "%"
                : value.ToString("C");
        }

        private void LoadRecentSales()
        {
            // CONCAT turns NULL names into ' ', so blank names and the walk-in placeholder become NULL here
            string query = @"
                SELECT TOP 15
                    s.Sale_ID,
                    s.Sale_DateTime,
                    CASE WHEN s.Customer_ID IS NULL OR s.Customer_ID = @WalkInId THEN NULL
                         ELSE NULLIF(LTRIM(RTRIM(CONCAT(c.Customer_FirstName, ' ', c.Customer_LastName))), '')
                    END AS CustomerName,
                    CONCAT(e.Employee_FirstName, ' ', e.Employee_LastName) AS EmployeeName,
                    s.Sale_TotalAmount
                FROM Sale s
                LEFT JOIN Customer c ON s.Customer_ID = c.Customer_ID
                LEFT JOIN Employee e ON s.Employee_ID = e.Employee_ID
                ORDER BY s.Sale_DateTime DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@WalkInId", WalkInCustomerId);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvRecentSales.DataSource = dt;
                    gvRecentSales.DataBind();
                }
            }
        }

        // Manage Sales opens a sale straight away when given ?sale=<id>
        protected string SaleDetailUrl(object saleId)
        {
            return ResolveUrl("~/Manager/Sales.aspx?sale=" + Convert.ToInt32(saleId));
        }

        // Makes each Recent Sales row clickable (and reachable with the keyboard) through to the sale's details
        protected void gvRecentSales_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            object saleId = DataBinder.Eval(e.Row.DataItem, "Sale_ID");
            string url = SaleDetailUrl(saleId);

            e.Row.CssClass = "dash-row-link";
            e.Row.Attributes["data-href"] = url;
            e.Row.Attributes["tabindex"] = "0";
            e.Row.Attributes["role"] = "link";
            e.Row.Attributes["aria-label"] = "View details for sale " + saleId;
            e.Row.Attributes["onclick"] = "if (event.target.closest('a')) return; window.location.href = this.getAttribute('data-href');";
            e.Row.Attributes["onkeydown"] = "if (event.key === 'Enter') { window.location.href = this.getAttribute('data-href'); }";
        }
        // Manage Discounts opens a discount (and its products) straight away when given ?view=<id>
        protected string DiscountUrl(object discountId)
        {
            return ResolveUrl("~/Manager/Discounts.aspx?view=" + Convert.ToInt32(discountId));
        }

        // Makes each Active Discounts row clickable (and reachable with the keyboard)
        protected void gvActiveDiscounts_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            object id = DataBinder.Eval(e.Row.DataItem, "Discount_ID");
            string url = DiscountUrl(id);

            e.Row.CssClass = "dash-row-link";
            e.Row.Attributes["data-href"] = url;
            e.Row.Attributes["tabindex"] = "0";
            e.Row.Attributes["role"] = "link";
            e.Row.Attributes["aria-label"] = "Open this discount in Manage Discounts";
            e.Row.Attributes["onclick"] = "if (event.target.closest('a')) return; window.location.href = this.getAttribute('data-href');";
            e.Row.Attributes["onkeydown"] = "if (event.key === 'Enter') { window.location.href = this.getAttribute('data-href'); }";
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
                  AND s.Customer_ID <> @WalkInId
                GROUP BY c.Customer_ID, c.Customer_FirstName, c.Customer_LastName
                ORDER BY TotalSpent DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Cutoff", cutoff);
                cmd.Parameters.AddWithValue("@WalkInId", WalkInCustomerId);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvTopCustomers.EmptyDataText = "No customer purchases in the last " + RangeLabel + ".";
                    gvTopCustomers.DataSource = dt;
                    gvTopCustomers.DataBind();
                }
            }
        }


        private void LoadActiveDiscounts()
        {
            string query = @"
               SELECT Discount_ID, Discount_Name, Discount_Type, Discount_Value, Discount_EndDate
                FROM Discount
                WHERE LTRIM(RTRIM(Discount_Status)) = 'Active'
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
