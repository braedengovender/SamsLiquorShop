using SamsLiquorShop.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SamsLiquorShop
{
    public partial class _Default : Page
    {
        private const int PerRow = 10;   // products shown in each moving row
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private class Item
        {
            public int Id, CategoryId;
            public string Name, Category;
            public decimal Shelf, Final;
            public bool OnSale { get { return Final < Shelf; } }
            public int Percent
            {
                get
                {
                    return (OnSale && Shelf > 0m)
                        ? Math.Max(1, (int)Math.Round((Shelf - Final) / Shelf * 100m, MidpointRounding.AwayFromZero))
                        : 0;
                }
            }
        }

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

        private string _addMessage;

        // ------------------------------------------------------------------ page load

        protected void Page_Load(object sender, EventArgs e)
        {
            // The rows sit outside the UpdatePanel, so there's nothing to rebuild when a pop-up opens
            var sm = ScriptManager.GetCurrent(Page);
            if (sm != null && sm.IsInAsyncPostBack) return;

            try { LoadProductRows(); }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Home: product rows failed: " + ex);
                secFeatured.Visible = secPromos.Visible = secNonAlc.Visible = secGifts.Visible = false;
            }

            try { LoadCategoryRow(); }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Home: category row failed: " + ex);
                secCategories.Visible = false;
            }
        }

        // ------------------------------------------------------------------ rows

        private void LoadProductRows()
        {
            var items = new List<Item>();

            const string sql =
                "SELECT p.Product_ID, p.Product_Name, p.Product_SellingPrice, p.Category_ID, c.Category_Name " +
                "FROM Product p INNER JOIN Category c ON p.Category_ID = c.Category_ID " +
                "WHERE p.Product_Status = 'Active' AND p.Product_QuantityInStock > 0";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        decimal price = Convert.ToDecimal(r["Product_SellingPrice"]);
                        items.Add(new Item
                        {
                            Id = Convert.ToInt32(r["Product_ID"]),
                            Name = Convert.ToString(r["Product_Name"]),
                            CategoryId = Convert.ToInt32(r["Category_ID"]),
                            Category = Convert.ToString(r["Category_Name"]),
                            Shelf = price,
                            Final = price
                        });
                    }
                }
            }

            // Work out the price the customer really pays (product and category discounts)
            if (items.Count > 0)
            {
                try
                {
                    var prices = PricingHelper.GetPrices(ConnStr, items.Select(i => i.Id).ToArray());
                    foreach (var i in items)
                        if (prices.ContainsKey(i.Id)) i.Final = prices[i.Id].FinalPrice;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("Home: pricing failed: " + ex);
                }
            }

            var rng = new Random();
            Func<IEnumerable<Item>, List<Item>> pick = src => src.OrderBy(x => rng.Next()).Take(PerRow).ToList();

            FillRow(secFeatured, litFeatured, pick(items));
            FillRow(secPromos, litPromos, pick(items.Where(i => i.OnSale)));
            FillRow(secNonAlc, litNonAlc, pick(items.Where(i => Shop.CategoryMatchesSlug("non-alcoholic", i.Category))));
            FillRow(secGifts, litGifts, pick(items.Where(i => Shop.CategoryMatchesSlug("gifts", i.Category))));
        }

        private void FillRow(System.Web.UI.HtmlControls.HtmlGenericControl section, Literal lit, List<Item> list)
        {
            if (list.Count == 0) { section.Visible = false; return; }   // nothing to show: hide the whole section

            var sb = new StringBuilder();
            sb.Append("<div class=\"marquee\"><div class=\"marquee-track\">");
            foreach (var p in list)
            {
                sb.Append("<a href=\"Shop.aspx?product=").Append(p.Id)
                  .Append("\" class=\"card-surface product-card\" data-pid=\"").Append(p.Id).Append("\">");
                if (p.OnSale)
                    sb.Append("<span class=\"discount-badge\">-").Append(p.Percent).Append("%</span>");
                sb.Append("<img src=\"/ProductImage.ashx?id=").Append(p.Id)
                  .Append("\" alt=\"").Append(HttpUtility.HtmlAttributeEncode(p.Name)).Append("\" />");
                sb.Append("<p class=\"product-name\">").Append(HttpUtility.HtmlEncode(p.Name)).Append("</p>");
                if (p.OnSale)
                    sb.Append("<p class=\"product-price-old\">").Append(Money(p.Shelf)).Append("</p>");
                sb.Append("<p class=\"product-price\">").Append(Money(p.Final)).Append("</p>");
                sb.Append("</a>");
            }
            sb.Append("</div></div>");
            lit.Text = sb.ToString();
        }

        private void LoadCategoryRow()
        {
            const string sql =
                "SELECT c.Category_ID, c.Category_Name, " +
                "ISNULL((SELECT TOP 1 p.Product_ID FROM Product p WHERE p.Category_ID = c.Category_ID AND p.Product_Status = 'Active' ORDER BY p.Product_ID), 0) AS SampleId " +
                "FROM Category c ORDER BY c.Category_Name";

            var sb = new StringBuilder();
            int count = 0;

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    sb.Append("<div class=\"marquee\"><div class=\"marquee-track\">");
                    while (r.Read())
                    {
                        int id = Convert.ToInt32(r["Category_ID"]);
                        string name = Convert.ToString(r["Category_Name"]);
                        string img = CategoryImage(name, Convert.ToInt32(r["SampleId"]));

                        sb.Append("<a href=\"Shop.aspx?cat=").Append(id).Append("\" class=\"card-surface category-card\">");
                        if (img.Length > 0)
                            sb.Append("<img src=\"").Append(HttpUtility.HtmlAttributeEncode(img)).Append("\" alt=\"\" />");
                        sb.Append("<p>").Append(HttpUtility.HtmlEncode(name)).Append("</p></a>");
                        count++;
                    }
                    sb.Append("</div></div>");
                }
            }

            if (count == 0) { secCategories.Visible = false; return; }
            litCategories.Text = sb.ToString();
        }

        // Uses your existing category pictures when the name matches, otherwise a product photo from that category
        private string CategoryImage(string name, int sampleProductId)
        {
            string n = Shop.NormalizeKey(name), file = null;

            if (n.Contains("nonalc")) file = "nonalcoholic.png";
            else if (n.Contains("beer") || n.Contains("cider")) file = "beer.jpg";
            else if (n.Contains("wine")) file = "wine.jpg";
            else if (n.Contains("whisk")) file = "whiskey.jpg";
            else if (n.Contains("spirit")) file = "spirits.jpg";
            else if (n.Contains("accessor") || n.Contains("barware") || n.Contains("gift")) file = "accessories.jpg";
            else if (n.Contains("snack") || n.Contains("food")) file = "snacks.jpg";
            else if (n.Contains("tobacco") || n.Contains("vap")) file = "tobaccoandvape.jpg";

            if (file != null && File.Exists(Server.MapPath("~/Images/categories/" + file)))
                return "Images/categories/" + file;

            return sampleProductId > 0 ? "/ProductImage.ashx?id=" + sampleProductId : "";
        }

        private static string Money(decimal value)
        {
            return "R" + value.ToString("N2");
        }

        // ------------------------------------------------------------------ product pop-up (same one as the Shop page)

        protected void btnOpenProduct_Click(object sender, EventArgs e)
        {
            int id;
            if (int.TryParse(hfOpenProduct.Value, out id) && id > 0) ShowDetail(id);
        }

        private void ShowDetail(int productId)
        {
            var d = ProductDetailHelper.Get(ConnStr, productId);
            if (d == null) { pnlDetail.Visible = false; return; }

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
            spnDetailOld.InnerText = Money(d.ShelfPrice);
            spnDetailPrice.InnerText = Money(d.FinalPrice);
            spnDetailPrice.Attributes["class"] = d.OnSale ? "shop-card-price is-sale" : "shop-card-price";

            litDetailDesc.Text = string.IsNullOrWhiteSpace(d.Description) ? "No description available yet." : d.Description.Trim();

            var facts = new StringBuilder();
            Action<string, string> add = (label, value) =>
            {
                if (!string.IsNullOrWhiteSpace(value))
                    facts.Append("<div><dt>").Append(Server.HtmlEncode(label)).Append("</dt><dd>")
                         .Append(Server.HtmlEncode(value.Trim())).Append("</dd></div>");
            };
            add("Type", d.Type);
            add("Origin", d.Origin);
            add("Flavour", d.Flavour);
            add("Alcohol", d.Abv > 0m ? d.Abv.ToString("0.##", CultureInfo.InvariantCulture) + "%" : null);
            litDetailFacts.Text = facts.Length > 0 ? "<dl class=\"pd-facts\">" + facts + "</dl>" : "";

            litDetailIngredients.Text = string.IsNullOrWhiteSpace(d.Ingredients)
                ? ""
                : "<p class=\"pd-ingredients\"><strong>Ingredients</strong> " + Server.HtmlEncode(d.Ingredients.Trim()) + "</p>";

            litDetailMsg.Text = "";
            btnDetailAdd.Text = d.Stock > 0 ? "Add to Cart" : "Out of Stock";
            btnDetailAdd.Enabled = d.Stock > 0;
            btnDetailWish.Text = CartHelper.GetWishlist(Session).Contains(productId) ? "Remove from wishlist" : "Add to wishlist";

            pnlDetail.Visible = true;
        }

        private bool TryAddToCart(int productId)
        {
            var d = ProductDetailHelper.Get(ConnStr, productId);
            if (d == null) return false;

            if (d.Stock <= 0)
            {
                _addMessage = "Sorry, this item is out of stock.";
                return false;
            }

            var inCart = CartHelper.GetCart(Session).FirstOrDefault(c => c.ProductId == productId);
            int max = Math.Min(d.Stock, CartHelper.MaxPerLine);
            if (inCart != null && inCart.Quantity >= max)
            {
                _addMessage = "You already have the maximum available (" + max + ") in your cart.";
                return false;
            }

            CartHelper.AddToCart(Session, productId, d.Name, d.FinalPrice);
            return true;
        }

        protected void btnCloseDetail_Click(object sender, EventArgs e)
        {
            pnlDetail.Visible = false;
        }

        protected void btnDetailAdd_Click(object sender, EventArgs e)
        {
            litDetailMsg.Text = TryAddToCart(DetailProductId)
                ? "<div class='shop-cart-toast'>Added to cart.</div>"
                : "<div class='shop-cart-toast'>" + Server.HtmlEncode(_addMessage ?? "Sorry, we couldn't add that item.") + "</div>";
        }

        protected void btnDetailWish_Click(object sender, EventArgs e)
        {
            int id = DetailProductId;
            CartHelper.ToggleWishlist(Session, id);
            ShowDetail(id);
        }

        protected void lnkDetailCategory_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Shop.aspx?cat=" + DetailCategoryId);
        }
    }
}