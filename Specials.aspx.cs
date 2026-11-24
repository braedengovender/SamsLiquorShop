using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using SamsLiquorShop.Helpers;

namespace SamsLiquorShop
{
    /// <summary>
    /// Lists every active product that has a discount code in the Product table. Nothing is typed in by hand:
    /// when a manager changes a product, a category, a discount or a picture, this page follows.
    /// Discount rules come from PricingHelper, so prices match the Shop and the Cart.
    /// </summary>
    public partial class Specials : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) SafeBind();
        }

        // ------------------------------------------------------------------ data

        /// <summary>
        /// Every active product that has a discount code (Product.Discount_ID) in the Product table,
        /// plus any product that is on sale right now through its category's discount.
        /// Running = the discount applies today. A listed product whose discount is not running
        /// keeps its shelf price and shows the reason, so the page never advertises a price the cart won't charge.
        /// </summary>
        private DataTable LoadSpecials()
        {
            DataTable products = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(@"
                SELECT p.Product_ID, p.Product_Name, p.Category_ID, c.Category_Name, p.Product_QuantityInStock,
                       p.Product_SellingPrice, p.Discount_ID,
                       d.Discount_Name, d.Discount_Status, d.Discount_StartDate, d.Discount_EndDate
                FROM Product p
                INNER JOIN Category c ON p.Category_ID = c.Category_ID
                LEFT JOIN Discount d ON p.Discount_ID = d.Discount_ID
                WHERE p.Product_Status = 'Active'", conn))
            {
                da.Fill(products);
            }

            List<int> ids = products.Rows.Cast<DataRow>().Select(r => Convert.ToInt32(r["Product_ID"])).ToList();
            Dictionary<int, PricingHelper.PriceInfo> prices = PricingHelper.GetPrices(ConnStr, ids);

            DataTable result = new DataTable();
            result.Columns.Add("Product_ID", typeof(int));
            result.Columns.Add("Product_Name", typeof(string));
            result.Columns.Add("Category_ID", typeof(int));
            result.Columns.Add("Category_Name", typeof(string));
            result.Columns.Add("Product_SellingPrice", typeof(decimal));
            result.Columns.Add("DiscountedPrice", typeof(decimal));
            result.Columns.Add("DiscountPercent", typeof(int));
            result.Columns.Add("InStock", typeof(bool));
            result.Columns.Add("Running", typeof(bool));
            result.Columns.Add("Note", typeof(string));
            result.Columns.Add("Stock", typeof(int));
            result.Columns.Add("MaxQty", typeof(int));      // how many more this customer can add right now
            result.Columns.Add("BtnText", typeof(string));

            // What's already in this customer's cart, per product
            Dictionary<int, int> cartQty = CartHelper.GetCart(Session)
                .GroupBy(c => c.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Quantity));

            foreach (DataRow r in products.Rows)
            {
                int id = Convert.ToInt32(r["Product_ID"]);
                bool hasCode = r["Discount_ID"] != DBNull.Value;

                PricingHelper.PriceInfo info;
                prices.TryGetValue(id, out info);
                bool running = info != null && info.OnSale;

                if (!hasCode && !running) continue;

                decimal shelf = info != null ? info.ShelfPrice : Convert.ToDecimal(r["Product_SellingPrice"]);
                decimal final = running ? info.FinalPrice : shelf;
                int percent = (running && shelf > 0m)
                    ? Math.Max(1, (int)Math.Round((shelf - final) / shelf * 100m, MidpointRounding.AwayFromZero))
                    : 0;

                int stock = r["Product_QuantityInStock"] == DBNull.Value ? 0 : Math.Max(0, Convert.ToInt32(r["Product_QuantityInStock"]));

                int inCart;
                if (!cartQty.TryGetValue(id, out inCart)) inCart = 0;
                int cap = Math.Min(stock, CartHelper.MaxPerLine);
                int maxQty = Math.Max(0, cap - inCart);
                string btnText = stock <= 0 ? "Out of Stock" : (maxQty <= 0 ? "Max in Cart" : "Add to Cart");

                result.Rows.Add(id, Convert.ToString(r["Product_Name"]), Convert.ToInt32(r["Category_ID"]),
                    Convert.ToString(r["Category_Name"]), shelf, final, percent, stock > 0, running,
                    running ? "" : WhyNotRunning(r), stock, maxQty, btnText);
            }

            // Running specials first, biggest saving first
            DataView view = result.DefaultView;
            view.Sort = "Running DESC, DiscountPercent DESC, Product_Name ASC";
            return view.ToTable();
        }

        private static string WhyNotRunning(DataRow r)
        {
            if (r["Discount_Status"] == DBNull.Value) return "Not running";

            string s = Convert.ToString(r["Discount_Status"]).Trim();
            bool active = s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("Active", StringComparison.OrdinalIgnoreCase);
            if (!active) return "Not active";

            DateTime now = DateTime.Now;

            string promo = Convert.ToString(r["Discount_Name"]);
            if (promo.IndexOf("wednesday", StringComparison.OrdinalIgnoreCase) >= 0 && now.DayOfWeek != DayOfWeek.Wednesday)
                return "Wednesdays only";

            if (r["Discount_StartDate"] != DBNull.Value)
            {
                DateTime start = Convert.ToDateTime(r["Discount_StartDate"]);
                if (now < start) return "Starts " + start.ToString("d MMM");
            }
            if (r["Discount_EndDate"] != DBNull.Value)
            {
                DateTime end = Convert.ToDateTime(r["Discount_EndDate"]);
                if (now > end) return "Ended " + end.ToString("d MMM");
            }
            return "Not running";
        }

        private void BindSpecials()
        {
            DataTable specials = LoadSpecials();
            bool any = specials.Rows.Count > 0;

            phSpecials.Visible = any;
            phNone.Visible = !any;
            if (!any) return;

            SortedDictionary<string, int> categories = new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow r in specials.Rows)
            {
                categories[Convert.ToString(r["Category_Name"])] = Convert.ToInt32(r["Category_ID"]);
            }

            DataTable tabs = new DataTable();
            tabs.Columns.Add("Category_ID", typeof(int));
            tabs.Columns.Add("Category_Name", typeof(string));
            foreach (KeyValuePair<string, int> c in categories) tabs.Rows.Add(c.Value, c.Key);

            rptTabs.DataSource = tabs;
            rptTabs.DataBind();
            rptSpecials.DataSource = specials;
            rptSpecials.DataBind();
        }

        // ------------------------------------------------------------------ cart

        protected void rptSpecials_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            litCartMessage.Text = "";          // clear the previous message
            litCartMessage.Visible = false;

            try
            {
                if (e.CommandName == "AddToCart") AddSpecialToCart(e);
                else if (e.CommandName == "ViewProduct") OpenDetail(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Specials: add to cart failed: " + ex);
                ShowMessage("Sorry, something went wrong adding that item. Please try again.", true);
            }

            SafeBind();   // always refresh so the quantity limits and buttons match the cart
        }

        private void AddSpecialToCart(RepeaterCommandEventArgs e)
        {
            int productId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), NumberStyles.None, CultureInfo.InvariantCulture, out productId) || productId <= 0)
            {
                ShowMessage("Sorry, we couldn't tell which product you meant.", true);
                return;
            }

            TextBox txtQty = e.Item.FindControl("txtQty") as TextBox;
            int qty;
            string error;
            if (!TryParseQty(txtQty == null ? "" : txtQty.Text, out qty, out error))
            {
                ShowMessage(error, true);
                return;
            }

            string message;
            bool ok = TryAdd(productId, qty, out message);
            ShowMessage(message, !ok);
        }

        /// <summary>Typed quantities are never trusted: whole number, 1 to MaxPerLine.</summary>
        private static bool TryParseQty(string text, out int qty, out string error)
        {
            qty = 0;
            error = null;

            string raw = (text ?? "").Trim();
            if (raw.Length == 0 || !int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out qty))
            {
                qty = 0;
                error = "Please enter a whole number for the quantity.";
                return false;
            }
            if (qty < 1)
            {
                error = "The quantity must be at least 1.";
                return false;
            }
            if (qty > CartHelper.MaxPerLine)
            {
                error = "You can add at most " + CartHelper.MaxPerLine + " of one product.";
                return false;
            }
            return true;
        }

        /// <summary>
        /// The one place that adds to the cart (grid cards and the popup both use it).
        /// Re-reads stock from the database every time and refuses anything over the limit.
        /// </summary>
        private bool TryAdd(int productId, int qty, out string message)
        {
            DataRow[] hit = LoadSpecials().Select("Product_ID = " + productId);
            if (hit.Length == 0)
            {
                message = "Sorry, that product is no longer listed.";
                return false;
            }

            DataRow row = hit[0];
            string name = Convert.ToString(row["Product_Name"]);
            int stock = Convert.ToInt32(row["Stock"]);
            int maxQty = Convert.ToInt32(row["MaxQty"]);          // limit minus what's already in the cart
            int cap = Math.Min(stock, CartHelper.MaxPerLine);

            CartItem existing = CartHelper.GetCart(Session).FirstOrDefault(c => c.ProductId == productId);
            int inCart = existing == null ? 0 : existing.Quantity;

            if (stock <= 0)
            {
                message = "Sorry, " + name + " is out of stock.";
                return false;
            }
            if (maxQty <= 0)
            {
                message = "You already have the maximum available (" + cap + ") of " + name + " in your cart.";
                return false;
            }
            if (qty > maxQty)
            {
                message = inCart > 0
                    ? "Only " + maxQty + " more of " + name + " can be added \u2013 you already have " + inCart + " in your cart."
                    : "Only " + maxQty + " of " + name + " available.";
                return false;
            }

            // The customer is charged what is shown: the special price while it runs, otherwise the shelf price
            CartHelper.AddToCart(Session, productId, name, (decimal)row["DiscountedPrice"], qty, cap);
            message = qty + " \u00D7 " + name + " added to cart.";
            return true;
        }

        // ------------------------------------------------------------------ product detail popup

        private static readonly string[] OptionalColumns =
        {
            "Product_Description", "Product_Type", "Product_OriginRegion",
            "Product_Flavour", "Product_Ingredients", "Product_AlcoholPercentage"
        };
        private static HashSet<string> _productColumns;

        private int DetailProductId
        {
            get { return ViewState["DetailProductId"] as int? ?? 0; }
            set { ViewState["DetailProductId"] = value; }
        }

        private HashSet<string> GetProductColumns()
        {
            if (_productColumns != null) return _productColumns;

            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Product'", conn))
            {
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read()) set.Add(r.GetString(0));
                }
            }
            _productColumns = set;
            return set;
        }

        /// <summary>Brand, description, type, origin, flavour, ingredients and alcohol % (columns that don't exist are just empty).</summary>
        private DataRow LoadExtras(int productId)
        {
            HashSet<string> existing = GetProductColumns();
            // Product_OriginRegion is the real column; it is selected as Product_Origin so the pop-up code keeps working
            List<string> optional = OptionalColumns.Where(c => existing.Contains(c))
                .Select(c => c == "Product_OriginRegion" ? "p.[Product_OriginRegion] AS [Product_Origin]" : "p.[" + c + "]")
                .ToList();
            string optionalSql = optional.Count > 0 ? ", " + string.Join(", ", optional) : "";

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT p.Product_Brand" + optionalSql + " FROM Product p WHERE p.Product_ID = @Pid AND p.Product_Status = 'Active'", conn))
            {
                cmd.Parameters.AddWithValue("@Pid", productId);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }

            foreach (string c in OptionalColumns)
            {
                string colName = c == "Product_OriginRegion" ? "Product_Origin" : c;
                if (!dt.Columns.Contains(colName))
                    dt.Columns.Add(colName, colName == "Product_AlcoholPercentage" ? typeof(decimal) : typeof(string));
            }

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        private static decimal ToDecimalOrZero(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            try { return Convert.ToDecimal(value); }
            catch (FormatException) { return 0m; }
            catch (InvalidCastException) { return 0m; }
        }

        private void OpenDetail(RepeaterCommandEventArgs e)
        {
            int productId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), NumberStyles.None, CultureInfo.InvariantCulture, out productId) || productId <= 0)
            {
                ShowMessage("Sorry, we couldn't open that product.", true);
                return;
            }
            ShowDetailSafe(productId);
        }

        private void ShowDetailSafe(int productId)
        {
            try
            {
                ShowDetail(productId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Specials: could not open product " + productId + ": " + ex);
                pnlDetail.Visible = false;
                ShowMessage("Sorry, we couldn't open that product. Please try again.", true);
            }
        }

        private void ShowDetail(int productId)
        {
            DataRow[] hit = productId > 0 ? LoadSpecials().Select("Product_ID = " + productId) : new DataRow[0];
            DataRow extra = hit.Length == 0 ? null : LoadExtras(productId);
            if (hit.Length == 0 || extra == null)
            {
                pnlDetail.Visible = false;
                ShowMessage("Sorry, that product is no longer listed.", true);
                return;
            }

            DataRow r = hit[0];
            DetailProductId = productId;

            string name = Convert.ToString(r["Product_Name"]);
            string category = Convert.ToString(r["Category_Name"]);
            bool onSale = (bool)r["Running"];
            int stock = Convert.ToInt32(r["Stock"]);
            int maxQty = Convert.ToInt32(r["MaxQty"]);

            imgDetail.Src = "/ProductImage.ashx?id=" + productId;
            imgDetail.Alt = name;

            litDetailBrand.Text = Convert.ToString(extra["Product_Brand"]);
            litDetailName.Text = name;

            // TEMPORARY: same fake-but-stable rating the Shop uses until real reviews exist
            Random rng = new Random(productId * 7919 + 13);
            double rating = Math.Round(3.2 + rng.NextDouble() * 1.8, 1);
            string ratingText = rating.ToString("0.0", CultureInfo.InvariantCulture);
            int ratingPct = (int)Math.Round(rating / 5.0 * 100.0);
            int ratingCount = rng.Next(4, 160);
            litDetailRating.Text = "<div class=\"rating\"><span class=\"rating-stars\"><span class=\"rating-stars-fill\" style=\"width:" + ratingPct + "%\"></span></span><span class=\"rating-count\">" + ratingText + " (" + ratingCount + " reviews)</span></div>";

            lnkDetailCategory.Text = Server.HtmlEncode(category);
            lnkDetailCategory.NavigateUrl = "Shop.aspx?cat=" + Convert.ToInt32(r["Category_ID"]);
            lnkDetailCategory.ToolTip = "Show all " + category + " products";

            spnDetailBadge.Visible = onSale;
            spnDetailBadge.InnerText = "-" + r["DiscountPercent"] + "%";
            spnDetailOld.Visible = onSale;
            spnDetailOld.InnerText = Money(r["Product_SellingPrice"]);
            spnDetailPrice.InnerText = Money(r["DiscountedPrice"]);
            spnDetailPrice.Attributes["class"] = onSale ? "shop-card-price is-sale" : "shop-card-price";

            string desc = Convert.ToString(extra["Product_Description"]);
            litDetailDesc.Text = string.IsNullOrWhiteSpace(desc) ? "No description available yet." : desc.Trim();

            StringBuilder facts = new StringBuilder();
            Action<string, string> add = (label, value) =>
            {
                if (!string.IsNullOrWhiteSpace(value))
                    facts.Append("<div><dt>").Append(Server.HtmlEncode(label)).Append("</dt><dd>")
                         .Append(Server.HtmlEncode(value.Trim())).Append("</dd></div>");
            };
            decimal abv = ToDecimalOrZero(extra["Product_AlcoholPercentage"]);
            add("Type", Convert.ToString(extra["Product_Type"]));
            add("Origin", Convert.ToString(extra["Product_Origin"]));
            add("Flavour", Convert.ToString(extra["Product_Flavour"]));
            add("Alcohol", abv > 0m ? abv.ToString("0.##", CultureInfo.InvariantCulture) + "%" : null);
            litDetailFacts.Text = facts.Length > 0 ? "<dl class=\"pd-facts\">" + facts + "</dl>" : "";

            string ingredients = Convert.ToString(extra["Product_Ingredients"]);
            litDetailIngredients.Text = string.IsNullOrWhiteSpace(ingredients)
                ? ""
                : "<p class=\"pd-ingredients\"><strong>Ingredients</strong> " + Server.HtmlEncode(ingredients.Trim()) + "</p>";

            // Quantity box + button follow the same limits as the grid
            bool canBuy = maxQty > 0;
            divDetailQty.Attributes["class"] = canBuy ? "special-qty" : "special-qty is-off";
            divDetailQty.Attributes["data-max"] = maxQty.ToString(CultureInfo.InvariantCulture);
            txtDetailQty.Text = canBuy ? "1" : "0";
            txtDetailQty.Enabled = canBuy;
            btnDetailAdd.Text = stock <= 0 ? "Out of Stock" : (maxQty <= 0 ? "Max in Cart" : "Add to Cart");
            btnDetailAdd.Enabled = canBuy;
            btnDetailWish.Text = CartHelper.GetWishlist(Session).Contains(productId) ? "Remove from wishlist" : "Add to wishlist";

            litDetailMsg.Text = "";
            pnlDetail.Visible = true;
        }

        protected void btnCloseDetail_Click(object sender, EventArgs e)
        {
            pnlDetail.Visible = false;
        }
        // Managers and cashiers can look at specials but not buy or wishlist them
        protected bool IsStaff { get { return CartHelper.IsStaff(Session); } }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (IsStaff)
            {
                btnDetailAdd.Visible = false;
                btnDetailWish.Visible = false;
            }
        }
        protected void btnDetailAdd_Click(object sender, EventArgs e)
        {
            int id = DetailProductId;
            string message = null;
            bool ok = false;

            try
            {
                int qty;
                string error;
                if (id <= 0)
                    message = "Sorry, that product is no longer listed.";
                else if (!TryParseQty(txtDetailQty.Text, out qty, out error))
                    message = error;
                else
                    ok = TryAdd(id, qty, out message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Specials: popup add to cart failed: " + ex);
                message = "Sorry, something went wrong adding that item. Please try again.";
                ok = false;
            }

            ShowDetailSafe(id);   // refresh the popup so its limits match the cart
            litDetailMsg.Text = "<div class='shop-cart-toast" + (ok ? "" : " is-error") + "'>" + Server.HtmlEncode(message ?? "") + "</div>";
            SafeBind();           // and the cards behind it
        }

        protected void btnDetailWish_Click(object sender, EventArgs e)
        {
            int id = DetailProductId;
            try
            {
                if (id > 0) CartHelper.ToggleWishlist(Session, id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Specials: wishlist toggle failed: " + ex);
                ShowMessage("Sorry, we couldn't update your wishlist. Please try again.", true);
            }
            ShowDetailSafe(id);
        }

        private void SafeBind()
        {
            try
            {
                BindSpecials();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Specials: could not load specials: " + ex);
                phSpecials.Visible = false;
                phNone.Visible = false;
                ShowMessage("Sorry, we couldn't load this week's specials. Please refresh the page in a moment.", true);
            }
        }

        private void ShowMessage(string text, bool isError = false)
        {
            litCartMessage.Text = "<div class='shop-cart-toast" + (isError ? " is-error" : "") + "' role='status'>" + Server.HtmlEncode(text) + "</div>";
            litCartMessage.Visible = true;
        }

        // ------------------------------------------------------------------ markup helpers

        protected string Money(object value)
        {
            return "R" + Convert.ToDecimal(value).ToString("N2");
        }

        protected string H(object value)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(value));
        }
    }
}
