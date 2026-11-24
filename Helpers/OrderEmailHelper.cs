using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SamsLiquorShop.Helpers
{
    /// <summary>Emails the customer when staff move their online order along. Never throws and never blocks.</summary>
    public static class OrderEmailHelper
    {
        private static string ConnStr
        {
            get { return ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString; }
        }

        public static void Notify(int onlineOrderId, string status, string note)
        {
            try
            {
                status = (status ?? "").Trim();

                string email = null, first = null, method = null, code = null;
                int saleId = 0;
                DateTime? slot = null;

                using (var conn = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand(@"
                    SELECT OnlineOrder_EmailAddress, OnlineOrder_FirstName, OnlineOrder_Method,
                           COALESCE(OnlineOrder_EstimatedDateTime, OnlineOrder_CollectionSlot), OnlineOrder_CollectionCode, Sale_ID
                    FROM OnlineOrder WHERE OnlineOrder_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", onlineOrderId);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return;
                        email = Convert.ToString(r[0]).Trim();
                        first = Convert.ToString(r[1]).Trim();
                        method = Convert.ToString(r[2]).Trim();
                        slot = r.IsDBNull(3) ? (DateTime?)null : Convert.ToDateTime(r[3]);
                        code = Convert.ToString(r[4]).Trim();
                        saleId = Convert.ToInt32(r[5]);
                    }
                }
                if (email.Length == 0 || email.IndexOf('@') < 0) return;

                bool delivery = string.Equals(method, OrderHelper.MethodDelivery, StringComparison.OrdinalIgnoreCase);
                string invoice = "INV-" + saleId.ToString("000000");
                string heading, intro, extra = "";
                string when = SlotText(delivery, slot);

                switch (status)
                {
                    case OrderHelper.StPacking:
                        heading = "We're packing your order";
                        intro = "Good news - our team has started packing order " + invoice + ".";
                        break;
                    case OrderHelper.StOut:
                        heading = "Your order is on its way";
                        intro = "Order " + invoice + " is out for delivery. Please have your collection code and ID ready - the driver will ask for the code and check that you are 18 or older.";
                        extra = "Delivery window: <strong>" + WebUtility.HtmlEncode(when) + "</strong>";
                        if (!string.IsNullOrWhiteSpace(code))
                            extra = "Collection code: <strong style=\"font-family:Consolas,'Courier New',monospace;font-size:22px;letter-spacing:4px;color:#0057b8;\">"
                                  + WebUtility.HtmlEncode(code) + "</strong><br/>" + extra;
                        break;
                    case OrderHelper.StReady:
                        heading = "Your order is ready for collection";
                        intro = "Order " + invoice + " is ready at our Tongaat store (21 Coronation Road, Mithangar, Tongaat, 4399). Bring your collection code and ID - we check that you are 18 or older. We hold orders for 48 hours.";
                        extra = "Collection code: <strong style=\"font-family:Consolas,'Courier New',monospace;font-size:22px;letter-spacing:4px;color:#0057b8;\">"
                              + WebUtility.HtmlEncode(code) + "</strong><br/>Your slot: " + WebUtility.HtmlEncode(when);
                        break;
                    case OrderHelper.StDelivered:
                        heading = "Your order was delivered";
                        intro = "Order " + invoice + " has been delivered. Thank you for shopping with Sam's Liquor Shop!";
                        break;
                    case OrderHelper.StCollected:
                        heading = "Thanks for collecting your order";
                        intro = "Order " + invoice + " has been collected. Thank you for shopping with Sam's Liquor Shop!";
                        break;
                    case OrderHelper.StCancelled:
                        heading = "Your order was cancelled";
                        intro = "Order " + invoice + " was cancelled. Any loyalty points you used have been returned to your account and any card payment will be refunded.";
                        if (!string.IsNullOrWhiteSpace(note)) extra = "Reason: " + WebUtility.HtmlEncode(note.Trim());
                        break;
                    case OrderHelper.StExpired:
                        heading = "Your order was not collected";
                        intro = "Order " + invoice + " was not collected within 48 hours, so it has been cancelled. Any loyalty points you used have been returned and any card payment will be refunded.";
                        break;
                    default:
                        return;   // "Placed" is covered by the receipt email
                }

                string html = BuildHtml(heading, first, intro, extra);
                string subject = heading + " - " + invoice;

                Task.Run(() =>
                {
                    try
                    {
                        if (!OtpHelper.SendEmail(email, first, subject, html))
                            System.Diagnostics.Trace.TraceWarning("Status email for order " + onlineOrderId + " was not sent (demo mode or mail error).");
                    }
                    catch (Exception ex) { System.Diagnostics.Trace.TraceError("Status email: " + ex); }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("OrderEmailHelper.Notify: " + ex);
            }
        }

        private static string SlotText(bool delivery, DateTime? slot)
        {
            if (!slot.HasValue) return "to be confirmed";
            DateTime s = slot.Value;
            string day = s.ToString("ddd d MMM yyyy", CultureInfo.InvariantCulture);
            if (delivery)
            {
                var w = SlotHelper.GetDeliveryWindows(s.Date).FirstOrDefault(x => x.Start == s);
                return day + ", " + (w != null ? w.Label : s.ToString("HH:mm", CultureInfo.InvariantCulture));
            }
            return day + ", " + s.ToString("HH:mm", CultureInfo.InvariantCulture) + " - " + s.AddHours(1).ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        // intro is plain text; extra is HTML (encode anything user-supplied before passing it in)
        internal static string BuildHtml(string heading, string first, string intro, string extra, string eyebrow = "Order update")
        {
            string hi = string.IsNullOrWhiteSpace(first) ? "Hi," : "Hi " + WebUtility.HtmlEncode(first) + ",";
            return "<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\" /><title>" + WebUtility.HtmlEncode(heading) + "</title></head>"
                + "<body style=\"margin:0;padding:0;background:#eef3f9;font-family:Segoe UI,Helvetica,Arial,sans-serif;color:#1d1d1f;\">"
                + "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#eef3f9;padding:32px 12px;\"><tr><td align=\"center\">"
                + "<table role=\"presentation\" width=\"600\" cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;max-width:600px;background:#ffffff;border-radius:10px;overflow:hidden;\">"
                + "<tr><td style=\"background:#0057b8;padding:28px 36px;\"><p style=\"margin:0;font-size:13px;letter-spacing:1.5px;text-transform:uppercase;color:#b9d6f7;\">" + WebUtility.HtmlEncode(eyebrow) + "</p>"
                + "<p style=\"margin:6px 0 0;font-size:24px;font-weight:600;color:#ffffff;\">Sam's Liquor Shop</p></td></tr>"
                + "<tr><td style=\"padding:32px 36px 8px;\"><p style=\"margin:0 0 14px;font-size:20px;font-weight:600;color:#0057b8;\">" + WebUtility.HtmlEncode(heading) + "</p>"
                + "<p style=\"margin:0 0 12px;font-size:15px;line-height:1.65;color:#3a3a3c;\">" + hi + "</p>"
                + "<p style=\"margin:0 0 12px;font-size:15px;line-height:1.65;color:#3a3a3c;\">" + WebUtility.HtmlEncode(intro) + "</p></td></tr>"
                + (extra.Length > 0 ? "<tr><td style=\"padding:4px 36px 8px;\"><div style=\"background:#f4f8fd;border:1px solid #dce7f5;border-radius:8px;padding:16px 18px;font-size:15px;line-height:1.7;color:#1d1d1f;\">" + extra + "</div></td></tr>" : "")
                + "<tr><td style=\"padding:20px 36px 28px;\"><p style=\"margin:0;font-size:14px;line-height:1.65;color:#3a3a3c;\">Questions? Call us on 082 405 5932 or email info@samsliquorshop.co.za.</p></td></tr>"
                + "<tr><td style=\"background:#f4f8fd;padding:20px 36px;border-top:1px solid #dce7f5;\"><p style=\"margin:0;font-size:12px;line-height:1.6;color:#6e6e73;\">Sam's Liquor Shop, 21 Coronation Road, Mithangar, Tongaat, 4399. Not for persons under the age of 18. Please drink responsibly.</p></td></tr>"
                + "</table></td></tr></table></body></html>";
        }
    }
}