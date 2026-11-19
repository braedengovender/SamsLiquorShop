using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace SamsLiquorShop
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        // Same rule as Register.aspx
        private const string PasswordRule = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,50}$";

        private void ShowStep(int step)
        {
            pnlEmail.Visible = step == 1;
            pnlCode.Visible = step == 2;
            pnlNew.Visible = step == 3;
            pnlDone.Visible = step == 4;
        }

        private void ClearReset()
        {
            foreach (string k in new[] { "ResetTable", "ResetId", "ResetCode", "ResetExpiry", "ResetAttempts", "ResetVerified" })
                Session.Remove(k);
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            if (email.Length == 0) return;

            ClearReset();
            string table = null;
            object id = null;
            string name = null;

            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Customer_ID, Customer_Username FROM Customer WHERE Customer_EmailAddress = @E AND Customer_Status = 'Active'", conn))
                {
                    cmd.Parameters.AddWithValue("@E", email);
                    using (SqlDataReader r = cmd.ExecuteReader())
                        if (r.Read()) { table = "Customer"; id = r["Customer_ID"]; name = r["Customer_Username"].ToString(); }
                }

                if (table == null)
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT Employee_ID, Employee_Username FROM Employee WHERE Employee_EmailAddress = @E AND Employee_Status = 'Active'", conn))
                    {
                        cmd.Parameters.AddWithValue("@E", email);
                        using (SqlDataReader r = cmd.ExecuteReader())
                            if (r.Read()) { table = "Employee"; id = r["Employee_ID"]; name = r["Employee_Username"].ToString(); }
                    }
                }
            }

            // Only send a code if the account exists, but always move on to step 2
            // so the page never reveals whether an email is registered.
            if (table != null)
            {
                string code = Helpers.OtpHelper.GenerateCode();
                if (!Helpers.OtpHelper.SendCode(email, name, code, "password reset"))
                {
                    lblEmailError.Text = "We couldn't send your code. Please try again.";
                    lblEmailError.Visible = true;
                    return;
                }

                Session["ResetTable"] = table;
                Session["ResetId"] = id;
                Session["ResetCode"] = code;
                Session["ResetExpiry"] = DateTime.Now.AddMinutes(10);
                Session["ResetAttempts"] = 0;

                if (Helpers.OtpHelper.DemoMode)
                {
                    spanDemo.InnerText = "Demo mode: your code is " + code;
                    spanDemo.Visible = true;
                }
            }

            ShowStep(2);
        }

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            if (Session["ResetId"] == null)
            {
                lblCodeError.Text = "That code isn't right. Please try again.";
                lblCodeError.Visible = true;
                return;
            }

            int attempts = (int)Session["ResetAttempts"] + 1;
            Session["ResetAttempts"] = attempts;

            if (DateTime.Now > (DateTime)Session["ResetExpiry"] || attempts > 5)
            {
                ClearReset();
                Response.Redirect("~/ForgotPassword.aspx");
                return;
            }

            if (txtCode.Text.Trim() != (string)Session["ResetCode"])
            {
                lblCodeError.Text = "That code isn't right. Please try again.";
                lblCodeError.Visible = true;
                return;
            }

            Session["ResetVerified"] = true;
            ShowStep(3);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            if (!(Session["ResetVerified"] is bool) || !(bool)Session["ResetVerified"] || Session["ResetId"] == null)
            {
                Response.Redirect("~/ForgotPassword.aspx");
                return;
            }

            string pw = txtNewPassword.Text;
            if (!Regex.IsMatch(pw, PasswordRule) || pw != txtConfirmPassword.Text)
            {
                lblResetError.Text = "Password must meet all the requirements and both entries must match.";
                lblResetError.Visible = true;
                return;
            }

            string table = (string)Session["ResetTable"];
            string sql = table == "Employee"
                ? "UPDATE Employee SET Employee_Password = @P WHERE Employee_ID = @Id"
                : "UPDATE Customer SET Customer_Password = @P WHERE Customer_ID = @Id";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@P", pw);
                cmd.Parameters.AddWithValue("@Id", Session["ResetId"]);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            ClearReset();
            ShowStep(4);
        }
    }
}