using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SamsLiquorShop
{
    public partial class Rewards : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Matches the login-state pattern used in Site.Master.cs
            if (Session["UserRole"] != null)
            {
                phSignedOut.Visible = false;
                phSignedIn.Visible = true;
            }
        }
    }
}