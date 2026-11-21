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

        protected void btnVerify_Click(object sender, EventArgs e)
        {
            int attempts = (int)Session["OtpAttempts"] + 1;
            Session["OtpAttempts"] = attempts;

            if (DateTime.Now > (DateTime)Session["OtpExpiry"] || attempts > 5)
            {
                Session.Remove("PendingRole");
                Response.Redirect("~/Login.aspx");
            }

            if (txtCode.Text.Trim() != (string)Session["OtpCode"])
            {
                lblError.Text = "That code isn't right. Please try again.";
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

            foreach (string k in new[] { "PendingRole", "PendingName", "PendingKey", "PendingId",
                "PendingRemember", "PendingReturnUrl", "OtpCode", "OtpExpiry", "OtpAttempts", "OtpDemoMode" })
                Session.Remove(k);

            if (role == "Customer") Helpers.WishlistHelper.MergeAtLogin(Session);

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