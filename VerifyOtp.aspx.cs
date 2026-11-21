using System;
using System.Web;

namespace SamsLiquorShop
{
    public partial class VerifyOtp : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["PendingRole"] == null || Session["OtpAttempts"] == null || Session["OtpExpiry"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack && Session["OtpDemoMode"] is bool && (bool)Session["OtpDemoMode"])
            {
                spanDemo.InnerText = "Demo mode: your code is " + Session["OtpCode"];
                spanDemo.Visible = true;
            }
        }

        private void ClearPending()
        {
            foreach (string k in new[] { "PendingRole", "PendingName", "PendingKey", "PendingId",
                "PendingRemember", "PendingReturnUrl", "PendingEmail", "OtpCode", "OtpExpiry",
                "OtpAttempts", "OtpDemoMode", "OtpResends", "OtpLastSent" })
                Session.Remove(k);
        }

        protected void btnResend_Click(object sender, EventArgs e)
        {
            const int MaxResends = 3;
            const int CooldownSeconds = 30;

            int resends = Session["OtpResends"] is int ? (int)Session["OtpResends"] : 0;
            if (resends >= MaxResends)
            {
                lblError.Text = "You've reached the maximum number of resends. Please go back and log in again.";
                lblError.Visible = true;
                return;
            }

            if (Session["OtpLastSent"] is DateTime)
            {
                double wait = CooldownSeconds - (DateTime.Now - (DateTime)Session["OtpLastSent"]).TotalSeconds;
                if (wait > 0)
                {
                    lblError.Text = "Please wait " + (int)Math.Ceiling(wait) + " seconds before requesting another code.";
                    lblError.Visible = true;
                    return;
                }
            }

            string email = Session["PendingEmail"] as string;
            string name = Session["PendingName"] as string;
            string code = Helpers.OtpHelper.GenerateCode();

            if (!Helpers.OtpHelper.SendCode(email, name, code))
            {
                lblError.Text = "We couldn't send your login code. Please try again.";
                lblError.Visible = true;
                return;
            }

            // The new code replaces the old one. Attempts are deliberately NOT reset.
            Session["OtpCode"] = code;
            Session["OtpExpiry"] = DateTime.Now.AddMinutes(10);
            Session["OtpLastSent"] = DateTime.Now;
            Session["OtpResends"] = resends + 1;

            txtCode.Text = "";
            lblInfo.Text = "A new code has been sent to your email.";
            lblInfo.Visible = true;

            if (Session["OtpDemoMode"] is bool && (bool)Session["OtpDemoMode"])
            {
                spanDemo.InnerText = "Demo mode: your code is " + code;
                spanDemo.Visible = true;
            }
        }

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            const int MaxAttempts = 5;

            if (DateTime.Now > (DateTime)Session["OtpExpiry"])
            {
                ClearPending();
                Response.Redirect("~/Login.aspx?otp=expired");
                return;
            }

            if (txtCode.Text.Trim() != (string)Session["OtpCode"])
            {
                int attempts = (int)Session["OtpAttempts"] + 1;
                Session["OtpAttempts"] = attempts;

                if (attempts >= MaxAttempts)
                {
                    ClearPending();
                    Response.Redirect("~/Login.aspx?otp=locked");
                    return;
                }

                int left = MaxAttempts - attempts;
                lblError.Text = "That code isn't right. You have " + left + (left == 1 ? " attempt" : " attempts") + " left.";
                lblError.Visible = true;
                return;
            }

            string role = (string)Session["PendingRole"];
            string displayName = (string)Session["PendingName"];
            Session[(string)Session["PendingKey"]] = Session["PendingId"];
            Session["UserRole"] = role;
            Session["UserName"] = displayName;
            string returnUrl = Session["PendingReturnUrl"] as string;
            bool remember = (bool)Session["PendingRemember"];
            string email = Session["PendingEmail"] as string;

            foreach (string k in new[] { "PendingRole", "PendingName", "PendingKey", "PendingId",
                "PendingRemember", "PendingReturnUrl", "PendingEmail", "OtpCode", "OtpExpiry", "OtpAttempts", "OtpDemoMode", "OtpResends", "OtpLastSent" })
                Session.Remove(k);

            if (role == "Customer") Helpers.WishlistHelper.MergeAtLogin(Session);

            // Customers, cashiers and managers all get login/logout notifications
            if (!string.IsNullOrEmpty(email))
            {
                Helpers.SessionWatch.Register(Session.SessionID, email, displayName);
                Helpers.OtpHelper.SendLoginNotice(email, displayName, "login", Request.UserAgent, Request.UserHostAddress);
            }

            if (remember)
            {
                HttpCookie rememberCookie = new HttpCookie("SamsRememberedUser", displayName);
                rememberCookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Add(rememberCookie);
            }

            System.Diagnostics.Debug.WriteLine("VERIFY sid=" + Session.SessionID + " role=" + Session["UserRole"]);

            switch (role.Trim().ToLower())
            {
                case "customer":
                    if (!string.IsNullOrEmpty(returnUrl) && !returnUrl.Contains(":") && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("\\"))
                        Response.Redirect("~/" + returnUrl.TrimStart('/'));
                    Response.Redirect("~/Customer/Dashboard.aspx");
                    break;
                case "cashier":
                    Response.Redirect("~/Employee/Dashboard.aspx");
                    break;
                case "manager":
                    Response.Redirect("~/Manager/Dashboard.aspx");
                    break;
            }
        }
    }
}