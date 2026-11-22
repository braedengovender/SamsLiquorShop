using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SamsLiquorShop.Helpers;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SamsLiquorShop
{
    public partial class Cart : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;
        private const decimal FreeDeliveryThreshold = 500m;
        private const decimal DeliveryFee = 70m;
        private const decimal MinDeliveryOrder = 250m;

        protected void Page_Load(object sender, EventArgs e)
        {
            litCartMessage.Text = "";

            if (!IsPostBack)
            {
                ValidateCartForCheckout();   // trims quantities / removes unavailable items and tells the customer
                BindCart();
            }
        }

        private void BindCart()
        {
            var cart = CartHelper.GetCart(Session);
            var saved = CartHelper.GetSavedForLater(Session);

            RefreshCartPrices();

            int itemCount = cart.Sum(c => c.Quantity);
            litItemCountSummary.Text = itemCount == 0 ? "" : itemCount + (itemCount == 1 ? " item" : " items") + " in your cart";

            if (cart.Count == 0)
            {
                pnlCartHasItems.Visible = false;
                pnlCartEmpty.Visible = true;
                LoadRecommended();
            }
            else
            {
                pnlCartHasItems.Visible = true;
                pnlCartEmpty.Visible = false;
                LoadRecommended();

                rptCart.DataSource = cart;
                rptCart.DataBind();

                BindSummary();
            }

            if (saved.Count > 0)
            {
                pnlSavedForLater.Visible = true;
                rptSaved.DataSource = saved;
                rptSaved.DataBind();
            }
            else
            {
                pnlSavedForLater.Visible = false;
            }
        }

        private void BindSummary()
        {
            decimal subtotal = CartHelper.GetCartTotal(Session);
            decimal deliveryFee = subtotal >= FreeDeliveryThreshold ? 0m : DeliveryFee;

            decimal discountAmount = GetPromoAmount(subtotal);
            decimal grandTotal = subtotal - discountAmount;   // delivery is added at checkout (collection is free)

            litSubtotal.Text = "R" + subtotal.ToString("N2");
            if (subtotal < MinDeliveryOrder)
            {
                litDelivery.Text = "Collection only";
                litDeliveryHint.Text = "<p class='cart-delivery-hint'>Add R"
                    + (MinDeliveryOrder - subtotal).ToString("N2") + " more to unlock delivery.</p>";
            }
            else if (deliveryFee == 0)
            {
                litDelivery.Text = "Free";
                litDeliveryHint.Text = "";
            }
            else
            {
                litDelivery.Text = "R" + deliveryFee.ToString("N2");
                litDeliveryHint.Text = "<p class='cart-delivery-hint'>Add R"
                    + (FreeDeliveryThreshold - subtotal).ToString("N2") + " more for free delivery.</p>";
            }

            if (discountAmount > 0)
            {
                pnlDiscountLine.Visible = true;
                litDiscount.Text = "-R" + discountAmount.ToString("N2");
            }
            else
            {
                pnlDiscountLine.Visible = false;
            }

            litGrandTotal.Text = "R" + grandTotal.ToString("N2");
            BindPoints();
        }

        // Promo discount in rand for the code held in Session, re-checked against the current cart and customer.
        // A code that stops being valid (cart dropped under the minimum, customer logged out) is removed.
        private decimal GetPromoAmount(decimal goodsTotal)
        {
            string code = Session["PromoCode"] as string;
            if (string.IsNullOrEmpty(code)) return 0m;

            PromoHelper.PromoResult result = PromoHelper.Validate(code, CurrentCustomerId(), goodsTotal);
            if (result.IsValid) return result.DiscountAmount;

            if (!result.IsError) Session["PromoCode"] = null;   // keep the code if it was only a database hiccup
            litPromoMessage.Text = "<span class='field-hint invalid'>" + Server.HtmlEncode(result.Message)
                + (result.IsError ? "" : " The code was removed.") + "</span>";
            return 0m;
        }

        private void LoadRecommended()
        {
            try { LoadRecommendedCore(); }
            catch (Exception ex)
            {
                LogError(ex);
                pnlRecommended.Visible = false;   // the cart still works without recommendations
            }
        }

        private void LoadRecommendedCore()
        {
            var cart = CartHelper.GetCart(Session);
            var cartIds = new HashSet<int>(cart.Select(c => c.ProductId));
            var wishlist = CartHelper.GetWishlist(Session);

            // Every active, in-stock product that isn't already in the cart
            DataTable products = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(
                "SELECT Product_ID, Product_Name, Product_SellingPrice, Category_ID FROM Product WHERE Product_Status = 'Active' AND Product_QuantityInStock > 0", conn))
            {
                da.Fill(products);
            }

            // Categories of what's in the cart
            var cartCategories = new HashSet<int>();
            if (cartIds.Count > 0)
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand("SELECT DISTINCT Category_ID FROM Product WHERE Product_ID IN (" + string.Join(",", cartIds) + ")", conn))
                {
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read()) cartCategories.Add(Convert.ToInt32(r[0]));
                }
            }

            // "Customers who bought items in your cart also bought": products that share a sale with a cart item
            var together = new Dictionary<int, int>();
            if (cartIds.Count > 0)
            {
                try
                {
                    string q = @"SELECT sl2.Product_ID, COUNT(DISTINCT sl2.Sale_ID) AS Times
                                 FROM SaleLine sl1
                                 INNER JOIN SaleLine sl2 ON sl1.Sale_ID = sl2.Sale_ID AND sl2.Product_ID <> sl1.Product_ID
                                 WHERE sl1.Product_ID IN (" + string.Join(",", cartIds) + @")
                                 GROUP BY sl2.Product_ID";
                    using (SqlConnection conn = new SqlConnection(ConnStr))
                    using (SqlCommand cmd = new SqlCommand(q, conn))
                    {
                        conn.Open();
                        using (SqlDataReader r = cmd.ExecuteReader())
                            while (r.Read()) together[Convert.ToInt32(r[0])] = Convert.ToInt32(r[1]);
                    }
                }
                catch { /* no sales history or different column names: fall back to the other signals */ }
            }

            var cartPrices = cart.Select(c => c.UnitPrice).ToList();
            var scored = new List<KeyValuePair<DataRow, double>>();
            var reasons = new Dictionary<int, string>();

            foreach (DataRow row in products.Rows)
            {
                int id = Convert.ToInt32(row["Product_ID"]);
                if (cartIds.Contains(id)) continue;

                double score = 0;
                string reason = "";
                int times;

                if (together.TryGetValue(id, out times)) { score += 5 + Math.Min(times, 10); reason = "Often bought together"; }
                if (wishlist.Contains(id)) { score += 6; if (reason == "") reason = "On your wishlist"; }
                if (cartCategories.Contains(Convert.ToInt32(row["Category_ID"]))) { score += 3; if (reason == "") reason = "Similar category"; }

                decimal price = Convert.ToDecimal(row["Product_SellingPrice"]);
                if (cartPrices.Any(p => p > 0m && Math.Abs(price - p) <= p * 0.25m)) { score += 2; if (reason == "") reason = "Similar price"; }

                if (score > 0)
                {
                    scored.Add(new KeyValuePair<DataRow, double>(row, score));
                    reasons[id] = reason;
                }
            }

            var rng = new Random();
            var picks = scored.OrderByDescending(s => s.Value).ThenBy(s => rng.Next()).Take(6).Select(s => s.Key).ToList();

            // Not enough signals (empty cart, no wishlist, no history): top up with random picks
            if (picks.Count < 4)
            {
                var pickedIds = new HashSet<int>(picks.Select(p => Convert.ToInt32(p["Product_ID"])));
                foreach (DataRow row in products.Rows.Cast<DataRow>().Where(r => !cartIds.Contains(Convert.ToInt32(r["Product_ID"])) && !pickedIds.Contains(Convert.ToInt32(r["Product_ID"]))).OrderBy(r => rng.Next()).Take(4 - picks.Count))
                {
                    picks.Add(row);
                    reasons[Convert.ToInt32(row["Product_ID"])] = "Picked for you";
                }
            }

            var prices = PricingHelper.GetPrices(ConnStr, picks.Select(p => Convert.ToInt32(p["Product_ID"])));

            DataTable view = new DataTable();
            view.Columns.Add("Product_ID", typeof(int));
            view.Columns.Add("Product_Name", typeof(string));
            view.Columns.Add("Product_SellingPrice", typeof(decimal));
            view.Columns.Add("DiscountedPrice", typeof(decimal));
            view.Columns.Add("IsOnSale", typeof(bool));
            view.Columns.Add("Reason", typeof(string));

            foreach (DataRow p in picks)
            {
                int id = Convert.ToInt32(p["Product_ID"]);
                decimal shelf = Convert.ToDecimal(p["Product_SellingPrice"]);
                decimal final = prices.ContainsKey(id) ? prices[id].FinalPrice : shelf;
                view.Rows.Add(id, p["Product_Name"], shelf, final, final < shelf, reasons.ContainsKey(id) ? reasons[id] : "");
            }

            bool hasTogether = view.Rows.Cast<DataRow>().Any(r => (string)r["Reason"] == "Often bought together");
            litRecommendedHeading.Text = hasTogether
                ? "Customers who bought items in your cart also bought"
                : (cartIds.Count > 0 ? "You might also like" : "You might like");

            pnlRecommended.Visible = view.Rows.Count > 0;
            rptRecommended.DataSource = view;
            rptRecommended.DataBind();
        }

        private void LoadRecommendedOld()
        {
            string query = @"SELECT TOP 4 Product_ID, Product_Name, Product_SellingPrice 
                              FROM Product 
                              WHERE Product_Status = 'Active' AND Product_QuantityInStock > 0
                              ORDER BY NEWID()";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                pnlRecommended.Visible = dt.Rows.Count > 0;
                rptRecommended.DataSource = dt;
                rptRecommended.DataBind();
            }
        }

        protected void rptCart_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int productId = Convert.ToInt32(e.CommandArgument);
            var cart = CartHelper.GetCart(Session);
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            switch (e.CommandName)
            {
                case "Increment":
                    if (item != null) SetQuantity(productId, item.Quantity + 1);
                    break;
                case "Decrement":
                    if (item != null) SetQuantity(productId, item.Quantity - 1);
                    break;
                case "RemoveItem":
                    CartHelper.RemoveFromCart(Session, productId);
                    break;
                case "SaveForLater":
                    CartHelper.MoveToSavedForLater(Session, productId);
                    break;
                case "ViewProduct":
                    ShowDetail(productId);
                    break;
                case "MoveToWishlist":
                    WishlistHelper.AddToWishlist(Session, productId);   // HashSet, so adding twice is harmless
                    CartHelper.RemoveFromCart(Session, productId);
                    break;
            }

            BindCart();
        }

        protected void rptSaved_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int productId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "MoveToCart")
            {
                CartHelper.MoveToCart(Session, productId);
            }
            else if (e.CommandName == "RemoveSaved")
            {
                CartHelper.RemoveFromSavedForLater(Session, productId);
            }

            BindCart();
        }

        protected void rptRecommended_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "AddToCart")
            {
                int productId = Convert.ToInt32(e.CommandArgument);
                string query = "SELECT Product_Name, Product_SellingPrice FROM Product WHERE Product_ID = @Id";
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", productId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var prices = PricingHelper.GetPrices(ConnStr, new[] { productId });
                            decimal price = prices.ContainsKey(productId) ? prices[productId].FinalPrice : Convert.ToDecimal(reader["Product_SellingPrice"]);
                            CartHelper.AddToCart(Session, productId, reader["Product_Name"].ToString(), price);
                        }
                    }
                }
                BindCart();
            }
        }

        protected void btnApplyPromo_Click(object sender, EventArgs e)
        {
            string code = (txtPromoCode.Text ?? "").Trim();

            if (code.Length == 0)
            {
                litPromoMessage.Text = "<span class='field-hint invalid'>Please enter a promo code.</span>";
                return;
            }
            if (code.Length > 20 || !System.Text.RegularExpressions.Regex.IsMatch(code, "^[A-Za-z0-9_-]+$"))
            {
                litPromoMessage.Text = "<span class='field-hint invalid'>Promo codes only contain letters, numbers, - and _.</span>";
                return;
            }
            PromoHelper.PromoResult promo = PromoHelper.Validate(code, CurrentCustomerId(), CartHelper.GetCartTotal(Session));

            if (promo.IsValid)
            {
                Session["PromoCode"] = promo.Code;
                litPromoMessage.Text = "<span class='field-hint valid'>" + Server.HtmlEncode(promo.Message) + "</span>";
            }
            else
            {
                Session["PromoCode"] = null;
                litPromoMessage.Text = "<span class='field-hint invalid'>" + Server.HtmlEncode(promo.Message) + "</span>";
            }

            BindCart();
        }

        // ---------- Quantity, stock and error handling ----------
        private const int MaxPerLine = 99;
        private readonly Dictionary<int, int> _stockCache = new Dictionary<int, int>();

        private void LogError(Exception ex)
        {
            System.Diagnostics.Trace.TraceError(ex.ToString());
        }

        // html must already be encoded
        private void ShowMessage(string html, bool isError)
        {
            litCartMessage.Text = "<div class='field-hint " + (isError ? "invalid" : "valid") + "' role='alert'>" + html + "</div>";
        }

        // Stock per product; inactive or missing products count as 0. Throws on a database error.
        private Dictionary<int, int> LoadStock(IEnumerable<int> productIds)
        {
            var result = new Dictionary<int, int>();
            var ids = productIds.Distinct().ToList();
            if (ids.Count == 0) return result;

            string sql = "SELECT Product_ID, Product_QuantityInStock, Product_Status FROM Product WHERE Product_ID IN (" + string.Join(",", ids) + ")";
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        bool active = string.Equals(Convert.ToString(r[2]).Trim(), "Active", StringComparison.OrdinalIgnoreCase);
                        int stock = r.IsDBNull(1) ? 0 : Convert.ToInt32(r[1]);
                        result[Convert.ToInt32(r[0])] = active ? Math.Max(stock, 0) : 0;
                    }
                }
            }
            return result;
        }

        // Cached per request. Loads the whole cart in one query. Throws on a database error.
        private int GetStock(int productId)
        {
            int s;
            if (_stockCache.TryGetValue(productId, out s)) return s;

            var ids = CartHelper.GetCart(Session).Select(c => c.ProductId).ToList();
            if (!ids.Contains(productId)) ids.Add(productId);
            foreach (var kv in LoadStock(ids)) _stockCache[kv.Key] = kv.Value;

            return _stockCache.TryGetValue(productId, out s) ? s : 0;
        }

        protected bool CanIncrease(object productId, object quantity)
        {
            try
            {
                int max = Math.Min(GetStock(Convert.ToInt32(productId)), MaxPerLine);
                return Convert.ToInt32(quantity) < max;
            }
            catch (Exception ex) { LogError(ex); return true; }   // the server re-validates on click anyway
        }

        protected string StockNoteHtml(object productId, object quantity)
        {
            try
            {
                int stock = GetStock(Convert.ToInt32(productId));
                int qty = Convert.ToInt32(quantity);
                int max = Math.Min(stock, MaxPerLine);

                if (stock <= 0) return "<p class='field-hint invalid'>Out of stock - this item will be removed at checkout.</p>";
                if (qty >= max) return "<p class='field-hint invalid'>Maximum available (" + max + ") is in your cart.</p>";
                if (stock <= 5) return "<p class='field-hint'>Only " + stock + " left in stock.</p>";
            }
            catch (Exception ex) { LogError(ex); }
            return "";
        }

        // The one place a cart quantity changes, so every rule lives here
        private void SetQuantity(int productId, int requested)
        {
            try
            {
                var item = CartHelper.GetCart(Session).FirstOrDefault(c => c.ProductId == productId);
                if (item == null) return;

                int stock = GetStock(productId);
                string name = Server.HtmlEncode(item.ProductName);

                if (stock <= 0)
                {
                    CartHelper.RemoveFromCart(Session, productId);
                    ShowMessage(name + " is out of stock and was removed from your cart.", true);
                    return;
                }

                if (requested < 1)
                {
                    ShowMessage("Quantity must be at least 1. Use Remove to take an item out of your cart.", true);
                    return;
                }

                int max = Math.Min(stock, MaxPerLine);
                if (requested > max)
                {
                    CartHelper.UpdateQuantity(Session, productId, max);
                    ShowMessage("Only " + max + " available for " + name + ". Your quantity was set to " + max + ".", true);
                    return;
                }

                CartHelper.UpdateQuantity(Session, productId, requested);
            }
            catch (Exception ex)
            {
                LogError(ex);
                ShowMessage("We couldn't update your cart right now. Please try again.", true);
            }
        }

        protected void txtQty_TextChanged(object sender, EventArgs e)
        {
            var box = (TextBox)sender;
            var row = (RepeaterItem)box.NamingContainer;
            var hf = row.FindControl("hfProductId") as HiddenField;

            int productId, qty;
            if (hf == null || !int.TryParse(hf.Value, out productId))
            {
                ShowMessage("Something went wrong updating that item. Please try again.", true);
            }
            else if (!int.TryParse((box.Text ?? "").Trim(), out qty))
            {
                ShowMessage("Please enter a whole number for the quantity.", true);
            }
            else
            {
                SetQuantity(productId, qty);
            }

            BindCart();   // resets the box to the real quantity
        }

        // Trims quantities / removes unavailable items. Returns true only when the cart is ready for checkout.
        private bool ValidateCartForCheckout()
        {
            try
            {
                var cart = CartHelper.GetCart(Session).ToList();
                if (cart.Count == 0) return true;

                var stock = LoadStock(cart.Select(c => c.ProductId));
                foreach (var kv in stock) _stockCache[kv.Key] = kv.Value;

                var notes = new List<string>();
                foreach (var item in cart)
                {
                    int s;
                    if (!stock.TryGetValue(item.ProductId, out s)) s = 0;
                    int max = Math.Min(s, MaxPerLine);
                    string name = Server.HtmlEncode(item.ProductName);

                    if (max <= 0)
                    {
                        CartHelper.RemoveFromCart(Session, item.ProductId);
                        notes.Add(name + " is no longer available and was removed from your cart.");
                    }
                    else if (item.Quantity > max)
                    {
                        CartHelper.UpdateQuantity(Session, item.ProductId, max);
                        notes.Add("Only " + max + " of " + name + " available. Your quantity was reduced.");
                    }
                }

                if (notes.Count > 0)
                {
                    ShowMessage(string.Join("<br />", notes) + "<br />Please review your cart and try again.", true);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex);
                ShowMessage("We couldn't check stock levels right now. Please try again in a moment.", true);
                return false;
            }
        }

        private void AddFromDetail(int id, ProductDetailHelper.ProductDetail d)
        {
            try
            {
                var inCart = CartHelper.GetCart(Session).FirstOrDefault(c => c.ProductId == id);
                int current = inCart != null ? inCart.Quantity : 0;
                int max = Math.Min(d.Stock, MaxPerLine);

                if (current >= max) return;   // the button is already disabled in this case
                CartHelper.AddToCart(Session, id, d.Name, d.FinalPrice);
            }
            catch (Exception ex)
            {
                LogError(ex);
                ShowMessage("We couldn't add that item right now. Please try again.", true);
            }
        }

        // ---------- Loyalty points + checkout (rules copied from the POS) ----------
        // POS: 1 point earned per R10 spent, 1 point = R1 when redeeming,
        // inactive customers earn/redeem nothing, and paying with points earns nothing.
        private const int RandPerPoint = 10;
        private const int OnlineSaleTypeId = 2;

        private class CartTotals
        {
            public decimal Merchandise;   // goods after promo discount (points are earned on this)
            public decimal Grand;         // what the customer pays, incl. delivery (points are redeemed against this)
        }

        // Same maths as BindSummary - keep the two in step
        private CartTotals ComputeTotals()
        {
            decimal subtotal = CartHelper.GetCartTotal(Session);
            decimal delivery = subtotal >= FreeDeliveryThreshold ? 0m : DeliveryFee;

            decimal discount = GetPromoAmount(subtotal);
            return new CartTotals { Merchandise = subtotal - discount, Grand = subtotal - discount };
        }

        private int? CurrentCustomerId()
        {
            if (Session["UserRole"] == null) return null;
            object id = Session["CustomerID"];   // <-- change to the session key your Login page sets
            int value;
            return id != null && int.TryParse(id.ToString(), out value) ? (int?)value : null;
        }

        private bool TryLoadLoyalty(int customerId, out int balance, out bool active)
        {
            balance = 0;
            active = false;

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand("SELECT Customer_LoyaltyPointsBalance, Customer_Status FROM Customer WHERE Customer_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", customerId);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return false;
                    balance = r.IsDBNull(0) ? 0 : Convert.ToInt32(r[0]);
                    active = !string.Equals(Convert.ToString(r[1]).Trim(), "Inactive", StringComparison.OrdinalIgnoreCase);
                    return true;
                }
            }
        }

        private void BindPoints()
        {
            CartTotals t = ComputeTotals();
            int earn = (int)Math.Floor(t.Merchandise / RandPerPoint);

            if (Session["UserRole"] == null)
            {
                litPointsInfo.Text = "<div class='cart-points'><strong>Earn " + earn.ToString("N0") + " loyalty points</strong> on this order. "
                    + "<a href='Login.aspx?returnUrl=Cart.aspx'>Log in</a> or <a href='Register.aspx'>sign up</a> to collect them.</div>";
                return;
            }

            int? customerId = CurrentCustomerId();
            int balance;
            bool active;
            if (!customerId.HasValue || !TryLoadLoyalty(customerId.Value, out balance, out active))
            {
                litPointsInfo.Text = "";
                return;
            }

            if (!active)
            {
                litPointsInfo.Text = "<div class='cart-points'>Your loyalty account is inactive, so this order won't earn points.</div>";
                return;
            }

            int needed = (int)Math.Ceiling(t.Grand);
            string html = "<div class='cart-points'><strong>You'll earn " + earn.ToString("N0") + " points</strong> on this order. Your balance: " + balance.ToString("N0") + " pts.";
            if (t.Grand > 0m)
            {
                html += balance >= needed
                    ? " You have enough points to get this order free - you can choose that at checkout."
                    : " You need " + (needed - balance).ToString("N0") + " more points to get this order free.";
            }
            litPointsInfo.Text = html + "</div>";
        }

        protected void btnCheckout_Click(object sender, EventArgs e)
        {
            try { CheckoutCore(); }
            catch (System.Threading.ThreadAbortException) { throw; }   // Response.Redirect
            catch (Exception ex)
            {
                LogError(ex);
                ShowMessage("We couldn't start checkout right now. Please try again in a moment.", true);
                BindCart();
            }
        }

        private void CheckoutCore()
        {
            if (CartHelper.GetCart(Session).Count == 0) return;

            // Re-check stock and availability before going any further
            if (!ValidateCartForCheckout())
            {
                BindCart();
                return;
            }

            // Not logged in: ask them to log in or sign up
            if (Session["UserRole"] == null)
            {
                pnlLoginPrompt.Visible = true;
                return;
            }

            // Loyalty points are chosen on the checkout page, once delivery or collection is known
            ContinueToCheckout(false);
        }

        protected void btnUsePoints_Click(object sender, EventArgs e) { ContinueToCheckout(true); }
        protected void btnPayNormally_Click(object sender, EventArgs e) { ContinueToCheckout(false); }

        // ---------- Prices: always show today's price, plus the original when discounted ----------
        private void RefreshCartPrices()
        {
            var cart = CartHelper.GetCart(Session);
            var saved = CartHelper.GetSavedForLater(Session);
            var items = cart.Concat(saved).ToList();
            if (items.Count == 0) return;

            var prices = PricingHelper.GetPrices(ConnStr, items.Select(i => i.ProductId));
            foreach (var item in items)
            {
                PricingHelper.PriceInfo p;
                if (prices.TryGetValue(item.ProductId, out p))
                {
                    item.UnitPrice = p.FinalPrice;
                    item.ShelfPrice = p.ShelfPrice;
                }
                else if (item.ShelfPrice < item.UnitPrice)
                {
                    item.ShelfPrice = item.UnitPrice;
                }
            }
        }

        protected string CartPriceHtml(object shelf, object unit, string suffix)
        {
            decimal s = Convert.ToDecimal(shelf);
            decimal u = Convert.ToDecimal(unit);
            string now = "R" + u.ToString("N2");

            if (s > u)
                return "<span class='shop-card-strike'>R" + s.ToString("N2") + "</span> <span class='cart-price-sale'>" + now + "</span>" + suffix;

            return now + suffix;
        }

        // ---------- Product pop-up (same layout as the shop page) ----------
        private int DetailProductId
        {
            get { return ViewState["DetailProductId"] as int? ?? 0; }
            set { ViewState["DetailProductId"] = value; }
        }

        private int DetailCategoryId
        {
            get { return ViewState["DetailCategoryId"] as int? ?? 0; }
            set { ViewState["DetailCategoryId"] = value; }
        }

        private void ShowDetail(int productId)
        {
            ProductDetailHelper.ProductDetail d = ProductDetailHelper.Get(ConnStr, productId);
            if (d == null)
            {
                pnlDetail.Visible = false;
                return;
            }

            DetailProductId = productId;
            DetailCategoryId = d.CategoryId;

            imgDetail.Src = "/ProductImage.ashx?id=" + productId;
            imgDetail.Alt = d.Name;

            litDetailBrand.Text = d.Brand;
            litDetailName.Text = d.Name;
            litDetailRating.Text = "<div class=\"rating\"><span class=\"rating-stars\"><span class=\"rating-stars-fill\" style=\"width:" + d.RatingPct + "%\"></span></span><span class=\"rating-count\">" + d.RatingText + " (" + d.RatingCount + " reviews)</span></div>";
            lnkDetailCategory.Text = Server.HtmlEncode(d.Category);
            lnkDetailCategory.ToolTip = "Show all " + d.Category + " products";

            spnDetailBadge.Visible = d.OnSale;
            spnDetailBadge.InnerText = "-" + d.DiscountPercent + "%";
            spnDetailOld.Visible = d.OnSale;
            spnDetailOld.InnerText = "R" + d.ShelfPrice.ToString("N2");
            spnDetailPrice.InnerText = "R" + d.FinalPrice.ToString("N2");
            spnDetailPrice.Attributes["class"] = d.OnSale ? "shop-card-price is-sale" : "shop-card-price";

            litDetailDesc.Text = string.IsNullOrWhiteSpace(d.Description) ? "No description available yet." : d.Description.Trim();

            var facts = new System.Text.StringBuilder();
            Action<string, string> add = (label, value) =>
            {
                if (!string.IsNullOrWhiteSpace(value))
                    facts.Append("<div><dt>").Append(Server.HtmlEncode(label)).Append("</dt><dd>")
                         .Append(Server.HtmlEncode(value.Trim())).Append("</dd></div>");
            };
            add("Type", d.Type);
            add("Origin", d.Origin);
            add("Flavour", d.Flavour);
            add("Alcohol", d.Abv > 0m ? d.Abv.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "%" : null);
            litDetailFacts.Text = facts.Length > 0 ? "<dl class=\"pd-facts\">" + facts + "</dl>" : "";

            litDetailIngredients.Text = string.IsNullOrWhiteSpace(d.Ingredients)
                ? ""
                : "<p class=\"pd-ingredients\"><strong>Ingredients</strong> " + Server.HtmlEncode(d.Ingredients.Trim()) + "</p>";

            var inCart = CartHelper.GetCart(Session).FirstOrDefault(c => c.ProductId == productId);
            litDetailMsg.Text = inCart != null ? "<div class='shop-cart-toast'>You have " + inCart.Quantity + " in your cart.</div>" : "";

            btnDetailAdd.Text = d.Stock <= 0 ? "Out of Stock"
                : (inCart != null && inCart.Quantity >= Math.Min(d.Stock, MaxPerLine)) ? "Maximum in cart"
                : (inCart != null ? "Add another to cart" : "Add to Cart");
            btnDetailAdd.Enabled = d.Stock > 0 && (inCart == null || inCart.Quantity < Math.Min(d.Stock, MaxPerLine));
            btnDetailWish.Text = CartHelper.GetWishlist(Session).Contains(productId) ? "Remove from wishlist" : "Add to wishlist";

            pnlDetail.Visible = true;
        }

        protected void btnCloseDetail_Click(object sender, EventArgs e)
        {
            pnlDetail.Visible = false;
        }

        protected void btnDetailAdd_Click(object sender, EventArgs e)
        {
            int id = DetailProductId;
            ProductDetailHelper.ProductDetail d = ProductDetailHelper.Get(ConnStr, id);

            if (d != null && d.Stock > 0)
                AddFromDetail(id, d);

            BindCart();       // refresh the cart, totals and the badge numbers behind the pop-up
            ShowDetail(id);
        }

        protected void btnDetailWish_Click(object sender, EventArgs e)
        {
            int id = DetailProductId;
            CartHelper.ToggleWishlist(Session, id);
            BindCart();       // recommendations use the wishlist
            ShowDetail(id);
        }

        protected void lnkDetailCategory_Click(object sender, EventArgs e)
        {
            Response.Redirect("Shop.aspx?cat=" + DetailCategoryId);
        }

        protected void btnClosePrompt_Click(object sender, EventArgs e)
        {
            pnlLoginPrompt.Visible = false;
            pnlPointsPrompt.Visible = false;
        }

        // Only records the customer's choices for the checkout page - no order is placed here
        private void ContinueToCheckout(bool payWithPoints)
        {
            CartTotals t = ComputeTotals();

            int pointsToEarn = 0;
            int? customerId = CurrentCustomerId();
            int balance;
            bool active;
            if (!payWithPoints && customerId.HasValue && TryLoadLoyalty(customerId.Value, out balance, out active) && active)
                pointsToEarn = (int)Math.Floor(t.Merchandise / RandPerPoint);

            Session["Checkout.SaleTypeID"] = OnlineSaleTypeId;
            Session["Checkout.PayWithPoints"] = payWithPoints;
            Session["Checkout.PointsToUse"] = payWithPoints ? (int)Math.Ceiling(t.Grand) : 0;
            Session["Checkout.PointsToEarn"] = pointsToEarn;

            Response.Redirect("Checkout.aspx");
        }
    }
}