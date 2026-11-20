using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace SamsLiquorShop.Helpers
{
    // Builds a till-slip style receipt for a POS sale. The same HTML is shown on Customer/Receipt.aspx
    // and sent by email, so it uses inline styles only (email apps ignore stylesheets).
    public static class ReceiptHelper
    {
        // Walk-in sales on the POS are recorded against this placeholder customer
        private const int WalkInCustomerId = 999999;

        private const string StoreName = "Sam's Liquor Shop";
        private const string StoreAddress = "21 Coronation Road, Tongaat, Durban, 4399";
        private const string StorePhone = "082 405 5932";
        private const string StoreEmail = "info@samsliquorshop.co.za";

        public class Line
        {
            public string ProductName;
            public int Quantity;
            public decimal OriginalUnitPrice;
            public decimal UnitPriceAfterDiscount;
            public decimal Subtotal;
            public string DiscountName;
        }

        public class PaymentLine
        {
            public string Method;
            public decimal AmountPaid;
            public decimal? Tendered;
            public decimal? Change;
            public int PointsUsed;
        }

        public class Receipt
        {
            public int SaleId;
            public string InvoiceNumber;
            public DateTime SaleDateTime;
            public string Status;
            public string CustomerName;
            public string CustomerEmail;
            public string CashierFirstName;
            public decimal Subtotal;
            public decimal DiscountAmount;
            public string DiscountName;
            public decimal Total;
            public int PointsEarned;
            public int PointsUsed;
            public List<Line> Lines = new List<Line>();
            public List<PaymentLine> Payments = new List<PaymentLine>();
        }

        public static string InvoiceNumber(int saleId)
        {
            return "INV-" + saleId.ToString("000000");
        }

        // Returns null unless the sale exists AND belongs to this customer,
        // so a customer can never open someone else's receipt by changing the URL.
        public static Receipt Load(string connStr, int saleId, int customerId)
        {
            if (customerId <= 0 || customerId == WalkInCustomerId) return null;

            var receipt = new Receipt();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT s.Sale_ID, s.Sale_DateTime, LTRIM(RTRIM(s.Sale_Status)) AS Sale_Status,
                           s.Sale_Subtotal, ISNULL(s.Sale_DiscountAmount, 0) AS Sale_DiscountAmount, s.Sale_TotalAmount,
                           ISNULL(s.Sale_LoyaltyPointsEarned, 0) AS Sale_LoyaltyPointsEarned,
                           LTRIM(RTRIM(CONCAT(c.Customer_FirstName, ' ', c.Customer_LastName))) AS CustomerName,
                           c.Customer_EmailAddress, e.Employee_FirstName, d.Discount_Name
                    FROM Sale s
                    INNER JOIN Customer c ON c.Customer_ID = s.Customer_ID
                    LEFT JOIN Employee e ON e.Employee_ID = s.Employee_ID
                    LEFT JOIN Discount d ON d.Discount_ID = s.Discount_ID
                    WHERE s.Sale_ID = @SaleId AND s.Customer_ID = @CustomerId", conn))
                {
                    cmd.Parameters.AddWithValue("@SaleId", saleId);
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;

                        receipt.SaleId = saleId;
                        receipt.InvoiceNumber = InvoiceNumber(saleId);
                        receipt.SaleDateTime = Convert.ToDateTime(r["Sale_DateTime"]);
                        receipt.Status = Text(r["Sale_Status"]);
                        receipt.Subtotal = Convert.ToDecimal(r["Sale_Subtotal"]);
                        receipt.DiscountAmount = Convert.ToDecimal(r["Sale_DiscountAmount"]);
                        receipt.Total = Convert.ToDecimal(r["Sale_TotalAmount"]);
                        receipt.PointsEarned = Convert.ToInt32(r["Sale_LoyaltyPointsEarned"]);
                        receipt.CustomerName = Text(r["CustomerName"]);
                        receipt.CustomerEmail = Text(r["Customer_EmailAddress"]);
                        receipt.CashierFirstName = Text(r["Employee_FirstName"]);
                        receipt.DiscountName = Text(r["Discount_Name"]);
                    }
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT p.Product_Name, sl.SaleLine_Quantity, sl.SaleLine_OriginalUnitPrice,
                           sl.SaleLine_UnitPriceAfterDiscount, sl.SaleLine_Subtotal, d.Discount_Name
                    FROM SaleLine sl
                    INNER JOIN Product p ON p.Product_ID = sl.Product_ID
                    LEFT JOIN Discount d ON d.Discount_ID = sl.Discount_ID
                    WHERE sl.Sale_ID = @SaleId
                    ORDER BY p.Product_Name", conn))
                {
                    cmd.Parameters.AddWithValue("@SaleId", saleId);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            receipt.Lines.Add(new Line
                            {
                                ProductName = Text(r["Product_Name"]),
                                Quantity = Convert.ToInt32(r["SaleLine_Quantity"]),
                                OriginalUnitPrice = Convert.ToDecimal(r["SaleLine_OriginalUnitPrice"]),
                                UnitPriceAfterDiscount = Convert.ToDecimal(r["SaleLine_UnitPriceAfterDiscount"]),
                                Subtotal = Convert.ToDecimal(r["SaleLine_Subtotal"]),
                                DiscountName = Text(r["Discount_Name"])
                            });
                        }
                    }
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT pm.PaymentMethod_Name, p.Payment_AmountPaid, p.Payment_AmountTendered,
                           p.Payment_ChangeAmount, ISNULL(p.Payment_LoyaltyPointsUsed, 0) AS PointsUsed
                    FROM Payment p
                    LEFT JOIN PaymentMethod pm ON pm.PaymentMethod_ID = p.PaymentMethod_ID
                    WHERE p.Sale_ID = @SaleId
                    ORDER BY p.Payment_DateTime, p.Payment_ID", conn))
                {
                    cmd.Parameters.AddWithValue("@SaleId", saleId);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var pay = new PaymentLine
                            {
                                Method = Text(r["PaymentMethod_Name"]),
                                AmountPaid = Convert.ToDecimal(r["Payment_AmountPaid"]),
                                Tendered = r["Payment_AmountTendered"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Payment_AmountTendered"]),
                                Change = r["Payment_ChangeAmount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Payment_ChangeAmount"]),
                                PointsUsed = Convert.ToInt32(r["PointsUsed"])
                            };
                            receipt.PointsUsed += pay.PointsUsed;
                            receipt.Payments.Add(pay);
                        }
                    }
                }
            }

            return receipt;
        }

        // ---------- HTML ----------
        public static string ToHtml(Receipt r)
        {
            const string muted = "color:#6e6e73;";
            const string cell = "padding:4px 0;vertical-align:top;";
            const string num = "padding:4px 0;text-align:right;white-space:nowrap;vertical-align:top;";
            const string rule = "border:0;border-top:1px dashed #c7c7cc;margin:14px 0;";

            var sb = new StringBuilder();
            sb.Append("<div style=\"max-width:400px;margin:0 auto;padding:28px 26px;background:#ffffff;color:#1d1d1f;")
              .Append("font-family:'Segoe UI',Helvetica,Arial,sans-serif;font-size:14px;line-height:1.45;border:1px solid #e5e5ea;border-radius:14px;\">");

            // Store header
            sb.Append("<div style=\"text-align:center;\">")
              .Append("<div style=\"font-family:Georgia,'Times New Roman',serif;font-size:22px;letter-spacing:1px;\">").Append(E(StoreName.ToUpper())).Append("</div>")
              .Append("<div style=\"font-size:12px;").Append(muted).Append("\">").Append(E(StoreAddress)).Append("<br />")
              .Append(E(StorePhone)).Append(" &middot; ").Append(E(StoreEmail)).Append("</div>")
              .Append("</div>");
            sb.Append("<hr style=\"").Append(rule).Append("\" />");

            // Sale details
            sb.Append("<table style=\"width:100%;border-collapse:collapse;font-size:13px;\">");
            DetailRow(sb, "Receipt", "<strong>" + E(r.InvoiceNumber) + "</strong>");
            DetailRow(sb, "Date", E(r.SaleDateTime.ToString("d MMM yyyy, HH:mm")));
            if (r.CustomerName.Length > 0) DetailRow(sb, "Customer", E(r.CustomerName));
            if (r.CashierFirstName.Length > 0) DetailRow(sb, "Served by", E(r.CashierFirstName));
            if (r.Status.Length > 0 && !r.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                DetailRow(sb, "Status", "<strong style=\"color:#d70015;\">" + E(r.Status) + "</strong>");
            sb.Append("</table>");
            sb.Append("<hr style=\"").Append(rule).Append("\" />");

            // Items
            sb.Append("<table style=\"width:100%;border-collapse:collapse;\">");
            foreach (Line line in r.Lines)
            {
                sb.Append("<tr><td style=\"").Append(cell).Append("\">").Append(E(line.ProductName))
                  .Append("<div style=\"font-size:12px;").Append(muted).Append("\">").Append(line.Quantity).Append(" &times; ")
                  .Append(E(Money(line.UnitPriceAfterDiscount)));
                if (line.UnitPriceAfterDiscount < line.OriginalUnitPrice)
                {
                    sb.Append(" <s>").Append(E(Money(line.OriginalUnitPrice))).Append("</s>");
                    if (line.DiscountName.Length > 0) sb.Append(" &middot; ").Append(E(line.DiscountName));
                }
                sb.Append("</div></td><td style=\"").Append(num).Append("\">").Append(E(Money(line.Subtotal))).Append("</td></tr>");
            }
            sb.Append("</table>");
            sb.Append("<hr style=\"").Append(rule).Append("\" />");

            // Totals
            sb.Append("<table style=\"width:100%;border-collapse:collapse;\">");
            TotalRow(sb, "Subtotal", Money(r.Subtotal), false);
            if (r.DiscountAmount > 0)
                TotalRow(sb, "Discount" + (r.DiscountName.Length > 0 ? " (" + r.DiscountName + ")" : ""), "-" + Money(r.DiscountAmount), false);
            TotalRow(sb, "Total", Money(r.Total), true);
            sb.Append("</table>");

            // Payments
            if (r.Payments.Count > 0)
            {
                sb.Append("<hr style=\"").Append(rule).Append("\" />");
                sb.Append("<table style=\"width:100%;border-collapse:collapse;font-size:13px;\">");
                foreach (PaymentLine p in r.Payments)
                {
                    TotalRow(sb, "Paid by " + (p.Method.Length > 0 ? p.Method : "other"), Money(p.AmountPaid), false);
                    if (p.Tendered.HasValue && p.Tendered.Value > p.AmountPaid)
                    {
                        TotalRow(sb, "Tendered", Money(p.Tendered.Value), false);
                        TotalRow(sb, "Change", Money(p.Change ?? (p.Tendered.Value - p.AmountPaid)), false);
                    }
                }
                sb.Append("</table>");
            }

            // Loyalty (1 point = R1 to redeem, 1 point per R10 earned — same rules as the POS)
            sb.Append("<hr style=\"").Append(rule).Append("\" />");
            sb.Append("<table style=\"width:100%;border-collapse:collapse;font-size:13px;\">");
            TotalRow(sb, "Loyalty points earned", r.PointsEarned.ToString("N0"), false);
            if (r.PointsUsed > 0) TotalRow(sb, "Loyalty points used", r.PointsUsed.ToString("N0"), false);
            sb.Append("</table>");

            sb.Append("<p style=\"text-align:center;margin:18px 0 0;font-size:12px;").Append(muted).Append("\">")
              .Append("Thank you for shopping with us!<br />Please drink responsibly. Not for persons under the age of 18.</p>");
            sb.Append("</div>");
            return sb.ToString();
        }

        private static void DetailRow(StringBuilder sb, string label, string valueHtml)
        {
            sb.Append("<tr><td style=\"padding:2px 0;color:#6e6e73;\">").Append(E(label))
              .Append("</td><td style=\"padding:2px 0;text-align:right;\">").Append(valueHtml).Append("</td></tr>");
        }

        private static void TotalRow(StringBuilder sb, string label, string value, bool emphasise)
        {
            string style = emphasise ? "font-size:17px;font-weight:700;padding:6px 0 2px;" : "padding:3px 0;";
            sb.Append("<tr><td style=\"").Append(style).Append("\">").Append(E(label))
              .Append("</td><td style=\"").Append(style).Append("text-align:right;white-space:nowrap;\">").Append(E(value)).Append("</td></tr>");
        }

        // ---------- EMAIL ----------
        // Sends the receipt to the customer's email address on file. Returns false with a reason when it can't.
        public static bool Email(string connStr, int saleId, int customerId, out string message)
        {
            Receipt r = Load(connStr, saleId, customerId);
            if (r == null)
            {
                message = "That receipt could not be found.";
                return false;
            }
            if (r.CustomerEmail.Length == 0)
            {
                message = "There's no email address on your account. Add one under Account Details first.";
                return false;
            }
            if (OtpHelper.DemoMode)
            {
                message = "Email isn't set up on this computer yet, so the receipt wasn't sent. Use Print or Download instead.";
                return false;
            }

            string subject = "Your Sam's Liquor Shop receipt " + r.InvoiceNumber;
            string html = "<p style=\"font-family:'Segoe UI',Helvetica,Arial,sans-serif;\">Hi " + E(r.CustomerName)
                + ", here's a copy of your receipt.</p>" + ToHtml(r);

            if (!OtpHelper.SendEmail(r.CustomerEmail, r.CustomerName, subject, html))
            {
                message = "The receipt couldn't be emailed right now. Please try again later.";
                return false;
            }

            message = "Receipt " + r.InvoiceNumber + " was emailed to " + r.CustomerEmail + ".";
            return true;
        }

        // ---------- HELPERS ----------
        private static string Text(object value)
        {
            return value == null || value == DBNull.Value ? "" : value.ToString().Trim();
        }

        private static string Money(decimal value)
        {
            return value.ToString("C");
        }

        private static string E(string value)
        {
            return WebUtility.HtmlEncode(value ?? "");
        }
    }
}
