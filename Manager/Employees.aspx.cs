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

        // ---------- READ ----------
        private DataTable LoadEmployeeTable()
        {
            var where = new List<string>();

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
                FROM Employee", conn))
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
            string js = "var f=document.getElementById('empFormCard'); if(f){f.scrollIntoView({behavior:'smooth',block:'start'});}";
            if (!string.IsNullOrEmpty(focusClientId))
            {
                js += " var i=document.getElementById('" + focusClientId + "'); if(i){setTimeout(function(){i.focus();},350);}";
            }
            ScriptManager.RegisterStartupScript(this, GetType(), "scrollForm", js, true);
        }
    }
}