using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SamsLiquorShop
{
    public partial class Contact : Page
    {
        // e.g. https://formspree.io/f/xyzabcde, or "" when no form ID is set in Web.config.
        protected string FormspreeEndpoint
        {
            get
            {
                string id = (System.Configuration.ConfigurationManager.AppSettings["FormspreeFormId"] ?? "").Trim();
                return id.Length == 0 ? "" : "https://formspree.io/f/" + HttpUtility.UrlPathEncode(id);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}