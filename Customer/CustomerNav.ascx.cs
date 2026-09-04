using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI.HtmlControls;

namespace SamsLiquorShop.Customer
{
    public partial class CustomerNav : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string currentPage = Path.GetFileName(Request.Path);

            HtmlControl active = null;
            switch (currentPage)
            {
                case "Dashboard.aspx": active = navOverview; break;
                case "Orders.aspx": active = navOrders; break;
                case "Wishlist.aspx": active = navWishlist; break;
                case "Account.aspx": active = navAccount; break;
            }

            if (active != null)
            {
                active.Attributes["class"] = "is-active";
            }
        }
    }
}