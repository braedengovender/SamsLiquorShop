using Microsoft.AspNet.Identity;
using SamsLiquorShop.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SamsLiquorShop
{
    public partial class SiteMaster : MasterPage
    {
        private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";
        private string _antiXsrfTokenValue;
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;
        protected void Page_Init(object sender, EventArgs e)
        {
            // The code below helps to protect against XSRF attacks
            var requestCookie = Request.Cookies[AntiXsrfTokenKey];
            Guid requestCookieGuidValue;
            if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            {
                // Use the Anti-XSRF token from the cookie
                _antiXsrfTokenValue = requestCookie.Value;
                Page.ViewStateUserKey = _antiXsrfTokenValue;
            }
            else
            {
                // Generate a new Anti-XSRF token and save to the cookie
                _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
                Page.ViewStateUserKey = _antiXsrfTokenValue;

                var responseCookie = new HttpCookie(AntiXsrfTokenKey)
                {
                    HttpOnly = true,
                    Value = _antiXsrfTokenValue
                };
                if (FormsAuthentication.RequireSSL && Request.IsSecureConnection)
                {
                    responseCookie.Secure = true;
                }
                Response.Cookies.Set(responseCookie);
            }

            Page.PreLoad += master_Page_PreLoad;
        }

        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set Anti-XSRF token
                ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;
                ViewState[AntiXsrfUserNameKey] = Context.User.Identity.Name ?? String.Empty;
            }
            else
            {
                // Validate the Anti-XSRF token
                if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
                    || (string)ViewState[AntiXsrfUserNameKey] != (Context.User.Identity.Name ?? String.Empty))
                {
                    throw new InvalidOperationException("Validation of Anti-XSRF token failed.");
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserRole"] != null)
            {
                phLoggedOut.Visible = false;
                phLoggedIn.Visible = true;

                lnkProfile.HRef = GetDashboardUrl(Session["UserRole"].ToString());
            }

            if (!IsPostBack)
            {
                LoadNavCategories();
            }
        }

        // Sends the profile icon to the dashboard that matches the logged-in role
        private string GetDashboardUrl(string role)
        {
            switch ((role ?? "").Trim().ToLowerInvariant())
            {
                case "customer": return "~/Customer/Dashboard.aspx";
                case "manager": return "~/Manager/Dashboard.aspx";   // adjust to your manager folder
                case "cashier": return "~/Cashier/Dashboard.aspx";   // adjust to your cashier folder
                default: return "~/Account/Profile.aspx";
            }
        }

        private void LoadNavCategories()
        {
            string query = "SELECT Category_ID, Category_Name FROM Category ORDER BY Category_Name";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptNavCategories.DataSource = dt;
                rptNavCategories.DataBind();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            int cartCount = CartHelper.GetCartCount(Session);
            litCartCount.Text = cartCount > 0
                ? "<span class='cart-count-badge'>" + cartCount + "</span>"
                : "";

            int wishCount = CartHelper.GetWishlist(Session).Count();
            litWishCount.Text = wishCount > 0
                ? "<span class='cart-count-badge'>" + wishCount + "</span>"
                : "";

            imgCart.ImageUrl = cartCount > 0
                ? "~/Images/icons/fullcart.png"
                : "~/Images/icons/emptycart.png";
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Default.aspx");
        }

        protected void Unnamed_LoggingOut(object sender, LoginCancelEventArgs e)
        {
            Context.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }
    }

}