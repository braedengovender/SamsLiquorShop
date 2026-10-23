using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

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

            int newId;
            try
            {
                newId = InsertCustomer(c);
            }
            catch (SqlException ex)
            {
                lblError.Text = "We couldn't create your account. Please try again or contact us."
                    + (HttpContext.Current.IsDebuggingEnabled ? " (" + ex.Message + ")" : "");
                return;
            }

            // Log the new customer straight in.
            Session["CustomerID"] = newId;
            Session["UserRole"] = "Customer";
            Session["UserName"] = c.Username;

            Response.Redirect("~/Customer/Dashboard.aspx");
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