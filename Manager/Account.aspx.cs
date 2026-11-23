using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace SamsLiquorShop.Manager
{
    public partial class Account : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private int EmployeeId => Convert.ToInt32(Session["EmployeeID"]);

        private TextBox[] AllTextBoxes => new[]
        {
            txtFirstName, txtLastName, txtIdNumber, txtPhone, txtUsername, txtEmail,
            txtCurrentPassword, txtNewPassword, txtConfirmPassword
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Manager")) return;

            if (!IsPostBack)
            {
                LoadDetails();
            }
        }

        // ---------- LOAD ----------
        private void LoadDetails()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT Employee_ID, Employee_FirstName, Employee_LastName, Employee_IDNumber,
                           Employee_PhoneNumber, Employee_Username, Employee_EmailAddress,
                           Employee_Role, Employee_Status, Employee_HireDate
                    FROM Employee WHERE Employee_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", EmployeeId);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            ShowMessage("Your account could not be found. Please log out and log in again.", false);
                            pnlDetails.Visible = false;
                            pnlPassword.Visible = false;
                            return;
                        }

                        litEmployeeId.Text = "#" + Val(r, "Employee_ID");
                        litRole.Text = Server.HtmlEncode(Val(r, "Employee_Role"));
                        litStatus.Text = Server.HtmlEncode(Val(r, "Employee_Status"));
                        litHireDate.Text = r["Employee_HireDate"] == DBNull.Value
                            ? "&ndash;"
                            : Convert.ToDateTime(r["Employee_HireDate"]).ToString("dd MMM yyyy");

                        txtFirstName.Text = Val(r, "Employee_FirstName");
                        txtLastName.Text = Val(r, "Employee_LastName");
                        txtIdNumber.Text = Val(r, "Employee_IDNumber");
                        txtPhone.Text = Val(r, "Employee_PhoneNumber");
                        txtUsername.Text = Val(r, "Employee_Username");
                        txtEmail.Text = Val(r, "Employee_EmailAddress");
                    }
                }
            }
            catch (SqlException ex)
            {
                ShowMessage("Your details could not be loaded: " + ex.Message, false);
                pnlDetails.Visible = false;
                pnlPassword.Visible = false;
            }
        }

        private static string Val(SqlDataReader r, string column)
        {
            object o = r[column];
            return o == DBNull.Value ? "" : o.ToString().Trim();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            litMessage.Text = "";
            ClearInvalid();
            LoadDetails();
        }

        // ---------- SAVE DETAILS ----------
        protected void btnSaveDetails_Click(object sender, EventArgs e)
        {
            ClearInvalid();

            string first = txtFirstName.Text.Trim();
            string last = txtLastName.Text.Trim();
            string idNumber = txtIdNumber.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();

            // Same rules as Manage Employees, so a manager can't save something there that fails here (or vice versa)
            WebControl bad = null;
            string error = null;

            if (first.Length == 0) { bad = txtFirstName; error = "First name is required."; }
            else if (last.Length == 0) { bad = txtLastName; error = "Last name is required."; }
            else if ((error = ValidateSaId(idNumber)) != null) { bad = txtIdNumber; }
            else if (!Regex.IsMatch(phone, @"^(\+27|0)[0-9]{9}$")) { bad = txtPhone; error = "Phone number must look like 0821234567 or +27821234567."; }
            else if (!Regex.IsMatch(username, @"^[A-Za-z0-9_.]{3,30}$")) { bad = txtUsername; error = "Username must be 3 to 30 characters: letters, numbers, underscores or full stops only."; }
            else if (!Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$")) { bad = txtEmail; error = "Please enter a valid email address."; }
            else if (IsTaken("SELECT COUNT(*) FROM Employee WHERE Employee_Username = @V AND Employee_ID <> @Id", username)
                  || IsTaken("SELECT COUNT(*) FROM Customer WHERE Customer_Username = @V", username)) { bad = txtUsername; error = "That username is already taken."; }
            else if (IsTaken("SELECT COUNT(*) FROM Employee WHERE Employee_EmailAddress = @V AND Employee_ID <> @Id", email)
                  || IsTaken("SELECT COUNT(*) FROM Customer WHERE Customer_EmailAddress = @V", email)) { bad = txtEmail; error = "That email address is already registered to another account."; }
            else if (IsTaken("SELECT COUNT(*) FROM Employee WHERE Employee_IDNumber = @V AND Employee_ID <> @Id", idNumber)) { bad = txtIdNumber; error = "Another employee already has that ID number."; }

            if (error != null)
            {
                bad.CssClass = "login-input invalid";
                ShowMessage(error, false);
                return;
            }

            int rows;
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Employee
                    SET Employee_FirstName = @First, Employee_LastName = @Last,
                        Employee_IDNumber = @IdNumber, Employee_PhoneNumber = @Phone,
                        Employee_Username = @Username, Employee_EmailAddress = @Email
                    WHERE Employee_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@First", first);
                    cmd.Parameters.AddWithValue("@Last", last);
                    cmd.Parameters.AddWithValue("@IdNumber", idNumber);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Id", EmployeeId);
                    conn.Open();
                    rows = cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                ShowMessage("Your details could not be saved: " + ex.Message, false);
                return;
            }

            if (rows == 0)
            {
                ShowMessage("Nothing was saved because your account could not be found. Please log out and log in again.", false);
                return;
            }

            // Keep the header / dashboard greeting in sync with the new username
            Session["UserName"] = username;

            LoadDetails();
            ShowMessage("Your details have been updated.", true);
        }

        // ---------- CHANGE PASSWORD ----------
        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            ClearInvalid();

            string current = txtCurrentPassword.Text;
            string newPw = txtNewPassword.Text;
            string confirm = txtConfirmPassword.Text;

            WebControl bad = null;
            string error = null;

            if (current.Length == 0) { bad = txtCurrentPassword; error = "Please enter your current password."; }
            else if (!Regex.IsMatch(newPw, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,50}$")) { bad = txtNewPassword; error = "The new password must be 8 to 50 characters with an upper-case letter, a lower-case letter, a number and a symbol."; }
            else if (newPw != confirm) { bad = txtConfirmPassword; error = "The new passwords do not match."; }
            else if (newPw == current) { bad = txtNewPassword; error = "The new password must be different from your current one."; }

            if (error == null)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(ConnStr))
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM Employee WHERE Employee_ID = @Id AND Employee_Password = @Pw", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", EmployeeId);
                        cmd.Parameters.AddWithValue("@Pw", current);
                        conn.Open();
                        if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                        {
                            bad = txtCurrentPassword;
                            error = "Your current password is incorrect.";
                        }
                    }
                }
                catch (SqlException ex)
                {
                    ShowMessage("Your password could not be checked: " + ex.Message, false);
                    return;
                }
            }

            if (error != null)
            {
                bad.CssClass = "login-input invalid";
                ShowMessage(error, false);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Employee SET Employee_Password = @Pw WHERE Employee_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Pw", newPw);
                    cmd.Parameters.AddWithValue("@Id", EmployeeId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                ShowMessage("Your password could not be updated: " + ex.Message, false);
                return;
            }

            txtCurrentPassword.Text = "";
            txtNewPassword.Text = "";
            txtConfirmPassword.Text = "";
            ShowMessage("Your password has been updated.", true);
        }

        // ---------- HELPERS ----------
        // Query must use @V for the value; @Id (the logged-in manager) is supplied when referenced.
        private bool IsTaken(string sql, string value)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@V", value);
                if (sql.Contains("@Id")) cmd.Parameters.AddWithValue("@Id", EmployeeId);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private void ClearInvalid()
        {
            foreach (TextBox t in AllTextBoxes) t.CssClass = "login-input";
        }

        private void ShowMessage(string text, bool success)
        {
            litMessage.Text = "<div class=\"emp-alert " + (success ? "emp-alert-success" : "emp-alert-error") + "\">"
                + Server.HtmlEncode(text) + "</div>";
        }

        // ---------- SA ID VALIDATION (same rules as Register / Manage Employees) ----------
        private static string ValidateSaId(string id)
        {
            if (!Regex.IsMatch(id, @"^\d{13}$")) return "ID number must be 13 digits.";
            if (!IsValidLuhn(id)) return "Enter a valid South African ID number.";

            DateTime birthDate;
            if (!TryGetBirthDateFromSaId(id, out birthDate)) return "That ID number contains an invalid date of birth.";

            int age = DateTime.Today.Year - birthDate.Year;
            if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;
            if (age < 18) return "An employee must be at least 18 years old.";

            return null;
        }

        private static bool IsValidLuhn(string number)
        {
            int sum = 0;
            bool alternate = false;
            for (int i = number.Length - 1; i >= 0; i--)
            {
                int n = number[i] - '0';
                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }
                sum += n;
                alternate = !alternate;
            }
            return sum % 10 == 0;
        }

        private static bool TryGetBirthDateFromSaId(string id, out DateTime birthDate)
        {
            birthDate = default(DateTime);
            int yy = int.Parse(id.Substring(0, 2));
            int mm = int.Parse(id.Substring(2, 2));
            int dd = int.Parse(id.Substring(4, 2));
            int century = yy > DateTime.Today.Year % 100 ? 1900 : 2000;
            try
            {
                birthDate = new DateTime(century + yy, mm, dd);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }
    }
}
