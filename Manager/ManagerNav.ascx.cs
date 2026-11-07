using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace SamsLiquorShop.Manager
{
    public partial class ManagerNav : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Highlight the current page in the sidebar
            // The site uses extensionless URLs (/Manager/Customers), so add .aspx back before matching
            string currentPage = Path.GetFileNameWithoutExtension(Request.Path) + ".aspx";

            HtmlControl active = null;
            switch (currentPage)
            {
                case "Dashboard.aspx": active = navOverview; break;
                case "Sales.aspx": active = navOrders; break;
                case "Products.aspx": active = navProducts; break;
                case "Employees.aspx": active = navEmployees; break;
                case "Customers.aspx": active = navCustomers; break;
                case "Discounts.aspx": active = navDiscounts; break;
                case "Account.aspx": active = navAccount; break;
            }

            if (active != null)
            {
                active.Attributes["class"] = "is-active";
            }
        }
    }
}