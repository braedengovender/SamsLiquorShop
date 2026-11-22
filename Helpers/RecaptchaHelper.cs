using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace SamsLiquorShop.Helpers
{
    // reCAPTCHA v2 "I'm not a robot" checkbox (Login, Register, Forgot password).
    //
    // Real keys go in Web.config <appSettings> (RecaptchaSiteKey / RecaptchaSecretKey), so everyone who
    // pulls gets them automatically. While those are blank, Google's published TEST keys are used instead:
    // they always pass and show a "for testing purposes only" note in the box.
    public static class RecaptchaHelper
    {
        // Google's official reCAPTCHA v2 test keys: https://developers.google.com/recaptcha/docs/faq
        private const string TestSiteKey = "6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI";
        private const string TestSecretKey = "6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe";

        public const string FailMessage = "Please tick \"I'm not a robot\" before continuing.";

        public static string SiteKey => Setting("RecaptchaSiteKey") ?? TestSiteKey;

        private static string SecretKey => Setting("RecaptchaSecretKey") ?? TestSecretKey;

        public static bool UsingTestKeys => Setting("RecaptchaSiteKey") == null;

        private static string Setting(string name)
        {
            string v = ConfigurationManager.AppSettings[name];
            return string.IsNullOrWhiteSpace(v) ? null : v.Trim();
        }

        // Asks Google whether the token the box put in the form ("g-recaptcha-response") is genuine.
        // Fails closed: no token, a rejected token, or Google being unreachable all count as "not verified".
        public static bool Verify(string token, string remoteIp)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                using (var client = new WebClient())
                {
                    var form = new NameValueCollection
                    {
                        { "secret", SecretKey },
                        { "response", token },
                        { "remoteip", remoteIp ?? "" }
                    };
                    byte[] reply = client.UploadValues("https://www.google.com/recaptcha/api/siteverify", "POST", form);
                    var result = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(Encoding.UTF8.GetString(reply));
                    return result != null && result.ContainsKey("success") && result["success"] is bool && (bool)result["success"];
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
