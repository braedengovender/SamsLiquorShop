using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace SamsLiquorShop
{
    public class AuthHelper
    {
        public static bool RequireRole(Page page, string requiredRole)
        {
            var session = HttpContext.Current.Session;
            if (session["UserRole"] == null || session["UserRole"].ToString() != requiredRole)
            {
                page.Response.Redirect("~/Login.aspx");
                return false;
            }
            return true;
        }
    }
}