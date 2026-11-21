using System;

namespace SamsLiquorShop
{
    // Deliberately standalone: no master page, no database, no session, so it can't fail.
    public partial class ErrorPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;
        }
    }
}