using System;

namespace SamsLiquorShop
{
    public partial class NotFound : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Tell browsers and search engines this really is a 404, and stop IIS replacing the page
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;

            // Logged in: offer a way back to the right dashboard
            string role = Context.Session != null ? Convert.ToString(Session["UserRole"]).Trim().ToLowerInvariant() : "";
            string dash = role == "customer" ? "~/Customer/Dashboard.aspx"
                        : role == "manager" ? "~/Manager/Dashboard.aspx"
                        : role == "cashier" ? "~/Employee/Dashboard.aspx"
                        : null;
            if (dash != null)
            {
                lnkDashboard.NavigateUrl = dash;
                lnkDashboard.Visible = true;
            }
        }
    }
}
