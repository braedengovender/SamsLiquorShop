using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SamsLiquorShop.Manager
{
    public partial class Employees : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private int CurrentEmployeeId
        {
            get { return Session["EmployeeID"] != null ? Convert.ToInt32(Session["EmployeeID"]) : 0; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Manager")) return;

            if (!IsPostBack)
            {
                BindEmployees();
            }
        }

        // ---------- READ ----------
        private void BindEmployees()
        {
            var where = new List<string>();

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;

                string search = txtSearch.Text.Trim();
                if (search.Length > 0)
                {
                    where.Add("(Employee_FirstName LIKE @Search OR Employee_LastName LIKE @Search OR Employee_Username LIKE @Search OR Employee_EmailAddress LIKE @Search)");
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

                cmd.CommandText = @"
                    SELECT Employee_ID, Employee_FirstName, Employee_LastName, Employee_Username,
                           Employee_EmailAddress, Employee_Role, Employee_Status
                    FROM Employee"
                    + (where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : "")
                    + " ORDER BY Employee_LastName, Employee_FirstName";

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvEmployees.DataSource = dt;
                    gvEmployees.DataBind();
                    litCount.Text = dt.Rows.Count + (dt.Rows.Count == 1 ? " employee" : " employees");
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
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
            ScrollToForm();
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
            ScrollToForm();
        }

        // ---------- SAVE (INSERT or UPDATE) ----------
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = string.IsNullOrEmpty(hfEmployeeId.Value) ? 0 : int.Parse(hfEmployeeId.Value);
            bool isNew = (id == 0);

            string error = ValidateForm(isNew, id);
            if (error != null)
            {
                ShowMessage(error, false);
                pnlForm.Visible = true;
                ScrollToForm();
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
            BindEmployees();
        }

        private string ValidateForm(bool isNew, int id)
        {
            string first = txtFirstName.Text.Trim();
            string last = txtLastName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string idNumber = txtIdNumber.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string password = txtPassword.Text;

            if (first.Length == 0 || last.Length == 0)
                return "First name and last name are required.";

            if (!Regex.IsMatch(username, @"^[A-Za-z0-9_.]{3,30}$"))
                return "Username must be 3 to 30 characters: letters, numbers, underscores or full stops only.";

            if (!Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                return "Please enter a valid email address.";

            string idError = ValidateSaId(idNumber);
            if (idError != null)
                return idError;

            if (!Regex.IsMatch(phone, @"^(\+27|0)[0-9]{9}$"))
                return "Phone number must look like 0824055932 or +27824055932.";

            DateTime hireDate;
            if (!DateTime.TryParseExact(txtHireDate.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out hireDate))
                return "Please choose a hire date.";
            if (hireDate.Date > DateTime.Today)
                return "The hire date cannot be in the future.";

            if (isNew && password.Length < 8)
                return "A password of at least 8 characters is required for a new employee.";

            if (!isNew && password.Length > 0 && password.Length < 8)
                return "The new password must be at least 8 characters.";

            if (!isNew && id == CurrentEmployeeId && (ddlRole.SelectedValue != "Manager" || ddlStatus.SelectedValue != "Active"))
                return "You cannot change your own role or deactivate your own account.";

            // Username/email must be unique across employees AND customers (Login checks
            // Customer first). The ID number must be unique across employees.
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT
                    (SELECT COUNT(*) FROM Employee
                      WHERE (Employee_Username = @U OR Employee_EmailAddress = @E) AND Employee_ID <> @Id)
                  + (SELECT COUNT(*) FROM Customer
                      WHERE Customer_Username = @U OR Customer_EmailAddress = @E),
                    (SELECT COUNT(*) FROM Employee
                      WHERE Employee_IDNumber = @Idn AND Employee_ID <> @Id)", conn))
            {
                cmd.Parameters.AddWithValue("@U", username);
                cmd.Parameters.AddWithValue("@E", email);
                cmd.Parameters.AddWithValue("@Idn", idNumber);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        if (r.GetInt32(0) > 0) return "That username or email address is already in use.";
                        if (r.GetInt32(1) > 0) return "An employee with that ID number already exists.";
                    }
                }
            }

            return null;
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
            BindEmployees();
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

            BindEmployees();
        }

        // ---------- HELPERS ----------
        private void ClearForm()
        {
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

        private void ScrollToForm()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "scrollForm",
                "var f=document.getElementById('empFormCard'); if(f){f.scrollIntoView({behavior:'smooth',block:'start'});}", true);
        }
    }
}