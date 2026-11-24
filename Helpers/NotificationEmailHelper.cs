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

        /// <summary>Emails every active manager that a new online order has come in. Call after the order is committed.</summary>
        public static void NotifyManagersNewOrder(int onlineOrderId)
        {
            try
            {
                string link = ManagerOrdersUrl();

                string first = "", last = "", method = "";
                decimal total = 0m;
                int saleId = 0, items = 0;
                DateTime? slot = null;
                var managers = new List<Tuple<string, string>>();

                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(@"
                        SELECT o.OnlineOrder_FirstName, o.OnlineOrder_LastName, o.OnlineOrder_Method,
                               COALESCE(o.OnlineOrder_EstimatedDateTime, o.OnlineOrder_CollectionSlot),
                               s.Sale_ID, s.Sale_TotalAmount,
                               (SELECT ISNULL(SUM(SaleLine_Quantity), 0) FROM SaleLine WHERE Sale_ID = s.Sale_ID)
                        FROM OnlineOrder o JOIN Sale s ON s.Sale_ID = o.Sale_ID
                        WHERE o.OnlineOrder_ID = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", onlineOrderId);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (!r.Read()) return;
                            first = Convert.ToString(r[0]).Trim();
                            last = Convert.ToString(r[1]).Trim();
                            method = Convert.ToString(r[2]).Trim();
                            slot = r.IsDBNull(3) ? (DateTime?)null : Convert.ToDateTime(r[3]);
                            saleId = Convert.ToInt32(r[4]);
                            total = Convert.ToDecimal(r[5]);
                            items = Convert.ToInt32(r[6]);
                        }
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

                string invoice = "INV-" + saleId.ToString("000000");
                string when = !slot.HasValue ? "to be confirmed"
                    : slot.Value.ToString("ddd d MMM yyyy, HH:mm", CultureInfo.InvariantCulture);

                string extra = Row("Invoice", invoice)
                             + Row("Customer", (first + " " + last).Trim())
                             + Row("Method", method)
                             + Row(string.Equals(method, OrderHelper.MethodDelivery, StringComparison.OrdinalIgnoreCase) ? "Delivery slot" : "Collection slot", when)
                             + Row("Items", items.ToString(CultureInfo.InvariantCulture))
                             + Row("Total", "R" + total.ToString("N2", CultureInfo.InvariantCulture));
                if (link != null)
                    extra += "<p style=\"margin:14px 0 0;\"><a href=\"" + WebUtility.HtmlEncode(link) + "\" style=\"display:inline-block;background:#0057b8;color:#ffffff;text-decoration:none;padding:10px 18px;border-radius:8px;font-weight:600;\">Open online orders</a></p>";

                string subject = "New online order " + invoice + " (" + method + ", R" + total.ToString("N2", CultureInfo.InvariantCulture) + ")";

                Task.Run(() =>
                {
                    foreach (var m in managers)
                    {
                        try
                        {
                            string html = OrderEmailHelper.BuildHtml("New online order " + invoice, m.Item2,
                                "A customer has just placed an online order. It is waiting to be packed.", extra, "New order");
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

        private static string Row(string label, string value)
        {
            return "<div><span style=\"display:inline-block;min-width:120px;color:#6e6e73;\">" + WebUtility.HtmlEncode(label) + "</span>"
                 + "<strong>" + WebUtility.HtmlEncode(value) + "</strong></div>";
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
