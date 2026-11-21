using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Hosting;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace SamsLiquorShop.Helpers
{
    public static class OtpHelper
    {
        // Reads ~/OtpSecrets.config. Returns null if the file or key is missing.
        private static string Setting(string name)
        {
            try
            {
                string path = HostingEnvironment.MapPath("~/OtpSecrets.config");
                if (!File.Exists(path)) return null;
                var el = XDocument.Load(path).Descendants("add")
                    .FirstOrDefault(x => (string)x.Attribute("key") == name);
                return el == null ? null : (string)el.Attribute("value");
            }
            catch { return null; }
        }

        // True if demo mode is on, or the key is missing (so login can never break).
        public static bool DemoMode
        {
            get
            {
                string v = Setting("OtpDemoMode");
                string key = Setting("MailjetApiKey");
                return string.IsNullOrEmpty(key)
                    || key == "PASTE_API_KEY_HERE"
                    || string.Equals(v, "true", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static string GenerateCode()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] b = new byte[4];
                rng.GetBytes(b);
                uint n = BitConverter.ToUInt32(b, 0) % 1000000;
                return n.ToString("D6");
            }
        }

        private static string BuildCodeEmail(string toName, string code, string purpose)
        {
            bool isReset = (purpose ?? "").ToLowerInvariant().Contains("reset");

            string first = (toName ?? "").Trim();
            int space = first.IndexOf(' ');
            if (space > 0) first = first.Substring(0, space);
            string greeting = first.Length > 0 ? "Hi " + WebUtility.HtmlEncode(first) + "," : "Hi,";

            bool isRegister = (purpose ?? "").ToLowerInvariant().Contains("registration");

            string eyebrow, heading, intro, ignore;
            if (isRegister)
            {
                eyebrow = "Email verification";
                heading = "Verify your email address";
                intro = "Thanks for creating an account with Sam's Liquor Shop. Enter the code below on the registration page to confirm that this email address belongs to you.";
                ignore = "If you didn't try to create an account, you can ignore this email. No account will be created without this code.";
            }
            else if (isReset)
            {
                eyebrow = "Password reset";
                heading = "Reset your password";
                intro = "We received a request to reset the password on your Sam's Liquor Shop account. Enter the code below to continue.";
                ignore = "If you didn't ask to reset your password, you can ignore this email and your password will stay the same.";
            }
            else
            {
                eyebrow = "Secure login";
                heading = "Your login code";
                intro = "Use the code below to finish logging in to your Sam's Liquor Shop account.";
                ignore = "If you didn't try to log in, you can ignore this email. Your account has not been accessed.";
            }

            string html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
<title>{{HEADING}}</title>
</head>
<body style=""margin:0;padding:0;background-color:#eef3f9;font-family:Segoe UI,Helvetica,Arial,sans-serif;color:#1d1d1f;"">
<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#eef3f9;padding:32px 12px;"">
<tr><td align=""center"">
  <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""width:100%;max-width:600px;background-color:#ffffff;border-radius:10px;overflow:hidden;"">

    <tr><td style=""background-color:#0057b8;padding:32px 40px;"">
      <p style=""margin:0;font-size:13px;letter-spacing:1.5px;text-transform:uppercase;color:#b9d6f7;"">{{EYEBROW}}</p>
      <p style=""margin:6px 0 0;font-size:26px;font-weight:600;color:#ffffff;"">Sam's Liquor Shop</p>
    </td></tr>

    <tr><td style=""padding:36px 40px 8px;"">
      <p style=""margin:0 0 16px;font-size:20px;font-weight:600;color:#0057b8;"">{{HEADING}}</p>
      <p style=""margin:0 0 12px;font-size:15px;line-height:1.65;color:#3a3a3c;"">{{GREETING}}</p>
      <p style=""margin:0 0 8px;font-size:15px;line-height:1.65;color:#3a3a3c;"">{{INTRO}}</p>
    </td></tr>

    <tr><td align=""center"" style=""padding:16px 40px 8px;"">
      <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""width:100%;"">
        <tr><td align=""center"" style=""background-color:#f4f8fd;border:1px solid #dce7f5;border-radius:8px;padding:22px 16px;"">
          <p style=""margin:0 0 6px;font-size:12px;letter-spacing:1.2px;text-transform:uppercase;color:#6e6e73;"">Your code</p>
          <p style=""margin:0;font-family:Consolas,'Courier New',monospace;font-size:34px;font-weight:700;letter-spacing:10px;color:#0057b8;"">{{CODE}}</p>
        </td></tr>
      </table>
    </td></tr>

    <tr><td style=""padding:20px 40px 32px;"">
      <p style=""margin:0 0 12px;font-size:14px;line-height:1.65;color:#3a3a3c;"">This code expires in 10 minutes and can only be used once.</p>
      <p style=""margin:0;font-size:14px;line-height:1.65;color:#3a3a3c;"">{{IGNORE}}</p>
    </td></tr>

    <tr><td style=""background-color:#f4f8fd;padding:24px 40px;border-top:1px solid #dce7f5;"">
      <p style=""margin:0 0 8px;font-size:12px;line-height:1.6;color:#6e6e73;"">For your security, Sam's Liquor Shop will never ask you for this code by phone, email or message.</p>
      <p style=""margin:0 0 8px;font-size:12px;line-height:1.6;color:#6e6e73;"">Sam's Liquor Shop, 21 Coronation Road, Tongaat, Durban, 4399, KwaZulu-Natal</p>
      <p style=""margin:0;font-size:12px;line-height:1.6;color:#6e6e73;"">Not for persons under the age of 18. Please drink responsibly.</p>
    </td></tr>

  </table>
</td></tr>
</table>
</body>
</html>";

            return html
                .Replace("{{EYEBROW}}", eyebrow)
                .Replace("{{HEADING}}", heading)
                .Replace("{{GREETING}}", greeting)
                .Replace("{{INTRO}}", intro)
                .Replace("{{IGNORE}}", ignore)
                .Replace("{{CODE}}", WebUtility.HtmlEncode(code));
        }

        // Returns true if the email was sent (or demo mode is on).
        public static bool SendCode(string toEmail, string toName, string code, string purpose = "login")
        {
            if (DemoMode) return true;

            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                var payload = new
                {
                    Messages = new[]
                    {
                        new
                        {
                            From = new { Email = Setting("BrevoSenderEmail"), Name = Setting("BrevoSenderName") },
                            To = new[] { new { Email = toEmail, Name = toName } },
                            Subject = "Your Sam's Liquor Shop " + purpose + " code",
                            HTMLPart = BuildCodeEmail(toName, code, purpose)
                        }
                    }
                };

                string json = new JavaScriptSerializer().Serialize(payload);

                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string creds = Setting("MailjetApiKey") + ":" + Setting("MailjetSecretKey");
                    client.Headers[HttpRequestHeader.Authorization] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(creds));
                    client.UploadString("https://api.mailjet.com/v3.1/send", "POST", json);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        // kind: "login", "logout", "closed" or "expired". Sends in the background so pages never wait on email.
        public static void SendLoginNotice(string toEmail, string toName, string kind, string userAgent, string ip)
        {
            if (string.IsNullOrEmpty(toEmail)) return;

            string when = DateTime.Now.ToString("dddd, d MMMM yyyy 'at' HH:mm");
            string device = DescribeDevice(userAgent);

            string subject;
            switch (kind)
            {
                case "logout": subject = "You've logged out of Sam's Liquor Shop"; break;
                case "closed": subject = "You closed Sam's Liquor Shop without logging out"; break;
                case "expired": subject = "Your Sam's Liquor Shop session has ended"; break;
                default: subject = "New login to your Sam's Liquor Shop account"; break;
            }

            string html = BuildNoticeEmail(toName, kind, when, device, ip);
            System.Threading.Tasks.Task.Run(() => SendEmail(toEmail, toName, subject, html));
        }

        private static string DescribeDevice(string ua)
        {
            if (string.IsNullOrEmpty(ua)) return null;
            string browser = ua.Contains("Edg/") ? "Edge"
                : ua.Contains("OPR/") ? "Opera"
                : ua.Contains("Firefox/") ? "Firefox"
                : ua.Contains("Chrome/") ? "Chrome"
                : ua.Contains("Safari/") ? "Safari" : "Unknown browser";
            string os = ua.Contains("Windows") ? "Windows"
                : ua.Contains("Android") ? "Android"
                : (ua.Contains("iPhone") || ua.Contains("iPad")) ? "iOS"
                : ua.Contains("Mac OS X") ? "macOS"
                : ua.Contains("Linux") ? "Linux" : "an unknown device";
            return browser + " on " + os;
        }

        private static string NoticeRow(string label, string value)
        {
            return "<tr><td style='padding:6px 0;font-size:13px;color:#6e6e73;width:110px;'>" + label + "</td>" +
                   "<td style='padding:6px 0;font-size:14px;color:#1d1d1f;font-weight:600;'>" + WebUtility.HtmlEncode(value) + "</td></tr>";
        }

        private static string BuildNoticeEmail(string toName, string kind, string when, string device, string ip)
        {
            string first = (toName ?? "").Trim();
            string greeting = first.Length > 0 ? "Hi " + WebUtility.HtmlEncode(first) + "," : "Hi,";

            string eyebrow = "Account activity", heading, intro, advice;
            string unexpected = "If this wasn't you, please change your password straight away and contact us on 082 405 5932 or info@samsliquorshop.co.za.";

            switch (kind)
            {
                case "logout":
                    heading = "You've been logged out";
                    intro = "You have successfully logged out of your Sam's Liquor Shop account.";
                    advice = unexpected;
                    break;
                case "closed":
                    heading = "Browser closed without logging out";
                    intro = "We noticed that your browser or tab was closed without logging out of your Sam's Liquor Shop account. Your session will end automatically after a period of inactivity.";
                    advice = "If this wasn't you, please change your password and contact us on 082 405 5932 or info@samsliquorshop.co.za.";
                    break;
                case "expired":
                    heading = "Your session has ended";
                    intro = "You were logged out of your Sam's Liquor Shop account automatically because there was no activity for a while.";
                    advice = "Nothing more is needed. Simply log in again when you're ready.";
                    break;
                default:
                    heading = "New login to your account";
                    intro = "You have just logged in to your Sam's Liquor Shop account.";
                    advice = unexpected;
                    break;
            }

            string rows = NoticeRow("When", when);
            if (!string.IsNullOrEmpty(device)) rows += NoticeRow("Device", device);
            if (!string.IsNullOrEmpty(ip)) rows += NoticeRow("IP address", ip);

            string html = @"<!DOCTYPE html>
<html lang='en'>
<head><meta charset='utf-8' /><meta name='viewport' content='width=device-width, initial-scale=1.0' /><title>{{HEADING}}</title></head>
<body style='margin:0;padding:0;background-color:#eef3f9;font-family:Segoe UI,Helvetica,Arial,sans-serif;color:#1d1d1f;'>
<table role='presentation' width='100%' cellpadding='0' cellspacing='0' style='background-color:#eef3f9;padding:32px 12px;'>
<tr><td align='center'>
  <table role='presentation' width='600' cellpadding='0' cellspacing='0' style='width:100%;max-width:600px;background-color:#ffffff;border-radius:10px;overflow:hidden;'>
    <tr><td style='background-color:#0057b8;padding:32px 40px;'>
      <p style='margin:0;font-size:13px;letter-spacing:1.5px;text-transform:uppercase;color:#b9d6f7;'>{{EYEBROW}}</p>
      <p style='margin:6px 0 0;font-size:26px;font-weight:600;color:#ffffff;'>Sam's Liquor Shop</p>
    </td></tr>
    <tr><td style='padding:36px 40px 8px;'>
      <p style='margin:0 0 16px;font-size:20px;font-weight:600;color:#0057b8;'>{{HEADING}}</p>
      <p style='margin:0 0 12px;font-size:15px;line-height:1.65;color:#3a3a3c;'>{{GREETING}}</p>
      <p style='margin:0 0 8px;font-size:15px;line-height:1.65;color:#3a3a3c;'>{{INTRO}}</p>
    </td></tr>
    <tr><td style='padding:16px 40px 8px;'>
      <table role='presentation' cellpadding='0' cellspacing='0' style='width:100%;background-color:#f4f8fd;border:1px solid #dce7f5;border-radius:8px;'>
        <tr><td style='padding:14px 20px;'>
          <table role='presentation' cellpadding='0' cellspacing='0' style='width:100%;'>{{ROWS}}</table>
        </td></tr>
      </table>
    </td></tr>
    <tr><td style='padding:20px 40px 32px;'>
      <p style='margin:0;font-size:14px;line-height:1.65;color:#3a3a3c;'>{{ADVICE}}</p>
    </td></tr>
    <tr><td style='background-color:#f4f8fd;padding:24px 40px;border-top:1px solid #dce7f5;'>
      <p style='margin:0 0 8px;font-size:12px;line-height:1.6;color:#6e6e73;'>Sam's Liquor Shop, 21 Coronation Road, Tongaat, Durban, 4399, KwaZulu-Natal</p>
      <p style='margin:0;font-size:12px;line-height:1.6;color:#6e6e73;'>Not for persons under the age of 18. Please drink responsibly.</p>
    </td></tr>
  </table>
</td></tr>
</table>
</body>
</html>";

            return html
                .Replace("{{EYEBROW}}", eyebrow)
                .Replace("{{HEADING}}", heading)
                .Replace("{{GREETING}}", greeting)
                .Replace("{{INTRO}}", WebUtility.HtmlEncode(intro))
                .Replace("{{ROWS}}", rows)
                .Replace("{{ADVICE}}", WebUtility.HtmlEncode(advice));
        }

        // Sends any HTML email through the same Mailjet account (used for emailed receipts).
        // Unlike SendCode, returns false in demo mode, because nothing is actually sent.
        public static bool SendEmail(string toEmail, string toName, string subject, string html)
        {
            if (DemoMode) return false;

            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                var payload = new
                {
                    Messages = new[]
                    {
                        new
                        {
                            From = new { Email = Setting("BrevoSenderEmail"), Name = Setting("BrevoSenderName") },
                            To = new[] { new { Email = toEmail, Name = toName } },
                            Subject = subject,
                            HTMLPart = html
                        }
                    }
                };

                string json = new JavaScriptSerializer().Serialize(payload);

                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string creds = Setting("MailjetApiKey") + ":" + Setting("MailjetSecretKey");
                    client.Headers[HttpRequestHeader.Authorization] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(creds));
                    client.UploadString("https://api.mailjet.com/v3.1/send", "POST", json);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}