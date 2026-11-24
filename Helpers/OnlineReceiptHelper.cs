using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace SamsLiquorShop.Helpers
{
    public class OnlineReceiptLine
    {
        public string Name;
        public int Quantity;
        public decimal OriginalPrice;
        public decimal UnitPrice;
        public decimal LineTotal;
    }

    public class OnlineReceipt
    {
        public int SaleId, OnlineOrderId, CustomerId, PointsEarned, PointsUsed;
        public DateTime PlacedAt;
        public string Method, FirstName, LastName, Email, Phone, AddressText, Instructions, CollectionCode, CardLastFour, PaymentMethod, Status;
        public DateTime? Slot;
        public decimal Subtotal, Discount, DeliveryFee, Total;
        public List<OnlineReceiptLine> Lines = new List<OnlineReceiptLine>();
        public bool IsDelivery { get { return string.Equals(Method, "Delivery", StringComparison.OrdinalIgnoreCase); } }
    }

    public static class OnlineReceiptHelper
    {
        private static string ConnStr
        {
            get { return ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString; }
        }

        /// <summary>Loads an online order for a receipt. When customerId is given, the sale must belong to that customer. Null if not found. Throws on a database error.</summary>
        public static OnlineReceipt Load(int saleId, int? customerId)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                OnlineReceipt r = null;

                using (var cmd = new SqlCommand(@"
                    SELECT s.Sale_ID, s.Customer_ID, s.Sale_DateTime, s.Sale_Subtotal, s.Sale_DiscountAmount, s.Sale_TotalAmount, s.Sale_LoyaltyPointsEarned,
                           o.OnlineOrder_ID, o.OnlineOrder_Method, o.OnlineOrder_FirstName, o.OnlineOrder_LastName, o.OnlineOrder_EmailAddress,
                           o.OnlineOrder_PhoneNumber, o.OnlineOrder_UnitNumber, o.OnlineOrder_UnitName, o.OnlineOrder_StreetNumber,
                           o.OnlineOrder_StreetName, o.OnlineOrder_Suburb, o.OnlineOrder_City, o.OnlineOrder_PostalCode,
                           o.OnlineOrder_AdditionalInstructions, o.OnlineOrder_DeliveryFee, o.OnlineOrder_EstimatedDateTime,
                           o.OnlineOrder_CollectionCode, o.OnlineOrder_CollectionSlot, o.OnlineOrder_CardLastFour,
                           p.Payment_LoyaltyPointsUsed, pm.PaymentMethod_Name,
                           (SELECT TOP 1 h.OnlineOrderStatusHistory_Status FROM OnlineOrderStatusHistory h
                            WHERE h.OnlineOrder_ID = o.OnlineOrder_ID ORDER BY h.OnlineOrderStatusHistory_ID DESC) AS CurrentStatus
                    FROM Sale s
                    INNER JOIN OnlineOrder o ON o.Sale_ID = s.Sale_ID
                    LEFT JOIN Payment p ON p.Sale_ID = s.Sale_ID
                    LEFT JOIN PaymentMethod pm ON pm.PaymentMethod_ID = p.PaymentMethod_ID
                    WHERE s.Sale_ID = @Sale AND (@Cust IS NULL OR s.Customer_ID = @Cust)", conn))
                {
                    cmd.Parameters.Add("@Sale", SqlDbType.Int).Value = saleId;
                    cmd.Parameters.Add("@Cust", SqlDbType.Int).Value = customerId.HasValue ? (object)customerId.Value : DBNull.Value;

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;

                        bool delivery = string.Equals(Convert.ToString(rd["OnlineOrder_Method"]).Trim(), "Delivery", StringComparison.OrdinalIgnoreCase);

                        var parts = new List<string>();
                        string unitNo = Convert.ToString(rd["OnlineOrder_UnitNumber"]).Trim();
                        string unitName = Convert.ToString(rd["OnlineOrder_UnitName"]).Trim();
                        if (unitNo.Length > 0) parts.Add("Unit " + unitNo);
                        if (unitName.Length > 0) parts.Add(unitName);
                        string street = (Convert.ToString(rd["OnlineOrder_StreetNumber"]).Trim() + " " + Convert.ToString(rd["OnlineOrder_StreetName"]).Trim()).Trim();
                        if (street.Length > 0) parts.Add(street);
                        foreach (string col in new[] { "OnlineOrder_Suburb", "OnlineOrder_City", "OnlineOrder_PostalCode" })
                        {
                            string v = Convert.ToString(rd[col]).Trim();
                            if (v.Length > 0) parts.Add(v);
                        }

                        r = new OnlineReceipt
                        {
                            SaleId = Convert.ToInt32(rd["Sale_ID"]),
                            CustomerId = rd["Customer_ID"] == DBNull.Value ? 0 : Convert.ToInt32(rd["Customer_ID"]),
                            PlacedAt = Convert.ToDateTime(rd["Sale_DateTime"]),
                            Subtotal = Convert.ToDecimal(rd["Sale_Subtotal"]),
                            Discount = rd["Sale_DiscountAmount"] == DBNull.Value ? 0m : Convert.ToDecimal(rd["Sale_DiscountAmount"]),
                            Total = Convert.ToDecimal(rd["Sale_TotalAmount"]),
                            PointsEarned = Convert.ToInt32(rd["Sale_LoyaltyPointsEarned"]),
                            OnlineOrderId = Convert.ToInt32(rd["OnlineOrder_ID"]),
                            Method = Convert.ToString(rd["OnlineOrder_Method"]).Trim(),
                            FirstName = Convert.ToString(rd["OnlineOrder_FirstName"]),
                            LastName = Convert.ToString(rd["OnlineOrder_LastName"]),
                            Email = Convert.ToString(rd["OnlineOrder_EmailAddress"]),
                            Phone = Convert.ToString(rd["OnlineOrder_PhoneNumber"]),
                            AddressText = string.Join(", ", parts),
                            Instructions = Convert.ToString(rd["OnlineOrder_AdditionalInstructions"]),
                            DeliveryFee = Convert.ToDecimal(rd["OnlineOrder_DeliveryFee"]),
                            Slot = delivery
                                ? (rd["OnlineOrder_EstimatedDateTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["OnlineOrder_EstimatedDateTime"]))
                                : (rd["OnlineOrder_CollectionSlot"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["OnlineOrder_CollectionSlot"])),
                            CollectionCode = Convert.ToString(rd["OnlineOrder_CollectionCode"]),
                            CardLastFour = Convert.ToString(rd["OnlineOrder_CardLastFour"]).Trim(),
                            PointsUsed = rd["Payment_LoyaltyPointsUsed"] == DBNull.Value ? 0 : Convert.ToInt32(rd["Payment_LoyaltyPointsUsed"]),
                            PaymentMethod = Convert.ToString(rd["PaymentMethod_Name"]),
                            Status = Convert.ToString(rd["CurrentStatus"])
                        };
                    }
                }

                using (var cmd = new SqlCommand(@"
                    SELECT pr.Product_Name, sl.SaleLine_Quantity, sl.SaleLine_OriginalUnitPrice, sl.SaleLine_UnitPriceAfterDiscount, sl.SaleLine_Subtotal
                    FROM SaleLine sl INNER JOIN Product pr ON pr.Product_ID = sl.Product_ID
                    WHERE sl.Sale_ID = @Sale ORDER BY pr.Product_Name", conn))
                {
                    cmd.Parameters.Add("@Sale", SqlDbType.Int).Value = saleId;
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            r.Lines.Add(new OnlineReceiptLine
                            {
                                Name = Convert.ToString(rd[0]),
                                Quantity = Convert.ToInt32(rd[1]),
                                OriginalPrice = Convert.ToDecimal(rd[2]),
                                UnitPrice = Convert.ToDecimal(rd[3]),
                                LineTotal = Convert.ToDecimal(rd[4])
                            });
                        }
                    }
                }
                return r;
            }
        }

        public static string SlotText(OnlineReceipt r)
        {
            if (!r.Slot.HasValue) return "To be confirmed";
            DateTime s = r.Slot.Value;
            string day = s.ToString("ddd d MMM yyyy", CultureInfo.InvariantCulture);

            if (r.IsDelivery)
            {
                var window = SlotHelper.GetDeliveryWindows(s.Date).FirstOrDefault(x => x.Start == s);
                return day + ", " + (window != null ? window.Label : s.ToString("HH:mm", CultureInfo.InvariantCulture));
            }
            return day + ", " + s.ToString("HH:mm", CultureInfo.InvariantCulture) + " - " + s.AddHours(1).ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        /// <summary>Self-contained, inline-styled HTML. fullWidth = two columns that fill the page (web); false = narrow single column (email).</summary>
        public static string BuildHtml(OnlineReceipt r, bool fullWidth = false)
        {
            Func<string, string> h = WebUtility.HtmlEncode;
            const string cell = "padding:8px 10px;border-bottom:1px solid #eef1f6;font-size:14px;";

            // ----- left / top block: invoice, contact, how it gets to the customer -----
            var info = new StringBuilder();
            info.Append("<p style=\"margin:0 0 4px;font-size:18px;font-weight:600;color:#0057b8;\">Invoice INV-" + r.SaleId.ToString("000000") + "</p>");
            info.Append("<p style=\"margin:0 0 16px;font-size:13px;color:#6e6e73;\">Placed " + h(r.PlacedAt.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture))
                      + " &middot; Status: " + h(r.Status ?? "Placed") + "</p>");
            info.Append("<p style=\"margin:0 0 16px;font-size:14px;\">" + h((r.FirstName + " " + r.LastName).Trim()) + "<br/>" + h(r.Email) + " &middot; " + h(r.Phone) + "</p>");

            info.Append("<div style=\"background:#f4f8fd;border:1px solid #dce7f5;border-radius:8px;padding:14px 16px;margin-bottom:18px;font-size:14px;line-height:1.6;\">");
            if (r.IsDelivery)
            {
                info.Append("<strong>Delivery</strong><br/>" + h(r.AddressText) + "<br/>Delivery window: " + h(SlotText(r)));
                if (!string.IsNullOrWhiteSpace(r.Instructions)) info.Append("<br/>Instructions: " + h(r.Instructions));
                if (!string.IsNullOrWhiteSpace(r.CollectionCode))
                {
                    info.Append("<br/>Collection code: <span style=\"font-family:Consolas,'Courier New',monospace;font-size:20px;font-weight:700;letter-spacing:4px;color:#0057b8;\">"
                              + h(r.CollectionCode) + "</span>");
                    info.Append("<br/><span style=\"color:#6e6e73;\">Give this code to the driver and have your ID ready - they will check that you are 18 or older.</span>");
                }
                else
                {
                    info.Append("<br/><span style=\"color:#6e6e73;\">Please have your ID ready - the driver will check that you are 18 or older.</span>");
                }
            }
            else
            {
                info.Append("<strong>Click &amp; Collect</strong><br/>21 Coronation Road, Mithangar, Tongaat, 4399<br/>Collection slot: " + h(SlotText(r)));
                if (!string.IsNullOrWhiteSpace(r.CollectionCode))
                {
                    info.Append("<br/>Collection code: <span style=\"font-family:Consolas,'Courier New',monospace;font-size:20px;font-weight:700;letter-spacing:4px;color:#0057b8;\">"
                              + h(r.CollectionCode) + "</span>");
                }
                info.Append("<br/><span style=\"color:#6e6e73;\">Bring this code and your ID - we check that you are 18 or older.</span>");
            }
            info.Append("</div>");

            // ----- items + totals -----
            var items = new StringBuilder();
            items.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse:collapse;margin-bottom:12px;\">");
            items.Append("<tr style=\"background:#0057b8;color:#ffffff;\"><th align=\"left\" style=\"padding:8px 10px;font-size:13px;\">Item</th>"
                      + "<th align=\"center\" style=\"padding:8px 10px;font-size:13px;\">Qty</th><th align=\"right\" style=\"padding:8px 10px;font-size:13px;\">Total</th></tr>");
            foreach (var l in r.Lines)
            {
                items.Append("<tr><td style=\"" + cell + "\">" + h(l.Name) + "<br/><span style=\"font-size:12px;color:#6e6e73;\">"
                          + h(OrderHelper.Money(l.UnitPrice)) + " each");
                if (l.OriginalPrice > l.UnitPrice)
                    items.Append(" <span style=\"color:#2e8b57;\">(was " + h(OrderHelper.Money(l.OriginalPrice)) + ")</span>");
                items.Append("</span></td><td align=\"center\" style=\"" + cell + "\">" + l.Quantity + "</td><td align=\"right\" style=\"" + cell + "\">"
                          + h(OrderHelper.Money(l.LineTotal)) + "</td></tr>");
            }
            items.Append("</table>");

            Func<string, string, string, string> row = (label, value, style) =>
                "<tr><td style=\"padding:4px 10px;font-size:14px;" + style + "\">" + label + "</td><td align=\"right\" style=\"padding:4px 10px;font-size:14px;" + style + "\">" + value + "</td></tr>";

            items.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin-bottom:14px;\">");
            items.Append(row("Subtotal", h(OrderHelper.Money(r.Subtotal)), ""));
            if (r.Discount > 0m) items.Append(row("Discounts", "-" + h(OrderHelper.Money(r.Discount)), "color:#2e8b57;"));
            items.Append(row("Delivery", r.DeliveryFee > 0m ? h(OrderHelper.Money(r.DeliveryFee)) : "Free", ""));
            items.Append(row("Total paid", h(OrderHelper.Money(r.Total)), "font-weight:700;font-size:16px;color:#0057b8;"));
            items.Append(row("Includes 15% VAT", h(OrderHelper.Money(Math.Round(r.Total * 15m / 115m, 2))), "color:#6e6e73;font-size:12px;"));
            items.Append("</table>");

            // ----- payment + points -----
            var pay = new StringBuilder();
            string payment;
            if (r.PointsUsed > 0) payment = "Paid with loyalty points (" + r.PointsUsed.ToString("N0", CultureInfo.InvariantCulture) + " pts)";
            else if (!string.IsNullOrEmpty(r.CardLastFour)) payment = "Card ending " + r.CardLastFour;
            else payment = "No payment was due";
            pay.Append("<p style=\"margin:0 0 4px;font-size:14px;\"><strong>Payment:</strong> " + h(payment) + "</p>");
            if (r.PointsEarned > 0)
                pay.Append("<p style=\"margin:0 0 4px;font-size:14px;\"><strong>Loyalty points earned:</strong> " + r.PointsEarned.ToString("N0", CultureInfo.InvariantCulture) + "</p>");

            // ----- assemble -----
            var sb = new StringBuilder();
            sb.Append("<div style=\"font-family:'Segoe UI',Helvetica,Arial,sans-serif;background:#ffffff;color:#1d1d1f;"
                      + (fullWidth ? "width:100%;" : "max-width:600px;margin:0 auto;border:1px solid #dce7f5;border-radius:10px;overflow:hidden;") + "\">");
            sb.Append("<div style=\"background:#0057b8;padding:24px 28px;\">");
            sb.Append("<p style=\"margin:0;font-size:12px;letter-spacing:1.5px;text-transform:uppercase;color:#b9d6f7;\">Order confirmation</p>");
            sb.Append("<p style=\"margin:4px 0 0;font-size:24px;font-weight:600;color:#ffffff;\">Sam's Liquor Shop</p></div>");

            if (fullWidth)
            {
                sb.Append("<div class=\"co-receipt-body\" style=\"padding:28px 32px;display:grid;grid-template-columns:minmax(0,1fr) minmax(0,1.5fr);gap:28px 44px;align-items:start;\">");
                sb.Append("<div>").Append(info).Append(pay).Append("</div><div>").Append(items).Append("</div></div>");
            }
            else
            {
                sb.Append("<div style=\"padding:24px 28px;\">").Append(info).Append(items).Append(pay).Append("</div>");
            }

            sb.Append("<div style=\"background:#f4f8fd;padding:16px 28px;border-top:1px solid #dce7f5;font-size:12px;line-height:1.6;color:#6e6e73;\">"
                      + "Sam's Liquor Shop, 21 Coronation Road, Mithangar, Tongaat, 4399 &middot; 082 405 5932<br/>Not for persons under the age of 18. Please drink responsibly.</div>");
            sb.Append("</div>");
            return sb.ToString();
        }
    }
}