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
                            HTMLPart = "<p>Your " + purpose + " code is:</p><h2 style='font-family:monospace;letter-spacing:4px'>"
                                        + code + "</h2><p>It expires in 10 minutes. If this wasn't you, ignore this email.</p>"
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