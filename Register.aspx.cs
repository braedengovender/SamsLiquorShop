using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Net.Mail;


namespace SamsLiquorShop
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
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

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            // Generate a 5-digit OTP
            Random random = new Random();
            string otp = random.Next(10000, 100000).ToString();

            // Store OTP temporarily in Session
            Session["RegistrationOTP"] = otp;
            Session["RegistrationEmail"] = txtEmail.Text.Trim();

            try
            {
                string emailAddress = "theofficialbyteclub@gmail.com";
                string emailPassword = "";

                string customerEmail = txtEmail.Text.Trim();

                // Create the email
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailAddress, "Sam's Liquor Shop");
                    mail.To.Add(customerEmail);

                    mail.Subject = "Sam's Liquor Shop - Verify Your Email";

                    // Professional HTML email
                    mail.IsBodyHtml = true;

                    mail.Body = @"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                    </head>

                    <body style='margin:0; padding:0; background-color:#f4f9fd; font-family:Arial, Helvetica, sans-serif;'>

                        <div style='padding:40px 15px;'>

                            <div style='max-width:520px; margin:0 auto; background-color:#ffffff; border:1px solid #dceaf4; border-radius:16px; overflow:hidden;'>

                                <!-- HEADER -->
                                <div style='text-align:center; padding:28px 25px; border-bottom:2px solid #00a8ff;'>

                                    <h1 style='margin:0; color:#0097e6; font-size:26px;'>
                                        Sam's Liquor Shop
                                    </h1>

                                    <p style='margin:7px 0 0 0; color:#718093; font-size:13px;'>
                                        Your trusted liquor store
                                    </p>

                                </div>

                                <!-- CONTENT -->
                                <div style='padding:35px 30px;'>

                                    <h2 style='margin:0 0 12px 0; color:#2f3640; font-size:22px;'>
                                        Verify your email
                                    </h2>

                                    <p style='margin:0 0 25px 0; color:#718093; font-size:14px; line-height:1.6;'>
                                        Thanks for creating an account with Sam's Liquor Shop.
                                        Please enter the verification code below to confirm your email address.
                                    </p>

                                    <!-- OTP BOX -->
                                    <div style='text-align:center; background-color:#f0f8ff; border:1px solid #cfe9fa; border-radius:12px; padding:25px; margin:25px 0;'>

                                        <p style='margin:0 0 10px 0; color:#718093; font-size:12px; text-transform:uppercase; letter-spacing:1px;'>
                                            Your verification code
                                        </p>

                                        <div style='font-size:34px; font-weight:bold; letter-spacing:8px; color:#0097e6;'>
                                            " + otp + @"
                                        </div>

                                    </div>

                                    <p style='margin:25px 0 0 0; color:#718093; font-size:13px; line-height:1.5;'>
                                        Enter this code on the registration page to activate your account.
                                    </p>

                                    <p style='margin:20px 0 0 0; color:#718093; font-size:12px;'>
                                        If you did not create this account, you can safely ignore this email.
                                    </p>

                                </div>

                                <!-- FOOTER -->
                                <div style='background-color:#f5f6fa; text-align:center; padding:18px 20px; border-top:1px solid #e5e9ef;'>

                                    <p style='margin:0; color:#7f8c8d; font-size:11px;'>
                                        Sam's Liquor Shop
                                    </p>

                                    <p style='margin:5px 0 0 0; color:#a0a5ab; font-size:10px;'>
                                        Powered by The Byte Club
                                    </p>

                                </div>

                            </div>

                        </div>

                    </body>
                    </html>";

                    // Gmail SMTP
                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.EnableSsl = true;

                        smtp.Credentials = new NetworkCredential(
                            emailAddress,
                            emailPassword
                        );

                        smtp.Send(mail);
                    }
                }

                // Email sent successfully
                pnlForm.Visible = false;
                pnlSuccess.Visible = true;
                lblError.Text = "";
            }
            catch
            {
                lblError.Text = "Unable to send verification email. Please try again.";
            }
        }

        protected void btnVerifyOTP_Click(object sender, EventArgs e)
        {
            string enteredOTP = txtOTP.Text.Trim();

            string storedOTP = Session["RegistrationOTP"] as string;

            if (string.IsNullOrEmpty(storedOTP))
            {
                lblOtpError.Text = "Your verification code has expired. Please register again.";
                return;
            }

            if (enteredOTP == storedOTP)
            {
                // OTP is correct
                Session.Remove("RegistrationOTP");

                pnlSuccess.Visible = true;

                lblOtpError.CssClass = "field-valid";
                lblOtpError.Text = "Email verified successfully! Your account has been activated.";
            }
            else
            {
                lblOtpError.Text = "Incorrect verification code. Please try again.";
            }
        }

        private static bool IsAllDigits(string value)
        {
            foreach (var c in value)
            {
                if (c < '0' || c > '9') return false;
            }
            return true;
        }

        // Standard Luhn checksum - the last digit of a valid SA ID number
        // must make this sum divisible by 10.
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

        // First 6 digits of an SA ID are YYMMDD. The ID itself doesn't encode
        // century, so we assume "not born in the future" - if the two-digit
        // year is greater than the current two-digit year, it must be 1900s,
        // otherwise 2000s.
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