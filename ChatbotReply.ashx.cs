using SamsLiquorShop.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace SamsLiquorShop
{
    // Database-powered chatbot answers: ChatbotReply.ashx?q=do you have jameson
    // Store-info questions (hours, location, delivery, loyalty) are still answered in the browser by the
    // chatbot script in Site.Master; everything else comes here. Returns JSON:
    //   { reply: "...", products: [{ name, price, oldPrice, stock, image, url }], link: { text, url } }
    // Read-only: nothing here changes data. Customers only ever see their own points and orders.
    public class ChatbotReply : IHttpHandler, IRequiresSessionState
    {
        private const int MaxMessageLength = 200;
        private const int MaxMessagesPerMinute = 20;

        private static string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        public bool IsReusable => false;

        private class Product
        {
            public string name;
            public string price;
            public string oldPrice;
            public string stock;
            public string image;
            public string url;
        }

        private class Link
        {
            public string text;
            public string url;
        }

        private class Reply
        {
            public string reply;
            public List<Product> products = new List<Product>();
            public Link link;
            public string redirect;   // set when the bot doesn't know: the browser sends the visitor here after a moment
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            Reply answer;
            try
            {
                // Unvalidated: a message like "<b>gin</b>" would otherwise trip ASP.NET's request validation.
                // Safe here: the message is only used as SQL parameters, and replies are shown as plain text.
                string message = (context.Request.Unvalidated.QueryString["q"] ?? "").Trim();
                if (message.Length > MaxMessageLength) message = message.Substring(0, MaxMessageLength);

                if (message.Length == 0)
                    answer = new Reply { reply = "Ask me about a product, today's specials, or your order." };
                else if (!WithinRateLimit(context))
                    answer = new Reply { reply = "You're sending messages quickly. Give me a moment and try again." };
                else
                    answer = Answer(context, message);
            }
            catch (Exception)
            {
                // Database or network problem: the browser shows its usual fallback message
                context.Response.StatusCode = 500;
                answer = new Reply { reply = "Sorry, I couldn't look that up right now. Please try again in a moment." };
            }

            context.Response.Write(new JavaScriptSerializer().Serialize(answer));
        }

        // ---------- WHICH KIND OF QUESTION? ----------
        private Reply Answer(HttpContext context, string message)
        {
            string text = message.ToLowerInvariant();

            if (Regex.IsMatch(text, @"\b(my )?(points?|loyalty balance|balance)\b") && !Regex.IsMatch(text, @"\bhow (do|does|can)\b"))
                return PointsAnswer(context);

            if (Regex.IsMatch(text, @"\b(my|last|latest|recent|previous) (order|purchase|receipt)s?\b|\bwhere('s| is) my order\b|\border status\b"))
                return LastOrderAnswer(context);

            if (Regex.IsMatch(text, @"\b(special|specials|sale|sales|discount|discounts|deal|deals|promo|promotion|promotions)\b"))
                return SpecialsAnswer(text);

            Reply byCategory = CategoryAnswer(text);
            if (byCategory != null) return byCategory;

            Reply byProduct = ProductAnswer(text);
            if (byProduct != null) return byProduct;

            // Not something the bot knows: send the visitor to the FAQ page instead
            return new Reply
            {
                reply = "I'm not sure about that one, so I'm taking you to our FAQ page, where you'll find answers to common questions.",
                link = new Link { text = "Go to the FAQ", url = "/FAQ.aspx" },
                redirect = "/FAQ.aspx"
            };
        }

        // ---------- POINTS (logged-in customers) ----------
        private Reply PointsAnswer(HttpContext context)
        {
            int customerId = LoggedInCustomerId(context);
            if (customerId == 0) return LoginPrompt("see your loyalty points");

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(Customer_LoyaltyPointsBalance, 0) FROM Customer WHERE Customer_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", customerId);
                conn.Open();
                int points = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

                // 1 point = R1 to redeem, 1 point earned per R10 (same rules as the POS)
                return new Reply
                {
                    reply = "You have " + points.ToString("N0") + " loyalty points, worth " + ((decimal)points).ToString("C")
                            + " off your next order. You earn 1 point for every R10 you spend.",
                    link = new Link { text = "How rewards work", url = "/Rewards.aspx" }
                };
            }
        }

        // ---------- LAST ORDER (logged-in customers) ----------
        private Reply LastOrderAnswer(HttpContext context)
        {
            int customerId = LoggedInCustomerId(context);
            if (customerId == 0) return LoginPrompt("see your orders");

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT TOP 1 s.Sale_ID, s.Sale_DateTime, s.Sale_TotalAmount,
                       (SELECT ISNULL(SUM(SaleLine_Quantity), 0) FROM SaleLine WHERE Sale_ID = s.Sale_ID) AS ItemCount
                FROM Sale s
                WHERE s.Customer_ID = @Id
                ORDER BY s.Sale_DateTime DESC", conn))
            {
                cmd.Parameters.AddWithValue("@Id", customerId);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        return new Reply
                        {
                            reply = "You haven't placed any orders yet.",
                            link = new Link { text = "Browse the shop", url = "/Shop.aspx" }
                        };
                    }

                    int saleId = Convert.ToInt32(r["Sale_ID"]);
                    int items = Convert.ToInt32(r["ItemCount"]);
                    return new Reply
                    {
                        reply = "Your last order was " + ReceiptHelper.InvoiceNumber(saleId) + " on "
                                + Convert.ToDateTime(r["Sale_DateTime"]).ToString("d MMM yyyy") + ": "
                                + items + (items == 1 ? " item" : " items") + ", " + Convert.ToDecimal(r["Sale_TotalAmount"]).ToString("C") + ".",
                        link = new Link { text = "View receipt", url = "/Customer/Receipt.aspx?sale=" + saleId }
                    };
                }
            }
        }

        // ---------- SPECIALS ----------
        // Products with a product or category discount; PricingHelper decides whether the discount is active today.
        private Reply SpecialsAnswer(string text)
        {
            string categoryFilter = MatchCategory(text);

            DataTable candidates = Query(@"
                SELECT p.Product_ID, p.Product_Name, p.Product_QuantityInStock
                FROM Product p
                INNER JOIN Category c ON c.Category_ID = p.Category_ID
                WHERE p.Product_Status = 'Active' AND p.Product_QuantityInStock > 0
                  AND (p.Discount_ID IS NOT NULL OR c.Discount_ID IS NOT NULL)"
                + (categoryFilter != null ? " AND c.Category_Name = @Category" : ""),
                categoryFilter != null ? new SqlParameter("@Category", categoryFilter) : null);

            // "vodka specials" -> only specials whose name mentions vodka
            var productWords = MeaningfulWords(Regex.Replace(text,
                @"\b(special|specials|sale|sales|discount|discounts|deal|deals|promo|promotion|promotions|on)\b", " "))
                .Where(w => categoryFilter == null || !categoryFilter.ToLowerInvariant().Contains(w))
                .ToList();

            var prices = PricingHelper.GetPrices(ConnStr, candidates.AsEnumerable().Select(r => Convert.ToInt32(r["Product_ID"])));
            var onSale = candidates.AsEnumerable()
                .Where(r => prices.ContainsKey(Convert.ToInt32(r["Product_ID"])) && prices[Convert.ToInt32(r["Product_ID"])].OnSale)
                .Where(r => productWords.Count == 0
                            || productWords.Any(w => Convert.ToString(r["Product_Name"]).ToLowerInvariant().Contains(w)))
                .OrderByDescending(r => prices[Convert.ToInt32(r["Product_ID"])].ShelfPrice - prices[Convert.ToInt32(r["Product_ID"])].FinalPrice)
                .Take(5)
                .ToList();

            if (onSale.Count == 0)
            {
                return new Reply
                {
                    reply = productWords.Count > 0
                        ? "There are no " + string.Join(" ", productWords) + " specials right now."
                        : categoryFilter != null
                            ? "There are no " + categoryFilter.ToLowerInvariant() + " specials right now."
                            : "There are no specials running right now. Check back soon!",
                    link = new Link { text = "See the Specials page", url = "/Specials.aspx" }
                };
            }

            var answer = new Reply
            {
                reply = "Here " + (onSale.Count == 1 ? "is a special" : "are some specials") + " on right now"
                        + (categoryFilter != null ? " in " + categoryFilter : "") + ":",
                link = new Link { text = "See all specials", url = "/Specials.aspx" }
            };
            foreach (DataRow r in onSale) answer.products.Add(ToCard(r, prices));
            return answer;
        }

        // ---------- A CATEGORY ----------
        private Reply CategoryAnswer(string text)
        {
            string category = MatchCategory(text);
            if (category == null) return null;

            DataTable rows = Query(@"
                SELECT TOP 5 p.Product_ID, p.Product_Name, p.Product_QuantityInStock
                FROM Product p
                INNER JOIN Category c ON c.Category_ID = p.Category_ID
                WHERE p.Product_Status = 'Active' AND c.Category_Name = @Category
                ORDER BY CASE WHEN p.Product_QuantityInStock > 0 THEN 0 ELSE 1 END, p.Product_Name",
                new SqlParameter("@Category", category));

            if (rows.Rows.Count == 0)
                return new Reply { reply = "We don't have any " + category.ToLowerInvariant() + " listed at the moment." };

            var prices = PricingHelper.GetPrices(ConnStr, rows.AsEnumerable().Select(r => Convert.ToInt32(r["Product_ID"])));
            var answer = new Reply
            {
                reply = "Here's some of our " + category + ":",
                link = new Link { text = "See all " + category, url = "/Shop.aspx?search=" + HttpUtility.UrlEncode(category) }
            };
            foreach (DataRow r in rows.Rows) answer.products.Add(ToCard(r, prices));
            return answer;
        }

        // Finds a category in the message by any main word of its name: "beers" matches "Beer & Cider",
        // "the wines" matches "Wines"
        private static string MatchCategory(string text)
        {
            DataTable categories = Query("SELECT Category_Name FROM Category");
            foreach (DataRow c in categories.Rows)
            {
                string name = Convert.ToString(c["Category_Name"]).Trim();
                foreach (string word in Regex.Split(name.ToLowerInvariant(), @"[^a-z]+"))
                {
                    if (word.Length < 4) continue;
                    string singular = Regex.Replace(word, @"(es|s)$", "");
                    if (Regex.IsMatch(text, @"\b" + Regex.Escape(singular) + @"(s|es)?\b")) return name;
                }
            }
            return null;
        }

        // ---------- A PRODUCT ----------
        private static readonly string[] FillerWords =
        {
            "do", "you", "have", "has", "got", "any", "sell", "stock", "in", "is", "are", "the", "a", "an", "of", "for",
            "how", "much", "price", "prices", "cost", "costs", "what", "whats", "what's", "can", "i", "get", "buy",
            "please", "there", "available", "some", "me", "show", "find", "looking", "want", "need", "your", "it",
            "with", "goes", "good", "best", "cheap", "cheapest", "recommend", "like", "and", "or", "to", "on", "that",
            "this", "which", "would", "should", "go", "anything", "something", "hey", "hello", "thanks", "tell", "about", "know"
        };

        // The words that matter in a message: filler removed, plurals made singular ("gins" -> "gin")
        private static List<string> MeaningfulWords(string text)
        {
            return Regex.Split(Regex.Replace(text, @"[^a-z0-9'&\- ]", " "), @"\s+")
                .Where(w => w.Length >= 3 && !FillerWords.Contains(w))
                .Select(w => w.Length >= 4 && w.EndsWith("s") && !w.EndsWith("ss") ? w.Substring(0, w.Length - 1) : w)
                .Distinct()
                .Take(5)
                .ToList();
        }

        private Reply ProductAnswer(string text)
        {
            var words = MeaningfulWords(text);
            if (words.Count == 0) return null;

            DataTable rows = SearchProducts(words);

            // Nothing? Try a looser search on the start of each word, which catches typos like "jamesn" -> "jame%"
            if (rows.Rows.Count == 0)
            {
                var stems = words.Where(w => w.Length >= 5).Select(w => w.Substring(0, 4)).Distinct().ToList();
                if (stems.Count > 0) rows = SearchProducts(stems, startsWith: true);
            }

            if (rows.Rows.Count == 0) return null;

            var prices = PricingHelper.GetPrices(ConnStr, rows.AsEnumerable().Select(r => Convert.ToInt32(r["Product_ID"])));
            bool anyInStock = rows.AsEnumerable().Any(r => Convert.ToInt32(r["Product_QuantityInStock"]) > 0);
            var answer = new Reply
            {
                reply = anyInStock ? "Here's what I found:" : "I found these, but they're out of stock at the moment:"
            };
            foreach (DataRow r in rows.Rows) answer.products.Add(ToCard(r, prices));
            return answer;
        }

        // Ranks active products by how many of the words appear in their name or brand (top 4)
        private static DataTable SearchProducts(List<string> words, bool startsWith = false)
        {
            var parameters = new List<SqlParameter>();
            var scoreParts = new List<string>();
            for (int i = 0; i < words.Count; i++)
            {
                // Match the start of a word ("jame%" or "% jame%") for typo searches and for short words,
                // so "gin" doesn't match "Original"; longer words can match anywhere
                bool wordStart = startsWith || words[i].Length <= 4;
                parameters.Add(new SqlParameter("@W" + i, wordStart ? words[i] + "%" : "%" + words[i] + "%"));
                parameters.Add(new SqlParameter("@X" + i, "% " + words[i] + "%"));
                scoreParts.Add(wordStart
                    ? "CASE WHEN p.Product_Name LIKE @W" + i + " OR p.Product_Name LIKE @X" + i + " OR ISNULL(p.Product_Brand, '') LIKE @W" + i + " THEN 1 ELSE 0 END"
                    : "CASE WHEN p.Product_Name LIKE @W" + i + " OR ISNULL(p.Product_Brand, '') LIKE @W" + i + " THEN 1 ELSE 0 END");
            }
            string score = string.Join(" + ", scoreParts);

            return Query(@"
                SELECT TOP 4 * FROM (
                    SELECT p.Product_ID, p.Product_Name, p.Product_QuantityInStock, (" + score + @") AS Score
                    FROM Product p
                    WHERE p.Product_Status = 'Active'
                ) t
                WHERE Score > 0
                ORDER BY Score DESC, CASE WHEN Product_QuantityInStock > 0 THEN 0 ELSE 1 END, Product_Name",
                parameters.ToArray());
        }

        // ---------- HELPERS ----------
        private static Product ToCard(DataRow r, Dictionary<int, PricingHelper.PriceInfo> prices)
        {
            int id = Convert.ToInt32(r["Product_ID"]);
            string name = Convert.ToString(r["Product_Name"]);
            int stock = Convert.ToInt32(r["Product_QuantityInStock"]);
            PricingHelper.PriceInfo price = prices.ContainsKey(id) ? prices[id] : null;

            return new Product
            {
                name = name,
                price = price != null ? price.FinalPrice.ToString("C") : "",
                oldPrice = price != null && price.OnSale ? price.ShelfPrice.ToString("C") : "",
                stock = stock <= 0 ? "Out of stock" : stock <= 5 ? "Only " + stock + " left" : "In stock",
                image = "/ProductImage.ashx?id=" + id,
                url = "/Shop.aspx?search=" + HttpUtility.UrlEncode(name)
            };
        }

        private static DataTable Query(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                    foreach (SqlParameter p in parameters) if (p != null) cmd.Parameters.Add(p);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        private static int LoggedInCustomerId(HttpContext context)
        {
            if (context.Session == null || Convert.ToString(context.Session["UserRole"]) != "Customer") return 0;
            int id;
            return int.TryParse(Convert.ToString(context.Session["CustomerID"]), out id) ? id : 0;
        }

        private static Reply LoginPrompt(string what)
        {
            return new Reply
            {
                reply = "Log in as a customer to " + what + ".",
                link = new Link { text = "Log in", url = "/Login.aspx" }
            };
        }

        // About 20 messages a minute per visitor, kept in their session
        private static bool WithinRateLimit(HttpContext context)
        {
            if (context.Session == null) return true;
            var times = context.Session["ChatbotTimes"] as Queue<DateTime> ?? new Queue<DateTime>();
            DateTime now = DateTime.UtcNow;
            while (times.Count > 0 && (now - times.Peek()).TotalSeconds > 60) times.Dequeue();
            if (times.Count >= MaxMessagesPerMinute) return false;
            times.Enqueue(now);
            context.Session["ChatbotTimes"] = times;
            return true;
        }
    }
}
