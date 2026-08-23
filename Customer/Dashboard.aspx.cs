using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SamsLiquorShop.Customer
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Customer")) return;

            litGreeting.Text = "Hello " + Server.HtmlEncode(Session["UserName"].ToString())
                + ", role: " + Server.HtmlEncode(Session["UserRole"].ToString());
        }
    }
}