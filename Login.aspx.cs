using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SamsLiquorShop
{
    public partial class Login : System.Web.UI.Page
    {
        // TEMPORARY — replace with real database-backed auth later
        private static readonly (string Username, string Password, string Role)[] TestAccounts = new[]
        {
            ("customer", "customer123", "Customer"),
            ("employee", "employee123", "Employee"),
            ("manager", "manager123", "Manager")
        };

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            foreach (var account in TestAccounts)
            {
                if (account.Username.Equals(username, StringComparison.OrdinalIgnoreCase)
                    && account.Password == password)
                {
                    Session["UserRole"] = account.Role;
                    Session["UserName"] = account.Username;

                    if (RememberMe.Checked)
                    {
                        HttpCookie rememberCookie = new HttpCookie("SamsRememberedUser", account.Username);
                        rememberCookie.Expires = DateTime.Now.AddDays(30);
                        Response.Cookies.Add(rememberCookie);
                    }

                    switch (account.Role)
                    {
                        case "Customer":
                            Response.Redirect("~/Customer/Dashboard.aspx");
                            break;
                        case "Employee":
                            Response.Redirect("~/Employee/Dashboard.aspx");
                            break;
                        case "Manager":
                            Response.Redirect("~/Manager/Dashboard.aspx");
                            break;
                    }
                    return;
                }
            }

            lblError.Text = "Invalid email/username or password.";
            lblError.Visible = true;
        }
    }
}