using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SamsLiquorShop.Manager
{
    // SAMPLE DATA for the "12 months" filter on the manager dashboard ONLY.
    // The database has very little sales history yet, so this view shows made-up but realistic
    // figures. Names (categories, products, cashiers, customers, payment methods) are read from the
    // database where possible so it looks like this shop; only the numbers are invented.
    // The other filters (7 days, 30 days, 3 months), Low Stock, Recent Sales and Active Discounts
    // always use real data. Set UseSampleDataFor12Months to false to go back to real 12-month data.
    public partial class Dashboard
    {
        private const bool UseSampleDataFor12Months = true;

        // Typical liquor-store year: January slump, Easter bump, festive December peak
        private static readonly Dictionary<int, double> MonthSeason = new Dictionary<int, double>
        {
            { 1, 0.78 }, { 2, 0.82 }, { 3, 0.90 }, { 4, 1.02 }, { 5, 0.88 }, { 6, 0.93 },
            { 7, 0.95 }, { 8, 0.98 }, { 9, 1.06 }, { 10, 1.00 }, { 11, 1.14 }, { 12, 1.46 }
        };

        private void LoadSample12Months(DateTime cutoff, DateTime now)
        {
            // Same seed for the same month, so refreshing the page shows the same numbers
            var rng = new Random(now.Year * 100 + now.Month);

            // ---- revenue by month ----
            var labels = new List<string>();
            var values = new List<decimal>();
            int i = 0;
            for (DateTime month = cutoff; month <= now; month = month.AddMonths(1), i++)
            {
                double trend = 1 + 0.012 * i;                    // steady growth over the year
                double noise = 0.97 + rng.NextDouble() * 0.06;   // +/- 3%
                decimal revenue = Math.Round((decimal)(214000 * MonthSeason[month.Month] * trend * noise), 2);
                labels.Add(month.ToString("MMM yy"));
                values.Add(revenue);
            }
            RevenueChartJson = Serialize(new ChartData { type = "bar", labels = labels.ToArray(), values = values.ToArray(), money = true });

            // ---- KPIs ----
            decimal totalRevenue = values.Sum();
            decimal avgBasket = 387.40m;
            int transactions = (int)Math.Round(totalRevenue / avgBasket);
            decimal prevRevenue = Math.Round(totalRevenue / 1.136m, 2);   // +13.6% on the year before
            int prevTransactions = (int)Math.Round(transactions / 1.091m); // +9.1%
            decimal prevAvgBasket = prevRevenue / prevTransactions;

            litRevenue.Text = totalRevenue.ToString("C");
            litTransactions.Text = transactions.ToString("N0");
            litAvgBasket.Text = (totalRevenue / transactions).ToString("C");
            litRevenueDelta.Text = DeltaBadge(totalRevenue, prevRevenue);
            litTransactionsDelta.Text = DeltaBadge(transactions, prevTransactions);
            litAvgBasketDelta.Text = DeltaBadge(totalRevenue / transactions, prevAvgBasket);
            LoadLowStockCount();

            // ---- sales by category ----
            List<string> categories = SampleNames("SELECT Category_Name FROM Category ORDER BY Category_Name");
            if (categories.Count == 0)
                categories = new List<string> { "Spirits", "Wine", "Beer & Cider", "Ready to Drink", "Non-Alcoholic", "Gifts & Accessories" };
            var catWeights = categories.Select(c => new { Name = c, W = CategoryWeight(c) }).ToList();
            double catTotal = catWeights.Sum(c => c.W);
            var catRows = catWeights
                .Select(c => new { c.Name, V = Math.Round(totalRevenue * (decimal)(c.W / catTotal), 2) })
                .OrderByDescending(c => c.V).Take(8).ToList();
            CategoryChartJson = Serialize(new ChartData { type = "hbar", labels = catRows.Select(c => c.Name).ToArray(), values = catRows.Select(c => c.V).ToArray(), money = true });

            // ---- payment methods ----
            List<string> methods = SampleNames("SELECT PaymentMethod_Name FROM PaymentMethod ORDER BY PaymentMethod_ID");
            if (methods.Count == 0) methods = new List<string> { "Card", "Cash", "Online" };
            var payWeights = methods.Select(m => new { Name = m, W = PaymentWeight(m) }).ToList();
            double payTotal = payWeights.Sum(p => p.W);
            var payRows = payWeights.Select(p => new { p.Name, V = Math.Round(totalRevenue * (decimal)(p.W / payTotal), 2) })
                .OrderByDescending(p => p.V).ToList();
            PaymentChartJson = Serialize(new ChartData { type = "doughnut", labels = payRows.Select(p => p.Name).ToArray(), values = payRows.Select(p => p.V).ToArray(), money = true });

            // ---- top 5 products (units) ----
            List<string> products = SampleNames("SELECT TOP 40 Product_Name FROM Product WHERE Product_Status = 'Active' ORDER BY Product_ID");
            if (products.Count < 5)
                products = new List<string> { "Premium Lager 6-Pack", "Cape Chenin Blanc 750ml", "Blended Scotch Whisky 750ml", "Dry Gin 750ml", "Brut Sparkling Wine 750ml" };
            var picked = products.OrderBy(p => StableHash(p + now.ToString("yyyyMM"))).Take(5).ToList();
            decimal[] units = { 642, 571, 498, 433, 386 };
            TopProductsChartJson = Serialize(new ChartData { type = "hbar", labels = picked.ToArray(), values = units.Take(picked.Count).ToArray(), money = false });

            // ---- revenue by cashier ----
            List<string> cashiers = SampleNames(@"SELECT TOP 6 CONCAT(Employee_FirstName, ' ', Employee_LastName) FROM Employee
                                                 WHERE Employee_Status = 'Active' ORDER BY CASE WHEN Employee_Role = 'Cashier' THEN 0 ELSE 1 END, Employee_ID");
            if (cashiers.Count == 0) cashiers = new List<string> { "Thandi Mokoena", "Ravi Naidoo", "Sipho Dlamini", "Ayesha Khan" };
            double[] cashierShare = { 0.31, 0.26, 0.19, 0.12, 0.07, 0.05 };
            double shareTotal = cashierShare.Take(cashiers.Count).Sum();
            CashierChartJson = Serialize(new ChartData
            {
                type = "hbar",
                labels = cashiers.ToArray(),
                values = cashiers.Select((c, k) => Math.Round(totalRevenue * (decimal)(cashierShare[k] / shareTotal), 2)).ToArray(),
                money = true
            });

            // ---- busiest hours (09:00 - 20:00, after-work peak) ----
            int[] hours = { 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
            double[] hourWeight = { 3, 5, 7, 9, 9.5, 8, 9, 11.5, 14.5, 13, 8, 3.5 };
            double hourTotal = hourWeight.Sum();
            HourlyChartJson = Serialize(new ChartData
            {
                type = "bar",
                labels = hours.Select(h => h.ToString("00") + ":00").ToArray(),
                values = hourWeight.Select(w => (decimal)Math.Round(transactions * w / hourTotal)).ToArray(),
                money = false
            });

            // ---- top customers ----
            List<string> customers = SampleNames(@"SELECT TOP 5 CONCAT(Customer_FirstName, ' ', Customer_LastName) FROM Customer
                                                  WHERE Customer_ID <> @WalkInId AND Customer_Status = 'Active'
                                                    AND LTRIM(RTRIM(CONCAT(Customer_FirstName, Customer_LastName))) <> ''
                                                  ORDER BY Customer_ID");
            if (customers.Count < 5)
            {
                foreach (string n in new[] { "Nomvula Zulu", "Kevin Pillay", "Lerato Mahlangu", "Johan van Wyk", "Priya Govender" })
                    if (customers.Count < 5 && !customers.Contains(n)) customers.Add(n);
            }
            int[] orders = { 46, 39, 33, 28, 24 };
            decimal[] spent = { 18420.50m, 15275.00m, 12890.75m, 10645.20m, 9310.00m };
            var dt = new DataTable();
            dt.Columns.Add("CustomerName", typeof(string));
            dt.Columns.Add("OrderCount", typeof(int));
            dt.Columns.Add("TotalSpent", typeof(decimal));
            for (int k = 0; k < customers.Count; k++) dt.Rows.Add(customers[k], orders[k], spent[k]);
            gvTopCustomers.EmptyDataText = "No customer purchases in the last " + RangeLabel + ".";
            gvTopCustomers.DataSource = dt;
            gvTopCustomers.DataBind();
        }

        // Share of revenue by category type (only the proportions matter)
        private static double CategoryWeight(string name)
        {
            string n = (name ?? "").ToLowerInvariant();
            if (n.Contains("whisk") || n.Contains("spirit") || n.Contains("brandy")) return 30;
            if (n.Contains("wine") || n.Contains("champagne") || n.Contains("sparkling")) return 24;
            if (n.Contains("beer") || n.Contains("cider") || n.Contains("lager")) return 21;
            if (n.Contains("vodka") || n.Contains("gin") || n.Contains("rum") || n.Contains("tequila")) return 14;
            if (n.Contains("ready") || n.Contains("rtd") || n.Contains("cooler")) return 9;
            if (n.Contains("non") || n.Contains("free") || n.Contains("mixer") || n.Contains("soft")) return 4.5;
            if (n.Contains("gift") || n.Contains("access") || n.Contains("glass")) return 3.5;
            return 6;
        }

        private static double PaymentWeight(string name)
        {
            string n = (name ?? "").ToLowerInvariant();
            if (n.Contains("card") || n.Contains("credit") || n.Contains("debit")) return 58;
            if (n.Contains("cash")) return 27;
            if (n.Contains("online") || n.Contains("eft") || n.Contains("web")) return 13;
            return 4;
        }

        // FNV-1a: same value on every server restart (string.GetHashCode can be randomized)
        private static uint StableHash(string s)
        {
            uint h = 2166136261;
            foreach (char c in s) { h ^= c; h *= 16777619; }
            return h;
        }

        // First column of a query as a list of non-blank strings; empty if the query fails
        private List<string> SampleNames(string sql)
        {
            var list = new List<string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@WalkInId", WalkInCustomerId);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string s = r[0] == DBNull.Value ? "" : r[0].ToString().Trim();
                            if (s.Length > 0 && !list.Contains(s)) list.Add(s);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceWarning("Dashboard sample data: " + ex.Message);
            }
            return list;
        }
    }
}
