using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SamsLiquorShop.Helpers
{
    /// <summary>
    /// Welcome email for new customers and "new online order" email for managers.
    /// Both send in the background and never throw, so a mail problem can't break registration or checkout.
    /// </summary>
    public static class NotificationEmailHelper
    {
        private static string ConnStr
        {
            get { return ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString; }
        }

        // ------------------------------------------------------------------ welcome (new customer)

        /// <summary>Emails a new customer the promo codes they qualify for (WELCOME20 and, for .ac.za emails, the student code).</summary>
        public static void SendWelcome(string email, string firstName, List<PromoHelper.AvailableOffer> offers)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || email.IndexOf('@') < 0) return;
                offers = offers ?? new List<PromoHelper.AvailableOffer>();

                var welcome = offers.FirstOrDefault(o => IsRule(o, "FirstOrder"));
                string intro = welcome != null
                    ? "Thanks for joining Sam's Liquor Shop. As a welcome gift, here's " + welcome.Description
                      + " your first online order. Enter the code below at checkout."
                    : "Thanks for joining Sam's Liquor Shop. Your account is ready, so you can start shopping straight away.";

                var extra = new StringBuilder();
                foreach (var o in OrderOffers(offers))
                    extra.Append(OfferBlock(o));
                if (extra.Length > 0)
                    extra.Append("<p style=\"margin:10px 0 0;font-size:13px;color:#6e6e73;\">Each code can be used once. You can also see your codes any time on your account dashboard.</p>");

                string html = OrderEmailHelper.BuildHtml("Welcome to Sam's Liquor Shop", firstName, intro, extra.ToString(), "Welcome");
                string subject = welcome != null
                    ? "Welcome to Sam's Liquor Shop - " + welcome.Description + " your first order"
                    : "Welcome to Sam's Liquor Shop";

                Task.Run(() =>
                {
                    try
                    {
                        if (!OtpHelper.SendEmail(email, firstName, subject, html))
                            System.Diagnostics.Trace.TraceWarning("Welcome email was not sent (demo mode or mail error).");
                    }
                    catch (Exception ex) { System.Diagnostics.Trace.TraceError("Welcome email: " + ex); }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("NotificationEmailHelper.SendWelcome: " + ex);
            }
        }

        /// <summary>Welcome code first, then student, then everything else.</summary>
        public static IEnumerable<PromoHelper.AvailableOffer> OrderOffers(IEnumerable<PromoHelper.AvailableOffer> offers)
        {
            return offers.OrderBy(o => IsRule(o, "FirstOrder") ? 0 : IsRule(o, "Student") ? 1 : 2);
        }

        private static bool IsRule(PromoHelper.AvailableOffer o, string rule)
        {
            return string.Equals(o.Rule, rule, StringComparison.OrdinalIgnoreCase);
        }

        private static string OfferBlock(PromoHelper.AvailableOffer o)
        {
            string conditions = IsRule(o, "FirstOrder") ? "First order only"
                : IsRule(o, "Student") ? "For students (.ac.za email)"
                : "";
            if (o.MinSpend.HasValue && o.MinSpend.Value > 0)
                conditions += (conditions.Length > 0 ? " &middot; " : "") + "Min. spend R" + o.MinSpend.Value.ToString("N2");
            conditions += (conditions.Length > 0 ? " &middot; " : "") + "Valid until " + o.EndDate.ToString("d MMM yyyy", CultureInfo.InvariantCulture);

            return "<div style=\"margin:0 0 10px;padding:12px 14px;background:#ffffff;border:1px dashed #0057b8;border-radius:8px;\">"
                 + "<div style=\"font-family:Consolas,'Courier New',monospace;font-size:20px;letter-spacing:3px;color:#0057b8;font-weight:700;\">" + WebUtility.HtmlEncode(o.Code) + "</div>"
                 + "<div style=\"font-size:15px;font-weight:600;margin-top:4px;\">" + WebUtility.HtmlEncode(o.Description) + (o.Name.Length > 0 ? " - " + WebUtility.HtmlEncode(o.Name) : "") + "</div>"
                 + "<div style=\"font-size:13px;color:#6e6e73;margin-top:2px;\">" + conditions + "</div>"
                 + "</div>";
        }

        // ------------------------------------------------------------------ new online order (managers)

        /// <summary>
        /// Emails every active manager a staff-style summary of a new online order: what to pack, who it's for,
        /// when it's due, how it was paid, and a link to Manage Online Orders. Call after the order is committed.
        /// </summary>
        public static void NotifyManagersNewOrder(int onlineOrderId)
        {
            try
            {
                string link = ManagerOrdersUrl();   // needs HttpContext, so read it before going to the background

                int saleId = 0;
                var managers = new List<Tuple<string, string>>();

                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT Sale_ID FROM OnlineOrder WHERE OnlineOrder_ID = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", onlineOrderId);
                        object v = cmd.ExecuteScalar();
                        if (v == null || v == DBNull.Value) return;
                        saleId = Convert.ToInt32(v);
                    }

                    using (var cmd = new SqlCommand(@"
                        SELECT Employee_EmailAddress, Employee_FirstName FROM Employee
                        WHERE Employee_Role = 'Manager' AND Employee_Status = 'Active'
                          AND Employee_EmailAddress IS NOT NULL AND Employee_EmailAddress LIKE '%@%'", conn))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            managers.Add(Tuple.Create(Convert.ToString(r[0]).Trim(), Convert.ToString(r[1]).Trim()));
                    }
                }
                if (managers.Count == 0) return;

                OnlineReceipt o = OnlineReceiptHelper.Load(saleId, null);
                if (o == null) return;

                string invoice = "INV-" + o.SaleId.ToString("000000");
                string subject = "New online order " + invoice + " - " + (o.IsDelivery ? "Delivery" : "Collection")
                               + ", R" + Money(o.Total) + " (" + (o.FirstName + " " + o.LastName).Trim() + ")";
                string html = BuildStaffOrderHtml(o, invoice, link);

                Task.Run(() =>
                {
                    foreach (var m in managers)
                    {
                        try
                        {
                            if (!OtpHelper.SendEmail(m.Item1, m.Item2, subject, html))
                                System.Diagnostics.Trace.TraceWarning("New-order email for " + invoice + " was not sent (demo mode or mail error).");
                        }
                        catch (Exception ex) { System.Diagnostics.Trace.TraceError("New-order email: " + ex); }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("NotificationEmailHelper.NotifyManagersNewOrder: " + ex);
            }
        }

        private static string Money(decimal value)
        {
            return value.ToString("N2", CultureInfo.InvariantCulture);
        }

        // Internal staff email: dark header, order facts up front, then a packing list. Inline styles only (email clients).
        private static string BuildStaffOrderHtml(OnlineReceipt o, string invoice, string link)
        {
            Func<string, string> h = WebUtility.HtmlEncode;
            const string label = "padding:6px 0;width:150px;vertical-align:top;font-size:13px;color:#6e6e73;";
            const string value = "padding:6px 0;vertical-align:top;font-size:14px;color:#1d1d1f;";
            const string th = "padding:8px 10px;background:#f2f4f7;font-size:12px;text-transform:uppercase;letter-spacing:.5px;color:#6e6e73;text-align:left;";
            const string td = "padding:8px 10px;border-bottom:1px solid #eef1f6;font-size:14px;color:#1d1d1f;";

            // v is already HTML (encode anything user-supplied first)
            Func<string, string, string> row = (l, v) =>
                "<tr><td style=\"" + label + "\">" + h(l) + "</td><td style=\"" + value + "\">" + v + "</td></tr>";

            string method = o.IsDelivery ? "Delivery" : "Collection";
            string badgeColour = o.IsDelivery ? "#0057b8" : "#a67f2e";
            int itemCount = o.Lines.Sum(l => l.Quantity);

            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\" /><title>New order ").Append(h(invoice)).Append("</title></head>");
            sb.Append("<body style=\"margin:0;padding:0;background:#f2f4f7;font-family:Segoe UI,Helvetica,Arial,sans-serif;color:#1d1d1f;\">");
            sb.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#f2f4f7;padding:24px 12px;\"><tr><td align=\"center\">");
            sb.Append("<table role=\"presentation\" width=\"640\" cellpadding=\"0\" cellspacing=\"0\" style=\"width:100%;max-width:640px;background:#ffffff;border:1px solid #e1e5ec;border-radius:8px;overflow:hidden;\">");

            // Header
            sb.Append("<tr><td style=\"background:#1d2733;padding:18px 28px;\">")
              .Append("<p style=\"margin:0;font-size:12px;letter-spacing:1.5px;text-transform:uppercase;color:#9fb0c4;\">Staff notification &middot; Sam&#39;s Liquor Shop</p>")
              .Append("<p style=\"margin:6px 0 0;font-size:22px;font-weight:600;color:#ffffff;\">New online order ").Append(h(invoice)).Append("</p>")
              .Append("</td></tr>");

            // Summary: method badge, total, item count, what to do next
            sb.Append("<tr><td style=\"padding:18px 28px 6px;\">")
              .Append("<span style=\"display:inline-block;background:").Append(badgeColour).Append(";color:#ffffff;font-size:12px;font-weight:700;letter-spacing:.5px;text-transform:uppercase;padding:4px 10px;border-radius:4px;\">").Append(method).Append("</span>")
              .Append("<span style=\"font-size:22px;font-weight:700;vertical-align:middle;margin-left:10px;\">R").Append(Money(o.Total)).Append("</span>")
              .Append("<span style=\"font-size:14px;color:#6e6e73;vertical-align:middle;margin-left:8px;\">").Append(itemCount).Append(itemCount == 1 ? " item" : " items").Append("</span>")
              .Append("<p style=\"margin:12px 0 0;padding:10px 12px;background:#fff7e6;border-left:4px solid #c9a24b;font-size:14px;\"><strong>Action needed:</strong> pack this order and mark it ")
              .Append(o.IsDelivery ? "&quot;Out for delivery&quot; before the delivery window." : "&quot;Ready for collection&quot; before the collection slot.")
              .Append("</p></td></tr>");

            // Order details
            sb.Append("<tr><td style=\"padding:10px 28px;\"><table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\">");
            sb.Append(row("Placed", h(o.PlacedAt.ToString("ddd d MMM yyyy, HH:mm", CultureInfo.InvariantCulture))));
            sb.Append(row(o.IsDelivery ? "Delivery window" : "Collection slot", "<strong>" + h(OnlineReceiptHelper.SlotText(o)) + "</strong>"));
            sb.Append(row("Customer", h((o.FirstName + " " + o.LastName).Trim()) + " <span style=\"color:#8a8f98;\">(#" + o.CustomerId + ")</span>"));
            if (!string.IsNullOrWhiteSpace(o.Phone))
                sb.Append(row("Phone", "<a href=\"tel:" + h(o.Phone.Trim()) + "\" style=\"color:#0057b8;\">" + h(o.Phone.Trim()) + "</a>"));
            if (!string.IsNullOrWhiteSpace(o.Email))
                sb.Append(row("Email", "<a href=\"mailto:" + h(o.Email.Trim()) + "\" style=\"color:#0057b8;\">" + h(o.Email.Trim()) + "</a>"));
            if (o.IsDelivery && !string.IsNullOrWhiteSpace(o.AddressText))
                sb.Append(row("Address", h(o.AddressText)));
            if (!string.IsNullOrWhiteSpace(o.CollectionCode))
                sb.Append(row("Collection code", "<strong style=\"font-family:Consolas,'Courier New',monospace;letter-spacing:2px;\">" + h(o.CollectionCode) + "</strong>"));
            if (!string.IsNullOrWhiteSpace(o.Instructions))
                sb.Append(row("Instructions", "<em>" + h(o.Instructions.Trim()) + "</em>"));
            string payment = string.IsNullOrWhiteSpace(o.PaymentMethod) ? "Card" : o.PaymentMethod.Trim();
            if (!string.IsNullOrWhiteSpace(o.CardLastFour)) payment += " ending " + o.CardLastFour;
            sb.Append(row("Payment", h(payment) + " &middot; paid online"));
            sb.Append(row("ID check", "Customer must show ID proving they are 18 or older on " + (o.IsDelivery ? "delivery." : "collection.")));
            sb.Append("</table></td></tr>");

            // Packing list
            sb.Append("<tr><td style=\"padding:10px 28px 4px;\"><p style=\"margin:0 0 8px;font-size:15px;font-weight:600;\">Packing list</p>");
            sb.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"border:1px solid #e1e5ec;border-collapse:collapse;\">");
            sb.Append("<tr><th style=\"" + th + "width:56px;\">Qty</th><th style=\"" + th + "\">Product</th><th style=\"" + th + "text-align:right;\">Unit</th><th style=\"" + th + "text-align:right;\">Line</th></tr>");
            foreach (var l in o.Lines)
            {
                sb.Append("<tr><td style=\"" + td + "font-weight:700;\">").Append(l.Quantity).Append(" &times;</td>")
                  .Append("<td style=\"" + td + "\">").Append(h(l.Name)).Append("</td>")
                  .Append("<td style=\"" + td + "text-align:right;\">R").Append(Money(l.UnitPrice)).Append("</td>")
                  .Append("<td style=\"" + td + "text-align:right;\">R").Append(Money(l.LineTotal)).Append("</td></tr>");
            }
            sb.Append("</table></td></tr>");

            // Totals
            Func<string, string, bool, string> totalRow = (l, v, bold) =>
                "<tr><td style=\"padding:3px 0;font-size:14px;" + (bold ? "font-weight:700;color:#1d1d1f;" : "color:#6e6e73;") + "\">" + h(l) + "</td>"
                + "<td style=\"padding:3px 0;font-size:14px;text-align:right;" + (bold ? "font-weight:700;" : "") + "\">" + v + "</td></tr>";
            sb.Append("<tr><td style=\"padding:8px 28px;\"><table role=\"presentation\" width=\"260\" align=\"right\" cellpadding=\"0\" cellspacing=\"0\">");
            sb.Append(totalRow("Subtotal", "R" + Money(o.Subtotal), false));
            if (o.Discount > 0) sb.Append(totalRow("Discount", "-R" + Money(o.Discount), false));
            if (o.IsDelivery) sb.Append(totalRow("Delivery fee", o.DeliveryFee > 0 ? "R" + Money(o.DeliveryFee) : "Free", false));
            if (o.PointsUsed > 0) sb.Append(totalRow("Loyalty points used", o.PointsUsed.ToString(CultureInfo.InvariantCulture), false));
            sb.Append(totalRow("Total", "R" + Money(o.Total), true));
            sb.Append("</table></td></tr>");

            if (link != null)
                sb.Append("<tr><td style=\"padding:14px 28px 22px;\"><a href=\"").Append(h(link))
                  .Append("\" style=\"display:inline-block;background:#1d2733;color:#ffffff;text-decoration:none;padding:11px 20px;border-radius:6px;font-weight:600;font-size:14px;\">Open Manage Online Orders</a></td></tr>");

            sb.Append("<tr><td style=\"background:#f7f8fa;padding:14px 28px;border-top:1px solid #e1e5ec;\"><p style=\"margin:0;font-size:12px;color:#8a8f98;\">Internal notification for active managers. Please don&#39;t forward to customers.</p></td></tr>");
            sb.Append("</table></td></tr></table></body></html>");
            return sb.ToString();
        }


        // Absolute link to the manager's online orders page, or null outside a web request
        private static string ManagerOrdersUrl()
        {
            try
            {
                var ctx = HttpContext.Current;
                if (ctx == null) return null;
                string root = ctx.Request.Url.GetLeftPart(UriPartial.Authority) + (HttpRuntime.AppDomainAppVirtualPath ?? "/").TrimEnd('/');
                return root + "/Manager/OnlineOrders";
            }
            catch
            {
                return null;
            }
        }
    }
}
