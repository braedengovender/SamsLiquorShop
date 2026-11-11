using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace SamsLiquorShop.Customer
{
    public partial class Account : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private int CustomerId => Convert.ToInt32(Session["CustomerID"]);

        private TextBox[] AllTextBoxes => new[]
        {
            txtFirstName, txtLastName, txtIdNumber, txtPhone, txtUsername, txtEmail,
            txtUnitNumber, txtUnitName, txtStreetNumber, txtStreetName, txtSuburb, txtCity, txtPostalCode,
            txtCurrentPassword, txtNewPassword, txtConfirmPassword
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Customer")) return;

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
                    SELECT Customer_FirstName, Customer_LastName, Customer_IDNumber, Customer_PhoneNumber,
                           Customer_Username, Customer_EmailAddress,
                           Customer_UnitNumber, Customer_UnitName, Customer_StreetNumber, Customer_StreetName,
                           Customer_Suburb, Customer_City, Customer_PostalCode, Customer_Province
                    FROM Customer WHERE Customer_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", CustomerId);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            ShowMessage("Your account could not be found.", false);
                            pnlDetails.Visible = false;
                            pnlPassword.Visible = false;
                            return;
                        }

                        txtFirstName.Text = Val(r, "Customer_FirstName");
                        txtLastName.Text = Val(r, "Customer_LastName");
                        txtIdNumber.Text = Val(r, "Customer_IDNumber");
                        txtPhone.Text = Val(r, "Customer_PhoneNumber");
                        txtUsername.Text = Val(r, "Customer_Username");
                        txtEmail.Text = Val(r, "Customer_EmailAddress");
                        txtUnitNumber.Text = Val(r, "Customer_UnitNumber");
                        txtUnitName.Text = Val(r, "Customer_UnitName");
                        txtStreetNumber.Text = Val(r, "Customer_StreetNumber");
                        txtStreetName.Text = Val(r, "Customer_StreetName");
                        txtSuburb.Text = Val(r, "Customer_Suburb");
                        txtCity.Text = Val(r, "Customer_City");
                        txtPostalCode.Text = Val(r, "Customer_PostalCode");

                        string province = Val(r, "Customer_Province");
                        ListItem item = ddlProvince.Items.FindByValue(province);
                        ddlProvince.ClearSelection();
                        if (item != null) item.Selected = true;
                    }
                }
            }
            catch (SqlException ex)
            {
                ShowMessage("Your details could not be loaded. If this mentions an invalid column name, run Database\\AddCustomerDetailColumns.sql on the GroupWst15 database first. (" + ex.Message + ")", false);
                pnlDetails.Visible = false;
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
            string unitNumber = txtUnitNumber.Text.Trim();
            string unitName = txtUnitName.Text.Trim();
            string streetNumber = txtStreetNumber.Text.Trim();
            string streetName = txtStreetName.Text.Trim();
            string suburb = txtSuburb.Text.Trim();
            string city = txtCity.Text.Trim();
            string postalCode = txtPostalCode.Text.Trim();
            string province = ddlProvince.SelectedValue;

            WebControl bad = null;
            string error = null;

            if (first.Length == 0) { bad = txtFirstName; error = "First name is required."; }
            else if (last.Length == 0) { bad = txtLastName; error = "Last name is required."; }
            else if ((error = ValidateSaId(idNumber)) != null) { bad = txtIdNumber; }
            else if (!Regex.IsMatch(phone, @"^(\+27|0)[1-9][0-9]{8}$")) { bad = txtPhone; error = "Phone number must look like 0821234567 or +27821234567."; }
            else if (!Regex.IsMatch(username, @"^[A-Za-z0-9_]{4,30}$")) { bad = txtUsername; error = "Username must be 4 to 30 characters: letters, numbers or underscores only."; }
            else if (!Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$")) { bad = txtEmail; error = "Please enter a valid email address."; }
            else if (streetNumber.Length == 0) { bad = txtStreetNumber; error = "Street number is required."; }
            else if (streetName.Length == 0) { bad = txtStreetName; error = "Street name is required."; }
            else if (suburb.Length == 0) { bad = txtSuburb; error = "Suburb is required."; }
            else if (city.Length == 0) { bad = txtCity; error = "City is required."; }
            else if (!Regex.IsMatch(postalCode, @"^\d{4}$")) { bad = txtPostalCode; error = "Postal code must be 4 digits."; }
            else if (province.Length == 0) { bad = ddlProvince; error = "Please select a province."; }
            else if (IsTaken("SELECT COUNT(*) FROM Customer WHERE Customer_Username = @V AND Customer_ID <> @Id", username)
                  || IsTaken("SELECT COUNT(*) FROM Employee WHERE Employee_Username = @V", username)) { bad = txtUsername; error = "That username is already taken."; }
            else if (IsTaken("SELECT COUNT(*) FROM Customer WHERE Customer_EmailAddress = @V AND Customer_ID <> @Id", email)
                  || IsTaken("SELECT COUNT(*) FROM Employee WHERE Employee_EmailAddress = @V", email)) { bad = txtEmail; error = "That email address is already registered."; }
            else if (IsTaken("SELECT COUNT(*) FROM Customer WHERE Customer_IDNumber = @V AND Customer_ID <> @Id", idNumber)) { bad = txtIdNumber; error = "That ID number is already registered to another account."; }

            if (error != null)
            {
                bad.CssClass = "login-input invalid";
                ShowMessage(error, false);
                return;
            }

            int rows;
            string target;
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Customer
                    SET Customer_FirstName = @First, Customer_LastName = @Last,
                        Customer_IDNumber = @IdNumber, Customer_PhoneNumber = @Phone,
                        Customer_Username = @Username, Customer_EmailAddress = @Email,
                        Customer_UnitNumber = @UnitNumber, Customer_UnitName = @UnitName,
                        Customer_StreetNumber = @StreetNumber, Customer_StreetName = @StreetName,
                        Customer_Suburb = @Suburb, Customer_City = @City,
                        Customer_PostalCode = @PostalCode, Customer_Province = @Province
                    WHERE Customer_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@First", first);
                    cmd.Parameters.AddWithValue("@Last", last);
                    cmd.Parameters.AddWithValue("@IdNumber", idNumber);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@UnitNumber", unitNumber);
                    cmd.Parameters.AddWithValue("@UnitName", unitName);
                    cmd.Parameters.AddWithValue("@StreetNumber", streetNumber);
                    cmd.Parameters.AddWithValue("@StreetName", streetName);
                    cmd.Parameters.AddWithValue("@Suburb", suburb);
                    cmd.Parameters.AddWithValue("@City", city);
                    cmd.Parameters.AddWithValue("@PostalCode", postalCode);
                    cmd.Parameters.AddWithValue("@Province", province);
                    cmd.Parameters.AddWithValue("@Id", CustomerId);
                    conn.Open();
                    rows = cmd.ExecuteNonQuery();
                    target = conn.DataSource + " / " + conn.Database;
                }
            }
            catch (SqlException ex)
            {
                ShowMessage("Your details could not be saved: " + ex.Message, false);
                return;
            }

            if (rows == 0)
            {
                ShowMessage("Nothing was saved: no Customer row with ID " + CustomerId + " was found in " + target + ". Please log out and log in again.", false);
                return;
            }

            // Keep the greeting / header in sync with the new username
            Session["UserName"] = username;

            // Re-read from the database so the form shows exactly what was stored
            LoadDetails();
            ShowMessage("Your details have been updated (" + rows + " row saved to " + target + ", Customer ID " + CustomerId + ").", true);
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
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Customer WHERE Customer_ID = @Id AND Customer_Password = @Pw", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", CustomerId);
                    cmd.Parameters.AddWithValue("@Pw", current);
                    conn.Open();
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        bad = txtCurrentPassword;
                        error = "Your current password is incorrect.";
                    }
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
                    "UPDATE Customer SET Customer_Password = @Pw WHERE Customer_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Pw", newPw);
                    cmd.Parameters.AddWithValue("@Id", CustomerId);
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
        // Query must use @V for the value; @Id (the current customer) is supplied when referenced.
        private bool IsTaken(string sql, string value)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@V", value);
                if (sql.Contains("@Id")) cmd.Parameters.AddWithValue("@Id", CustomerId);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private void ClearInvalid()
        {
            foreach (TextBox t in AllTextBoxes) t.CssClass = "login-input";
            ddlProvince.CssClass = "login-input";
        }

        private void ShowMessage(string text, bool success)
        {
            litMessage.Text = "<div class=\"emp-alert " + (success ? "emp-alert-success" : "emp-alert-error") + "\">"
                + Server.HtmlEncode(text) + "</div>";
        }

        // ---------- SA ID VALIDATION (same rules as Register) ----------
        private static string ValidateSaId(string id)
        {
            if (id.Length != 13 || !Regex.IsMatch(id, @"^\d{13}$")) return "ID number must be 13 digits.";
            if (!IsValidLuhn(id)) return "Enter a valid South African ID number.";

            DateTime birthDate;
            if (!TryGetBirthDateFromSaId(id, out birthDate)) return "Enter a valid South African ID number.";

            int age = DateTime.Today.Year - birthDate.Year;
            if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;
            if (age < 18) return "You must be 18 or older.";

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
