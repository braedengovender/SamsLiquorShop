using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SamsLiquorShop.Helpers
{
    [Serializable]
    public class OrderLine
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderRequest
    {
        public int CustomerId { get; set; }
        public List<OrderLine> Lines { get; set; }
        public string Method { get; set; }                 // "Delivery" or "Collection"
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string UnitNumber { get; set; }
        public string UnitName { get; set; }
        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string Suburb { get; set; }
        public string PostalCode { get; set; }
        public string Instructions { get; set; }
        public DateTime? SlotStart { get; set; }
        public string PromoCode { get; set; }
        public bool UsePoints { get; set; }
        public string CardName { get; set; }               // name printed on the card - checked, never stored
        public string CardNumber { get; set; }             // checked, then thrown away - only the last four digits are stored
        public string CardExpiry { get; set; }
        public string CardCvv { get; set; }
        public bool AgeConfirmed { get; set; }
        public bool CheckExpectedTotal { get; set; }
        public decimal ExpectedTotal { get; set; }         // the total the customer saw on screen
    }

    public class OrderResult
    {
        public bool Success;
        public string Message;                             // plain text - HTML-encode before showing
        public int SaleId;
        public int OnlineOrderId;
        public decimal Total;
        public int PointsEarned;
        public string CollectionCode;
        public bool CartChanged;                           // stock/availability problem: send the customer back to the cart
    }

    public class QuoteLine
    {
        public int ProductId;
        public string Name;
        public int Quantity;
        public decimal OriginalPrice;
        public decimal UnitPrice;
        public int? DiscountId;
        public decimal LineTotal { get { return UnitPrice * Quantity; } }
        public decimal LineOriginal { get { return OriginalPrice * Quantity; } }
    }

    public class Quote
    {
        public bool Ok;
        public string Error;
        public List<QuoteLine> Lines = new List<QuoteLine>();
        public decimal Subtotal, ItemDiscount, GoodsAfterItems, PromoAmount, GoodsAfterPromo, DeliveryFee, Total, Vat;
        public string PromoCode, PromoMessage;
        public bool PromoValid, PromoError;
        public int? PromoDiscountId;
        public bool DeliveryAllowed = true;
        public string DeliveryMessage;
        public bool CustomerActive;
        public int PointsBalance, PointsNeeded, PointsToEarn;
        public bool CanUsePoints, UsePoints;
    }

    public class StatusResult
    {
        public bool Success;
        public string Message;
        public string NewStatus;
    }

    public class CustomerProfile
    {
        public string FirstName, LastName, Email, Phone, UnitNumber, UnitName, StreetNumber, StreetName, Suburb, PostalCode;
    }

    public static class OrderHelper
    {
        public const int OnlineSaleTypeId = 2;
        public const string OnlineEmployeeUsername = "onlinestore";
        public const decimal DeliveryFeeAmount = 70m;
        public const decimal FreeDeliveryThreshold = 500m;
        public const decimal MinDeliveryOrder = 250m;
        public const int RandPerPoint = 10;
        public const string FixedCity = "Durban";
        public const string FixedProvince = "KwaZulu-Natal";
        public const string FixedCountry = "South Africa";
        public const string MethodDelivery = "Delivery";
        public const string MethodCollection = "Collection";

        public const string StPlaced = "Placed";
        public const string StPacking = "Packing";
        public const string StOut = "Out for delivery";
        public const string StDelivered = "Delivered";
        public const string StReady = "Ready for collection";
        public const string StCollected = "Collected";
        public const string StCancelled = "Cancelled";
        public const string StExpired = "Expired";

        private static readonly string[] AllStatuses = { StPlaced, StPacking, StOut, StDelivered, StReady, StCollected, StCancelled, StExpired };
        private static readonly string[] DeliveryPath = { StPlaced, StPacking, StOut, StDelivered };
        private static readonly string[] CollectionPath = { StPlaced, StPacking, StReady, StCollected };

        private static readonly Regex NameFormat = new Regex(@"^\p{L}[\p{L} '\.\-]*$", RegexOptions.Compiled);
        private static readonly Regex EmailFormat = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly Regex PhoneFormat = new Regex(@"^\+?[0-9]{9,15}$", RegexOptions.Compiled);
        private static readonly Regex PostalFormat = new Regex(@"^[0-9]{4}$", RegexOptions.Compiled);
        private static readonly Regex PromoFormat = new Regex(@"^[A-Za-z0-9_-]{1,20}$", RegexOptions.Compiled);
        private static readonly Regex StreetNumberFormat = new Regex(@"^[0-9A-Za-z/\- ]{1,10}$", RegexOptions.Compiled);

        private static string ConnStr
        {
            get { return ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString; }
        }

        // ------------------------------------------------------------------ small helpers

        public static string Money(decimal value)
        {
            return "R" + value.ToString("N2", CultureInfo.InvariantCulture);
        }

        private static SqlCommand Cmd(string sql, SqlConnection conn, SqlTransaction tx)
        {
            return new SqlCommand(sql, conn, tx);
        }

        // Adds a parameter; null and empty strings become database NULL
        private static void P(SqlCommand cmd, string name, object value)
        {
            string s = value as string;
            if (s != null && s.Length == 0) value = null;
            cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        private static string Clean(string s)
        {
            if (s == null) return "";
            s = Regex.Replace(s, @"[\x00-\x1F\x7F]+", " ");
            return Regex.Replace(s.Trim(), @"\s{2,}", " ");
        }

        private static void SafeRollback(SqlTransaction tx)
        {
            try { if (tx != null && tx.Connection != null) tx.Rollback(); }
            catch { /* already rolled back or the connection dropped */ }
        }

        private static OrderResult Fail(string message, bool cartChanged = false)
        {
            return new OrderResult { Success = false, Message = message, CartChanged = cartChanged };
        }

        // ------------------------------------------------------------------ validation

        /// <summary>
        /// Checks everything that can be checked without the database. Cleans the text fields in place and merges duplicate cart lines.
        /// Messages are plain text - HTML-encode before showing them.
        /// </summary>
        public static bool ValidateRequest(OrderRequest r, out List<string> errors)
        {
            errors = new List<string>();
            if (r == null)
            {
                errors.Add("Something went wrong with your order. Please try again.");
                return false;
            }

            if (r.CustomerId <= 0) errors.Add("Please log in to place an order.");

            // cart lines: merge duplicates, then check quantities
            var merged = new List<OrderLine>();
            if (r.Lines != null)
            {
                foreach (var g in r.Lines.Where(l => l != null).GroupBy(l => l.ProductId))
                    merged.Add(new OrderLine { ProductId = g.Key, Quantity = g.Sum(l => l.Quantity) });
            }
            r.Lines = merged;
            if (merged.Count == 0) errors.Add("Your cart is empty.");
            if (merged.Any(l => l.ProductId <= 0 || l.Quantity < 1 || l.Quantity > CartHelper.MaxPerLine))
                errors.Add("One of the quantities in your cart isn't valid (1 to " + CartHelper.MaxPerLine + " per item).");

            // method
            r.Method = Clean(r.Method);
            bool delivery = r.Method == MethodDelivery;
            bool collection = r.Method == MethodCollection;
            if (!delivery && !collection) errors.Add("Please choose delivery or click & collect.");

            // contact details
            r.FirstName = Clean(r.FirstName);
            r.LastName = Clean(r.LastName);
            r.Email = Clean(r.Email);
            r.Phone = Regex.Replace(Clean(r.Phone), @"[\s\-\(\)]", "");

            if (r.FirstName.Length == 0 || r.FirstName.Length > 50 || !NameFormat.IsMatch(r.FirstName))
                errors.Add("Please enter your first name (letters only, up to 50 characters).");
            if (r.LastName.Length == 0 || r.LastName.Length > 50 || !NameFormat.IsMatch(r.LastName))
                errors.Add("Please enter your last name (letters only, up to 50 characters).");
            if (r.Email.Length == 0 || r.Email.Length > 100 || !EmailFormat.IsMatch(r.Email))
                errors.Add("Please enter a valid email address.");
            if (!PhoneFormat.IsMatch(r.Phone))
                errors.Add("Please enter a valid phone number (9 to 15 digits).");

            // delivery address
            if (delivery)
            {
                r.UnitNumber = Clean(r.UnitNumber);
                r.UnitName = Clean(r.UnitName);
                r.StreetNumber = Clean(r.StreetNumber);
                r.StreetName = Clean(r.StreetName);
                r.Suburb = Clean(r.Suburb);
                r.PostalCode = Clean(r.PostalCode);
                r.Instructions = Clean(r.Instructions);

                if (r.UnitNumber.Length > 10) errors.Add("Unit number can be at most 10 characters.");
                if (r.UnitName.Length > 50) errors.Add("Complex or building name can be at most 50 characters.");
                if (!StreetNumberFormat.IsMatch(r.StreetNumber)) errors.Add("Please enter a valid street number (up to 10 characters).");
                if (r.StreetName.Length == 0 || r.StreetName.Length > 100) errors.Add("Please enter your street name (up to 100 characters).");
                if (r.Suburb.Length == 0 || r.Suburb.Length > 50) errors.Add("Please enter your suburb (up to 50 characters).");
                if (!PostalFormat.IsMatch(r.PostalCode)) errors.Add("Please enter a 4-digit postal code.");
                if (r.Instructions.Length > 250) errors.Add("Delivery instructions can be at most 250 characters.");
            }

            // time slot (checked again inside PlaceOrder)
            if (delivery || collection)
            {
                if (!r.SlotStart.HasValue)
                {
                    errors.Add(delivery ? "Please choose a delivery window." : "Please choose a collection time.");
                }
                else
                {
                    DateTime now = SlotHelper.Now();
                    bool ok = delivery
                        ? SlotHelper.IsValidDeliverySlot(r.SlotStart.Value, now)
                        : SlotHelper.IsValidCollectionSlot(r.SlotStart.Value, now);
                    if (!ok) errors.Add("That time slot isn't available any more. Please pick another one.");
                }
            }

            r.PromoCode = Clean(r.PromoCode);
            if (r.PromoCode.Length > 0 && !PromoFormat.IsMatch(r.PromoCode))
                errors.Add("Promo codes only contain letters, numbers, - and _.");

            if (!r.AgeConfirmed)
                errors.Add("Please confirm that you are 18 or older and will show valid ID on " + (collection ? "collection." : "delivery."));

            return errors.Count == 0;
        }

        /// <summary>Checks the dummy card. Only the last four digits are ever returned or stored.</summary>
        public static bool ValidateCard(OrderRequest r, out string lastFour, out List<string> errors)
        {
            errors = new List<string>();
            lastFour = null;

            string cardName = Regex.Replace((r.CardName ?? "").Trim(), @"\s+", " ");
            if (cardName.Length < 2 || cardName.Length > 50 || !NameFormat.IsMatch(cardName))
                errors.Add("Please enter the name on the card (letters only, 2 to 50 characters).");

            string digits = Regex.Replace(r.CardNumber ?? "", @"[\s\-]", "");
            if (!Regex.IsMatch(digits, @"^[0-9]{13,19}$") || !PassesLuhn(digits))
                errors.Add("Please enter a valid card number.");
            else
                lastFour = digits.Substring(digits.Length - 4);

            DateTime now = SlotHelper.Now();
            Match m = Regex.Match((r.CardExpiry ?? "").Trim(), @"^(0[1-9]|1[0-2])\s*/\s*([0-9]{2}|[0-9]{4})$");
            if (!m.Success)
            {
                errors.Add("Enter the card's expiry date as MM/YY.");
            }
            else
            {
                int month = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                int year = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                if (year < 100) year += 2000;

                if (year < 2000 || year > now.Year + 20)
                    errors.Add("Please check the card's expiry year.");
                else if (new DateTime(year, month, 1).AddMonths(1).AddDays(-1) < now.Date)
                    errors.Add("That card has expired.");
            }

            if (!Regex.IsMatch((r.CardCvv ?? "").Trim(), @"^[0-9]{3,4}$"))
                errors.Add("Enter the 3 or 4 digit security code.");

            return errors.Count == 0;
        }

        private static bool PassesLuhn(string digits)
        {
            int sum = 0;
            bool alternate = false;
            for (int i = digits.Length - 1; i >= 0; i--)
            {
                int n = digits[i] - '0';
                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }
                sum += n;
                alternate = !alternate;
            }
            return sum % 10 == 0;
        }

        /// <summary>Age from a 13-digit SA ID number, using the same century rule as the POS. False when the number can't be read.</summary>
        public static bool TryGetAge(string idNumber, DateTime today, out int age)
        {
            age = 0;
            string id = (idNumber ?? "").Trim();
            if (!Regex.IsMatch(id, @"^[0-9]{13}$")) return false;

            int yy = int.Parse(id.Substring(0, 2), CultureInfo.InvariantCulture);
            int mm = int.Parse(id.Substring(2, 2), CultureInfo.InvariantCulture);
            int dd = int.Parse(id.Substring(4, 2), CultureInfo.InvariantCulture);
            int fullYear = yy <= (today.Year % 100) ? 2000 + yy : 1900 + yy;

            DateTime birth;
            try { birth = new DateTime(fullYear, mm, dd); }
            catch (ArgumentOutOfRangeException) { return false; }

            age = today.Year - birth.Year;
            if (today < birth.AddYears(age)) age--;
            return age >= 0;
        }

        // ------------------------------------------------------------------ customer + quote

        public static CustomerProfile GetCustomerProfile(int customerId)
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                using (var cmd = Cmd(@"SELECT Customer_FirstName, Customer_LastName, Customer_EmailAddress, Customer_PhoneNumber,
                                              Customer_UnitNumber, Customer_UnitName, Customer_StreetNumber, Customer_StreetName,
                                              Customer_Suburb, Customer_PostalCode
                                       FROM Customer WHERE Customer_ID = @Id", conn, null))
                {
                    P(cmd, "@Id", customerId);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;
                        return new CustomerProfile
                        {
                            FirstName = Convert.ToString(r[0]),
                            LastName = Convert.ToString(r[1]),
                            Email = Convert.ToString(r[2]),
                            Phone = Convert.ToString(r[3]),
                            UnitNumber = Convert.ToString(r[4]),
                            UnitName = Convert.ToString(r[5]),
                            StreetNumber = Convert.ToString(r[6]),
                            StreetName = Convert.ToString(r[7]),
                            Suburb = Convert.ToString(r[8]),
                            PostalCode = Convert.ToString(r[9])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("OrderHelper.GetCustomerProfile: " + ex);
                return null;
            }
        }

        private static bool LoadCustomer(SqlConnection conn, SqlTransaction tx, int customerId, bool lockRow,
                                         out bool active, out int balance, out string idNumber)
        {
            active = false;
            balance = 0;
            idNumber = null;

            string sql = "SELECT Customer_Status, Customer_IDNumber, Customer_LoyaltyPointsBalance FROM Customer "
                       + (lockRow ? "WITH (UPDLOCK, ROWLOCK) " : "") + "WHERE Customer_ID = @Id";
            using (var cmd = Cmd(sql, conn, tx))
            {
                P(cmd, "@Id", customerId);
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return false;
                    active = !string.Equals(Convert.ToString(r[0]).Trim(), "Inactive", StringComparison.OrdinalIgnoreCase);
                    idNumber = Convert.ToString(r[1]);
                    balance = r.IsDBNull(2) ? 0 : Convert.ToInt32(r[2]);
                    return true;
                }
            }
        }

        private static List<QuoteLine> BuildLines(IList<OrderLine> lines, Dictionary<int, PricingHelper.PriceInfo> prices,
                                                  IDictionary<int, string> names, out string error)
        {
            error = null;
            var list = new List<QuoteLine>();
            foreach (var l in lines)
            {
                PricingHelper.PriceInfo p;
                if (!prices.TryGetValue(l.ProductId, out p))
                {
                    error = "One of the items in your cart is no longer available.";
                    return null;
                }
                string name;
                if (names == null || !names.TryGetValue(l.ProductId, out name)) name = "Item #" + l.ProductId;

                list.Add(new QuoteLine
                {
                    ProductId = l.ProductId,
                    Name = name,
                    Quantity = l.Quantity,
                    OriginalPrice = p.ShelfPrice,
                    UnitPrice = p.FinalPrice,
                    DiscountId = p.DiscountId
                });
            }
            return list;
        }

        // The single place where totals are worked out (shared by the checkout page and PlaceOrder)
        private static void ApplyTotals(Quote q, string method, PromoHelper.PromoResult promo, bool active, int balance, bool usePoints)
        {
            q.Subtotal = q.Lines.Sum(l => l.LineOriginal);
            q.GoodsAfterItems = q.Lines.Sum(l => l.LineTotal);
            q.ItemDiscount = q.Subtotal - q.GoodsAfterItems;

            q.PromoAmount = 0m;
            q.PromoDiscountId = null;
            q.PromoValid = false;
            q.PromoError = false;
            q.PromoCode = null;
            q.PromoMessage = null;
            if (promo != null)
            {
                q.PromoCode = promo.Code;
                q.PromoMessage = promo.Message;
                q.PromoError = promo.IsError;
                q.PromoValid = promo.IsValid;
                if (promo.IsValid)
                {
                    q.PromoAmount = promo.DiscountAmount;
                    q.PromoDiscountId = promo.DiscountId;
                }
            }
            q.GoodsAfterPromo = Math.Max(0m, q.GoodsAfterItems - q.PromoAmount);

            // Delivery thresholds use the goods total after item discounts but BEFORE the promo code
            bool delivery = method == MethodDelivery;
            q.DeliveryAllowed = !delivery || q.GoodsAfterItems >= MinDeliveryOrder;
            q.DeliveryFee = delivery && q.GoodsAfterItems < FreeDeliveryThreshold ? DeliveryFeeAmount : 0m;
            q.DeliveryMessage = (delivery && !q.DeliveryAllowed)
                ? "Delivery needs a minimum order of " + Money(MinDeliveryOrder) + " (after product discounts). Add "
                  + Money(MinDeliveryOrder - q.GoodsAfterItems) + " more, or choose click & collect."
                : null;

            q.Total = q.GoodsAfterPromo + q.DeliveryFee;
            q.Vat = Math.Round(q.Total * 15m / 115m, 2);   // 15% VAT inclusive

            q.CustomerActive = active;
            q.PointsBalance = balance;
            q.PointsNeeded = (int)Math.Ceiling(q.Total);
            q.CanUsePoints = active && q.Total > 0m && balance >= q.PointsNeeded;   // balance must cover the WHOLE order
            q.UsePoints = usePoints && q.CanUsePoints;
            // 1 point per R10 of goods (after promo); inactive accounts and points-paid orders earn nothing
            q.PointsToEarn = (active && !q.UsePoints) ? (int)Math.Floor(q.GoodsAfterPromo / RandPerPoint) : 0;
        }

        /// <summary>Totals for the checkout page. Never throws; check Ok/Error.</summary>
        public static Quote GetQuote(int customerId, IList<OrderLine> lines, IDictionary<int, string> names,
                                     string method, string promoCode, bool usePoints)
        {
            var q = new Quote();
            try
            {
                if (lines == null || lines.Count == 0) { q.Error = "Your cart is empty."; return q; }

                var prices = PricingHelper.GetPrices(ConnStr, lines.Select(l => l.ProductId));
                string err;
                List<QuoteLine> ql = BuildLines(lines, prices, names, out err);
                if (ql == null) { q.Error = err; return q; }
                q.Lines = ql;

                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    bool active; int balance; string idNumber;
                    if (!LoadCustomer(conn, null, customerId, false, out active, out balance, out idNumber))
                    {
                        q.Error = "We couldn't find your account. Please log in again.";
                        return q;
                    }

                    PromoHelper.PromoResult promo = null;
                    if (!string.IsNullOrWhiteSpace(promoCode))
                        promo = PromoHelper.Validate(promoCode, customerId, ql.Sum(x => x.LineTotal), conn, null);

                    ApplyTotals(q, method, promo, active, balance, usePoints);
                }
                q.Ok = true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("OrderHelper.GetQuote: " + ex);
                q.Ok = false;
                q.Error = "We couldn't load your order totals right now. Please try again.";
            }
            return q;
        }

        // ------------------------------------------------------------------ placing an order

        public static OrderResult PlaceOrder(OrderRequest req)
        {
            try
            {
                return PlaceOrderCore(req);
            }
            catch (SqlException ex) when (ex.Number == 1205)   // deadlock victim
            {
                Trace.TraceError("OrderHelper.PlaceOrder deadlock: " + ex);
                return Fail("Our shop is busy right now. Your order was not placed and you were not charged - please try again.");
            }
            catch (Exception ex)
            {
                Trace.TraceError("OrderHelper.PlaceOrder: " + ex);
                return Fail("We couldn't place your order because of a problem on our side. You were not charged. Please try again in a moment.");
            }
        }

        private static OrderResult PlaceOrderCore(OrderRequest req)
        {
            List<string> errors;
            if (!ValidateRequest(req, out errors)) return Fail(string.Join(" ", errors));

            DateTime now = SlotHelper.Now();

            // Prices are read BEFORE the transaction starts, so this read can never wait on our own row locks
            var prices = PricingHelper.GetPrices(ConnStr, req.Lines.Select(l => l.ProductId));

            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                SqlTransaction tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {
                    OrderResult result = PlaceInTransaction(req, prices, now, conn, tx);
                    if (result.Success) tx.Commit();
                    else SafeRollback(tx);
                    return result;
                }
                catch
                {
                    SafeRollback(tx);
                    throw;
                }
                finally
                {
                    tx.Dispose();
                }
            }
        }

        private static OrderResult PlaceInTransaction(OrderRequest req, Dictionary<int, PricingHelper.PriceInfo> prices,
                                                      DateTime now, SqlConnection conn, SqlTransaction tx)
        {
            var ids = req.Lines.Select(l => l.ProductId).Distinct().ToList();

            // 1. Lock the product rows and re-check stock
            var names = new Dictionary<int, string>();
            var stock = new Dictionary<int, int>();
            var activeIds = new HashSet<int>();
            using (var cmd = Cmd("SELECT Product_ID, Product_Name, Product_QuantityInStock, Product_Status FROM Product WITH (UPDLOCK, ROWLOCK) WHERE Product_ID IN ("
                                 + string.Join(",", ids) + ")", conn, tx))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    int id = Convert.ToInt32(rd[0]);
                    names[id] = Convert.ToString(rd[1]);
                    stock[id] = rd.IsDBNull(2) ? 0 : Convert.ToInt32(rd[2]);
                    if (string.Equals(Convert.ToString(rd[3]).Trim(), "Active", StringComparison.OrdinalIgnoreCase)) activeIds.Add(id);
                }
            }

            var problems = new List<string>();
            foreach (var l in req.Lines)
            {
                string name = names.ContainsKey(l.ProductId) ? names[l.ProductId] : "An item in your cart";
                if (!names.ContainsKey(l.ProductId) || !activeIds.Contains(l.ProductId))
                    problems.Add(name + " is no longer available.");
                else if (stock[l.ProductId] <= 0)
                    problems.Add(name + " is out of stock.");
                else if (stock[l.ProductId] < l.Quantity)
                    problems.Add("Only " + stock[l.ProductId] + " of " + name + " left in stock.");
            }
            if (problems.Count > 0)
                return Fail(string.Join(" ", problems) + " Please update your cart and try again.", true);

            // 2. Customer: lock the row, check age
            bool active; int balance; string idNumber;
            if (!LoadCustomer(conn, tx, req.CustomerId, true, out active, out balance, out idNumber))
                return Fail("We couldn't find your account. Please log in again.");

            int age;
            if (!TryGetAge(idNumber, now.Date, out age))
                return Fail("We couldn't read the ID number on your account to confirm your age. Please contact us on 082 405 5932 to update your details.");
            if (age < 18)
                return Fail("You must be 18 or older to buy alcohol, so your order was not placed.");

            // 3. Lines, promo and totals (promo re-validated inside the transaction)
            string err;
            List<QuoteLine> ql = BuildLines(req.Lines, prices, names, out err);
            if (ql == null) return Fail(err, true);

            PromoHelper.PromoResult promo = null;
            if (!string.IsNullOrWhiteSpace(req.PromoCode))
            {
                promo = PromoHelper.Validate(req.PromoCode, req.CustomerId, ql.Sum(x => x.LineTotal), conn, tx);
                if (promo.IsError) return Fail(promo.Message);
                if (!promo.IsValid) return Fail(promo.Message + " Please remove the code and place your order again.");
            }

            var q = new Quote { Lines = ql };
            ApplyTotals(q, req.Method, promo, active, balance, req.UsePoints);

            if (req.Method == MethodDelivery && !q.DeliveryAllowed) return Fail(q.DeliveryMessage);
            if (req.UsePoints && !q.UsePoints)
                return Fail("You no longer have enough loyalty points to pay for this order. Please untick the points option and pay by card.");

            if (req.CheckExpectedTotal && Math.Abs(req.ExpectedTotal - q.Total) > 0.005m)
                return Fail("Prices or discounts changed while you were checking out. Your new total is " + Money(q.Total)
                            + ". Please review it and place your order again.");

            // 4. Payment details (dummy card, only the last four digits are kept)
            string lastFour = null;
            bool needsCard = !q.UsePoints && q.Total > 0m;
            if (needsCard)
            {
                List<string> cardErrors;
                if (!ValidateCard(req, out lastFour, out cardErrors)) return Fail(string.Join(" ", cardErrors));
            }

            // 4b. Simulated payment portal: a decline rolls the whole transaction back, so nothing is saved
            string paymentRef = null;
            if (needsCard)
            {
                PaymentGateway.Result auth = PaymentGateway.Authorise(req.CardNumber, req.CardExpiry, q.Total);
                if (!auth.Approved) return Fail(auth.Message);
                paymentRef = auth.Reference;
            }

            int employeeId = LookupId(conn, tx, "SELECT TOP 1 Employee_ID FROM Employee WHERE Employee_Username = @V", OnlineEmployeeUsername, "the 'onlinestore' employee");
            int paymentMethodId = LookupId(conn, tx,
                "SELECT TOP 1 PaymentMethod_ID FROM PaymentMethod WHERE PaymentMethod_Name = @V AND PaymentMethod_Status = 'Active'",
                q.UsePoints ? "Loyalty Points" : "Card", "the payment method");

            // 5. Take the stock (guarded, so it can never go below zero)
            foreach (var l in ql)
            {
                int rows;
                using (var cmd = Cmd(@"UPDATE Product SET Product_QuantityInStock = Product_QuantityInStock - @Q
                                       WHERE Product_ID = @Id AND Product_QuantityInStock >= @Q AND Product_Status = 'Active'", conn, tx))
                {
                    P(cmd, "@Q", l.Quantity);
                    P(cmd, "@Id", l.ProductId);
                    rows = cmd.ExecuteNonQuery();
                }
                if (rows != 1)
                    return Fail(l.Name + " just sold out. Please update your cart and try again.", true);
            }

            // 6. Loyalty points: spend them, or earn them
            int pointsUsed = q.UsePoints ? q.PointsNeeded : 0;
            int pointsEarned = q.PointsToEarn;
            int delta = q.UsePoints ? -pointsUsed : pointsEarned;
            if (delta != 0)
            {
                int rows;
                using (var cmd = Cmd(@"UPDATE Customer SET Customer_LoyaltyPointsBalance = Customer_LoyaltyPointsBalance + @D
                                       WHERE Customer_ID = @C AND Customer_LoyaltyPointsBalance + @D >= 0", conn, tx))
                {
                    P(cmd, "@D", delta);
                    P(cmd, "@C", req.CustomerId);
                    rows = cmd.ExecuteNonQuery();
                }
                if (rows != 1) return Fail("Your loyalty points balance changed while you were checking out. Please try again.");
            }

            // 7. Sale
            int saleId;
            using (var cmd = Cmd(@"INSERT INTO Sale (Customer_ID, Employee_ID, SaleType_ID, Discount_ID, Sale_DateTime, Sale_Subtotal,
                                                     Sale_DiscountAmount, Sale_TotalAmount, Sale_LoyaltyPointsEarned, Sale_Status)
                                   VALUES (@Cust, @Emp, @Type, @Disc, @When, @Sub, @DiscAmt, @Total, @Pts, 'Completed');
                                   SELECT CAST(SCOPE_IDENTITY() AS int);", conn, tx))
            {
                P(cmd, "@Cust", req.CustomerId);
                P(cmd, "@Emp", employeeId);
                P(cmd, "@Type", OnlineSaleTypeId);
                P(cmd, "@Disc", q.PromoDiscountId);
                P(cmd, "@When", now);
                P(cmd, "@Sub", q.Subtotal);
                P(cmd, "@DiscAmt", q.ItemDiscount + q.PromoAmount);
                P(cmd, "@Total", q.Total);
                P(cmd, "@Pts", pointsEarned);
                saleId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // 8. Sale lines
            foreach (var l in ql)
            {
                using (var cmd = Cmd(@"INSERT INTO SaleLine (Sale_ID, Product_ID, Discount_ID, SaleLine_Quantity, SaleLine_OriginalUnitPrice,
                                                             SaleLine_UnitPriceAfterDiscount, SaleLine_Subtotal)
                                       VALUES (@S, @P, @D, @Q, @Orig, @After, @Sub)", conn, tx))
                {
                    P(cmd, "@S", saleId);
                    P(cmd, "@P", l.ProductId);
                    P(cmd, "@D", l.DiscountId);
                    P(cmd, "@Q", l.Quantity);
                    P(cmd, "@Orig", l.OriginalPrice);
                    P(cmd, "@After", l.UnitPrice);
                    P(cmd, "@Sub", l.LineTotal);
                    cmd.ExecuteNonQuery();
                }
            }

            // 9. Payment
            using (var cmd = Cmd(@"INSERT INTO Payment (Sale_ID, PaymentMethod_ID, Payment_DateTime, Payment_LoyaltyPointsUsed, Payment_AmountPaid,
                                                        Payment_AmountTendered, Payment_ChangeAmount, Payment_Status)
                                   VALUES (@S, @M, @When, @Pts, @Paid, @Tendered, 0, 'Completed')", conn, tx))
            {
                P(cmd, "@S", saleId);
                P(cmd, "@M", paymentMethodId);
                P(cmd, "@When", now);
                P(cmd, "@Pts", pointsUsed);
                P(cmd, "@Paid", q.Total);
                P(cmd, "@Tendered", q.Total);
                cmd.ExecuteNonQuery();
            }

            // 10. Online order
            bool delivery = req.Method == MethodDelivery;
            string collectionCode = delivery ? null : NewCollectionCode(conn, tx);
            int onlineOrderId;
            using (var cmd = Cmd(@"INSERT INTO OnlineOrder (Sale_ID, OnlineOrder_Method, OnlineOrder_FirstName, OnlineOrder_LastName,
                                       OnlineOrder_EmailAddress, OnlineOrder_PhoneNumber, OnlineOrder_UnitNumber, OnlineOrder_UnitName,
                                       OnlineOrder_StreetNumber, OnlineOrder_StreetName, OnlineOrder_Suburb, OnlineOrder_City,
                                       OnlineOrder_PostalCode, OnlineOrder_Province, OnlineOrder_Country, OnlineOrder_AdditionalInstructions,
                                       OnlineOrder_DeliveryFee, OnlineOrder_EstimatedDateTime, OnlineOrder_CollectionCode,
                                       OnlineOrder_CollectionSlot, OnlineOrder_CardLastFour, OnlineOrder_PaymentStatus, OnlineOrder_IDChecked)
                                   VALUES (@S, @Method, @First, @Last, @Email, @Phone, @UnitNo, @UnitName, @StreetNo, @Street, @Suburb, @City,
                                       @Postal, @Province, @Country, @Instr, @Fee, @Est, @Code, @Slot, @Card, 'Paid', 0);
                                   SELECT CAST(SCOPE_IDENTITY() AS int);", conn, tx))
            {
                P(cmd, "@S", saleId);
                P(cmd, "@Method", req.Method);
                P(cmd, "@First", req.FirstName);
                P(cmd, "@Last", req.LastName);
                P(cmd, "@Email", req.Email);
                P(cmd, "@Phone", req.Phone);
                P(cmd, "@UnitNo", delivery ? req.UnitNumber : null);
                P(cmd, "@UnitName", delivery ? req.UnitName : null);
                P(cmd, "@StreetNo", delivery ? req.StreetNumber : null);
                P(cmd, "@Street", delivery ? req.StreetName : null);
                P(cmd, "@Suburb", delivery ? req.Suburb : null);
                P(cmd, "@City", delivery ? FixedCity : null);
                P(cmd, "@Postal", delivery ? req.PostalCode : null);
                P(cmd, "@Province", delivery ? FixedProvince : null);
                P(cmd, "@Country", delivery ? FixedCountry : null);
                P(cmd, "@Instr", delivery ? req.Instructions : null);
                P(cmd, "@Fee", q.DeliveryFee);
                P(cmd, "@Est", delivery ? (object)req.SlotStart.Value : null);
                P(cmd, "@Code", collectionCode);
                P(cmd, "@Slot", delivery ? null : (object)req.SlotStart.Value);
                P(cmd, "@Card", lastFour);
                onlineOrderId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // 11. First status row, through the one shared method, inside this same transaction
            string placedNote = "Order placed online"
                + (paymentRef != null ? ". Payment reference " + paymentRef : (q.UsePoints ? ". Paid with loyalty points" : ""));
            StatusResult placed = ChangeStatus(onlineOrderId, StPlaced, null, placedNote, false, conn, tx);
            if (!placed.Success) throw new InvalidOperationException("Could not record the 'Placed' status: " + placed.Message);

            return new OrderResult
            {
                Success = true,
                Message = "Order placed.",
                SaleId = saleId,
                OnlineOrderId = onlineOrderId,
                Total = q.Total,
                PointsEarned = pointsEarned,
                CollectionCode = collectionCode
            };
        }

        private static int LookupId(SqlConnection conn, SqlTransaction tx, string sql, string value, string what)
        {
            using (var cmd = Cmd(sql, conn, tx))
            {
                P(cmd, "@V", value);
                object o = cmd.ExecuteScalar();
                if (o == null || o == DBNull.Value)
                    throw new InvalidOperationException("Setup problem: could not find " + what + ".");
                return Convert.ToInt32(o);
            }
        }

        private static string NewCollectionCode(SqlConnection conn, SqlTransaction tx)
        {
            const string alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";   // no 0/O/1/I/L, easy to read out loud
            using (var rng = new RNGCryptoServiceProvider())
            {
                for (int attempt = 0; attempt < 10; attempt++)
                {
                    byte[] bytes = new byte[6];
                    rng.GetBytes(bytes);
                    var chars = new char[6];
                    for (int i = 0; i < 6; i++) chars[i] = alphabet[bytes[i] % alphabet.Length];
                    string code = new string(chars);

                    using (var cmd = Cmd("SELECT COUNT(*) FROM OnlineOrder WHERE OnlineOrder_CollectionCode = @C", conn, tx))
                    {
                        P(cmd, "@C", code);
                        if (Convert.ToInt32(cmd.ExecuteScalar()) == 0) return code;
                    }
                }
            }
            throw new InvalidOperationException("Could not generate a unique collection code.");
        }

        // ------------------------------------------------------------------ status changes (the ONLY code that inserts history rows)

        public static string GetCurrentStatus(int onlineOrderId, SqlConnection conn, SqlTransaction tx)
        {
            using (var cmd = Cmd(@"SELECT TOP 1 OnlineOrderStatusHistory_Status FROM OnlineOrderStatusHistory
                                   WHERE OnlineOrder_ID = @Id ORDER BY OnlineOrderStatusHistory_ID DESC", conn, tx))
            {
                P(cmd, "@Id", onlineOrderId);
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? null : Convert.ToString(o).Trim();
            }
        }

        /// <summary>
        /// Moves an order to its next status. Forward-only: the next step on its path, or Cancelled / Expired.
        /// Cancelled and Expired put the stock and loyalty points back and mark the sale Cancelled.
        /// Pass conn/tx to run inside an existing transaction (PlaceOrder does); otherwise it uses its own.
        /// </summary>
        public static StatusResult ChangeStatus(int onlineOrderId, string newStatus, int? employeeId, string note,
                                                bool idChecked = false, SqlConnection conn = null, SqlTransaction tx = null)
        {
            newStatus = (newStatus ?? "").Trim();
            note = Clean(note);

            if (onlineOrderId <= 0) return StatusFail("That order couldn't be found.");
            if (!AllStatuses.Contains(newStatus, StringComparer.Ordinal)) return StatusFail("That isn't a valid status.");
            if (note.Length > 250) return StatusFail("Notes can be at most 250 characters.");

            bool ownsConnection = conn == null;
            SqlTransaction ownTx = null;
            try
            {
                if (ownsConnection)
                {
                    conn = new SqlConnection(ConnStr);
                    conn.Open();
                    ownTx = conn.BeginTransaction();
                    tx = ownTx;
                }

                StatusResult result = ChangeStatusCore(onlineOrderId, newStatus, employeeId, note, idChecked, conn, tx);

                if (ownsConnection)
                {
                    if (result.Success) ownTx.Commit();
                    else SafeRollback(ownTx);
                }
                return result;
            }
            catch (Exception ex)
            {
                Trace.TraceError("OrderHelper.ChangeStatus: " + ex);
                if (!ownsConnection) throw;   // the caller owns the transaction and will roll it back
                SafeRollback(ownTx);
                return StatusFail("We couldn't update that order right now. Please try again.");
            }
            finally
            {
                if (ownsConnection)
                {
                    if (ownTx != null) ownTx.Dispose();
                    if (conn != null) conn.Dispose();
                }
            }
        }

        private static StatusResult StatusFail(string message)
        {
            return new StatusResult { Success = false, Message = message };
        }

        private static StatusResult ChangeStatusCore(int orderId, string newStatus, int? employeeId, string note,
                                                     bool idChecked, SqlConnection conn, SqlTransaction tx)
        {
            int saleId;
            string method;
            bool alreadyChecked;
            using (var cmd = Cmd("SELECT Sale_ID, OnlineOrder_Method, OnlineOrder_IDChecked FROM OnlineOrder WITH (UPDLOCK, ROWLOCK) WHERE OnlineOrder_ID = @Id", conn, tx))
            {
                P(cmd, "@Id", orderId);
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return StatusFail("That order couldn't be found.");
                    saleId = Convert.ToInt32(r[0]);
                    method = Convert.ToString(r[1]).Trim();
                    alreadyChecked = Convert.ToBoolean(r[2]);
                }
            }

            string current = GetCurrentStatus(orderId, conn, tx);
            string problem = CheckTransition(method, current, newStatus);
            if (problem != null) return StatusFail(problem);

            bool completing = newStatus == StDelivered || newStatus == StCollected;
            bool reversing = newStatus == StCancelled || newStatus == StExpired;

            if (completing && !alreadyChecked && !idChecked)
                return StatusFail("Please confirm the customer's ID was checked (they must be 18 or older) before completing this order.");
            if (newStatus == StCancelled && note.Length == 0)
                return StatusFail("Please give a reason for cancelling this order.");

            if (completing && !alreadyChecked)
            {
                using (var cmd = Cmd("UPDATE OnlineOrder SET OnlineOrder_IDChecked = 1 WHERE OnlineOrder_ID = @Id", conn, tx))
                {
                    P(cmd, "@Id", orderId);
                    cmd.ExecuteNonQuery();
                }
            }

            if (reversing) ReverseSale(orderId, saleId, note.Length > 0 ? note : "Not collected in time", conn, tx);

            using (var cmd = Cmd(@"INSERT INTO OnlineOrderStatusHistory (OnlineOrder_ID, Employee_ID, OnlineOrderStatusHistory_Status,
                                                                         OnlineOrderStatusHistory_DateTime, OnlineOrderStatusHistory_Notes)
                                   VALUES (@O, @E, @S, @When, @N)", conn, tx))
            {
                P(cmd, "@O", orderId);
                P(cmd, "@E", employeeId);
                P(cmd, "@S", newStatus);
                P(cmd, "@When", SlotHelper.Now());
                P(cmd, "@N", note);
                cmd.ExecuteNonQuery();
            }

            return new StatusResult { Success = true, NewStatus = newStatus, Message = "Order marked as " + newStatus + "." };
        }

        /// <summary>The next step on the order's path (null when it is finished).</summary>
        public static string GetNextStatus(string method, string current)
        {
            if (string.IsNullOrWhiteSpace(current)) return StPlaced;
            string[] path = string.Equals((method ?? "").Trim(), MethodDelivery, StringComparison.OrdinalIgnoreCase) ? DeliveryPath : CollectionPath;
            int i = Array.IndexOf(path, current.Trim());
            return (i >= 0 && i < path.Length - 1) ? path[i + 1] : null;
        }

        public static bool IsLive(string status)
        {
            string s = (status ?? "").Trim();
            return s.Length > 0 && s != StDelivered && s != StCollected && s != StCancelled && s != StExpired;
        }

        public static bool CanCancel(string status)
        {
            string s = (status ?? "").Trim();
            return s == StPlaced || s == StPacking || s == StReady;
        }

        /// <summary>Current status of an order (own connection). Null if the order has no history or can't be read.</summary>
        public static string GetCurrentStatus(int onlineOrderId)
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    return GetCurrentStatus(onlineOrderId, conn, null);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("OrderHelper.GetCurrentStatus: " + ex);
                return null;
            }
        }

        /// <summary>
        /// Expires every collection order that has been "Ready for collection" for longer than the given hours
        /// (restocks it, returns points, emails the customer). Returns how many were expired.
        /// </summary>
        public static int ExpireUncollected(int? employeeId, int hours, out int failed)
        {
            failed = 0;
            int done = 0;
            var ids = new List<int>();

            try
            {
                using (var conn = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand(@"
                    SELECT h.OnlineOrder_ID FROM OnlineOrderStatusHistory h
                    WHERE h.OnlineOrderStatusHistory_Status = 'Ready for collection'
                      AND h.OnlineOrderStatusHistory_DateTime <= @Cutoff
                      AND h.OnlineOrderStatusHistory_ID = (SELECT MAX(h2.OnlineOrderStatusHistory_ID)
                                                           FROM OnlineOrderStatusHistory h2 WHERE h2.OnlineOrder_ID = h.OnlineOrder_ID)", conn))
                {
                    cmd.Parameters.AddWithValue("@Cutoff", SlotHelper.Now().AddHours(-Math.Max(1, hours)));
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) ids.Add(Convert.ToInt32(r[0]));
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("OrderHelper.ExpireUncollected (find): " + ex);
                failed = 1;
                return 0;
            }

            foreach (int id in ids)
            {
                string note = "Not collected within " + hours + " hours";
                StatusResult r = ChangeStatus(id, StExpired, employeeId, note);   // each order has its own transaction
                if (r.Success) { done++; OrderEmailHelper.Notify(id, StExpired, note); }
                else { failed++; Trace.TraceWarning("Could not expire order " + id + ": " + r.Message); }
            }
            return done;
        }

        // Returns null when the move is allowed, otherwise a message for the user
        private static string CheckTransition(string method, string current, string next)
        {
            if (current == null) return next == StPlaced ? null : "The first status of an order must be Placed.";

            if (current == StDelivered || current == StCollected || current == StCancelled || current == StExpired)
                return "This order is already " + current.ToLowerInvariant() + " and can't be changed.";

            if (next == StCancelled)
            {
                if (current == StPlaced || current == StPacking || current == StReady) return null;
                return "It's too late to cancel this order because it is already " + current.ToLowerInvariant() + ".";
            }

            if (next == StExpired)
                return (current == StReady && method == MethodCollection) ? null : "Only an uncollected order that is ready for collection can expire.";

            string[] path = method == MethodDelivery ? DeliveryPath : CollectionPath;
            int i = Array.IndexOf(path, current);
            int j = Array.IndexOf(path, next);
            if (j < 0) return "'" + next + "' doesn't apply to a " + method.ToLowerInvariant() + " order.";
            if (j <= i) return "An order can only move forward. It is already '" + current + "'.";
            if (j != i + 1) return "The next step for this order is '" + path[i + 1] + "'.";
            return null;
        }

        // Cancel / expire: restock, give points back, mark the sale cancelled. Runs once only (guarded by Sale_Status).
        private static void ReverseSale(int orderId, int saleId, string reason, SqlConnection conn, SqlTransaction tx)
        {
            int changed;
            using (var cmd = Cmd("UPDATE Sale SET Sale_Status = 'Cancelled' WHERE Sale_ID = @S AND Sale_Status <> 'Cancelled'", conn, tx))
            {
                P(cmd, "@S", saleId);
                changed = cmd.ExecuteNonQuery();
            }

            if (changed == 1)
            {
                using (var cmd = Cmd(@"UPDATE p SET p.Product_QuantityInStock = p.Product_QuantityInStock + sl.SaleLine_Quantity
                                       FROM Product p INNER JOIN SaleLine sl ON sl.Product_ID = p.Product_ID
                                       WHERE sl.Sale_ID = @S", conn, tx))
                {
                    P(cmd, "@S", saleId);
                    cmd.ExecuteNonQuery();
                }

                int? customerId = null;
                int earned = 0;
                using (var cmd = Cmd("SELECT Customer_ID, Sale_LoyaltyPointsEarned FROM Sale WHERE Sale_ID = @S", conn, tx))
                {
                    P(cmd, "@S", saleId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            if (!r.IsDBNull(0)) customerId = Convert.ToInt32(r[0]);
                            earned = r.IsDBNull(1) ? 0 : Convert.ToInt32(r[1]);
                        }
                    }
                }

                int used;
                using (var cmd = Cmd("SELECT ISNULL(SUM(Payment_LoyaltyPointsUsed), 0) FROM Payment WHERE Sale_ID = @S", conn, tx))
                {
                    P(cmd, "@S", saleId);
                    used = Convert.ToInt32(cmd.ExecuteScalar());
                }

                int delta = used - earned;   // points spent come back, points earned are taken back
                if (customerId.HasValue && delta != 0)
                {
                    using (var cmd = Cmd(@"UPDATE Customer SET Customer_LoyaltyPointsBalance =
                                               CASE WHEN Customer_LoyaltyPointsBalance + @D < 0 THEN 0 ELSE Customer_LoyaltyPointsBalance + @D END
                                           WHERE Customer_ID = @C", conn, tx))
                    {
                        P(cmd, "@D", delta);
                        P(cmd, "@C", customerId.Value);
                        cmd.ExecuteNonQuery();
                    }
                }

                using (var cmd = Cmd("UPDATE Payment SET Payment_Status = 'Refunded' WHERE Sale_ID = @S", conn, tx))
                {
                    P(cmd, "@S", saleId);
                    cmd.ExecuteNonQuery();
                }
            }

            using (var cmd = Cmd("UPDATE OnlineOrder SET OnlineOrder_CancellationReason = @R, OnlineOrder_PaymentStatus = 'Refunded' WHERE OnlineOrder_ID = @Id", conn, tx))
            {
                P(cmd, "@R", reason);
                P(cmd, "@Id", orderId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}