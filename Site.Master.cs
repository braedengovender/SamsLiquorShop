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
            if (Context.Session != null && Session["UserRole"] != null)
            {
                phLoggedOut.Visible = false;
                phLoggedIn.Visible = true;

                lnkProfile.HRef = GetDashboardUrl(Session["UserRole"].ToString());

                // Managers and cashiers get no cart or wishlist icons
                phShopIcons.Visible = !CartHelper.IsStaff(Session);
            }

            if (!IsPostBack)
            {
                try { LoadNavCategories(); }
                catch { /* the Shop menu is optional; never let it crash a page */ }
            }
        }

        // Sends the profile icon to the dashboard that matches the logged-in role
        private string GetDashboardUrl(string role)
        {
            switch ((role ?? "").Trim().ToLowerInvariant())
            {
                case "customer": return "~/Customer/Dashboard.aspx";
                case "manager": return "~/Manager/Dashboard.aspx";   // adjust to your manager folder
                case "cashier": return "~/Employee/Dashboard.aspx";
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
            int cartCount = Context.Session != null ? CartHelper.GetCartCount(Session) : 0;
            litCartCount.Text = cartCount > 0
                ? "<span class='cart-count-badge'>" + cartCount + "</span>"
                : "";

            int wishCount = Context.Session != null ? CartHelper.GetWishlist(Session).Count() : 0;
            litWishCount.Text = wishCount > 0
                ? "<span class='cart-count-badge'>" + wishCount + "</span>"
                : "";

            imgCart.ImageUrl = cartCount > 0
                ? "~/Images/icons/fullcart.png"
                : "~/Images/icons/emptycart.png";

            // Name next to the profile icon. Done in PreRender so a name just changed on My Account shows straight away.
            if (Context.Session != null && Session["UserRole"] != null)
            {
                string name = GetNavName(Session["UserRole"].ToString());
                litNavName.Text = Server.HtmlEncode(name);
                lnkProfile.Attributes["aria-label"] = "My Dashboard (" + name + ")";
                lnkProfile.Attributes["title"] = "Logged in as " + name;
            }
        }

        // The logged-in person's first name, read once and kept in the session. The cache is tied to the
        // account it was read for, and the My Account pages clear "NavName" when the name is changed.
        private string GetNavName(string role)
        {
            bool customer = string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase);
            string id = Convert.ToString(Session[customer ? "CustomerID" : "EmployeeID"]);
            string key = role + ":" + id;

            if (Session["NavName"] is string cached && Convert.ToString(Session["NavNameFor"]) == key)
                return cached;

            string name = null;
            int numericId;
            if (int.TryParse(id, out numericId))
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(ConnStr))
                    using (SqlCommand cmd = new SqlCommand(customer
                        ? "SELECT Customer_FirstName FROM Customer WHERE Customer_ID = @Id"
                        : "SELECT Employee_FirstName FROM Employee WHERE Employee_ID = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", numericId);
                        conn.Open();
                        object o = cmd.ExecuteScalar();
                        if (o != null && o != DBNull.Value) name = o.ToString().Trim();
                    }
                }
                catch { /* the name is a nicety; fall back to the username below */ }
            }

            if (string.IsNullOrEmpty(name)) name = Convert.ToString(Session["UserName"]);
            Session["NavName"] = name;
            Session["NavNameFor"] = key;
            return name;
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            string noticeEmail, noticeName;
            if (SessionWatch.Unregister(Session.SessionID, out noticeEmail, out noticeName))
                OtpHelper.SendLoginNotice(noticeEmail, noticeName, "logout", Request.UserAgent, Request.UserHostAddress);

            Session.Clear();
            Response.Redirect("~/Default.aspx");
        }

        protected void Unnamed_LoggingOut(object sender, LoginCancelEventArgs e)
        {
            Context.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }
    }

}