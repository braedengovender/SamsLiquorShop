using System;

namespace SamsLiquorShop.Employee
{
    // Cashier Orders: read-only sales list, defaulting to the cashier's own sales.
    // The list itself lives in Controls/SalesBrowser.ascx, shared with Manager/Sales.aspx.
    public partial class Orders : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AuthHelper.RequireRole(this, "Cashier");
        }
    }
}
