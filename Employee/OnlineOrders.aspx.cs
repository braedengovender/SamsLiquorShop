using System;

namespace SamsLiquorShop.Employee
{
    public partial class OnlineOrders : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AuthHelper.RequireRole(this, "Cashier");
        }
    }
}