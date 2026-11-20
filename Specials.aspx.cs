using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
            if (!IsPostBack) BindSpecials();
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

                int stock = r["Product_QuantityInStock"] == DBNull.Value ? 0 : Convert.ToInt32(r["Product_QuantityInStock"]);

                result.Rows.Add(id, Convert.ToString(r["Product_Name"]), Convert.ToInt32(r["Category_ID"]),
                    Convert.ToString(r["Category_Name"]), shelf, final, percent, stock > 0, running,
                    running ? "" : WhyNotRunning(r));
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

            int maxOff = 0, runningCount = 0;
            SortedDictionary<string, int> categories = new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow r in specials.Rows)
            {
                if ((bool)r["Running"])
                {
                    runningCount++;
                    maxOff = Math.Max(maxOff, Convert.ToInt32(r["DiscountPercent"]));
                }
                categories[Convert.ToString(r["Category_Name"])] = Convert.ToInt32(r["Category_ID"]);
            }

            // The "up to X% off" strip only makes sense while something is actually running
            phStats.Visible = runningCount > 0;
            litMaxOff.Text = maxOff.ToString();
            litCount.Text = runningCount.ToString();

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
            if (e.CommandName == "AddToCart")
            {
                int productId;
                if (int.TryParse(Convert.ToString(e.CommandArgument), out productId))
                {
                    DataRow[] hit = LoadSpecials().Select("Product_ID = " + productId);

                    if (hit.Length == 0)
                    {
                        ShowMessage("Sorry, that product is no longer listed.");
                    }
                    else if (!(bool)hit[0]["InStock"])
                    {
                        ShowMessage("Sorry, that product is out of stock.");
                    }
                    else
                    {
                        string name = Convert.ToString(hit[0]["Product_Name"]);
                        // The customer is charged what is shown: the special price while it runs, otherwise the shelf price
                        CartHelper.AddToCart(Session, productId, name, (decimal)hit[0]["DiscountedPrice"]);
                        ShowMessage(name + " added to cart.");
                    }
                }
            }

            BindSpecials();
        }

        private void ShowMessage(string text)
        {
            litCartMessage.Text = "<div class='shop-cart-toast'>" + Server.HtmlEncode(text) + "</div>";
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
