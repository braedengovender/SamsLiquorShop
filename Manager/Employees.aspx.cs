using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SamsLiquorShop.Manager
{
    public partial class Employees : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private static readonly HashSet<string> AllowedSort = new HashSet<string>
        {
            "Employee_LastName", "Employee_EmailAddress", "Employee_Role", "Employee_Status", "Employee_HireDate"
        };

        private string SortField
        {
            get { return (string)ViewState["SortField"] ?? "Employee_LastName"; }
            set { ViewState["SortField"] = value; }
        }

        private string SortDir
        {
            get { return (string)ViewState["SortDir"] ?? "ASC"; }
            set { ViewState["SortDir"] = value; }
        }

        private int CurrentEmployeeId
        {
            get { return Session["EmployeeID"] != null ? Convert.ToInt32(Session["EmployeeID"]) : 0; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Manager")) return;

            if (!IsPostBack)
            {
                RefreshAll();
            }
        }

        private void RefreshAll()
        {
            LoadKpis();
            BindEmployees();
        }
        // The "Online Store" account belongs to the website itself, so it is never shown on this page
        private const string HiddenEmployeeSql =
            "NOT (LTRIM(RTRIM(Employee_FirstName + ' ' + ISNULL(Employee_LastName, ''))) = 'Online Store' " +
            "OR REPLACE(REPLACE(Employee_Username, '_', ''), ' ', '') = 'onlinestore')";
        // ---------- READ ----------
        private DataTable LoadEmployeeTable()
        {
            var where = new List<string> { HiddenEmployeeSql };

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;

                string search = txtSearch.Text.Trim();
                if (search.Length > 0)
                {
                    where.Add(@"(Employee_FirstName LIKE @Search OR Employee_LastName LIKE @Search
                                 OR (Employee_FirstName + ' ' + Employee_LastName) LIKE @Search
                                 OR Employee_Username LIKE @Search OR Employee_EmailAddress LIKE @Search
                                 OR Employee_PhoneNumber LIKE @Search)");
                    cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                }
                if (ddlRoleFilter.SelectedValue != "")
                {
                    where.Add("Employee_Role = @Role");
                    cmd.Parameters.AddWithValue("@Role", ddlRoleFilter.SelectedValue);
                }
                if (ddlStatusFilter.SelectedValue != "")
                {
                    where.Add("Employee_Status = @Status");
                    cmd.Parameters.AddWithValue("@Status", ddlStatusFilter.SelectedValue);
                }

                string sortField = AllowedSort.Contains(SortField) ? SortField : "Employee_LastName";
                string dir = SortDir == "DESC" ? "DESC" : "ASC";

                cmd.CommandText = @"
                    SELECT Employee_ID, Employee_FirstName, Employee_LastName, Employee_Username,
                           Employee_EmailAddress, Employee_PhoneNumber, Employee_HireDate,
                           Employee_Role, Employee_Status
                    FROM Employee"
                    + (where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : "")
                    + " ORDER BY " + sortField + " " + dir + ", Employee_FirstName, Employee_ID";

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        private void BindEmployees()
        {
            DataTable dt = LoadEmployeeTable();
            int total = dt.Rows.Count;

            int pages = (int)Math.Ceiling(total / (double)gvEmployees.PageSize);
            if (pages > 0 && gvEmployees.PageIndex >= pages) gvEmployees.PageIndex = pages - 1;

            gvEmployees.DataSource = dt;
            gvEmployees.DataBind();

            if (total == 0)
            {
                litCount.Text = "No employees found";
            }
            else
            {
                int start = gvEmployees.PageIndex * gvEmployees.PageSize + 1;
                int end = Math.Min(start + gvEmployees.PageSize - 1, total);
                litCount.Text = "Showing " + start + "\u2013" + end + " of " + total + (total == 1 ? " employee" : " employees");
            }
        }

        private void LoadKpis()
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(*),
                       ISNULL(SUM(CASE WHEN Employee_Status = 'Active' THEN 1 ELSE 0 END), 0),
                       ISNULL(SUM(CASE WHEN Employee_Status <> 'Active' THEN 1 ELSE 0 END), 0),
                       ISNULL(SUM(CASE WHEN Employee_Role = 'Manager' THEN 1 ELSE 0 END), 0)
                FROM Employee WHERE " + HiddenEmployeeSql, conn))
            {
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        litKpiTotal.Text = r.GetInt32(0).ToString();
                        litKpiActive.Text = r.GetInt32(1).ToString();
                        litKpiInactive.Text = r.GetInt32(2).ToString();
                        litKpiManagers.Text = r.GetInt32(3).ToString();
                    }
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvEmployees.PageIndex = 0;
            BindEmployees();
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            gvEmployees.PageIndex = 0;
            BindEmployees();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlRoleFilter.SelectedIndex = 0;
            ddlStatusFilter.SelectedIndex = 0;
            gvEmployees.PageIndex = 0;
            BindEmployees();
        }

        protected void gvEmployees_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvEmployees.PageIndex = e.NewPageIndex;
            BindEmployees();
        }

        protected void gvEmployees_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (SortField == e.SortExpression)
            {
                SortDir = (SortDir == "ASC") ? "DESC" : "ASC";
            }
            else
            {
                SortField = e.SortExpression;
                SortDir = "ASC";
            }
            gvEmployees.PageIndex = 0;
            BindEmployees();
        }

        // Adds an up/down arrow to the header of the column being sorted
        protected void gvEmployees_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.Header) return;

            foreach (TableCell cell in e.Row.Cells)
            {
                if (cell.Controls.Count == 0) continue;
                LinkButton lb = cell.Controls[0] as LinkButton;
                if (lb != null && lb.CommandArgument == SortField)
                {
                    lb.Text += (SortDir == "ASC") ? " \u25B2" : " \u25BC";
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

        protected string DeleteJs(object first, object last)
        {
            string name = HttpUtility.JavaScriptStringEncode((Convert.ToString(first) + " " + Convert.ToString(last)).Trim());
            return "return empConfirm(this, 'Delete employee', 'Permanently delete " + name
                + "? This cannot be undone.', 'Delete', true);";
        }

        protected string DeactivateJs(object status, object first, object last)
        {
            if ((Convert.ToString(status) ?? "").Trim() != "Active") return "";

            string name = HttpUtility.JavaScriptStringEncode((Convert.ToString(first) + " " + Convert.ToString(last)).Trim());
            return "return empConfirm(this, 'Deactivate employee', '" + name
                + " will no longer be able to log in. You can reactivate them at any time.', 'Deactivate', false);";
        }

        // ---------- ROW BUTTONS ----------
        protected void gvEmployees_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EditEmp" && e.CommandName != "ToggleEmp" && e.CommandName != "DeleteEmp") return;

            int id = Convert.ToInt32(e.CommandArgument);
            litMessage.Text = "";

            switch (e.CommandName)
            {
                case "EditEmp": LoadForEdit(id); break;
                case "ToggleEmp": ToggleStatus(id); break;
                case "DeleteEmp": DeleteEmployee(id); break;
            }
        }

        // ---------- CREATE (open blank form) ----------
        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            litMessage.Text = "";
            pnlReport.Visible = false;
            ClearForm();
            litFormTitle.Text = "Add Employee";
            litPasswordHint.Text = "Minimum 8 characters.";
            pnlForm.Visible = true;
            ScrollToForm(txtFirstName.ClientID);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            ClearForm();
        }

        // ---------- UPDATE (load into form) ----------
        private void LoadForEdit(int id)
        {
            pnlReport.Visible = false;

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT Employee_ID, Employee_FirstName, Employee_LastName, Employee_Username,
                       Employee_EmailAddress, Employee_IDNumber, Employee_PhoneNumber,
                       Employee_HireDate, Employee_Role, Employee_Status
                FROM Employee WHERE Employee_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        ShowMessage("That employee could not be found.", false);
                        return;
                    }

                    ResetInvalid();
                    hfEmployeeId.Value = id.ToString();
                    txtFirstName.Text = r["Employee_FirstName"].ToString();
                    txtLastName.Text = r["Employee_LastName"].ToString();
                    txtUsername.Text = r["Employee_Username"].ToString();
                    txtEmail.Text = r["Employee_EmailAddress"].ToString();
                    txtIdNumber.Text = r["Employee_IDNumber"].ToString().Trim();
                    txtPhone.Text = r["Employee_PhoneNumber"].ToString().Trim();
                    txtHireDate.Text = Convert.ToDateTime(r["Employee_HireDate"]).ToString("yyyy-MM-dd");
                    txtPassword.Text = "";
                    SelectValue(ddlRole, r["Employee_Role"].ToString().Trim());
                    SelectValue(ddlStatus, r["Employee_Status"].ToString().Trim());
                }
            }

            litFormTitle.Text = "Edit Employee";
            litPasswordHint.Text = "Leave blank to keep the current password.";
            pnlForm.Visible = true;
            ScrollToForm(txtFirstName.ClientID);
        }

        // ---------- SAVE (INSERT or UPDATE) ----------
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = string.IsNullOrEmpty(hfEmployeeId.Value) ? 0 : int.Parse(hfEmployeeId.Value);
            bool isNew = (id == 0);

            ResetInvalid();

            WebControl badField;
            string error = ValidateForm(isNew, id, out badField);
            if (error != null)
            {
                ShowMessage(error, false);
                pnlForm.Visible = true;
                if (badField != null)
                {
                    badField.CssClass = "login-input invalid";
                    ScrollToForm(badField.ClientID);
                }
                else
                {
                    ScrollToForm(null);
                }
                return;
            }

            string first = txtFirstName.Text.Trim();
            string last = txtLastName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string idNumber = txtIdNumber.Text.Trim();
            string phone = txtPhone.Text.Trim();
            DateTime hireDate = DateTime.ParseExact(txtHireDate.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
            string password = txtPassword.Text;
            string role = ddlRole.SelectedValue;
            string status = ddlStatus.SelectedValue;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    if (isNew)
                    {
                        cmd.CommandText = @"
                            INSERT INTO Employee
                                (Employee_FirstName, Employee_LastName, Employee_IDNumber, Employee_Role,
                                 Employee_EmailAddress, Employee_PhoneNumber, Employee_HireDate,
                                 Employee_Username, Employee_Password, Employee_Status)
                            VALUES
                                (@First, @Last, @IdNumber, @Role,
                                 @Email, @Phone, @HireDate,
                                 @Username, @Password, @Status)";
                        cmd.Parameters.AddWithValue("@Password", password);
                    }
                    else
                    {
                        string passwordSql = password.Length > 0 ? ", Employee_Password = @Password" : "";
                        cmd.CommandText = @"
                            UPDATE Employee
                            SET Employee_FirstName = @First, Employee_LastName = @Last,
                                Employee_IDNumber = @IdNumber, Employee_Role = @Role,
                                Employee_EmailAddress = @Email, Employee_PhoneNumber = @Phone,
                                Employee_HireDate = @HireDate, Employee_Username = @Username,
                                Employee_Status = @Status" + passwordSql + @"
                            WHERE Employee_ID = @Id";
                        cmd.Parameters.AddWithValue("@Id", id);
                        if (password.Length > 0) cmd.Parameters.AddWithValue("@Password", password);
                    }

                    cmd.Parameters.AddWithValue("@First", first);
                    cmd.Parameters.AddWithValue("@Last", last);
                    cmd.Parameters.AddWithValue("@IdNumber", idNumber);
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.Add("@HireDate", SqlDbType.Date).Value = hireDate;
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Status", status);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                ShowMessage("The employee could not be saved: " + ex.Message, false);
                pnlForm.Visible = true;
                return;
            }

            pnlForm.Visible = false;
            ClearForm();
            ShowMessage(isNew ? "Employee added successfully." : "Employee updated successfully.", true);
            RefreshAll();
        }

        private string ValidateForm(bool isNew, int id, out WebControl field)
        {
            field = null;

            string first = txtFirstName.Text.Trim();
            string last = txtLastName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string idNumber = txtIdNumber.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string password = txtPassword.Text;

            if (first.Length == 0) { field = txtFirstName; return "First name is required."; }
            if (last.Length == 0) { field = txtLastName; return "Last name is required."; }

            if (!Regex.IsMatch(username, @"^[A-Za-z0-9_.]{3,30}$"))
            {
                field = txtUsername;
                return "Username must be 3 to 30 characters: letters, numbers, underscores or full stops only.";
            }

            if (!Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            {
                field = txtEmail;
                return "Please enter a valid email address.";
            }

            string idError = ValidateSaId(idNumber);
            if (idError != null) { field = txtIdNumber; return idError; }

            if (!Regex.IsMatch(phone, @"^(\+27|0)[0-9]{9}$"))
            {
                field = txtPhone;
                return "Phone number must look like 0824055932 or +27824055932.";
            }

            DateTime hireDate;
            if (!DateTime.TryParseExact(txtHireDate.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out hireDate))
            {
                field = txtHireDate;
                return "Please choose a hire date.";
            }
            if (hireDate.Date > DateTime.Today)
            {
                field = txtHireDate;
                return "The hire date cannot be in the future.";
            }

            if (isNew && password.Length < 8)
            {
                field = txtPassword;
                return "A password of at least 8 characters is required for a new employee.";
            }
            if (!isNew && password.Length > 0 && password.Length < 8)
            {
                field = txtPassword;
                return "The new password must be at least 8 characters.";
            }

            if (!isNew && id == CurrentEmployeeId && (ddlRole.SelectedValue != "Manager" || ddlStatus.SelectedValue != "Active"))
            {
                field = ddlRole;
                return "You cannot change your own role or deactivate your own account.";
            }

            // ----- Uniqueness: only check values that are new or have changed -----
            string origUser = null, origEmail = null, origIdn = null;
            if (!isNew)
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Employee_Username, Employee_EmailAddress, Employee_IDNumber FROM Employee WHERE Employee_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            origUser = r["Employee_Username"].ToString();
                            origEmail = r["Employee_EmailAddress"].ToString();
                            origIdn = r["Employee_IDNumber"].ToString();
                        }
                    }
                }
            }

            if (isNew || !SameText(origUser, username))
            {
                if (Exists("SELECT COUNT(*) FROM Employee WHERE Employee_Username = @V AND Employee_ID <> @Id", username, id)
                    || Exists("SELECT COUNT(*) FROM Customer WHERE Customer_Username = @V", username, id))
                {
                    field = txtUsername;
                    return "That username is already taken. Please choose another.";
                }
            }

            if (isNew || !SameText(origEmail, email))
            {
                if (Exists("SELECT COUNT(*) FROM Employee WHERE Employee_EmailAddress = @V AND Employee_ID <> @Id", email, id)
                    || Exists("SELECT COUNT(*) FROM Customer WHERE Customer_EmailAddress = @V", email, id))
                {
                    field = txtEmail;
                    return "That email address is already registered to another account.";
                }
            }

            if (isNew || !SameText(origIdn, idNumber))
            {
                if (Exists("SELECT COUNT(*) FROM Employee WHERE Employee_IDNumber = @V AND Employee_ID <> @Id", idNumber, id))
                {
                    field = txtIdNumber;
                    return "An employee with that ID number already exists.";
                }
            }

            return null;
        }

        private static bool SameText(string a, string b)
        {
            return string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
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

        // 13 digits, valid date of birth, Luhn check digit, and at least 18 years old.
        private static string ValidateSaId(string id)
        {
            if (!Regex.IsMatch(id, @"^[0-9]{13}$"))
                return "The ID number must be exactly 13 digits.";

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
            if (sum % 10 != 0)
                return "That ID number is not valid. Please check it and try again.";

            int yy = int.Parse(id.Substring(0, 2));
            int mm = int.Parse(id.Substring(2, 2));
            int dd = int.Parse(id.Substring(4, 2));
            int century = yy > DateTime.Today.Year % 100 ? 1900 : 2000;

            DateTime birth;
            try { birth = new DateTime(century + yy, mm, dd); }
            catch (ArgumentOutOfRangeException) { return "That ID number contains an invalid date of birth."; }

            int age = DateTime.Today.Year - birth.Year;
            if (birth.Date > DateTime.Today.AddYears(-age)) age--;
            if (age < 18)
                return "An employee must be at least 18 years old.";

            return null;
        }

        // ---------- DEACTIVATE / REACTIVATE ----------
        private void ToggleStatus(int id)
        {
            if (id == CurrentEmployeeId)
            {
                ShowMessage("You cannot deactivate your own account.", false);
                return;
            }

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE Employee
                SET Employee_Status = CASE WHEN Employee_Status = 'Active' THEN 'Inactive' ELSE 'Active' END
                WHERE Employee_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            ShowMessage("Employee status updated.", true);
            RefreshAll();
        }

        // ---------- DELETE ----------
        private void DeleteEmployee(int id)
        {
            if (id == CurrentEmployeeId)
            {
                ShowMessage("You cannot delete your own account.", false);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Employee WHERE Employee_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                ShowMessage("Employee deleted.", true);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // foreign key conflict
                    ShowMessage("This employee has sales on record and cannot be deleted. Use Deactivate instead.", false);
                else
                    ShowMessage("The employee could not be deleted: " + ex.Message, false);
            }

            RefreshAll();
        }

        // ---------- STAFF PERFORMANCE REPORT ----------
        private const string ValidSaleSql = "LTRIM(RTRIM(ISNULL(s.Sale_Status, ''))) NOT IN ('Cancelled', 'Canceled', 'Refunded', 'Voided', 'Void')";

        protected void btnPerfReport_Click(object sender, EventArgs e)
        {
            litMessage.Text = "";
            pnlForm.Visible = false;

            if (txtPrFrom.Text.Trim().Length == 0) txtPrFrom.Text = DateTime.Today.AddDays(-29).ToString("yyyy-MM-dd");
            if (txtPrTo.Text.Trim().Length == 0) txtPrTo.Text = DateTime.Today.ToString("yyyy-MM-dd");

            pnlReport.Visible = true;
            RunPerfReport();
            ScrollToElement("empReportCard", txtPrFrom.ClientID, "scrollReport");
        }

        protected void btnPrRun_Click(object sender, EventArgs e)
        {
            litMessage.Text = "";
            pnlReport.Visible = true;
            RunPerfReport();
        }

        protected void btnPrClose_Click(object sender, EventArgs e)
        {
            pnlReport.Visible = false;
        }

        private bool TryReadPerfDates(out DateTime from, out DateTime to)
        {
            from = DateTime.MinValue;
            to = DateTime.MinValue;

            if (!DateTime.TryParseExact(txtPrFrom.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out from)
                || !DateTime.TryParseExact(txtPrTo.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out to))
            {
                ShowMessage("Choose both a \"Sales from\" and a \"Sales to\" date for the report.", false);
                return false;
            }
            if (from > to)
            {
                ShowMessage("The \"Sales from\" date must be on or before the \"Sales to\" date.", false);
                return false;
            }
            return true;
        }

        private static string KpiCard(string label, string value)
        {
            return "<div class=\"mgr-kpi-card\"><p class=\"mgr-kpi-label\">" + label + "</p><p class=\"mgr-kpi-value\">" + value + "</p></div>";
        }

        // One row per employee with their totals, averages and rank, best first
        private DataView LoadPerfView(DateTime from, DateTime to)
        {
            string staffFilter = (ddlPrStaff.SelectedValue == "sales"
                ? "WHERE x.Sales > 0"
                : "WHERE (e.Employee_Status = 'Active' OR x.Sales > 0)") + " AND " + HiddenEmployeeSql;

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT e.Employee_ID, e.Employee_FirstName, e.Employee_LastName, e.Employee_Role, e.Employee_Status,
                       ISNULL(x.Sales, 0) AS Sales,
                       ISNULL(x.Revenue, 0) AS Revenue,
                       ISNULL(x.Items, 0) AS Items,
                       ISNULL(x.DaysWorked, 0) AS DaysWorked
                FROM Employee e
                LEFT JOIN (
                    SELECT s.Employee_ID,
                           COUNT(*) AS Sales,
                           SUM(s.Sale_TotalAmount) AS Revenue,
                           SUM(ISNULL(li.Items, 0)) AS Items,
                           COUNT(DISTINCT CAST(s.Sale_DateTime AS DATE)) AS DaysWorked
                    FROM Sale s
                    LEFT JOIN (SELECT Sale_ID, SUM(SaleLine_Quantity) AS Items FROM SaleLine GROUP BY Sale_ID) li
                           ON li.Sale_ID = s.Sale_ID
                    WHERE s.Sale_DateTime >= @From AND s.Sale_DateTime < @ToExcl
                      AND " + ValidSaleSql + @"
                    GROUP BY s.Employee_ID
                ) x ON x.Employee_ID = e.Employee_ID
                " + staffFilter, conn))
            {
                cmd.Parameters.Add("@From", SqlDbType.DateTime).Value = from.Date;
                cmd.Parameters.Add("@ToExcl", SqlDbType.DateTime).Value = to.Date.AddDays(1);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            dt.Columns.Add("AvgSale", typeof(decimal));
            dt.Columns.Add("ItemsPerSale", typeof(decimal));
            dt.Columns.Add("RevPerDay", typeof(decimal));
            dt.Columns.Add("SharePct", typeof(decimal));
            dt.Columns.Add("Rank", typeof(int));

            decimal total = 0;
            foreach (DataRow r in dt.Rows) total += Convert.ToDecimal(r["Revenue"]);

            foreach (DataRow r in dt.Rows)
            {
                int sales = Convert.ToInt32(r["Sales"]);
                int items = Convert.ToInt32(r["Items"]);
                int days = Convert.ToInt32(r["DaysWorked"]);
                decimal revenue = Convert.ToDecimal(r["Revenue"]);

                r["AvgSale"] = sales > 0 ? Math.Round(revenue / sales, 2) : 0m;
                r["ItemsPerSale"] = sales > 0 ? Math.Round((decimal)items / sales, 1) : 0m;
                r["RevPerDay"] = days > 0 ? Math.Round(revenue / days, 2) : 0m;
                r["SharePct"] = total > 0 ? Math.Round(revenue * 100m / total, 1) : 0m;
            }

            const string order = "Revenue DESC, Employee_LastName ASC, Employee_FirstName ASC";
            int rank = 0;
            foreach (DataRow r in dt.Select("", order))
            {
                if (Convert.ToInt32(r["Sales"]) > 0) r["Rank"] = ++rank;
            }

            dt.DefaultView.Sort = order;
            return dt.DefaultView;
        }

        private void RunPerfReport()
        {
            DateTime from, to;
            if (!TryReadPerfDates(out from, out to))
            {
                litPrSummary.Text = "";
                gvPerf.DataSource = null;
                gvPerf.DataBind();
                return;
            }

            DataView dv;
            try
            {
                dv = LoadPerfView(from, to);
            }
            catch (SqlException)
            {
                ShowMessage("The performance report could not be loaded. Please try again.", false);
                return;
            }

            decimal revenue = 0;
            int sales = 0, noSales = 0;
            string top = "\u2014";

            foreach (DataRowView v in dv)
            {
                revenue += Convert.ToDecimal(v["Revenue"]);
                sales += Convert.ToInt32(v["Sales"]);
                if (Convert.ToInt32(v["Sales"]) == 0) noSales++;
                if (v["Rank"] != DBNull.Value && Convert.ToInt32(v["Rank"]) == 1)
                {
                    top = Server.HtmlEncode((Convert.ToString(v["Employee_FirstName"]) + " " + Convert.ToString(v["Employee_LastName"])).Trim());
                }
            }

            decimal avg = sales > 0 ? revenue / sales : 0m;

            litPrSummary.Text = "<div class=\"mgr-kpi-grid\">"
                + KpiCard("Revenue", revenue.ToString("C", CultureInfo.CurrentCulture))
                + KpiCard("Sales", sales.ToString("N0", CultureInfo.CurrentCulture))
                + KpiCard("Average sale", avg.ToString("C", CultureInfo.CurrentCulture))
                + KpiCard("Top performer", top)
                + KpiCard("Staff with no sales", noSales.ToString())
                + "</div>";

            gvPerf.DataSource = dv;
            gvPerf.DataBind();
        }

        protected string PerfRankText(object rank)
        {
            return (rank == null || rank == DBNull.Value) ? "\u2014" : Convert.ToString(rank);
        }

        protected string PerfTopBadge(object rank)
        {
            if (rank == null || rank == DBNull.Value || Convert.ToInt32(rank) != 1) return "";
            return "<span class=\"emp-badge emp-badge-active\">Top</span>";
        }

        // "Cashier" or "Cashier · Inactive"
        protected string PerfSub(object role, object status)
        {
            string r = (Convert.ToString(role) ?? "").Trim();
            bool inactive = !(Convert.ToString(status) ?? "").Trim().Equals("Active", StringComparison.OrdinalIgnoreCase);
            return Server.HtmlEncode(r + (inactive ? " \u00B7 Inactive" : ""));
        }

        protected string PerfShareText(object share)
        {
            return Convert.ToDecimal(share).ToString("0.0", CultureInfo.InvariantCulture) + "%";
        }

        protected void btnPrExport_Click(object sender, EventArgs e)
        {
            DateTime from, to;
            if (!TryReadPerfDates(out from, out to))
            {
                pnlReport.Visible = true;
                return;
            }

            DataView dv = LoadPerfView(from, to);

            var sb = new StringBuilder();
            sb.AppendLine("Staff performance report," + from.ToString("yyyy-MM-dd") + " to " + to.ToString("yyyy-MM-dd"));
            sb.AppendLine("Rank,Employee ID,First Name,Last Name,Role,Status,Sales,Items Sold,Average Sale,Days With Sales,Revenue Per Day,Revenue,Share %");

            foreach (DataRowView r in dv)
            {
                sb.AppendLine(string.Join(",", new[]
                {
                    r["Rank"] == DBNull.Value ? "\"\"" : Csv(r["Rank"]),
                    Csv(r["Employee_ID"]),
                    Csv(r["Employee_FirstName"]),
                    Csv(r["Employee_LastName"]),
                    Csv(r["Employee_Role"]),
                    Csv(r["Employee_Status"]),
                    Csv(r["Sales"]),
                    Csv(r["Items"]),
                    CsvNum(Convert.ToDecimal(r["AvgSale"])),
                    Csv(r["DaysWorked"]),
                    CsvNum(Convert.ToDecimal(r["RevPerDay"])),
                    CsvNum(Convert.ToDecimal(r["Revenue"])),
                    CsvNum(Convert.ToDecimal(r["SharePct"]))
                }));
            }

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.ContentEncoding = Encoding.UTF8;
            Response.AddHeader("Content-Disposition", "attachment; filename=staff-performance-" + from.ToString("yyyyMMdd") + "-" + to.ToString("yyyyMMdd") + ".csv");
            Response.BinaryWrite(Encoding.UTF8.GetPreamble());
            Response.Write(sb.ToString());
            Response.End();
        }

        private static string CsvNum(decimal value)
        {
            return "\"" + value.ToString("0.00", CultureInfo.InvariantCulture) + "\"";
        }

        // ---------- EXPORT ----------
        protected void btnExport_Click(object sender, EventArgs e)
        {
            DataTable dt = LoadEmployeeTable();

            var sb = new StringBuilder();
            sb.AppendLine("Employee ID,First Name,Last Name,Username,Email,Phone,Role,Status,Hire Date");

            foreach (DataRow r in dt.Rows)
            {
                sb.AppendLine(string.Join(",", new[]
                {
                    Csv(r["Employee_ID"]),
                    Csv(r["Employee_FirstName"]),
                    Csv(r["Employee_LastName"]),
                    Csv(r["Employee_Username"]),
                    Csv(r["Employee_EmailAddress"]),
                    Csv(r["Employee_PhoneNumber"]),
                    Csv(r["Employee_Role"]),
                    Csv(r["Employee_Status"]),
                    Csv(Convert.ToDateTime(r["Employee_HireDate"]).ToString("yyyy-MM-dd"))
                }));
            }

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.ContentEncoding = Encoding.UTF8;
            Response.AddHeader("Content-Disposition", "attachment; filename=employees-" + DateTime.Today.ToString("yyyyMMdd") + ".csv");
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
        private void ClearForm()
        {
            ResetInvalid();
            hfEmployeeId.Value = "";
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtUsername.Text = "";
            txtEmail.Text = "";
            txtIdNumber.Text = "";
            txtPhone.Text = "";
            txtHireDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            txtPassword.Text = "";
            ddlRole.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
        }

        private void ResetInvalid()
        {
            foreach (WebControl c in new WebControl[]
            {
                txtFirstName, txtLastName, txtUsername, txtEmail,
                txtIdNumber, txtPhone, txtHireDate, txtPassword, ddlRole, ddlStatus
            })
            {
                c.CssClass = "login-input";
            }
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

        private void ScrollToForm(string focusClientId)
        {
            ScrollToElement("empFormCard", focusClientId, "scrollForm");
        }

        // Scrolls below the sticky header, and focuses the box without letting the focus
        // jump cancel the smooth scroll.
        private void ScrollToElement(string elementId, string focusClientId, string key)
        {
            string js =
                "(function(){" +
                "var f=document.getElementById('" + elementId + "'); if(!f) return;" +
                "var hdr=document.querySelector('.site-header');" +
                "var offset=(hdr?hdr.getBoundingClientRect().height:60)+32;" +
                "var top=f.getBoundingClientRect().top+window.pageYOffset-offset;" +
                "window.scrollTo({top:Math.max(top,0),behavior:'smooth'});" +
                (string.IsNullOrEmpty(focusClientId) ? "" :
                    "var i=document.getElementById('" + focusClientId + "');" +
                    "if(i){setTimeout(function(){i.focus({preventScroll:true});},500);}") +
                "})();";
            ScriptManager.RegisterStartupScript(this, GetType(), key, js, true);
        }
    }
}