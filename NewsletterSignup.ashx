<%@ WebHandler Language="C#" Class="SamsLiquorShop.NewsletterSignup" %>

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using SamsLiquorShop.Helpers;

namespace SamsLiquorShop
{
    public class NewsletterSignup : IHttpHandler
    {
        private static readonly Regex EmailPattern = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            if (context.Request.HttpMethod != "POST")
            {
                Respond(context, false, "Invalid request.");
                return;
            }

            string first = (context.Request.Form["firstName"] ?? "").Trim();
            string last = (context.Request.Form["lastName"] ?? "").Trim();
            string email = (context.Request.Form["email"] ?? "").Trim();

            if (first.Length == 0 || first.Length > 50 || last.Length == 0 || last.Length > 50)
            {
                Respond(context, false, "Please enter your first and last name.");
                return;
            }
            if (email.Length > 150 || !EmailPattern.IsMatch(email))
            {
                Respond(context, false, "Please enter a valid email address.");
                return;
            }

            // Stops the same address being emailed repeatedly (double clicks, or someone abusing the form)
            string cacheKey = "nl_" + email.ToLowerInvariant();
            if (HttpRuntime.Cache[cacheKey] != null)
            {
                Respond(context, true, "Already signed up.");
                return;
            }

            string baseUrl = context.Request.Url.GetLeftPart(UriPartial.Authority)
                             + HttpRuntime.AppDomainAppVirtualPath.TrimEnd('/');

            bool sent = OtpHelper.SendEmail(
                email,
                first + " " + last,
                "You're subscribed to the Sam's Liquor Shop newsletter",
                BuildEmail(first, email, baseUrl));

            if (!sent)
            {
                Respond(context, false, "We couldn't send your confirmation email. Please try again shortly.");
                return;
            }

            HttpRuntime.Cache.Insert(cacheKey, true, null, DateTime.UtcNow.AddMinutes(1), System.Web.Caching.Cache.NoSlidingExpiration);
            Respond(context, true, "Subscribed.");
        }

        private static void Respond(HttpContext context, bool ok, string message)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(new { ok = ok, message = message }));
        }

        private static string BuildEmail(string firstName, string email, string baseUrl)
        {
            string html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
<title>Newsletter subscription confirmed</title>
</head>
<body style=""margin:0;padding:0;background-color:#eef3f9;font-family:Segoe UI,Helvetica,Arial,sans-serif;color:#1d1d1f;"">
<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#eef3f9;padding:32px 12px;"">
<tr><td align=""center"">
  <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""width:100%;max-width:600px;background-color:#ffffff;border-radius:10px;overflow:hidden;"">

    <tr><td style=""background-color:#0057b8;padding:32px 40px;"">
      <p style=""margin:0;font-size:13px;letter-spacing:1.5px;text-transform:uppercase;color:#b9d6f7;"">Newsletter</p>
      <p style=""margin:6px 0 0;font-size:26px;font-weight:600;color:#ffffff;"">Sam's Liquor Shop</p>
    </td></tr>

    <tr><td style=""padding:36px 40px 8px;"">
      <p style=""margin:0 0 16px;font-size:20px;font-weight:600;color:#0057b8;"">Hi {{FIRST}}, you're subscribed.</p>
      <p style=""margin:0 0 16px;font-size:15px;line-height:1.65;color:#3a3a3c;"">Thanks for signing up to the Sam's Liquor Shop newsletter. We'll send updates to <strong>{{EMAIL}}</strong> every now and then, and we'll keep them short.</p>
      <p style=""margin:0 0 12px;font-size:15px;line-height:1.65;color:#3a3a3c;"">Here is what you can expect from us:</p>
    </td></tr>

    <tr><td style=""padding:0 40px 8px;"">
      <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
        <tr><td style=""padding:12px 16px;border-left:3px solid #0a84ff;background-color:#f4f8fd;font-size:14px;line-height:1.5;color:#3a3a3c;""><strong style=""color:#0057b8;"">New arrivals.</strong> A heads-up when new spirits, wines and beers hit the shelves.</td></tr>
        <tr><td style=""height:8px;font-size:0;line-height:0;"">&nbsp;</td></tr>
        <tr><td style=""padding:12px 16px;border-left:3px solid #0a84ff;background-color:#f4f8fd;font-size:14px;line-height:1.5;color:#3a3a3c;""><strong style=""color:#0057b8;"">Tastings and events.</strong> Dates and details for in-store tastings.</td></tr>
        <tr><td style=""height:8px;font-size:0;line-height:0;"">&nbsp;</td></tr>
        <tr><td style=""padding:12px 16px;border-left:3px solid #0a84ff;background-color:#f4f8fd;font-size:14px;line-height:1.5;color:#3a3a3c;""><strong style=""color:#0057b8;"">Subscriber offers.</strong> Specials and discounts that we share with our mailing list first.</td></tr>
      </table>
    </td></tr>

    <tr><td align=""center"" style=""padding:28px 40px 8px;"">
      <a href=""{{BASE}}/Shop.aspx"" style=""display:inline-block;background-color:#0a84ff;color:#ffffff;text-decoration:none;font-size:15px;font-weight:600;padding:13px 32px;border-radius:6px;"">Browse the shop</a>
    </td></tr>

    <tr><td style=""padding:24px 40px 32px;"">
      <p style=""margin:0;font-size:14px;line-height:1.65;color:#3a3a3c;"">You can unsubscribe at any time using the link at the bottom of this email, or in any newsletter we send you.</p>
    </td></tr>

    <tr><td style=""background-color:#f4f8fd;padding:24px 40px;border-top:1px solid #dce7f5;"">
      <p style=""margin:0 0 8px;font-size:12px;line-height:1.6;color:#6e6e73;"">You received this email because {{EMAIL}} was used to sign up for the Sam's Liquor Shop newsletter. If this wasn't you, you can ignore this email or <a href=""#"" style=""color:#0057b8;"">unsubscribe</a>.</p>
      <p style=""margin:0 0 8px;font-size:12px;line-height:1.6;color:#6e6e73;"">Sam's Liquor Shop, 21 Coronation Road, Tongaat, Durban, 4399, KwaZulu-Natal</p>
      <p style=""margin:0;font-size:12px;line-height:1.6;color:#6e6e73;"">Not for persons under the age of 18. Please drink responsibly.</p>
    </td></tr>

  </table>
</td></tr>
</table>
</body>
</html>";

            return html
                .Replace("{{FIRST}}", HttpUtility.HtmlEncode(firstName))
                .Replace("{{EMAIL}}", HttpUtility.HtmlEncode(email))
                .Replace("{{BASE}}", baseUrl);
        }
    }
}