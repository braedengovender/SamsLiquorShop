using System;

namespace SamsLiquorShop.Manager
{
    // Manage Sales: all POS sales, with details and CSV export. The list itself lives in Controls/SalesBrowser.ascx,
    // which the cashier Orders page (Employee/Orders.aspx) shares.
    public partial class Sales : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AuthHelper.RequireRole(this, "Manager");
        }
    }
}
