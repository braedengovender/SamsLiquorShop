using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using SamsLiquorShop.Helpers;

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
            txtCurrentPassword, txtNewPassword, txtConfirmPassword, txtDeactivatePassword
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Customer")) return;

            if (!IsPostBack)
            {
                LoadDetails();
                // Came back to the page with a code still waiting: show the box again
                if (PendingEmail != null) ShowEmailVerify();
                if (PasswordChangePending) ShowPasswordVerify();
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
                            pnlDeactivate.Visible = false;
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
            //string idNumber = txtIdNumber.Text.Trim();
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
            //else if ((error = ValidateSaId(idNumber)) != null) { bad = txtIdNumber; }
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
           // else if (IsTaken("SELECT COUNT(*) FROM Customer WHERE Customer_IDNumber = @V AND Customer_ID <> @Id", idNumber)) { bad = txtIdNumber; error = "That ID number is already registered to another account."; }

            if (error != null)
            {
                bad.CssClass = "login-input invalid";
                ShowMessage(error, false);
                return;
            }

            // A new email address is only saved after the customer enters the code we send to it.
            // Everything else is saved now; the email stays as it is until then.
            string currentEmail = GetCurrentEmail();
            bool emailChanged = currentEmail != null && !string.Equals(email, currentEmail, StringComparison.OrdinalIgnoreCase);
            string newEmail = email;
            if (emailChanged) email = currentEmail;

            int rows;
            string target;
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Customer
                    SET Customer_FirstName = @First, Customer_LastName = @Last,
                        Customer_PhoneNumber = @Phone,
                        Customer_Username = @Username, Customer_EmailAddress = @Email,
                        Customer_UnitNumber = @UnitNumber, Customer_UnitName = @UnitName,
                        Customer_StreetNumber = @StreetNumber, Customer_StreetName = @StreetName,
                        Customer_Suburb = @Suburb, Customer_City = @City,
                        Customer_PostalCode = @PostalCode, Customer_Province = @Province
                    WHERE Customer_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@First", first);
                    cmd.Parameters.AddWithValue("@Last", last);
                    //cmd.Parameters.AddWithValue("@IdNumber", idNumber);
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

            if (emailChanged)
            {
                if (!StartEmailChange(newEmail, first))
                {
                    ShowMessage("Your other details were saved, but we couldn't send a code to " + newEmail + ". Please check the address and try again.", false);
                    return;
                }
                ShowMessage("Your other details were saved. To change your email, enter the code we sent to " + newEmail + ".", true);
                return;
            }

            ShowMessage("Your details have been updated (" + rows + " row saved to " + target + ", Customer ID " + CustomerId + ").", true);
        }

        // ---------- CHANGE EMAIL (OTP) ----------
        private const int MaxEmailCodeAttempts = 5;

        // The new address waiting for its code, or null (also null if it belongs to a different login or has expired)
        private string PendingEmail
        {
            get
            {
                if (!(Session["EmailChangeCustomer"] is int) || (int)Session["EmailChangeCustomer"] != CustomerId) return null;
                if (!(Session["EmailChangeExpiry"] is DateTime) || DateTime.Now > (DateTime)Session["EmailChangeExpiry"]) return null;
                return Session["EmailChangeNew"] as string;
            }
        }

        private void ClearEmailChange()
        {
            foreach (string k in new[] { "EmailChangeCustomer", "EmailChangeNew", "EmailChangeCode", "EmailChangeExpiry", "EmailChangeAttempts" })
                Session.Remove(k);
        }

        private bool StartEmailChange(string newEmail, string firstName)
        {
            string code = OtpHelper.GenerateCode();
            if (!OtpHelper.SendCode(newEmail, firstName, code, "email change")) return false;

            Session["EmailChangeCustomer"] = CustomerId;
            Session["EmailChangeNew"] = newEmail;
            Session["EmailChangeCode"] = code;
            Session["EmailChangeExpiry"] = DateTime.Now.AddMinutes(10);
            Session["EmailChangeAttempts"] = 0;

            txtEmailCode.Text = "";
            lblEmailCodeError.Visible = false;
            ShowEmailVerify();
            return true;
        }

        private void ShowEmailVerify()
        {
            pnlEmailVerify.Visible = true;
            lblPendingEmail.Text = Server.HtmlEncode(PendingEmail ?? "");
            lblEmailDemo.Visible = OtpHelper.DemoMode && Session["EmailChangeCode"] != null;
            if (lblEmailDemo.Visible) lblEmailDemo.Text = "Demo mode: your code is " + Session["EmailChangeCode"];
        }

        private void HideEmailVerify()
        {
            pnlEmailVerify.Visible = false;
            txtEmailCode.Text = "";
            lblEmailCodeError.Visible = false;
        }

        private void EmailCodeError(string text)
        {
            lblEmailCodeError.Text = text;
            lblEmailCodeError.Visible = true;
            txtEmailCode.Text = "";
            ShowEmailVerify();
        }

        protected void btnVerifyEmail_Click(object sender, EventArgs e)
        {
            litMessage.Text = "";
            string newEmail = PendingEmail;
            if (newEmail == null)
            {
                ClearEmailChange();
                HideEmailVerify();
                ShowMessage("That code has expired. Change your email and save again to get a new one.", false);
                return;
            }

            int attempts = (int)Session["EmailChangeAttempts"] + 1;
            Session["EmailChangeAttempts"] = attempts;
            if (attempts > MaxEmailCodeAttempts)
            {
                ClearEmailChange();
                HideEmailVerify();
                ShowMessage("Too many wrong codes. Your email was not changed. Save it again to get a new code.", false);
                return;
            }

            if (txtEmailCode.Text.Trim() != (string)Session["EmailChangeCode"])
            {
                EmailCodeError("That code isn't right. Please try again (" + (MaxEmailCodeAttempts - attempts) + " tries left).");
                return;
            }

            // Someone may have registered the address in the last few minutes
            if (IsTaken("SELECT COUNT(*) FROM Customer WHERE Customer_EmailAddress = @V AND Customer_ID <> @Id", newEmail)
                || IsTaken("SELECT COUNT(*) FROM Employee WHERE Employee_EmailAddress = @V", newEmail))
            {
                ClearEmailChange();
                HideEmailVerify();
                ShowMessage("That email address is already registered to another account. Your email was not changed.", false);
                return;
            }

            string oldEmail = GetCurrentEmail();
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand("UPDATE Customer SET Customer_EmailAddress = @Email WHERE Customer_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Email", newEmail);
                    cmd.Parameters.AddWithValue("@Id", CustomerId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                EmailCodeError("Your email could not be changed: " + ex.Message);
                return;
            }

            ClearEmailChange();
            HideEmailVerify();
            SessionWatch.UpdateEmail(Session.SessionID, newEmail);   // logout / expiry notices go to the new address
            NotifyOldEmail(oldEmail, newEmail);
            LoadDetails();
            ShowMessage("Your email address has been changed to " + newEmail + ".", true);
        }

        protected void btnResendEmailCode_Click(object sender, EventArgs e)
        {
            litMessage.Text = "";
            string newEmail = Session["EmailChangeCustomer"] is int && (int)Session["EmailChangeCustomer"] == CustomerId
                ? Session["EmailChangeNew"] as string : null;
            if (newEmail == null)
            {
                HideEmailVerify();
                ShowMessage("Change your email and save again to get a code.", false);
                return;
            }
            if (!StartEmailChange(newEmail, txtFirstName.Text.Trim()))
            {
                EmailCodeError("We couldn't send a new code. Please try again.");
                return;
            }
            ShowMessage("We sent a new code to " + newEmail + ".", true);
        }

        protected void btnCancelEmailChange_Click(object sender, EventArgs e)
        {
            ClearEmailChange();
            HideEmailVerify();
            LoadDetails();
            ShowMessage("Your email address was not changed.", true);
        }

        private string GetCurrentEmail()
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand("SELECT Customer_EmailAddress FROM Customer WHERE Customer_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", CustomerId);
                conn.Open();
                object o = cmd.ExecuteScalar();
                return o == null || o == DBNull.Value ? null : o.ToString().Trim();
            }
        }

        // Security notice to the old address, so the real owner hears about it if someone else made the change
        private void NotifyOldEmail(string oldEmail, string newEmail)
        {
            if (string.IsNullOrWhiteSpace(oldEmail) || OtpHelper.DemoMode) return;
            string first = Server.HtmlEncode(txtFirstName.Text.Trim());
            string html = "<p>Hi " + (first.Length > 0 ? first : "there") + ",</p>"
                + "<p>The email address on your Sam's Liquor Shop account was changed to <strong>" + Server.HtmlEncode(newEmail) + "</strong>.</p>"
                + "<p>If you made this change, you don't need to do anything. If you didn't, please contact us straight away.</p>"
                + "<p>Sam's Liquor Shop</p>";
            try { OtpHelper.SendEmail(oldEmail, txtFirstName.Text.Trim(), "Your Sam's Liquor Shop email address was changed", html); }
            catch { /* the change itself has already succeeded */ }
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

            // Passwords are fine: confirm it's really the account owner with a code sent to the account's email.
            // The new password is only saved once the code is entered.
            string accountEmail = GetCurrentEmail();
            if (string.IsNullOrWhiteSpace(accountEmail))
            {
                ShowMessage("There's no email address on your account, so we can't send a code. Please add one above first.", false);
                return;
            }
            if (!StartPasswordChange(newPw, accountEmail))
            {
                ShowMessage("We couldn't send a code to " + accountEmail + ". Your password has not been changed. Please try again.", false);
                return;
            }
            ShowMessage("Enter the code we sent to " + accountEmail + " to finish changing your password.", true);
        }

        // ---------- CHANGE PASSWORD (OTP) ----------
        private const int MaxPasswordCodeAttempts = 5;
        private const int PasswordResendSeconds = 30;

        // True when a new password is waiting for its code (for this customer, and not expired)
        private bool PasswordChangePending =>
            Session["PwChangeCustomer"] is int && (int)Session["PwChangeCustomer"] == CustomerId
            && Session["PwChangeExpiry"] is DateTime && DateTime.Now <= (DateTime)Session["PwChangeExpiry"]
            && Session["PwChangeNew"] is string && Session["PwChangeCode"] is string;

        private void ClearPasswordChange()
        {
            foreach (string k in new[] { "PwChangeCustomer", "PwChangeNew", "PwChangeCode", "PwChangeExpiry", "PwChangeAttempts", "PwChangeEmail", "PwChangeSent" })
                Session.Remove(k);
        }

        private bool StartPasswordChange(string newPw, string email)
        {
            string code = OtpHelper.GenerateCode();
            if (!OtpHelper.SendCode(email, txtFirstName.Text.Trim(), code, "password change")) return false;

            Session["PwChangeCustomer"] = CustomerId;
            Session["PwChangeNew"] = newPw;
            Session["PwChangeCode"] = code;
            Session["PwChangeEmail"] = email;
            Session["PwChangeExpiry"] = DateTime.Now.AddMinutes(10);
            Session["PwChangeAttempts"] = 0;
            Session["PwChangeSent"] = DateTime.Now;

            ShowPasswordVerify();
            return true;
        }

        private void ShowPasswordVerify()
        {
            pnlPassword.Visible = false;
            pnlPasswordVerify.Visible = true;
            txtPasswordCode.Text = "";
            lblPasswordEmail.Text = Server.HtmlEncode(Session["PwChangeEmail"] as string ?? "");
            lblPasswordDemo.Visible = OtpHelper.DemoMode && Session["PwChangeCode"] != null;
            if (lblPasswordDemo.Visible) lblPasswordDemo.Text = "Demo mode: your code is " + Session["PwChangeCode"];
        }

        private void HidePasswordVerify()
        {
            pnlPasswordVerify.Visible = false;
            pnlPassword.Visible = true;
            txtPasswordCode.Text = "";
            lblPasswordCodeError.Visible = false;
        }

        private void PasswordCodeError(string text)
        {
            ShowPasswordVerify();
            lblPasswordCodeError.Text = Server.HtmlEncode(text);
            lblPasswordCodeError.Visible = true;
        }

        protected void btnVerifyPassword_Click(object sender, EventArgs e)
        {
            litMessage.Text = "";
            if (!PasswordChangePending)
            {
                ClearPasswordChange();
                HidePasswordVerify();
                ShowMessage("That code has expired. Your password was not changed. Please enter your new password again.", false);
                return;
            }

            int attempts = (int)Session["PwChangeAttempts"] + 1;
            Session["PwChangeAttempts"] = attempts;
            if (attempts > MaxPasswordCodeAttempts)
            {
                ClearPasswordChange();
                HidePasswordVerify();
                ShowMessage("Too many wrong codes. Your password was not changed. Please start again.", false);
                return;
            }

            if (txtPasswordCode.Text.Trim() != (string)Session["PwChangeCode"])
            {
                int left = MaxPasswordCodeAttempts - attempts;
                PasswordCodeError(left > 0
                    ? "That code isn't right. Please try again (" + left + " tries left)."
                    : "That code isn't right. Please request a new code.");
                return;
            }

            string newPw = (string)Session["PwChangeNew"];
            string email = Session["PwChangeEmail"] as string;
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
                PasswordCodeError("Your password could not be updated: " + ex.Message);
                return;
            }

            ClearPasswordChange();
            HidePasswordVerify();
            NotifyPasswordChanged(email);
            ShowMessage("Your password has been updated.", true);
        }

        protected void btnResendPasswordCode_Click(object sender, EventArgs e)
        {
            litMessage.Text = "";
            if (!(Session["PwChangeCustomer"] is int) || (int)Session["PwChangeCustomer"] != CustomerId || !(Session["PwChangeNew"] is string))
            {
                ClearPasswordChange();
                HidePasswordVerify();
                ShowMessage("Please enter your new password again to get a code.", false);
                return;
            }

            if (Session["PwChangeSent"] is DateTime)
            {
                double elapsed = (DateTime.Now - (DateTime)Session["PwChangeSent"]).TotalSeconds;
                if (elapsed < PasswordResendSeconds)
                {
                    PasswordCodeError("Please wait " + (int)Math.Ceiling(PasswordResendSeconds - elapsed) + " second(s) before asking for another code.");
                    return;
                }
            }

            string email = GetCurrentEmail();
            if (string.IsNullOrWhiteSpace(email) || !StartPasswordChange((string)Session["PwChangeNew"], email))
            {
                PasswordCodeError("We couldn't send a new code. Please try again.");
                return;
            }
            ShowMessage("We sent a new code to " + email + ".", true);
        }

        protected void btnCancelPasswordChange_Click(object sender, EventArgs e)
        {
            ClearPasswordChange();
            HidePasswordVerify();
            ShowMessage("Your password was not changed.", true);
        }

        // Security notice, so the real owner hears about it if someone else changed the password
        private void NotifyPasswordChanged(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || OtpHelper.DemoMode) return;
            string first = txtFirstName.Text.Trim();
            string html = "<p>Hi " + (first.Length > 0 ? Server.HtmlEncode(first) : "there") + ",</p>"
                + "<p>The password on your Sam's Liquor Shop account was just changed.</p>"
                + "<p>If you made this change, you don't need to do anything. If you didn't, please reset your password straight away and contact us on 082 405 5932.</p>"
                + "<p>Sam's Liquor Shop</p>";
            System.Threading.Tasks.Task.Run(() =>
            {
                try { OtpHelper.SendEmail(email, first, "Your Sam's Liquor Shop password was changed", html); }
                catch { /* the change itself has already succeeded */ }
            });
        }

        // ---------- DEACTIVATE PROFILE ----------
        protected void btnDeactivate_Click(object sender, EventArgs e)
        {
            ClearInvalid();

            string password = txtDeactivatePassword.Text;
            if (password.Length == 0)
            {
                txtDeactivatePassword.CssClass = "login-input invalid";
                ShowMessage("Please enter your password to deactivate your profile.", false);
                return;
            }

            int rows;
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Customer SET Customer_Status = 'Inactive'
                    WHERE Customer_ID = @Id AND Customer_Password = @Pw AND Customer_Status = 'Active'", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", CustomerId);
                    cmd.Parameters.AddWithValue("@Pw", password);
                    conn.Open();
                    rows = cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                ShowMessage("Your profile could not be deactivated: " + ex.Message, false);
                return;
            }

            if (rows == 0)
            {
                txtDeactivatePassword.Text = "";
                txtDeactivatePassword.CssClass = "login-input invalid";
                ShowMessage("Your password is incorrect. Your profile has not been deactivated.", false);
                return;
            }

            // Log the customer out (same steps as the Log out button, minus the logout email)
            string ignoredEmail, ignoredName;
            SessionWatch.Unregister(Session.SessionID, out ignoredEmail, out ignoredName);
            Session.Clear();
            Response.Redirect("~/Login.aspx?account=deactivated", false);
            Context.ApplicationInstance.CompleteRequest();
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
