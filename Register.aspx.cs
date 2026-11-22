using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;
using SamsLiquorShop.Helpers;

namespace SamsLiquorShop
{
    public partial class Register : System.Web.UI.Page
    {
        private const string RegistrationDateColumn = "Customer_RegistrationDateTime";

        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private class NewCustomer
        {
            public string First, Last, IdNumber, Phone, Email;
            public string UnitNumber, UnitName, StreetNumber, StreetName, Suburb, PostalCode, City, Province;
            public string Username, Password;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Someone who is already logged in has no need to register.
            if (Session["UserRole"] != null)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            // A fresh visit always starts registration from scratch.
            if (!IsPostBack) ClearPending();

            // Keep field lengths in step with the database columns.
            txtEmail.MaxLength = 100;
            txtPhone.MaxLength = 12;
            txtUnitName.MaxLength = 50;
            txtStreetNumber.MaxLength = 10;
            txtSuburb.MaxLength = 50;
            txtCity.MaxLength = 50;
            txtPassword.MaxLength = 50;
            txtConfirmPassword.MaxLength = 50;
            revPassword.ValidationExpression = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,50}$";
        }

        // ---------- Validators ----------
        protected void cvIdNumber_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var id = txtIdNumber.Text.Trim();

            if (id.Length != 13 || !IsAllDigits(id))
            {
                args.IsValid = false;
                return;
            }

            if (!IsValidLuhn(id))
            {
                args.IsValid = false;
                return;
            }

            if (!TryGetBirthDateFromSaId(id, out var birthDate))
            {
                args.IsValid = false;
                return;
            }

            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;

            args.IsValid = age >= 18;
        }

        protected void cvTerms_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = chkTerms.Checked;
        }

        // DEMO ID photo check: real file checks only, no actual OCR / ID matching.
        protected void cvIdPhoto_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = false;

            if (!fuIdPhoto.HasFile) return;

            string ext = System.IO.Path.GetExtension(fuIdPhoto.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png") return;

            if (fuIdPhoto.PostedFile.ContentLength > 5 * 1024 * 1024) return;

            try
            {
                using (var img = System.Drawing.Image.FromStream(fuIdPhoto.PostedFile.InputStream))
                {
                    bool realFormat = img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg)
                                   || img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png);

                    args.IsValid = realFormat && img.Width >= 400 && img.Height >= 250;
                }
            }
            catch (ArgumentException)
            {
                // Not a valid image file (e.g. renamed .exe).
                args.IsValid = false;
            }
        }

        // ---------- Create the account ----------
        protected void btnRegister_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            if (!Page.IsValid) return;

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                lblError.Text = "Passwords do not match.";
                return;
            }

            var c = new NewCustomer
            {
                First = txtFirstName.Text.Trim(),
                Last = txtLastName.Text.Trim(),
                IdNumber = txtIdNumber.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                UnitNumber = txtUnitNumber.Text.Trim(),
                UnitName = txtUnitName.Text.Trim(),
                StreetNumber = txtStreetNumber.Text.Trim(),
                StreetName = txtStreetName.Text.Trim(),
                Suburb = txtSuburb.Text.Trim(),
                PostalCode = txtPostalCode.Text.Trim(),
                City = txtCity.Text.Trim(),
                Province = ddlProvince.SelectedValue,
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text
            };

            // Blank username: use the email address, as the form promises.
            if (c.Username.Length == 0)
            {
                if (c.Email.Length > 50)
                {
                    lblError.Text = "Your email address is too long to use as a username. Please enter a username above.";
                    return;
                }
                c.Username = c.Email;
            }

            string duplicate = FindDuplicate(c);
            if (duplicate != null)
            {
                lblError.Text = duplicate;
                return;
            }

            // Hold the details in the session until the email is verified.
            Session["RegPending"] = c;
            Session["RegResends"] = 0;

            if (!SendNewCode(c))
            {
                ClearPending();
                lblError.Text = "We couldn't send a verification email. Please check your email address and try again.";
                return;
            }

            ShowVerify(c.Email);
        }

        // ---------- Email verification (OTP) ----------
        private const int CodeMinutes = 10;
        private const int MaxAttempts = 5;
        private const int ResendSeconds = 60;
        private const int MaxResends = 5;

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            var c = Session["RegPending"] as NewCustomer;
            string code = Session["RegCode"] as string;

            if (c == null || code == null || !(Session["RegExpires"] is DateTime))
            {
                ShowForm("Your verification session expired. Please fill in the form again.");
                return;
            }

            if (DateTime.UtcNow > (DateTime)Session["RegExpires"])
            {
                lblVerifyError.Text = "That code has expired. Please request a new one.";
                ShowVerify(c.Email);
                return;
            }

            int attempts = Session["RegAttempts"] is int ? (int)Session["RegAttempts"] : 0;
            if (attempts >= MaxAttempts)
            {
                lblVerifyError.Text = "Too many incorrect attempts. Please request a new code.";
                ShowVerify(c.Email);
                return;
            }

            string entered = txtOtp.Text.Trim();
            if (entered != code)
            {
                attempts++;
                Session["RegAttempts"] = attempts;
                txtOtp.Text = "";
                lblVerifyError.Text = attempts >= MaxAttempts
                    ? "Too many incorrect attempts. Please request a new code."
                    : "That code isn't correct. You have " + (MaxAttempts - attempts) + " attempt(s) left.";
                ShowVerify(c.Email);
                return;
            }

            // Code is correct. Re-check for duplicates in case someone registered in the meantime.
            string duplicate = FindDuplicate(c);
            if (duplicate != null)
            {
                ShowForm(duplicate);
                return;
            }

            int newId;
            try
            {
                newId = InsertCustomer(c);
            }
            catch (SqlException ex)
            {
                lblVerifyError.Text = "We couldn't create your account. Please try again or contact us."
                    + (HttpContext.Current.IsDebuggingEnabled ? " (" + ex.Message + ")" : "");
                ShowVerify(c.Email);
                return;
            }

            ClearPending();

            // Log the new customer straight in.
            Session["CustomerID"] = newId;
            Session["UserRole"] = "Customer";
            Session["UserName"] = c.Username;

            Response.Redirect("~/Customer/Dashboard.aspx");
        }

        protected void btnResend_Click(object sender, EventArgs e)
        {
            var c = Session["RegPending"] as NewCustomer;
            if (c == null)
            {
                ShowForm("Your verification session expired. Please fill in the form again.");
                return;
            }

            int resends = Session["RegResends"] is int ? (int)Session["RegResends"] : 0;
            if (resends >= MaxResends)
            {
                ShowForm("Too many codes were requested. Please try again later.");
                return;
            }

            if (Session["RegLastSent"] is DateTime)
            {
                double elapsed = (DateTime.UtcNow - (DateTime)Session["RegLastSent"]).TotalSeconds;
                if (elapsed < ResendSeconds)
                {
                    lblVerifyError.Text = "Please wait " + (int)Math.Ceiling(ResendSeconds - elapsed) + " second(s) before requesting another code.";
                    ShowVerify(c.Email);
                    return;
                }
            }

            if (!SendNewCode(c))
            {
                lblVerifyError.Text = "We couldn't send the email. Please try again shortly.";
                ShowVerify(c.Email);
                return;
            }

            Session["RegResends"] = resends + 1;
            txtOtp.Text = "";
            lblVerifyInfo.Text = "A new code is on its way.";
            ShowVerify(c.Email);
        }

        protected void btnChangeEmail_Click(object sender, EventArgs e)
        {
            ShowForm("");
        }

        // Creates a fresh code, stores it in the session and emails it.
        private bool SendNewCode(NewCustomer c)
        {
            string code = OtpHelper.GenerateCode();
            Session["RegCode"] = code;
            Session["RegExpires"] = DateTime.UtcNow.AddMinutes(CodeMinutes);
            Session["RegAttempts"] = 0;
            Session["RegLastSent"] = DateTime.UtcNow;

            bool sent = OtpHelper.SendCode(c.Email, c.First + " " + c.Last, code, "registration");
            if (!sent) Session["RegCode"] = null;
            return sent;
        }

        private void ShowVerify(string email)
        {
            pnlForm.Visible = false;
            pnlVerify.Visible = true;
            lblVerifyEmail.Text = Server.HtmlEncode(email);

            // In demo mode no email is sent, so show the code to allow testing.
            lblDemo.Visible = OtpHelper.DemoMode && Session["RegCode"] is string;
            if (lblDemo.Visible)
                lblDemo.Text = "Demo mode is on, so no email was sent. Your code is " + (string)Session["RegCode"] + ".";
        }

        private void ShowForm(string message)
        {
            ClearPending();
            pnlVerify.Visible = false;
            pnlForm.Visible = true;
            lblError.Text = message;
        }

        private void ClearPending()
        {
            Session.Remove("RegPending");
            Session.Remove("RegCode");
            Session.Remove("RegExpires");
            Session.Remove("RegAttempts");
            Session.Remove("RegLastSent");
            Session.Remove("RegResends");
        }

        // ---------- Database ----------
        private int InsertCustomer(NewCustomer c)
        {
            string sql = @"
                INSERT INTO Customer
                    (Customer_FirstName, Customer_LastName, Customer_EmailAddress, Customer_IDNumber,
                     Customer_PhoneNumber, Customer_UnitNumber, Customer_UnitName, Customer_StreetNumber,
                     Customer_StreetName, Customer_Suburb, Customer_PostalCode, Customer_City,
                     Customer_Province, Customer_Country, " + RegistrationDateColumn + @",
                     Customer_Status, Customer_Username, Customer_Password)
                OUTPUT INSERTED.Customer_ID
                VALUES
                    (@First, @Last, @Email, @IdNumber,
                     @Phone, @UnitNumber, @UnitName, @StreetNumber,
                     @StreetName, @Suburb, @PostalCode, @City,
                     @Province, 'South Africa', @Registered,
                     'Active', @Username, @Password)";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@First", c.First);
                cmd.Parameters.AddWithValue("@Last", c.Last);
                cmd.Parameters.AddWithValue("@Email", c.Email);
                cmd.Parameters.AddWithValue("@IdNumber", c.IdNumber);
                cmd.Parameters.AddWithValue("@Phone", c.Phone);
                cmd.Parameters.AddWithValue("@UnitNumber", string.IsNullOrWhiteSpace(c.UnitNumber) ? (object)DBNull.Value : c.UnitNumber);
                cmd.Parameters.AddWithValue("@UnitName", string.IsNullOrWhiteSpace(c.UnitName) ? (object)DBNull.Value : c.UnitName);
                cmd.Parameters.AddWithValue("@StreetNumber", c.StreetNumber);
                cmd.Parameters.AddWithValue("@StreetName", c.StreetName);
                cmd.Parameters.AddWithValue("@Suburb", c.Suburb);
                cmd.Parameters.AddWithValue("@PostalCode", c.PostalCode);
                cmd.Parameters.AddWithValue("@City", c.City);
                cmd.Parameters.AddWithValue("@Province", c.Province);
                cmd.Parameters.Add("@Registered", SqlDbType.DateTime2).Value = DateTime.Now;
                cmd.Parameters.AddWithValue("@Username", c.Username);
                cmd.Parameters.AddWithValue("@Password", c.Password);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private string FindDuplicate(NewCustomer c)
        {
            if (LoginNameTaken(c.Email))
                return "An account with that email address already exists. Try logging in instead.";

            if (!string.Equals(c.Username, c.Email, StringComparison.OrdinalIgnoreCase) && LoginNameTaken(c.Username))
                return "That username is already taken. Please choose another.";

            if (IdNumberTaken(c.IdNumber))
                return "An account with that ID number already exists. Try logging in instead.";

            return null;
        }

        // Login accepts a username OR an email, so neither may collide with
        // another customer's or employee's username or email.
        private bool LoginNameTaken(string value)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT
                    (SELECT COUNT(*) FROM Customer WHERE Customer_Username = @V OR Customer_EmailAddress = @V)
                  + (SELECT COUNT(*) FROM Employee WHERE Employee_Username = @V OR Employee_EmailAddress = @V)", conn))
            {
                cmd.Parameters.AddWithValue("@V", value);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool IdNumberTaken(string idNumber)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Customer WHERE Customer_IDNumber = @V", conn))
            {
                cmd.Parameters.AddWithValue("@V", idNumber);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ---------- ID helpers ----------
        private static bool IsAllDigits(string value)
        {
            foreach (var c in value)
            {
                if (c < '0' || c > '9') return false;
            }
            return true;
        }

        private static bool IsValidLuhn(string number)
        {
            var sum = 0;
            var alternate = false;

            for (var i = number.Length - 1; i >= 0; i--)
            {
                var n = number[i] - '0';
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
            birthDate = default;

            var yy = int.Parse(id.Substring(0, 2));
            var mm = int.Parse(id.Substring(2, 2));
            var dd = int.Parse(id.Substring(4, 2));

            var currentYY = DateTime.Today.Year % 100;
            var century = yy > currentYY ? 1900 : 2000;

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