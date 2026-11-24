using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SamsLiquorShop.Helpers
{
    public static class PromoHelper
    {
        public class PromoResult
        {
            public bool IsValid;
            public bool IsError;          // true = we couldn't check (database problem), not "code is wrong"
            public string Code;           // normalised, upper case
            public int DiscountId;
            public decimal DiscountAmount;
            public string Message;        // plain text - HTML-encode before showing it
        }

        public class PromoOffer
        {
            public string Code;
            public string Name;
        }

        private static string ConnStr
        {
            get { return ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString; }
        }

        private static readonly Regex CodeFormat = new Regex("^[A-Z0-9_-]{1,20}$", RegexOptions.Compiled);

        public static bool IsStudentEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email)
                && email.Contains("@")
                && email.Trim().EndsWith(".ac.za", StringComparison.OrdinalIgnoreCase);
        }

        private static PromoResult Fail(string code, string message)
        {
            return new PromoResult { IsValid = false, Code = code, Message = message };
        }

        /// <summary>
        /// Checks a promo code against the Discount table, today's date, the goods total and the customer.
        /// Pass conn/tx to run inside an existing transaction (PlaceOrder does this).
        /// When IsError is true the caller must treat the order as NOT valid for promo purposes.
        /// </summary>
        public static PromoResult Validate(string code, int? customerId, decimal goodsTotal,
                                           SqlConnection conn = null, SqlTransaction tx = null)
        {
            code = (code ?? "").Trim().ToUpperInvariant();
            if (code.Length == 0) return Fail(code, "Please enter a promo code.");
            if (!CodeFormat.IsMatch(code)) return Fail(code, "Promo codes only contain letters, numbers, - and _.");

            bool ownsConnection = conn == null;
            try
            {
                if (ownsConnection)
                {
                    conn = new SqlConnection(ConnStr);
                    conn.Open();
                }
                return ValidateCore(code, customerId, goodsTotal, conn, tx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                return new PromoResult
                {
                    IsValid = false,
                    IsError = true,
                    Code = code,
                    Message = "We couldn't check that promo code right now. Please try again."
                };
            }
            finally
            {
                if (ownsConnection && conn != null) conn.Dispose();
            }
        }

        private static PromoResult ValidateCore(string code, int? customerId, decimal goodsTotal,
                                                SqlConnection conn, SqlTransaction tx)
        {
            int id;
            string type, status, rule;
            decimal value;
            DateTime start, end;
            decimal? minSpend;

            using (var cmd = new SqlCommand(
                @"SELECT Discount_ID, Discount_Type, Discount_Value, Discount_StartDate, Discount_EndDate,
                         Discount_Status, Discount_MinSpend, Discount_Rule
                  FROM Discount WHERE Discount_Code = @Code", conn, tx))
            {
                cmd.Parameters.AddWithValue("@Code", code);
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return Fail(code, "That promo code isn't valid.");
                    id = r.GetInt32(0);
                    type = r.GetString(1);
                    value = r.GetDecimal(2);
                    start = r.GetDateTime(3);
                    end = r.GetDateTime(4);
                    status = r.GetString(5);
                    minSpend = r.IsDBNull(6) ? (decimal?)null : r.GetDecimal(6);
                    rule = r.IsDBNull(7) ? "" : r.GetString(7).Trim();
                }
            }

            bool active = string.Equals(status.Trim(), "Active", StringComparison.OrdinalIgnoreCase);
            DateTime today = SlotHelper.Now().Date;
            if (!active || today < start.Date || today > end.Date)
                return Fail(code, "That promo code isn't available right now.");

            if (minSpend.HasValue && goodsTotal < minSpend.Value)
                return Fail(code, "Spend at least R" + minSpend.Value.ToString("N2")
                                  + " to use this code (add R" + (minSpend.Value - goodsTotal).ToString("N2") + " more).");

            string ruleProblem = CheckCustomerRules(id, rule, customerId, conn, tx);
            if (ruleProblem != null) return Fail(code, ruleProblem);

            decimal amount = type.Equals("Percentage", StringComparison.OrdinalIgnoreCase)
                ? Math.Round(goodsTotal * value / 100m, 2)
                : value;
            amount = Math.Min(Math.Max(amount, 0m), goodsTotal);   // never below R0

            if (amount <= 0m) return Fail(code, "That promo code can't be applied to this order.");

            string description = type.Equals("Percentage", StringComparison.OrdinalIgnoreCase)
                ? value.ToString("0.##", CultureInfo.InvariantCulture) + "% off"
                : "R" + value.ToString("N2") + " off";

            return new PromoResult
            {
                IsValid = true,
                Code = code,
                DiscountId = id,
                DiscountAmount = amount,
                Message = "Promo code applied: " + description + " (-R" + amount.ToString("N2") + ")."
            };
        }

        /// <summary>
        /// The customer-specific rules for a promo code. Returns null if this customer may use it,
        /// otherwise the reason they can't. Shared by Validate (checkout) and GetAvailableOffers (dashboard),
        /// so a customer is only ever shown codes that checkout will accept.
        /// </summary>
        private static string CheckCustomerRules(int discountId, string rule, int? customerId,
                                                 SqlConnection conn, SqlTransaction tx)
        {
            rule = (rule ?? "").Trim();
            bool student = rule.Equals("Student", StringComparison.OrdinalIgnoreCase);
            bool firstOrder = rule.Equals("FirstOrder", StringComparison.OrdinalIgnoreCase);

            if (rule.Length > 0 && !student && !firstOrder)
                return "That promo code isn't valid.";   // unknown rule: never apply it

            if (!customerId.HasValue)
            {
                if (student) return "Log in with your student email address to use this code.";
                if (firstOrder) return "Log in to use this code.";
                return null;   // open code: the once-per-customer check happens again at checkout, where login is required
            }

            // Each promo code can be used once per customer (a cancelled order gives it back)
            if (HasUsedDiscount(customerId.Value, discountId, conn, tx))
                return "You've already used this promo code.";

            if (student && !IsStudentEmail(GetCustomerEmail(customerId.Value, conn, tx)))
                return "This code is for students. Your account email needs to end in .ac.za.";

            if (firstOrder && CountPreviousSales(customerId.Value, conn, tx) > 0)
                return "This code is only valid on your first order.";

            return null;
        }

        private static bool HasUsedDiscount(int customerId, int discountId, SqlConnection conn, SqlTransaction tx)
        {
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Sale WHERE Customer_ID = @C AND Discount_ID = @D AND Sale_Status <> 'Cancelled'", conn, tx))
            {
                cmd.Parameters.AddWithValue("@C", customerId);
                cmd.Parameters.AddWithValue("@D", discountId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public class AvailableOffer
        {
            public string Code;
            public string Name;
            public string Rule;          // "", "Student" or "FirstOrder"
            public string Description;   // e.g. "20% off" or "R50.00 off"
            public decimal? MinSpend;
            public DateTime EndDate;
        }

        /// <summary>
        /// Every live promo code this customer can use right now: active, in date, not used by them before,
        /// and passing its rule (student email, first order). Minimum spend is shown, not checked,
        /// because it depends on the cart. Returns an empty list if the database can't be read.
        /// </summary>
        public static System.Collections.Generic.List<AvailableOffer> GetAvailableOffers(int customerId)
        {
            var offers = new System.Collections.Generic.List<AvailableOffer>();
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var candidates = new System.Collections.Generic.List<Tuple<int, AvailableOffer>>();
                    DateTime today = SlotHelper.Now().Date;

                    using (var cmd = new SqlCommand(
                        @"SELECT Discount_ID, Discount_Code, Discount_Name, Discount_Type, Discount_Value,
                                 Discount_MinSpend, Discount_Rule, Discount_EndDate
                          FROM Discount
                          WHERE Discount_Code IS NOT NULL AND LTRIM(RTRIM(Discount_Code)) <> ''
                            AND LTRIM(RTRIM(Discount_Status)) = 'Active'
                            AND @Today BETWEEN CAST(Discount_StartDate AS date) AND CAST(Discount_EndDate AS date)
                          ORDER BY Discount_EndDate", conn))
                    {
                        cmd.Parameters.AddWithValue("@Today", today);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                string type = r.GetString(3);
                                decimal value = r.GetDecimal(4);
                                bool pct = type.Equals("Percentage", StringComparison.OrdinalIgnoreCase);
                                candidates.Add(Tuple.Create(r.GetInt32(0), new AvailableOffer
                                {
                                    Code = r.GetString(1).Trim().ToUpperInvariant(),
                                    Name = r.IsDBNull(2) ? "" : r.GetString(2).Trim(),
                                    Description = pct ? value.ToString("0.##", CultureInfo.InvariantCulture) + "% off" : "R" + value.ToString("N2") + " off",
                                    MinSpend = r.IsDBNull(5) ? (decimal?)null : r.GetDecimal(5),
                                    Rule = r.IsDBNull(6) ? "" : r.GetString(6).Trim(),
                                    EndDate = r.GetDateTime(7)
                                }));
                            }
                        }
                    }

                    foreach (var c in candidates)
                    {
                        if (!CodeFormat.IsMatch(c.Item2.Code)) continue;
                        if (CheckCustomerRules(c.Item1, c.Item2.Rule, customerId, conn, null) == null)
                            offers.Add(c.Item2);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("PromoHelper.GetAvailableOffers: " + ex);
            }
            return offers;
        }

        private static string GetCustomerEmail(int customerId, SqlConnection conn, SqlTransaction tx)
        {
            using (var cmd = new SqlCommand("SELECT Customer_EmailAddress FROM Customer WHERE Customer_ID = @Id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@Id", customerId);
                return Convert.ToString(cmd.ExecuteScalar());
            }
        }

        private static int CountPreviousSales(int customerId, SqlConnection conn, SqlTransaction tx)
        {
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Sale WHERE Customer_ID = @Id AND Sale_Status <> 'Cancelled'", conn, tx))
            {
                cmd.Parameters.AddWithValue("@Id", customerId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>Finds the live promo for a rule ("Student" or "FirstOrder"), for emails and pop-ups. Null if none.</summary>
        public static PromoOffer GetOffer(string rule)
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand(
                    @"SELECT TOP 1 Discount_Code, Discount_Name FROM Discount
                      WHERE Discount_Rule = @Rule AND Discount_Code IS NOT NULL AND Discount_Status = 'Active'
                        AND CAST(GETDATE() AS date) BETWEEN Discount_StartDate AND Discount_EndDate", conn))
                {
                    cmd.Parameters.AddWithValue("@Rule", rule);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read()) return new PromoOffer { Code = r.GetString(0), Name = r.GetString(1) };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
            }
            return null;
        }
    }
}