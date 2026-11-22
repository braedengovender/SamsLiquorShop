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
            if (!active || DateTime.Today < start.Date || DateTime.Today > end.Date)
                return Fail(code, "That promo code isn't available right now.");

            if (minSpend.HasValue && goodsTotal < minSpend.Value)
                return Fail(code, "Spend at least R" + minSpend.Value.ToString("N2")
                                  + " to use this code (add R" + (minSpend.Value - goodsTotal).ToString("N2") + " more).");

            if (rule.Equals("Student", StringComparison.OrdinalIgnoreCase))
            {
                if (!customerId.HasValue) return Fail(code, "Log in with your student email address to use this code.");
                if (!IsStudentEmail(GetCustomerEmail(customerId.Value, conn, tx)))
                    return Fail(code, "This code is for students. Your account email needs to end in .ac.za.");
            }
            else if (rule.Equals("FirstOrder", StringComparison.OrdinalIgnoreCase))
            {
                if (!customerId.HasValue) return Fail(code, "Log in to use this code.");
                if (CountPreviousSales(customerId.Value, conn, tx) > 0)
                    return Fail(code, "This code is only valid on your first order.");
            }
            else if (rule.Length > 0)
            {
                return Fail(code, "That promo code isn't valid.");   // unknown rule: never apply it
            }

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