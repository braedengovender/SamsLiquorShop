using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SamsLiquorShop.Controls
{
    // Read-only browser for POS sales (Sale / SaleLine / Payment). The POS is where sales are made,
    // so nothing here changes a sale. Used by Manager/Sales.aspx and Employee/Orders.aspx.
    public partial class SalesBrowser : System.Web.UI.UserControl
    {
        // Walk-in sales on the POS are recorded against this placeholder customer
        private const int WalkInCustomerId = 999999;

        // Cashier view: defaults to the logged-in cashier's own sales and has no CSV export
        public bool CashierMode { get; set; }

        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private static readonly HashSet<string> AllowedSort = new HashSet<string>
        {
            "Sale_ID", "Sale_DateTime", "CustomerName", "EmployeeName", "ItemCount", "Sale_Status", "Sale_TotalAmount"
        };

        private string SortField
        {
            get { return (string)ViewState["SortField"] ?? "Sale_DateTime"; }
            set { ViewState["SortField"] = value; }
        }

        private string SortDir
        {
            get { return (string)ViewState["SortDir"] ?? "DESC"; }
            set { ViewState["SortDir"] = value; }
        }

        private int CurrentEmployeeId => Session["EmployeeID"] != null ? Convert.ToInt32(Session["EmployeeID"]) : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Typing in the search box clicks Search after a short pause (handled in Site.Master)
            txtSearch.Attributes["data-live-search"] = btnSearch.ClientID;

            if (!IsPostBack)
            {
                litTitle.Text = CashierMode ? "Orders" : "Manage Sales";
                litSubtitle.Text = CashierMode
                    ? "Sales processed at the till. Showing your own sales by default; pick another cashier or All cashiers to look further."
                    : "Every sale made at the till, with items, payments, discounts and loyalty points.";
                btnExport.Visible = !CashierMode;

                LoadFilterOptions();
                ResetFilters();
                BindSales();

                // Links such as Sales.aspx?sale=64 (from a customer's purchase history) open that sale straight away
                int saleId;
                if (int.TryParse(Request.QueryString["sale"], out saleId) && saleId > 0)
                {
                    ShowDetail(saleId);
                }
            }
        }

        // ---------- FILTER OPTIONS ----------
        private void LoadFilterOptions()
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                ddlCashier.Items.Clear();
                ddlCashier.Items.Add(new ListItem("All cashiers", ""));
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT Employee_ID, LTRIM(RTRIM(CONCAT(Employee_FirstName, ' ', Employee_LastName))) AS Name
                    FROM Employee ORDER BY Employee_FirstName, Employee_LastName", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read()) ddlCashier.Items.Add(new ListItem(r["Name"].ToString(), r["Employee_ID"].ToString()));
                }

                ddlPayment.Items.Clear();
                ddlPayment.Items.Add(new ListItem("All payment methods", ""));
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT PaymentMethod_ID, PaymentMethod_Name FROM PaymentMethod ORDER BY PaymentMethod_Name", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read()) ddlPayment.Items.Add(new ListItem(r["PaymentMethod_Name"].ToString(), r["PaymentMethod_ID"].ToString()));
                }

                // Statuses come from the data, so whatever the POS records (Completed, Voided, ...) can be filtered on
                ddlStatus.Items.Clear();
                ddlStatus.Items.Add(new ListItem("All statuses", ""));
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT DISTINCT LTRIM(RTRIM(Sale_Status)) AS Status FROM Sale WHERE Sale_Status IS NOT NULL ORDER BY Status", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read()) ddlStatus.Items.Add(new ListItem(r["Status"].ToString(), r["Status"].ToString()));
                }
            }
        }

        private void ResetFilters()
        {
            txtSearch.Text = "";
            ddlRange.SelectedValue = "all";
            ddlPayment.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
            txtFrom.Text = "";
            txtTo.Text = "";
            pnlCustomDates.Visible = false;

            ddlCashier.ClearSelection();
            ListItem mine = CashierMode ? ddlCashier.Items.FindByValue(CurrentEmployeeId.ToString()) : null;
            if (mine != null) mine.Selected = true;
            else ddlCashier.SelectedIndex = 0;
        }

        // Start (inclusive) and end (exclusive) of the chosen date range; null means unbounded
        private void GetDateRange(out DateTime? from, out DateTime? to)
        {
            DateTime today = DateTime.Today;
            from = null;
            to = null;

            switch (ddlRange.SelectedValue)
            {
                case "today": from = today; break;
                case "7d": from = today.AddDays(-6); break;
                case "30d": from = today.AddDays(-29); break;
                case "3m": from = today.AddMonths(-3); break;
                case "12m": from = today.AddMonths(-12); break;
                case "custom":
                    DateTime d;
                    if (DateTime.TryParseExact(txtFrom.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) from = d;
                    if (DateTime.TryParseExact(txtTo.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) to = d.AddDays(1);
                    break;
            }
        }

        // ---------- READ ----------
        private DataTable LoadSalesTable()
        {
            var where = new List<string>();

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@WalkInId", WalkInCustomerId);

                string search = txtSearch.Text.Trim();
                if (search.Length > 0)
                {
                    // "INV-000064", "000064" and "64" all find sale 64; anything else searches customer names
                    string digits = Regex.Replace(search, @"^(?i:inv)-?", "");
                    int saleId;
                    if (Regex.IsMatch(digits, @"^\d{1,9}$") && int.TryParse(digits, out saleId))
                    {
                        where.Add("s.Sale_ID = @SaleId");
                        cmd.Parameters.AddWithValue("@SaleId", saleId);
                    }
                    else
                    {
                        where.Add(@"(c.Customer_FirstName LIKE @Search OR c.Customer_LastName LIKE @Search
                                     OR (c.Customer_FirstName + ' ' + c.Customer_LastName) LIKE @Search)");
                        cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                    }
                }

                DateTime? from, to;
                GetDateRange(out from, out to);
                if (from.HasValue)
                {
                    where.Add("s.Sale_DateTime >= @From");
                    cmd.Parameters.AddWithValue("@From", from.Value);
                }
                if (to.HasValue)
                {
                    where.Add("s.Sale_DateTime < @To");
                    cmd.Parameters.AddWithValue("@To", to.Value);
                }
                if (ddlCashier.SelectedValue != "")
                {
                    where.Add("s.Employee_ID = @EmployeeId");
                    cmd.Parameters.AddWithValue("@EmployeeId", int.Parse(ddlCashier.SelectedValue));
                }
                if (ddlPayment.SelectedValue != "")
                {
                    where.Add("EXISTS (SELECT 1 FROM Payment px WHERE px.Sale_ID = s.Sale_ID AND px.PaymentMethod_ID = @PaymentMethodId)");
                    cmd.Parameters.AddWithValue("@PaymentMethodId", int.Parse(ddlPayment.SelectedValue));
                }
                if (ddlStatus.SelectedValue != "")
                {
                    where.Add("LTRIM(RTRIM(s.Sale_Status)) = @Status");
                    cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                }

                string sortField = AllowedSort.Contains(SortField) ? SortField : "Sale_DateTime";
                string dir = SortDir == "ASC" ? "ASC" : "DESC";

                // CONCAT turns NULL names into ' ', so blank names and the walk-in placeholder become NULL (shown as "Walk-in")
                cmd.CommandText = @"
                    SELECT * FROM (
                        SELECT s.Sale_ID, s.Sale_DateTime, s.Sale_Subtotal, ISNULL(s.Sale_DiscountAmount, 0) AS Sale_DiscountAmount,
                               s.Sale_TotalAmount, LTRIM(RTRIM(s.Sale_Status)) AS Sale_Status,
                               ISNULL(s.Sale_LoyaltyPointsEarned, 0) AS Sale_LoyaltyPointsEarned,
                               CASE WHEN s.Customer_ID IS NULL OR s.Customer_ID = @WalkInId THEN NULL
                                    ELSE NULLIF(LTRIM(RTRIM(CONCAT(c.Customer_FirstName, ' ', c.Customer_LastName))), '')
                               END AS CustomerName,
                               NULLIF(LTRIM(RTRIM(CONCAT(e.Employee_FirstName, ' ', e.Employee_LastName))), '') AS EmployeeName,
                               (SELECT ISNULL(SUM(SaleLine_Quantity), 0) FROM SaleLine WHERE Sale_ID = s.Sale_ID) AS ItemCount,
                               ISNULL((SELECT STRING_AGG(m.Name, ', ') FROM
                                          (SELECT DISTINCT pm.PaymentMethod_Name AS Name
                                           FROM Payment p INNER JOIN PaymentMethod pm ON pm.PaymentMethod_ID = p.PaymentMethod_ID
                                           WHERE p.Sale_ID = s.Sale_ID) m), '') AS PaymentMethods
                        FROM Sale s
                        LEFT JOIN Customer c ON c.Customer_ID = s.Customer_ID
                        LEFT JOIN Employee e ON e.Employee_ID = s.Employee_ID"
                        + (where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : "") + @"
                    ) t
                    ORDER BY " + sortField + " " + dir + ", Sale_ID DESC";

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        private void BindSales()
        {
            DataTable dt = LoadSalesTable();
            int total = dt.Rows.Count;

            int pages = (int)Math.Ceiling(total / (double)gvSales.PageSize);
            if (pages > 0 && gvSales.PageIndex >= pages) gvSales.PageIndex = pages - 1;

            gvSales.DataSource = dt;
            gvSales.DataBind();

            // Summary cards describe exactly the sales matching the filters
            decimal revenue = 0, discounts = 0;
            foreach (DataRow r in dt.Rows)
            {
                revenue += Convert.ToDecimal(r["Sale_TotalAmount"]);
                discounts += Convert.ToDecimal(r["Sale_DiscountAmount"]);
            }
            litKpiRevenue.Text = revenue.ToString("C");
            litKpiCount.Text = total.ToString("N0");
            litKpiAverage.Text = (total > 0 ? revenue / total : 0).ToString("C");
            litKpiDiscounts.Text = discounts.ToString("C");

            if (total == 0)
            {
                litCount.Text = "No sales found";
            }
            else
            {
                int start = gvSales.PageIndex * gvSales.PageSize + 1;
                int end = Math.Min(start + gvSales.PageSize - 1, total);
                litCount.Text = "Showing " + start + "–" + end + " of " + total.ToString("N0") + (total == 1 ? " sale" : " sales");
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvSales.PageIndex = 0;
            BindSales();
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            pnlCustomDates.Visible = ddlRange.SelectedValue == "custom";
            gvSales.PageIndex = 0;
            BindSales();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ResetFilters();
            gvSales.PageIndex = 0;
            BindSales();
        }

        protected void gvSales_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvSales.PageIndex = e.NewPageIndex;
            BindSales();
        }

        protected void gvSales_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (SortField == e.SortExpression)
            {
                SortDir = (SortDir == "ASC") ? "DESC" : "ASC";
            }
            else
            {
                SortField = e.SortExpression;
                // Newest, biggest and busiest first; names A-Z
                SortDir = (e.SortExpression == "CustomerName" || e.SortExpression == "EmployeeName" || e.SortExpression == "Sale_Status")
                    ? "ASC" : "DESC";
            }
            gvSales.PageIndex = 0;
            BindSales();
        }

        // Adds an up/down arrow to the header of the column being sorted
        protected void gvSales_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.Header) return;

            foreach (TableCell cell in e.Row.Cells)
            {
                if (cell.Controls.Count == 0) continue;
                LinkButton lb = cell.Controls[0] as LinkButton;
                if (lb != null && lb.CommandArgument == SortField)
                {
                    lb.Text += (SortDir == "ASC") ? " ▲" : " ▼";
                }
            }
        }

        protected void gvSales_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "ViewSale") return;
            litMessage.Text = "";
            ShowDetail(Convert.ToInt32(e.CommandArgument));
        }

        // ---------- Helpers used inside the grid markup ----------
        protected string InvoiceNumber(object saleId)
        {
            return "INV-" + Convert.ToInt32(saleId).ToString("000000");
        }

        // Walk-in sales (no named customer) get a small grey tag
        protected string CustomerCell(object customerName)
        {
            if (customerName == null || customerName == DBNull.Value)
            {
                return "<span class=\"dash-tag\">Walk-in</span>";
            }
            return Server.HtmlEncode(customerName.ToString());
        }

        // Green badge for finished sales/payments, grey for anything else (voided, refunded, pending...)
        protected string StatusClass(object status)
        {
            string s = (Convert.ToString(status) ?? "").Trim().ToLower();
            return s == "completed" || s == "complete" || s == "paid" || s == "active" ? "active" : "inactive";
        }

        protected string LineDiscount(object discountName)
        {
            string d = (Convert.ToString(discountName) ?? "").Trim();
            return d.Length == 0 ? "" : "<span class=\"emp-sub\">" + Server.HtmlEncode(d) + "</span>";
        }

        // ---------- SALE DETAILS ----------
        private void ShowDetail(int saleId)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT s.Sale_ID, s.Sale_DateTime, s.Sale_Subtotal, ISNULL(s.Sale_DiscountAmount, 0) AS Sale_DiscountAmount,
                           s.Sale_TotalAmount, LTRIM(RTRIM(s.Sale_Status)) AS Sale_Status,
                           ISNULL(s.Sale_LoyaltyPointsEarned, 0) AS Sale_LoyaltyPointsEarned, s.Employee_ID,
                           CASE WHEN s.Customer_ID IS NULL OR s.Customer_ID = @WalkInId THEN NULL
                                ELSE NULLIF(LTRIM(RTRIM(CONCAT(c.Customer_FirstName, ' ', c.Customer_LastName))), '')
                           END AS CustomerName,
                           NULLIF(LTRIM(RTRIM(CONCAT(e.Employee_FirstName, ' ', e.Employee_LastName))), '') AS EmployeeName,
                           st.SaleType_Name, d.Discount_Name,
                           (SELECT ISNULL(SUM(Payment_LoyaltyPointsUsed), 0) FROM Payment WHERE Sale_ID = s.Sale_ID) AS PointsUsed
                    FROM Sale s
                    LEFT JOIN Customer c ON c.Customer_ID = s.Customer_ID
                    LEFT JOIN Employee e ON e.Employee_ID = s.Employee_ID
                    LEFT JOIN SaleType st ON st.SaleType_ID = s.SaleType_ID
                    LEFT JOIN Discount d ON d.Discount_ID = s.Discount_ID
                    WHERE s.Sale_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", saleId);
                    cmd.Parameters.AddWithValue("@WalkInId", WalkInCustomerId);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            ShowMessage("Sale " + InvoiceNumber(saleId) + " could not be found.");
                            pnlDetail.Visible = false;
                            return;
                        }

                        string status = r["Sale_Status"].ToString();
                        litDetailInvoice.Text = InvoiceNumber(saleId);
                        litDetailStatus.Text = "<span class=\"emp-badge emp-badge-" + StatusClass(status) + "\">" + Server.HtmlEncode(status) + "</span>";
                        litDetailDate.Text = Convert.ToDateTime(r["Sale_DateTime"]).ToString("d MMM yyyy, HH:mm");
                        litDetailCustomer.Text = CustomerCell(r["CustomerName"]);
                        litDetailCashier.Text = r["EmployeeName"] == DBNull.Value ? "Unknown" : Server.HtmlEncode(r["EmployeeName"].ToString());
                        litDetailType.Text = r["SaleType_Name"] == DBNull.Value ? "&mdash;" : Server.HtmlEncode(r["SaleType_Name"].ToString());
                        litDetailPointsEarned.Text = Convert.ToInt32(r["Sale_LoyaltyPointsEarned"]).ToString("N0");
                        litDetailPointsUsed.Text = Convert.ToInt32(r["PointsUsed"]).ToString("N0");

                        decimal discount = Convert.ToDecimal(r["Sale_DiscountAmount"]);
                        litDetailSubtotal.Text = Convert.ToDecimal(r["Sale_Subtotal"]).ToString("C");
                        litDetailDiscount.Text = discount > 0 ? "&minus;" + discount.ToString("C") : 0m.ToString("C");
                        litDetailDiscountName.Text = r["Discount_Name"] == DBNull.Value
                            ? ""
                            : "<span class=\"emp-sub\">(" + Server.HtmlEncode(r["Discount_Name"].ToString()) + ")</span>";
                        litDetailTotal.Text = Convert.ToDecimal(r["Sale_TotalAmount"]).ToString("C");
                    }
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT p.Product_Name, sl.SaleLine_Quantity, sl.SaleLine_OriginalUnitPrice,
                           sl.SaleLine_UnitPriceAfterDiscount, sl.SaleLine_Subtotal, d.Discount_Name
                    FROM SaleLine sl
                    INNER JOIN Product p ON p.Product_ID = sl.Product_ID
                    LEFT JOIN Discount d ON d.Discount_ID = sl.Discount_ID
                    WHERE sl.Sale_ID = @Id
                    ORDER BY p.Product_Name", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", saleId);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        gvLines.DataSource = dt;
                        gvLines.DataBind();
                    }
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT pm.PaymentMethod_Name, p.Payment_DateTime, ISNULL(p.Payment_LoyaltyPointsUsed, 0) AS Payment_LoyaltyPointsUsed,
                           p.Payment_AmountTendered, p.Payment_ChangeAmount, p.Payment_AmountPaid, LTRIM(RTRIM(p.Payment_Status)) AS Payment_Status
                    FROM Payment p
                    LEFT JOIN PaymentMethod pm ON pm.PaymentMethod_ID = p.PaymentMethod_ID
                    WHERE p.Sale_ID = @Id
                    ORDER BY p.Payment_DateTime, p.Payment_ID", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", saleId);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        gvPayments.DataSource = dt;
                        gvPayments.DataBind();
                    }
                }
            }

            pnlDetail.Visible = true;
            ScriptManager.RegisterStartupScript(this, GetType(), "scrollDetail",
                "var f=document.getElementById('saleDetailCard'); if(f){f.scrollIntoView({behavior:'smooth',block:'start'});}", true);
        }

        protected void btnCloseDetail_Click(object sender, EventArgs e)
        {
            pnlDetail.Visible = false;
        }

        // ---------- EXPORT (manager only) ----------
        protected void btnExport_Click(object sender, EventArgs e)
        {
            if (CashierMode) return;

            DataTable dt = LoadSalesTable();
            var inv = CultureInfo.InvariantCulture;

            var sb = new StringBuilder();
            sb.AppendLine("Invoice,Date,Customer,Cashier,Items,Paid By,Status,Subtotal,Discount,Total,Points Earned");

            foreach (DataRow r in dt.Rows)
            {
                sb.AppendLine(string.Join(",", new[]
                {
                    Csv(InvoiceNumber(r["Sale_ID"])),
                    Csv(Convert.ToDateTime(r["Sale_DateTime"]).ToString("yyyy-MM-dd HH:mm")),
                    Csv(r["CustomerName"] == DBNull.Value ? "Walk-in" : r["CustomerName"]),
                    Csv(r["EmployeeName"]),
                    Csv(r["ItemCount"]),
                    Csv(r["PaymentMethods"]),
                    Csv(r["Sale_Status"]),
                    Csv(Convert.ToDecimal(r["Sale_Subtotal"]).ToString("0.00", inv)),
                    Csv(Convert.ToDecimal(r["Sale_DiscountAmount"]).ToString("0.00", inv)),
                    Csv(Convert.ToDecimal(r["Sale_TotalAmount"]).ToString("0.00", inv)),
                    Csv(r["Sale_LoyaltyPointsEarned"])
                }));
            }

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.ContentEncoding = Encoding.UTF8;
            Response.AddHeader("Content-Disposition", "attachment; filename=sales-" + DateTime.Today.ToString("yyyyMMdd") + ".csv");
            Response.BinaryWrite(Encoding.UTF8.GetPreamble());
            Response.Write(sb.ToString());
            Response.End();
        }

        // Quotes a CSV value and defuses spreadsheet formulas (cells starting with = + - @)
        private static string Csv(object value)
        {
            string s = Convert.ToString(value) ?? "";
            if (s.Length > 0 && "=+-@".IndexOf(s[0]) >= 0 && !Regex.IsMatch(s, @"^\+?[0-9]+$"))
                s = "'" + s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        private void ShowMessage(string text)
        {
            litMessage.Text = "<div class=\"emp-alert emp-alert-error\">" + Server.HtmlEncode(text) + "</div>";
        }
    }
}
