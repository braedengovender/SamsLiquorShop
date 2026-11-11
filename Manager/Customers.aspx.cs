using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SamsLiquorShop.Manager
{
    public partial class Customers : System.Web.UI.Page
    {
        // Walk-in sales on the POS are recorded against this placeholder customer; it isn't a real person to manage
        private const int WalkInCustomerId = 999999;

        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private static readonly HashSet<string> AllowedSort = new HashSet<string>
        {
            "Customer_LastName", "Customer_EmailAddress", "Customer_Status",
            "Customer_LoyaltyPointsBalance", "TotalSpent", "OrderCount"
        };

        private string SortField
        {
            get { return (string)ViewState["SortField"] ?? "Customer_LastName"; }
            set { ViewState["SortField"] = value; }
        }

        private string SortDir
        {
            get { return (string)ViewState["SortDir"] ?? "ASC"; }
            set { ViewState["SortDir"] = value; }
        }

        private TextBox[] FormTextBoxes => new[]
        {
            txtFirstName, txtLastName, txtUsername, txtEmail, txtIdNumber, txtPhone,
            txtUnitNumber, txtUnitName, txtStreetNumber, txtStreetName, txtSuburb, txtCity, txtPostalCode
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Manager")) return;

            // Typing in the search box clicks Search after a short pause (handled in Site.Master)
            txtSearch.Attributes["data-live-search"] = btnSearch.ClientID;

            if (!IsPostBack)
            {
                RefreshAll();
            }
        }

        private void RefreshAll()
        {
            LoadKpis();
            BindCustomers();
        }

        // ---------- READ ----------
        private DataTable LoadCustomerTable()
        {
            var where = new List<string> { "c.Customer_ID <> @WalkInId" };

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@WalkInId", WalkInCustomerId);

                string search = txtSearch.Text.Trim();
                if (search.Length > 0)
                {
                    where.Add(@"(c.Customer_FirstName LIKE @Search OR c.Customer_LastName LIKE @Search
                                 OR (c.Customer_FirstName + ' ' + c.Customer_LastName) LIKE @Search
                                 OR c.Customer_Username LIKE @Search OR c.Customer_EmailAddress LIKE @Search
                                 OR c.Customer_PhoneNumber LIKE @Search OR c.Customer_IDNumber LIKE @Search)");
                    cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                }
                if (ddlStatusFilter.SelectedValue != "")
                {
                    where.Add("c.Customer_Status = @Status");
                    cmd.Parameters.AddWithValue("@Status", ddlStatusFilter.SelectedValue);
                }
                if (ddlTypeFilter.SelectedValue == "online")
                {
                    where.Add("NULLIF(LTRIM(RTRIM(c.Customer_Username)), '') IS NOT NULL");
                }
                else if (ddlTypeFilter.SelectedValue == "instore")
                {
                    where.Add("NULLIF(LTRIM(RTRIM(c.Customer_Username)), '') IS NULL");
                }

                string sortField = AllowedSort.Contains(SortField) ? SortField : "Customer_LastName";
                string dir = SortDir == "DESC" ? "DESC" : "ASC";

                // Purchase totals come from the POS Sale table; the outer SELECT lets us sort by those totals
                cmd.CommandText = @"
                    SELECT * FROM (
                        SELECT c.Customer_ID, c.Customer_FirstName, c.Customer_LastName, c.Customer_Username,
                               c.Customer_EmailAddress, c.Customer_PhoneNumber, c.Customer_IDNumber,
                               c.Customer_Status, ISNULL(c.Customer_LoyaltyPointsBalance, 0) AS Customer_LoyaltyPointsBalance,
                               c.Customer_RegistrationDateTime,
                               ISNULL(s.OrderCount, 0) AS OrderCount, ISNULL(s.TotalSpent, 0) AS TotalSpent, s.LastPurchase
                        FROM Customer c
                        LEFT JOIN (SELECT Customer_ID, COUNT(*) AS OrderCount, SUM(Sale_TotalAmount) AS TotalSpent,
                                          MAX(Sale_DateTime) AS LastPurchase
                                   FROM Sale GROUP BY Customer_ID) s ON s.Customer_ID = c.Customer_ID
                        WHERE " + string.Join(" AND ", where) + @"
                    ) t
                    ORDER BY " + sortField + " " + dir + ", Customer_FirstName, Customer_ID";

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        private void BindCustomers()
        {
            DataTable dt = LoadCustomerTable();
            int total = dt.Rows.Count;

            int pages = (int)Math.Ceiling(total / (double)gvCustomers.PageSize);
            if (pages > 0 && gvCustomers.PageIndex >= pages) gvCustomers.PageIndex = pages - 1;

            gvCustomers.DataSource = dt;
            gvCustomers.DataBind();

            if (total == 0)
            {
                litCount.Text = "No customers found";
            }
            else
            {
                int start = gvCustomers.PageIndex * gvCustomers.PageSize + 1;
                int end = Math.Min(start + gvCustomers.PageSize - 1, total);
                litCount.Text = "Showing " + start + "–" + end + " of " + total + (total == 1 ? " customer" : " customers");
            }
        }

        private void LoadKpis()
        {
            DateTime monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(*),
                       ISNULL(SUM(CASE WHEN Customer_Status = 'Active' THEN 1 ELSE 0 END), 0),
                       ISNULL(SUM(CASE WHEN Customer_Status <> 'Active' THEN 1 ELSE 0 END), 0),
                       ISNULL(SUM(CASE WHEN Customer_RegistrationDateTime >= @MonthStart THEN 1 ELSE 0 END), 0)
                FROM Customer
                WHERE Customer_ID <> @WalkInId", conn))
            {
                cmd.Parameters.AddWithValue("@MonthStart", monthStart);
                cmd.Parameters.AddWithValue("@WalkInId", WalkInCustomerId);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        litKpiTotal.Text = r.GetInt32(0).ToString();
                        litKpiActive.Text = r.GetInt32(1).ToString();
                        litKpiInactive.Text = r.GetInt32(2).ToString();
                        litKpiNew.Text = r.GetInt32(3).ToString();
                    }
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvCustomers.PageIndex = 0;
            BindCustomers();
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            gvCustomers.PageIndex = 0;
            BindCustomers();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlStatusFilter.SelectedIndex = 0;
            ddlTypeFilter.SelectedIndex = 0;
            gvCustomers.PageIndex = 0;
            BindCustomers();
        }

        protected void gvCustomers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvCustomers.PageIndex = e.NewPageIndex;
            BindCustomers();
        }

        protected void gvCustomers_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (SortField == e.SortExpression)
            {
                SortDir = (SortDir == "ASC") ? "DESC" : "ASC";
            }
            else
            {
                SortField = e.SortExpression;
                // Money and points read best biggest-first
                SortDir = (e.SortExpression == "TotalSpent" || e.SortExpression == "OrderCount"
                           || e.SortExpression == "Customer_LoyaltyPointsBalance") ? "DESC" : "ASC";
            }
            gvCustomers.PageIndex = 0;
            BindCustomers();
        }

        // Adds an up/down arrow to the header of the column being sorted
        protected void gvCustomers_RowDataBound(object sender, GridViewRowEventArgs e)
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

        // ---------- Helpers used inside the grid markup ----------
        protected string Initials(object first, object last)
        {
            string f = (Convert.ToString(first) ?? "").Trim();
            string l = (Convert.ToString(last) ?? "").Trim();
            string s = (f.Length > 0 ? f.Substring(0, 1) : "") + (l.Length > 0 ? l.Substring(0, 1) : "");
            return Server.HtmlEncode(s.ToUpper());
        }

        // "@username" for website accounts, a grey tag for customers who only buy in store
        protected string AccountLabel(object username)
        {
            string u = (Convert.ToString(username) ?? "").Trim();
            return u.Length > 0
                ? "<span class=\"emp-sub\">@" + Server.HtmlEncode(u) + "</span>"
                : "<span class=\"emp-sub\"><span class=\"dash-tag\">In-store only</span></span>";
        }

        protected string StatusClass(object status)
        {
            return (Convert.ToString(status) ?? "").Trim().ToLower() == "active" ? "active" : "inactive";
        }

        protected string InvoiceNumber(object saleId)
        {
            return "INV-" + Convert.ToInt32(saleId).ToString("000000");
        }

        protected string DeleteJs(object first, object last)
        {
            string name = HttpUtility.JavaScriptStringEncode((Convert.ToString(first) + " " + Convert.ToString(last)).Trim());
            return "return dashConfirm(this, 'Delete customer', 'Permanently delete " + name
                + " and their wishlist? This cannot be undone.', 'Delete', true);";
        }

        protected string DeactivateJs(object status, object first, object last)
        {
            if (StatusClass(status) != "active") return "";

            string name = HttpUtility.JavaScriptStringEncode((Convert.ToString(first) + " " + Convert.ToString(last)).Trim());
            return "return dashConfirm(this, 'Deactivate customer', '" + name
                + " will no longer be able to log in or earn loyalty points. You can reactivate them at any time.', 'Deactivate', false);";
        }

        // ---------- ROW BUTTONS ----------
        protected void gvCustomers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "ViewCust" && e.CommandName != "EditCust"
                && e.CommandName != "ToggleCust" && e.CommandName != "DeleteCust") return;

            int id = Convert.ToInt32(e.CommandArgument);
            litMessage.Text = "";

            switch (e.CommandName)
            {
                case "ViewCust": ShowHistory(id); break;
                case "EditCust": LoadForEdit(id); break;
                case "ToggleCust": ToggleStatus(id); break;
                case "DeleteCust": DeleteCustomer(id); break;
            }
        }

        // ---------- PURCHASE HISTORY ----------
        private void ShowHistory(int id)
        {
            pnlForm.Visible = false;

            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT c.Customer_FirstName, c.Customer_LastName, ISNULL(c.Customer_LoyaltyPointsBalance, 0) AS Points,
                           COUNT(s.Sale_ID) AS OrderCount, ISNULL(SUM(s.Sale_TotalAmount), 0) AS TotalSpent,
                           MAX(s.Sale_DateTime) AS LastPurchase
                    FROM Customer c
                    LEFT JOIN Sale s ON s.Customer_ID = c.Customer_ID
                    WHERE c.Customer_ID = @Id
                    GROUP BY c.Customer_FirstName, c.Customer_LastName, c.Customer_LoyaltyPointsBalance", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            ShowMessage("That customer could not be found.", false);
                            return;
                        }

                        int orders = Convert.ToInt32(r["OrderCount"]);
                        decimal spent = Convert.ToDecimal(r["TotalSpent"]);

                        litHistoryName.Text = Server.HtmlEncode((r["Customer_FirstName"] + " " + r["Customer_LastName"]).Trim());
                        litHistorySpent.Text = spent.ToString("C");
                        litHistoryOrders.Text = orders.ToString();
                        litHistoryAvg.Text = (orders > 0 ? spent / orders : 0).ToString("C");
                        litHistoryPoints.Text = Convert.ToInt32(r["Points"]).ToString("N0")
                            + " <span class=\"emp-sub\">worth " + Convert.ToDecimal(r["Points"]).ToString("C") + "</span>";
                        litHistoryLast.Text = r["LastPurchase"] == DBNull.Value
                            ? "Never"
                            : Convert.ToDateTime(r["LastPurchase"]).ToString("d MMM yyyy");
                    }
                }

                // Latest 20 purchases with item count and how they were paid
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP 20 s.Sale_ID, s.Sale_DateTime, s.Sale_TotalAmount, ISNULL(s.Sale_LoyaltyPointsEarned, 0) AS Sale_LoyaltyPointsEarned,
                           (SELECT ISNULL(SUM(SaleLine_Quantity), 0) FROM SaleLine WHERE Sale_ID = s.Sale_ID) AS ItemCount,
                           ISNULL((SELECT STRING_AGG(pm.PaymentMethod_Name, ', ')
                                   FROM Payment p INNER JOIN PaymentMethod pm ON pm.PaymentMethod_ID = p.PaymentMethod_ID
                                   WHERE p.Sale_ID = s.Sale_ID), '') AS PaymentMethods
                    FROM Sale s
                    WHERE s.Customer_ID = @Id
                    ORDER BY s.Sale_DateTime DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        gvHistory.DataSource = dt;
                        gvHistory.DataBind();
                    }
                }
            }

            pnlHistory.Visible = true;
            ScrollTo("custHistoryCard", null);
        }

        protected void btnCloseHistory_Click(object sender, EventArgs e)
        {
            pnlHistory.Visible = false;
        }

        // ---------- UPDATE (load into form) ----------
        private void LoadForEdit(int id)
        {
            pnlHistory.Visible = false;

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT Customer_FirstName, Customer_LastName, Customer_Username, Customer_EmailAddress,
                       Customer_IDNumber, Customer_PhoneNumber, Customer_UnitNumber, Customer_UnitName,
                       Customer_StreetNumber, Customer_StreetName, Customer_Suburb, Customer_City,
                       Customer_PostalCode, Customer_Province, Customer_Status,
                       ISNULL(Customer_LoyaltyPointsBalance, 0) AS Points
                FROM Customer WHERE Customer_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        ShowMessage("That customer could not be found.", false);
                        return;
                    }

                    ResetInvalid();
                    hfCustomerId.Value = id.ToString();
                    txtFirstName.Text = Val(r, "Customer_FirstName");
                    txtLastName.Text = Val(r, "Customer_LastName");
                    txtUsername.Text = Val(r, "Customer_Username");
                    txtEmail.Text = Val(r, "Customer_EmailAddress");
                    txtIdNumber.Text = Val(r, "Customer_IDNumber");
                    txtPhone.Text = Val(r, "Customer_PhoneNumber");
                    txtUnitNumber.Text = Val(r, "Customer_UnitNumber");
                    txtUnitName.Text = Val(r, "Customer_UnitName");
                    txtStreetNumber.Text = Val(r, "Customer_StreetNumber");
                    txtStreetName.Text = Val(r, "Customer_StreetName");
                    txtSuburb.Text = Val(r, "Customer_Suburb");
                    txtCity.Text = Val(r, "Customer_City");
                    txtPostalCode.Text = Val(r, "Customer_PostalCode");
                    SelectValue(ddlProvince, Val(r, "Customer_Province"));
                    SelectValue(ddlStatus, StatusClass(r["Customer_Status"]) == "active" ? "Active" : "Inactive");

                    // Points are shown but not editable: balances must stay in step with the POS loyalty rules
                    litFormPoints.Text = Convert.ToInt32(r["Points"]).ToString("N0") + " loyalty points";
                    litFormTitle.Text = "Edit Customer: " + Server.HtmlEncode((txtFirstName.Text + " " + txtLastName.Text).Trim());
                }
            }

            pnlForm.Visible = true;
            ScrollTo("custFormCard", txtFirstName.ClientID);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            ResetInvalid();
            hfCustomerId.Value = "";
        }

        // ---------- SAVE ----------
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(hfCustomerId.Value, out id) || id == 0 || id == WalkInCustomerId)
            {
                ShowMessage("Choose a customer to edit first.", false);
                pnlForm.Visible = false;
                return;
            }

            ResetInvalid();

            WebControl badField;
            string error = ValidateForm(id, out badField);
            if (error != null)
            {
                ShowMessage(error, false);
                pnlForm.Visible = true;
                if (badField != null) badField.CssClass = "login-input invalid";
                ScrollTo("custFormCard", badField != null ? badField.ClientID : null);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Customer
                    SET Customer_FirstName = @First, Customer_LastName = @Last,
                        Customer_Username = @Username, Customer_EmailAddress = @Email,
                        Customer_IDNumber = @IdNumber, Customer_PhoneNumber = @Phone,
                        Customer_UnitNumber = @UnitNumber, Customer_UnitName = @UnitName,
                        Customer_StreetNumber = @StreetNumber, Customer_StreetName = @StreetName,
                        Customer_Suburb = @Suburb, Customer_City = @City,
                        Customer_PostalCode = @PostalCode, Customer_Province = @Province,
                        Customer_Status = @Status
                    WHERE Customer_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@First", txtFirstName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Last", txtLastName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Username", NullIfBlank(txtUsername.Text));
                    cmd.Parameters.AddWithValue("@Email", NullIfBlank(txtEmail.Text));
                    cmd.Parameters.AddWithValue("@IdNumber", NullIfBlank(txtIdNumber.Text));
                    cmd.Parameters.AddWithValue("@Phone", NullIfBlank(txtPhone.Text));
                    cmd.Parameters.AddWithValue("@UnitNumber", NullIfBlank(txtUnitNumber.Text));
                    cmd.Parameters.AddWithValue("@UnitName", NullIfBlank(txtUnitName.Text));
                    cmd.Parameters.AddWithValue("@StreetNumber", NullIfBlank(txtStreetNumber.Text));
                    cmd.Parameters.AddWithValue("@StreetName", NullIfBlank(txtStreetName.Text));
                    cmd.Parameters.AddWithValue("@Suburb", NullIfBlank(txtSuburb.Text));
                    cmd.Parameters.AddWithValue("@City", NullIfBlank(txtCity.Text));
                    cmd.Parameters.AddWithValue("@PostalCode", NullIfBlank(txtPostalCode.Text));
                    cmd.Parameters.AddWithValue("@Province", NullIfBlank(ddlProvince.SelectedValue));
                    cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                ShowMessage("The customer could not be saved: " + ex.Message, false);
                pnlForm.Visible = true;
                return;
            }

            pnlForm.Visible = false;
            hfCustomerId.Value = "";
            ShowMessage("Customer updated successfully.", true);
            RefreshAll();
        }

        // In-store customers often have no username, email or ID on file, so those are optional —
        // but anything that is filled in must be valid and not already used by another account.
        private string ValidateForm(int id, out WebControl field)
        {
            field = null;

            string first = txtFirstName.Text.Trim();
            string last = txtLastName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string idNumber = txtIdNumber.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string postalCode = txtPostalCode.Text.Trim();

            if (first.Length == 0) { field = txtFirstName; return "First name is required."; }
            if (last.Length == 0) { field = txtLastName; return "Last name is required."; }

            if (username.Length > 0 && !Regex.IsMatch(username, @"^[A-Za-z0-9_]{4,30}$"))
            {
                field = txtUsername;
                return "Username must be 4 to 30 characters: letters, numbers or underscores only.";
            }
            if (email.Length > 0 && !Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            {
                field = txtEmail;
                return "Please enter a valid email address.";
            }
            if (username.Length > 0 && email.Length == 0)
            {
                field = txtEmail;
                return "Customers with a website account need an email address.";
            }
            if (idNumber.Length > 0)
            {
                string idError = ValidateSaId(idNumber);
                if (idError != null) { field = txtIdNumber; return idError; }
            }
            if (phone.Length > 0 && !Regex.IsMatch(phone, @"^(\+27|0)[1-9][0-9]{8}$"))
            {
                field = txtPhone;
                return "Phone number must look like 0821234567 or +27821234567.";
            }
            if (postalCode.Length > 0 && !Regex.IsMatch(postalCode, @"^\d{4}$"))
            {
                field = txtPostalCode;
                return "Postal code must be 4 digits.";
            }

            if (username.Length > 0
                && (Exists("SELECT COUNT(*) FROM Customer WHERE Customer_Username = @V AND Customer_ID <> @Id", username, id)
                    || Exists("SELECT COUNT(*) FROM Employee WHERE Employee_Username = @V", username, id)))
            {
                field = txtUsername;
                return "That username is already taken.";
            }
            if (email.Length > 0
                && (Exists("SELECT COUNT(*) FROM Customer WHERE Customer_EmailAddress = @V AND Customer_ID <> @Id", email, id)
                    || Exists("SELECT COUNT(*) FROM Employee WHERE Employee_EmailAddress = @V", email, id)))
            {
                field = txtEmail;
                return "That email address is already registered to another account.";
            }
            if (idNumber.Length > 0
                && Exists("SELECT COUNT(*) FROM Customer WHERE Customer_IDNumber = @V AND Customer_ID <> @Id", idNumber, id))
            {
                field = txtIdNumber;
                return "That ID number is already registered to another customer.";
            }

            return null;
        }

        private bool Exists(string sql, string value, int excludeId)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@V", value);
                cmd.Parameters.AddWithValue("@Id", excludeId);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // Same rules as Register and the customer's own Account page:
        // 13 digits, Luhn check digit, a real date of birth, and 18 or older
        private static string ValidateSaId(string id)
        {
            if (!Regex.IsMatch(id, @"^\d{13}$")) return "ID number must be 13 digits.";

            int sum = 0;
            bool alternate = false;
            for (int i = id.Length - 1; i >= 0; i--)
            {
                int n = id[i] - '0';
                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }
                sum += n;
                alternate = !alternate;
            }
            if (sum % 10 != 0) return "Enter a valid South African ID number.";

            int yy = int.Parse(id.Substring(0, 2));
            int mm = int.Parse(id.Substring(2, 2));
            int dd = int.Parse(id.Substring(4, 2));
            int century = yy > DateTime.Today.Year % 100 ? 1900 : 2000;

            DateTime birth;
            try { birth = new DateTime(century + yy, mm, dd); }
            catch (ArgumentOutOfRangeException) { return "Enter a valid South African ID number."; }

            int age = DateTime.Today.Year - birth.Year;
            if (birth.Date > DateTime.Today.AddYears(-age)) age--;
            if (age < 18) return "Customers must be 18 or older.";

            return null;
        }

        // ---------- DEACTIVATE / REACTIVATE ----------
        private void ToggleStatus(int id)
        {
            if (id == WalkInCustomerId) return;

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE Customer
                SET Customer_Status = CASE WHEN Customer_Status = 'Active' THEN 'Inactive' ELSE 'Active' END
                WHERE Customer_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            ShowMessage("Customer status updated.", true);
            RefreshAll();
        }

        // ---------- DELETE (only customers with no sales; their wishlist goes with them) ----------
        private void DeleteCustomer(int id)
        {
            if (id == WalkInCustomerId) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Sale WHERE Customer_ID = @Id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            {
                                tx.Rollback();
                                ShowMessage("This customer has purchases on record and cannot be deleted. Use Deactivate instead.", false);
                                RefreshAll();
                                return;
                            }
                        }

                        // The Wishlist table is optional (added by Database\CreateWishlistTable.sql)
                        using (SqlCommand cmd = new SqlCommand(
                            "IF OBJECT_ID('dbo.Wishlist', 'U') IS NOT NULL DELETE FROM dbo.Wishlist WHERE Customer_ID = @Id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand("DELETE FROM Customer WHERE Customer_ID = @Id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                }
                ShowMessage("Customer deleted.", true);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // foreign key conflict
                    ShowMessage("This customer is linked to other records and cannot be deleted. Use Deactivate instead.", false);
                else
                    ShowMessage("The customer could not be deleted: " + ex.Message, false);
            }

            pnlForm.Visible = false;
            pnlHistory.Visible = false;
            RefreshAll();
        }

        // ---------- EXPORT ----------
        protected void btnExport_Click(object sender, EventArgs e)
        {
            DataTable dt = LoadCustomerTable();

            var sb = new StringBuilder();
            sb.AppendLine("Customer ID,First Name,Last Name,Username,Email,Phone,Status,Loyalty Points,Purchases,Total Spent,Last Purchase");

            foreach (DataRow r in dt.Rows)
            {
                sb.AppendLine(string.Join(",", new[]
                {
                    Csv(r["Customer_ID"]),
                    Csv(r["Customer_FirstName"]),
                    Csv(r["Customer_LastName"]),
                    Csv(r["Customer_Username"]),
                    Csv(r["Customer_EmailAddress"]),
                    Csv(r["Customer_PhoneNumber"]),
                    Csv(r["Customer_Status"]),
                    Csv(r["Customer_LoyaltyPointsBalance"]),
                    Csv(r["OrderCount"]),
                    Csv(Convert.ToDecimal(r["TotalSpent"]).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)),
                    Csv(r["LastPurchase"] == DBNull.Value ? "" : Convert.ToDateTime(r["LastPurchase"]).ToString("yyyy-MM-dd HH:mm"))
                }));
            }

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.ContentEncoding = Encoding.UTF8;
            Response.AddHeader("Content-Disposition", "attachment; filename=customers-" + DateTime.Today.ToString("yyyyMMdd") + ".csv");
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

        // ---------- HELPERS ----------
        private static string Val(SqlDataReader r, string column)
        {
            object o = r[column];
            return o == DBNull.Value ? "" : o.ToString().Trim();
        }

        private static object NullIfBlank(string value)
        {
            string v = (value ?? "").Trim();
            return v.Length == 0 ? (object)DBNull.Value : v;
        }

        private void ResetInvalid()
        {
            foreach (TextBox t in FormTextBoxes) t.CssClass = "login-input";
            ddlProvince.CssClass = "login-input";
            ddlStatus.CssClass = "login-input";
        }

        private static void SelectValue(DropDownList ddl, string value)
        {
            ddl.ClearSelection();
            ListItem item = ddl.Items.FindByValue(value);
            if (item != null) item.Selected = true;
        }

        private void ShowMessage(string text, bool success)
        {
            litMessage.Text = "<div class=\"emp-alert " + (success ? "emp-alert-success" : "emp-alert-error") + "\">"
                + Server.HtmlEncode(text) + "</div>";
        }

        private void ScrollTo(string elementId, string focusClientId)
        {
            string js = "var f=document.getElementById('" + elementId + "'); if(f){f.scrollIntoView({behavior:'smooth',block:'start'});}";
            if (!string.IsNullOrEmpty(focusClientId))
            {
                js += " var i=document.getElementById('" + focusClientId + "'); if(i){setTimeout(function(){i.focus();},350);}";
            }
            ScriptManager.RegisterStartupScript(this, GetType(), "scrollTo", js, true);
        }
    }
}
