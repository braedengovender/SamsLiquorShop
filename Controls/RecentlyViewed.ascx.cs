using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using SamsLiquorShop.Helpers;

namespace SamsLiquorShop.Controls
{
    public partial class RecentlyViewed : System.Web.UI.UserControl
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        /// <summary>Heading above the strip.</summary>
        public string Title { get; set; } = "Recently viewed";

        // Built at PreRender so a product opened during this request is already included
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            Bind();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            RecentlyViewedHelper.Clear(Session);
        }

        private class Item { public int Id; public string Name; public decimal Shelf; public decimal Final; }

        private void Bind()
        {
            List<int> ids = RecentlyViewedHelper.Get(Session).ToList();
            phRecent.Visible = false;
            if (ids.Count == 0) return;

            var items = new Dictionary<int, Item>();
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand())
                {
                    var names = new List<string>();
                    for (int i = 0; i < ids.Count; i++)
                    {
                        names.Add("@p" + i);
                        cmd.Parameters.AddWithValue("@p" + i, ids[i]);
                    }
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT Product_ID, Product_Name, Product_SellingPrice FROM Product " +
                                      "WHERE Product_Status = 'Active' AND Product_ID IN (" + string.Join(",", names) + ")";
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            decimal price = r["Product_SellingPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Product_SellingPrice"]);
                            int id = Convert.ToInt32(r["Product_ID"]);
                            items[id] = new Item { Id = id, Name = Convert.ToString(r["Product_Name"]), Shelf = price, Final = price };
                        }
                    }
                }

                if (items.Count > 0)
                {
                    var prices = PricingHelper.GetPrices(ConnStr, items.Keys.ToArray());
                    foreach (var it in items.Values)
                        if (prices.ContainsKey(it.Id)) it.Final = prices[it.Id].FinalPrice;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("RecentlyViewed: " + ex);
                return;   // the strip is optional: never break the page
            }

            var sb = new StringBuilder();
            foreach (int id in ids)   // keep newest-first order
            {
                Item p;
                if (!items.TryGetValue(id, out p)) continue;   // product removed or inactive
                bool onSale = p.Final < p.Shelf;
                sb.Append("<a href=\"").Append(ResolveUrl("~/Shop.aspx?product=" + p.Id)).Append("\" class=\"card-surface product-card\">");
                if (onSale && p.Shelf > 0)
                    sb.Append("<span class=\"discount-badge\">-").Append(Math.Round((p.Shelf - p.Final) / p.Shelf * 100)).Append("%</span>");
                sb.Append("<img src=\"/ProductImage.ashx?id=").Append(p.Id).Append("\" alt=\"").Append(HttpUtility.HtmlAttributeEncode(p.Name)).Append("\" loading=\"lazy\" />");
                sb.Append("<p class=\"product-name\">").Append(HttpUtility.HtmlEncode(p.Name)).Append("</p>");
                if (onSale) sb.Append("<p class=\"product-price-old\">").Append(Money(p.Shelf)).Append("</p>");
                sb.Append("<p class=\"product-price\">").Append(Money(p.Final)).Append("</p>");
                sb.Append("</a>");
            }

            if (sb.Length == 0) return;
            litCards.Text = sb.ToString();
            phRecent.Visible = true;
        }

        private static string Money(decimal v)
        {
            return "R" + v.ToString("N2", CultureInfo.InvariantCulture);
        }
    }
}
