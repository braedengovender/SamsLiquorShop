using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SamsLiquorShop
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Writing to Session forces ASP.NET to issue the session cookie now,
            // so the session is stable by the time the login button is clicked.
            if (!IsPostBack) Session["SessionInit"] = true;
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string usernameOrEmail = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            string role = null;
            string displayName = null;
            object pendingId = null;
            string pendingEmail = null;
            string pendingKey = null;

            string connStr = ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 1. Check Customer table (username OR email, must be Active)
                string customerQuery = @"
                    SELECT Customer_ID, Customer_Username, Customer_EmailAddress
                    FROM Customer 
                    WHERE (Customer_Username = @Login OR Customer_EmailAddress = @Login) 
                      AND Customer_Password = @Password 
                      AND Customer_Status = 'Active'";

                using (SqlCommand cmd = new SqlCommand(customerQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Login", usernameOrEmail);
                    cmd.Parameters.AddWithValue("@Password", password);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            role = "Customer";
                            displayName = reader["Customer_Username"].ToString();
                            pendingId = reader["Customer_ID"];
                            pendingEmail = reader["Customer_EmailAddress"].ToString();
                            pendingKey = "CustomerID";
                        }
                    }
                }

                // 2. If not a customer, check Employee table (covers Cashier + Manager)
                if (role == null)
                {
                    string employeeQuery = @"
                        SELECT Employee_ID, Employee_Username, Employee_Role, Employee_EmailAddress
                        FROM Employee 
                        WHERE (Employee_Username = @Login OR Employee_EmailAddress = @Login) 
                          AND Employee_Password = @Password 
                          AND Employee_Status = 'Active'";

                    using (SqlCommand cmd = new SqlCommand(employeeQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Login", usernameOrEmail);
                        cmd.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                role = reader["Employee_Role"].ToString().Trim();
                                displayName = reader["Employee_Username"].ToString();
                                pendingId = reader["Employee_ID"];
                                pendingEmail = reader["Employee_EmailAddress"].ToString();
                                pendingKey = "EmployeeID";
                            }
                        }
                    }
                }
            }

            if (role != null)
            {
                string code = Helpers.OtpHelper.GenerateCode();
                Session["PendingRole"] = role;
                Session["PendingName"] = displayName;
                Session["PendingKey"] = pendingKey;
                Session["PendingId"] = pendingId;
                Session["PendingRemember"] = RememberMe.Checked;
                Session["PendingReturnUrl"] = Request.QueryString["returnUrl"];
                Session["OtpCode"] = code;
                Session["OtpExpiry"] = DateTime.Now.AddMinutes(10);
                Session["OtpAttempts"] = 0;
                Session["OtpDemoMode"] = Helpers.OtpHelper.DemoMode;

                if (!Helpers.OtpHelper.SendCode(pendingEmail, displayName, code))
                {
                    lblError.Text = "We couldn't send your login code. Please try again.";
                    lblError.Visible = true;
                    return;
                }
                Response.Redirect("~/VerifyOtp.aspx");
                return;
            }

            lblError.Text = "Invalid email/username or password.";
            lblError.Visible = true;
        }
    }
}