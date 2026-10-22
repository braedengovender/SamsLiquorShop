using SamsLiquorShop.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace SamsLiquorShop
{
    public partial class Shop : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        // Columns that are only selected if they really exist on the Product table.
        // If one of these names is wrong the page still works - that feature is just empty/hidden.
        private static readonly string[] OptionalColumns =
        {
            "Product_Description", "Product_Type", "Product_Origin",
            "Product_Flavour", "Product_Ingredients", "Product_AlcoholPercentage"
        };
        private static HashSet<string> _productColumns;

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

        // ------------------------------------------------------------------ page load

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            DataTable all = LoadProductTable();

            LoadCategories();

            if (int.TryParse(Request.QueryString["cat"], out int catId))
            {
                foreach (ListItem item in cblCategories.Items)
                {
                    if (item.Value == catId.ToString()) item.Selected = true;
                }
            }

            string searchParam = Request.QueryString["search"];
            if (!string.IsNullOrEmpty(searchParam)) txtSearch.Text = searchParam;

            if (Request.QueryString["sale"] == "1") chkOnSale.Checked = true;
            if (Request.QueryString["wishlist"] == "1") chkWishlistOnly.Checked = true;

            LoadProducts(all);
        }

        private void LoadCategories()
        {
            string query = "SELECT Category_ID, Category_Name FROM Category ORDER BY Category_Name";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                cblCategories.DataSource = dt;
                cblCategories.DataTextField = "Category_Name";
                cblCategories.DataValueField = "Category_ID";
                cblCategories.DataBind();
            }
        }

        private void RebuildFacet(CheckBoxList list, HtmlGenericControl block, IEnumerable<DataRow> scope, string column)
        {
            var keep = Selected(list);   // remember what was ticked
            list.Items.Clear();

            var groups = scope
                .Select(r => Convert.ToString(r[column]).Trim())
                .Where(v => v.Length > 0)
                .GroupBy(v => v, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase);

            foreach (var g in groups)
            {
                list.Items.Add(new ListItem(g.Key + " (" + g.Count() + ")", g.Key) { Selected = keep.Contains(g.Key) });
            }

            block.Visible = list.Items.Count > 0;
        }

        // ------------------------------------------------------------------ data

        private HashSet<string> GetProductColumns()
        {
            if (_productColumns != null) return _productColumns;

            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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

        /// <summary>
        /// Loads active products (or just one) with the discount already worked out.
        /// Adds: DiscountedPrice, IsOnSale, DiscountPercent, AbvValue, SearchBlob.
        /// </summary>
        private DataTable LoadProductTable(int? onlyProductId = null)
        {
            var existing = GetProductColumns();
            var optional = OptionalColumns.Where(c => existing.Contains(c)).Select(c => "p.[" + c + "]").ToList();
            string optionalSql = optional.Count > 0 ? ", " + string.Join(", ", optional) : "";

            string where = "p.Product_Status = 'Active'";
            if (onlyProductId.HasValue) where += " AND p.Product_ID = @Pid";

            string query = $@"
                SELECT
                    p.Product_ID, p.Product_Name, p.Product_Brand, p.Product_SellingPrice,
                    p.Product_QuantityInStock, p.Category_ID, cat.Category_Name,
                    p.Discount_ID AS ProductDiscountId, cat.Discount_ID AS CategoryDiscountId,
                    pd.Discount_Name AS ProductDiscountName, pd.Discount_Type AS ProductDiscountType, pd.Discount_Value AS ProductDiscountValue,
                    pd.Discount_Status AS ProductDiscountStatus, pd.Discount_StartDate AS ProductDiscountStart, pd.Discount_EndDate AS ProductDiscountEnd,
                    cd.Discount_Name AS CategoryDiscountName, cd.Discount_Type AS CategoryDiscountType, cd.Discount_Value AS CategoryDiscountValue,
                    cd.Discount_Status AS CategoryDiscountStatus, cd.Discount_StartDate AS CategoryDiscountStart, cd.Discount_EndDate AS CategoryDiscountEnd
                    {optionalSql}
                FROM Product p
                INNER JOIN Category cat ON p.Category_ID = cat.Category_ID
                LEFT JOIN Discount pd ON p.Discount_ID = pd.Discount_ID
                LEFT JOIN Discount cd ON cat.Discount_ID = cd.Discount_ID
                WHERE {where}";

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (onlyProductId.HasValue) cmd.Parameters.AddWithValue("@Pid", onlyProductId.Value);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }

            // Make sure every optional column exists so the rest of the code never has to check
            foreach (string c in OptionalColumns)
            {
                if (!dt.Columns.Contains(c))
                    dt.Columns.Add(c, c == "Product_AlcoholPercentage" ? typeof(decimal) : typeof(string));
            }

            dt.Columns.Add("DiscountedPrice", typeof(decimal));
            dt.Columns.Add("IsOnSale", typeof(bool));
            dt.Columns.Add("DiscountPercent", typeof(int));
            dt.Columns.Add("AbvValue", typeof(decimal));
            dt.Columns.Add("SearchBlob", typeof(string));
            dt.Columns.Add("RatingText", typeof(string));
            dt.Columns.Add("RatingPct", typeof(int));
            dt.Columns.Add("RatingCount", typeof(int));

            foreach (DataRow row in dt.Rows)
            {
                decimal basePrice = Convert.ToDecimal(row["Product_SellingPrice"]);
                decimal finalPrice = basePrice;

                bool hasProductDiscount = row["ProductDiscountId"] != DBNull.Value && IsDiscountActive(row, "Product");
                bool hasCategoryDiscount = row["CategoryDiscountId"] != DBNull.Value && IsDiscountActive(row, "Category");

                if (hasProductDiscount)
                    finalPrice = ApplyDiscount(basePrice, row["ProductDiscountType"].ToString(), Convert.ToDecimal(row["ProductDiscountValue"]));
                else if (hasCategoryDiscount)
                    finalPrice = ApplyDiscount(basePrice, row["CategoryDiscountType"].ToString(), Convert.ToDecimal(row["CategoryDiscountValue"]));

                bool onSale = finalPrice < basePrice;
                int percent = (onSale && basePrice > 0m)
                    ? Math.Max(1, (int)Math.Round((basePrice - finalPrice) / basePrice * 100m, MidpointRounding.AwayFromZero))
                    : 0;

                row["DiscountedPrice"] = finalPrice;
                row["IsOnSale"] = onSale;
                row["DiscountPercent"] = percent;
                row["AbvValue"] = row["Product_AlcoholPercentage"] == DBNull.Value ? 0m : Convert.ToDecimal(row["Product_AlcoholPercentage"]);

                // TEMPORARY: fake but stable (seeded by product id) until real reviews exist
                var rng = new Random(Convert.ToInt32(row["Product_ID"]) * 7919 + 13);
                double rating = Math.Round(3.2 + rng.NextDouble() * 1.8, 1);
                row["RatingText"] = rating.ToString("0.0", CultureInfo.InvariantCulture);
                row["RatingPct"] = (int)Math.Round(rating / 5.0 * 100.0);
                row["RatingCount"] = rng.Next(4, 160);

                row["SearchBlob"] = string.Join(" ", new[]
                {
                    Convert.ToString(row["Product_Name"]),
                    Convert.ToString(row["Product_Brand"]),
                    Convert.ToString(row["Category_Name"]),
                    Convert.ToString(row["Product_Type"]),
                    Convert.ToString(row["Product_Origin"]),
                    Convert.ToString(row["Product_Flavour"]),
                    Convert.ToString(row["Product_Description"]),
                    Convert.ToString(row["Product_Ingredients"])
                });
            }

            return dt;
        }

        private bool IsDiscountActive(DataRow row, string prefix)
        {
            object statusValue = row[prefix + "DiscountStatus"];
            if (statusValue == DBNull.Value) return false;

            string statusStr = statusValue.ToString().Trim();
            bool isActive = statusStr == "1"
                || statusStr.Equals("true", StringComparison.OrdinalIgnoreCase)
                || statusStr.Equals("Active", StringComparison.OrdinalIgnoreCase);

            if (!isActive) return false;

            DateTime now = DateTime.Now;

            // Same rule as the POS: "Wine Wednesday" style promos only run on Wednesdays
            string promoName = Convert.ToString(row[prefix + "DiscountName"]);
            if (promoName.IndexOf("wednesday", StringComparison.OrdinalIgnoreCase) >= 0 && now.DayOfWeek != DayOfWeek.Wednesday)
                return false;

            DateTime start = Convert.ToDateTime(row[prefix + "DiscountStart"]);
            DateTime end = Convert.ToDateTime(row[prefix + "DiscountEnd"]);

            return now >= start && now <= end;
        }

        private decimal ApplyDiscount(decimal basePrice, string discountType, decimal discountValue)
        {
            if (discountType == "Percentage")
            {
                return Math.Round(basePrice - (basePrice * discountValue / 100m), 2);
            }
            else // Fixed
            {
                decimal result = basePrice - discountValue;
                return result < 0 ? 0 : result;
            }
        }

        // ------------------------------------------------------------------ filtering

        private static HashSet<string> Selected(CheckBoxList list)
        {
            return new HashSet<string>(
                list.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Value),
                StringComparer.OrdinalIgnoreCase);
        }

        private static bool TryInv(string s, out decimal value)
        {
            return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        private void LoadProducts(DataTable all = null)
        {
            if (all == null) all = LoadProductTable();

            IEnumerable<DataRow> rows = all.Rows.Cast<DataRow>();

            // Search: every word must appear somewhere in name/brand/category/type/origin/flavour/description/ingredients
            string term = (txtSearch.Text ?? "").Trim();
            if (term.Length > 0)
            {
                foreach (string token in term.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string t = token;
                    rows = rows.Where(r => ((string)r["SearchBlob"]).IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0);
                }
            }

            if (chkOnSale.Checked)
                rows = rows.Where(r => (bool)r["IsOnSale"]);

            if (chkWishlistOnly.Checked)
            {
                var wishlist = CartHelper.GetWishlist(Session);
                rows = rows.Where(r => wishlist.Contains(Convert.ToInt32(r["Product_ID"])));
            }

            var cats = Selected(cblCategories);
            if (cats.Count > 0)
                rows = rows.Where(r => cats.Contains(Convert.ToString(r["Category_ID"])));

            // Everything above (search, sale, wishlist, category) defines the "scope".
            // The brand/type/origin lists and both sliders are rebuilt from it.
            List<DataRow> scope = rows.ToList();
            rows = scope;
            RebuildFacet(cblBrands, blkBrand, scope, "Product_Brand");
            RebuildFacet(cblTypes, blkType, scope, "Product_Type");
            RebuildFacet(cblOrigins, blkOrigin, scope, "Product_Origin");
            BuildRangeData(scope);

            var brands = Selected(cblBrands);
            if (brands.Count > 0)
                rows = rows.Where(r => brands.Contains(Convert.ToString(r["Product_Brand"]).Trim()));

            var types = Selected(cblTypes);
            if (types.Count > 0)
                rows = rows.Where(r => types.Contains(Convert.ToString(r["Product_Type"]).Trim()));

            var origins = Selected(cblOrigins);
            if (origins.Count > 0)
                rows = rows.Where(r => origins.Contains(Convert.ToString(r["Product_Origin"]).Trim()));

            // Price slider works on the price the customer actually pays (after discount)
            decimal priceMin, priceMax, abvMin, abvMax;
            if (TryInv(hfPriceMin.Value, out priceMin)) rows = rows.Where(r => (decimal)r["DiscountedPrice"] >= priceMin);
            if (TryInv(hfPriceMax.Value, out priceMax)) rows = rows.Where(r => (decimal)r["DiscountedPrice"] <= priceMax);
            if (TryInv(hfAbvMin.Value, out abvMin)) rows = rows.Where(r => (decimal)r["AbvValue"] >= abvMin);
            if (TryInv(hfAbvMax.Value, out abvMax)) rows = rows.Where(r => (decimal)r["AbvValue"] <= abvMax);

            switch (ddlSort.SelectedValue)
            {
                case "price_asc": rows = rows.OrderBy(r => (decimal)r["DiscountedPrice"]); break;
                case "price_desc": rows = rows.OrderByDescending(r => (decimal)r["DiscountedPrice"]); break;
                case "name_asc": rows = rows.OrderBy(r => Convert.ToString(r["Product_Name"]), StringComparer.CurrentCultureIgnoreCase); break;
                default: rows = rows.OrderBy(r => Convert.ToInt32(r["Product_ID"])); break;
            }

            DataTable result = all.Clone();
            foreach (DataRow r in rows) result.ImportRow(r);

            rptProducts.DataSource = result;
            rptProducts.DataBind();

            litResultCount.Text = result.Rows.Count + (result.Rows.Count == 1 ? " product found" : " products found");
            litNoResults.Visible = result.Rows.Count == 0;
        }

        /// <summary>Histogram buckets + bounds for the two sliders, built from the whole catalogue.</summary>
        private void BuildRangeData(IEnumerable<DataRow> scope)
        {
            var prices = scope.Select(r => (double)(decimal)r["DiscountedPrice"]).ToList();
            var abvs = scope.Select(r => (double)(decimal)r["AbvValue"]).ToList();

            var payload = new Dictionary<string, object>();

            bool showPrice = prices.Count > 0;
            bool showAbv = abvs.Any(v => v > 0);

            if (showPrice) payload["price"] = MakeRange(prices, 1.0, 12);
            if (showAbv) payload["abv"] = MakeRange(abvs, 0.5, 10);

            hfRangeData.Value = new JavaScriptSerializer().Serialize(payload);
            blkPrice.Visible = showPrice;
            blkAbv.Visible = showAbv;

            if (showPrice)
            {
                var p = (Dictionary<string, object>)payload["price"];
                ClampRange(hfPriceMin, hfPriceMax, (double)p["min"], (double)p["max"]);
            }
            else { hfPriceMin.Value = ""; hfPriceMax.Value = ""; }

            if (showAbv)
            {
                var a = (Dictionary<string, object>)payload["abv"];
                ClampRange(hfAbvMin, hfAbvMax, (double)a["min"], (double)a["max"]);
            }
            else { hfAbvMin.Value = ""; hfAbvMax.Value = ""; }
        }

        private static void ClampRange(HiddenField lo, HiddenField hi, double min, double max)
        {
            double v;
            if (double.TryParse(lo.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out v) && (v <= min || v > max)) lo.Value = "";
            if (double.TryParse(hi.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out v) && (v >= max || v < min)) hi.Value = "";
        }

        private static Dictionary<string, object> MakeRange(List<double> values, double step, int bucketCount)
        {
            double min = Math.Floor(values.Min() / step) * step;
            double max = Math.Ceiling(values.Max() / step) * step;
            if (max <= min) max = min + step;

            var buckets = new int[bucketCount];
            double width = (max - min) / bucketCount;
            foreach (double v in values)
            {
                int i = (int)((v - min) / width);
                if (i >= bucketCount) i = bucketCount - 1;
                if (i < 0) i = 0;
                buckets[i]++;
            }

            return new Dictionary<string, object>
            {
                { "min", min }, { "max", max }, { "step", step }, { "buckets", buckets }
            };
        }

        // ------------------------------------------------------------------ helpers used by the markup

        protected string Money(object value)
        {
            return "R" + Convert.ToDecimal(value).ToString("N2");
        }

        protected bool IsWishlisted(int productId)
        {
            return CartHelper.GetWishlist(Session).Contains(productId);
        }

        // ------------------------------------------------------------------ cart / wishlist

        private bool TryAddToCart(int productId, out string productName)
        {
            productName = null;

            DataTable t = LoadProductTable(productId);
            if (t.Rows.Count == 0) return false;

            DataRow r = t.Rows[0];
            if (Convert.ToInt32(r["Product_QuantityInStock"]) <= 0) return false;

            productName = Convert.ToString(r["Product_Name"]);

            // The customer is charged the discounted price, not the shelf price
            CartHelper.AddToCart(Session, productId, productName, (decimal)r["DiscountedPrice"]);
            return true;
        }

        protected void rptProducts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int productId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "AddToCart")
            {
                if (TryAddToCart(productId, out string productName))
                {
                    litCartMessage.Text = "<div class='shop-cart-toast'>" + Server.HtmlEncode(productName) + " added to cart.</div>";
                    litCartMessage.Visible = true;
                }
            }
            else if (e.CommandName == "ToggleWishlist")
            {
                CartHelper.ToggleWishlist(Session, productId);
            }
            else if (e.CommandName == "ViewProduct")
            {
                ShowDetail(productId);
            }

            LoadProducts();
        }

        // ------------------------------------------------------------------ product detail overlay

        private void ShowDetail(int productId)
        {
            DataTable t = LoadProductTable(productId);
            if (t.Rows.Count == 0)
            {
                pnlDetail.Visible = false;
                return;
            }

            DataRow r = t.Rows[0];
            DetailProductId = productId;
            DetailCategoryId = Convert.ToInt32(r["Category_ID"]);

            string name = Convert.ToString(r["Product_Name"]);
            string category = Convert.ToString(r["Category_Name"]);
            bool onSale = (bool)r["IsOnSale"];
            int stock = Convert.ToInt32(r["Product_QuantityInStock"]);

            imgDetail.Src = "/ProductImage.ashx?id=" + productId;
            imgDetail.Alt = name;

            litDetailBrand.Text = Convert.ToString(r["Product_Brand"]);
            litDetailName.Text = name;
            litDetailRating.Text = "<div class=\"rating\"><span class=\"rating-stars\"><span class=\"rating-stars-fill\" style=\"width:" + r["RatingPct"] + "%\"></span></span><span class=\"rating-count\">" + r["RatingText"] + " (" + r["RatingCount"] + " reviews)</span></div>";
            lnkDetailCategory.Text = Server.HtmlEncode(category);
            lnkDetailCategory.ToolTip = "Show all " + category + " products";

            spnDetailBadge.Visible = onSale;
            spnDetailBadge.InnerText = "-" + r["DiscountPercent"] + "%";
            spnDetailOld.Visible = onSale;
            spnDetailOld.InnerText = Money(r["Product_SellingPrice"]);
            spnDetailPrice.InnerText = Money(r["DiscountedPrice"]);
            spnDetailPrice.Attributes["class"] = onSale ? "shop-card-price is-sale" : "shop-card-price";

            string desc = Convert.ToString(r["Product_Description"]);
            litDetailDesc.Text = string.IsNullOrWhiteSpace(desc) ? "No description available yet." : desc.Trim();

            var facts = new StringBuilder();
            Action<string, string> add = (label, value) =>
            {
                if (!string.IsNullOrWhiteSpace(value))
                    facts.Append("<div><dt>").Append(Server.HtmlEncode(label)).Append("</dt><dd>")
                         .Append(Server.HtmlEncode(value.Trim())).Append("</dd></div>");
            };
            decimal abv = (decimal)r["AbvValue"];
            add("Type", Convert.ToString(r["Product_Type"]));
            add("Origin", Convert.ToString(r["Product_Origin"]));
            add("Flavour", Convert.ToString(r["Product_Flavour"]));
            add("Alcohol", abv > 0m ? abv.ToString("0.##", CultureInfo.InvariantCulture) + "%" : null);
            litDetailFacts.Text = facts.Length > 0 ? "<dl class=\"pd-facts\">" + facts + "</dl>" : "";

            string ingredients = Convert.ToString(r["Product_Ingredients"]);
            litDetailIngredients.Text = string.IsNullOrWhiteSpace(ingredients)
                ? ""
                : "<p class=\"pd-ingredients\"><strong>Ingredients</strong> " + Server.HtmlEncode(ingredients.Trim()) + "</p>";

            litDetailMsg.Text = "";
            btnDetailAdd.Text = stock > 0 ? "Add to Cart" : "Out of Stock";
            btnDetailAdd.Enabled = stock > 0;
            btnDetailWish.Text = IsWishlisted(productId) ? "Remove from wishlist" : "Add to wishlist";

            pnlDetail.Visible = true;
        }

        protected void btnCloseDetail_Click(object sender, EventArgs e)
        {
            pnlDetail.Visible = false;
        }

        protected void btnDetailAdd_Click(object sender, EventArgs e)
        {
            if (TryAddToCart(DetailProductId, out string productName))
                litDetailMsg.Text = "<div class='shop-cart-toast'>Added to cart.</div>";
            else
                litDetailMsg.Text = "<div class='shop-cart-toast'>Sorry, this item is out of stock.</div>";
        }

        protected void btnDetailWish_Click(object sender, EventArgs e)
        {
            int id = DetailProductId;
            CartHelper.ToggleWishlist(Session, id);
            ShowDetail(id);
            LoadProducts();   // refresh the hearts behind the overlay
        }

        protected void lnkDetailCategory_Click(object sender, EventArgs e)
        {
            string catId = DetailCategoryId.ToString();
            pnlDetail.Visible = false;

            ResetFilters();
            foreach (ListItem item in cblCategories.Items) item.Selected = item.Value == catId;

            LoadProducts();
        }

        // ------------------------------------------------------------------ filter events

        protected void btnSearch_Click(object sender, EventArgs e) => LoadProducts();
        protected void Filter_Changed(object sender, EventArgs e) => LoadProducts();
        protected void btnApplyRanges_Click(object sender, EventArgs e) => LoadProducts();

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            ResetFilters();
            LoadProducts();
        }

        private void ResetFilters()
        {
            txtSearch.Text = "";
            // The search box sits outside the UpdatePanel, so tell the browser to empty it too
            ScriptManager.RegisterStartupScript(this, GetType(), "clearShopSearch",
                "var s=document.getElementById('" + txtSearch.ClientID + "'); if(s){s.value=''; if(window.shopSyncSearch) window.shopSyncSearch();}", true);

            chkOnSale.Checked = false;
            chkWishlistOnly.Checked = false;
            hfPriceMin.Value = "";
            hfPriceMax.Value = "";
            hfAbvMin.Value = "";
            hfAbvMax.Value = "";
            ddlSort.SelectedValue = "featured";

            foreach (CheckBoxList list in new[] { cblCategories, cblBrands, cblTypes, cblOrigins })
            {
                foreach (ListItem item in list.Items) item.Selected = false;
            }
        }
    }
}