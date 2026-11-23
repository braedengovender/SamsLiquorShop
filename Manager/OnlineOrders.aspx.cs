using System;

namespace SamsLiquorShop.Manager
{
    public partial class OnlineOrders : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AuthHelper.RequireRole(this, "Manager");
        }
    }
}