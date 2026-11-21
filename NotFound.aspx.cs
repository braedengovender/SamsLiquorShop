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
        }
    }
}