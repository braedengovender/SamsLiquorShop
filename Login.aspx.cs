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
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string usernameOrEmail = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            string role = null;
            string displayName = null;

            string connStr = ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 1. Check Customer table (username OR email, must be Active)
                string customerQuery = @"
                    SELECT Customer_Username 
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
                        }
                    }
                }

                // 2. If not a customer, check Employee table (covers Cashier + Manager)
                if (role == null)
                {
                    string employeeQuery = @"
                        SELECT Employee_Username, Employee_Role 
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
                            }
                        }
                    }
                }
            }

            if (role != null)
            {
                Session["UserRole"] = role;
                Session["UserName"] = displayName;

                if (RememberMe.Checked)
                {
                    HttpCookie rememberCookie = new HttpCookie("SamsRememberedUser", displayName);
                    rememberCookie.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Add(rememberCookie);
                }

                switch (role.Trim().ToLower())
                {
                    case "customer":
                        Response.Redirect("~/Customer/Dashboard.aspx");
                        break;
                    case "cashier":
                        Response.Redirect("~/Employee/Dashboard.aspx");
                        break;
                    case "manager":
                        Response.Redirect("~/Manager/Dashboard.aspx");
                        break;
                }
                return;
            }

            lblError.Text = "Invalid email/username or password.";
            lblError.Visible = true;
        }
    }
}