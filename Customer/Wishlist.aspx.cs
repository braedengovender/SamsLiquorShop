using SamsLiquorShop.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SamsLiquorShop.Customer
{
    public partial class Wishlist : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private int CustomerId => Convert.ToInt32(Session["CustomerID"]);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Customer")) return;

            if (!IsPostBack)
            {
                LoadWishlist();
            }
        }

        // Used by the markup to decide whether the Add to Cart button is enabled
        protected bool IsAvailable(object dataItem)
        {
            DataRowView row = (DataRowView)dataItem;
            return row["Product_Status"].ToString().Trim() == "Active"
                && Convert.ToInt32(row["Product_QuantityInStock"]) > 0;
        }

        // ---------- LOAD ----------
        private DataTable GetWishlistTable()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT p.Product_ID, p.Product_Name, p.Product_Brand, p.Product_SellingPrice,
                       p.Product_QuantityInStock, p.Product_Status
                FROM Wishlist w
                INNER JOIN Product p ON w.Product_ID = p.Product_ID
                WHERE w.Customer_ID = @C
                ORDER BY w.Wishlist_DateAdded DESC", conn))
            {
                cmd.Parameters.AddWithValue("@C", CustomerId);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }

            // current price including any active discount (same rules as the Shop and Cart)
            var ids = new System.Collections.Generic.List<int>();
            foreach (DataRow r in dt.Rows) ids.Add(Convert.ToInt32(r["Product_ID"]));
            var prices = PricingHelper.GetPrices(ConnStr, ids);

            dt.Columns.Add("DiscountedPrice", typeof(decimal));
            foreach (DataRow r in dt.Rows)
            {
                int id = Convert.ToInt32(r["Product_ID"]);
                decimal shelf = Convert.ToDecimal(r["Product_SellingPrice"]);
                r["DiscountedPrice"] = prices.ContainsKey(id) ? prices[id].FinalPrice : shelf;
            }
            return dt;
        }

        private void LoadWishlist()
        {
            DataTable dt;
            try
            {
                dt = GetWishlistTable();
            }
            catch (SqlException ex)
            {
                ShowMessage("Your wishlist could not be loaded. If this mentions an invalid object name, run Database\\CreateWishlistTable.sql on the GroupWst15 database first. (" + ex.Message + ")", false);
                pnlToolbar.Visible = false;
                return;
            }

            rptWishlist.DataSource = dt;
            rptWishlist.DataBind();

            int available = 0;
            foreach (DataRow r in dt.Rows)
            {
                if (r["Product_Status"].ToString().Trim() == "Active" && Convert.ToInt32(r["Product_QuantityInStock"]) > 0) available++;
            }

            bool any = dt.Rows.Count > 0;
            pnlEmpty.Visible = !any;
            pnlToolbar.Visible = any;
            btnAddAll.Visible = available > 0;
            litCount.Text = dt.Rows.Count + (dt.Rows.Count == 1 ? " saved product" : " saved products");

            // keep the Shop page's hearts in step with what is stored
            WishlistHelper.LoadIntoSession(Session);
        }

        // ---------- ROW BUTTONS ----------
        protected void rptWishlist_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            int productId = Convert.ToInt32(e.CommandArgument);
            litMessage.Text = "";

            if (e.CommandName == "Remove")
            {
                if (!WishlistHelper.Remove(CustomerId, productId))
                {
                    ShowMessage("That product could not be removed. Please try again.", false);
                    return;
                }
                CartHelper.GetWishlist(Session).Remove(productId);
            }
            else if (e.CommandName == "AddToCart")
            {
                string name;
                if (AddProductToCart(productId, out name))
                    ShowMessage(name + " added to your cart.", true);
                else
                    ShowMessage("That product is currently unavailable.", false);
            }

            LoadWishlist();
        }

        protected void btnAddAll_Click(object sender, EventArgs e)
        {
            int added = 0;
            foreach (DataRow r in GetWishlistTable().Rows)
            {
                string name;
                if (AddProductToCart(Convert.ToInt32(r["Product_ID"]), out name)) added++;
            }

            if (added == 0) ShowMessage("None of your saved products are currently available.", false);
            else ShowMessage(added + " product(s) added to your cart.", true);

            LoadWishlist();
        }

        // Adds one unit using the current price; returns false when the product is inactive or out of stock
        private bool AddProductToCart(int productId, out string name)
        {
            name = "";
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT Product_Name, Product_SellingPrice, Product_QuantityInStock, Product_Status
                FROM Product WHERE Product_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", productId);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return false;
                    if (r["Product_Status"].ToString().Trim() != "Active" || Convert.ToInt32(r["Product_QuantityInStock"]) <= 0) return false;

                    name = r["Product_Name"].ToString();
                    decimal shelf = Convert.ToDecimal(r["Product_SellingPrice"]);
                    r.Close();

                    var prices = PricingHelper.GetPrices(ConnStr, new[] { productId });
                    CartHelper.AddToCart(Session, productId, name, prices.ContainsKey(productId) ? prices[productId].FinalPrice : shelf);
                    return true;
                }
            }
        }

        private void ShowMessage(string text, bool success)
        {
            litMessage.Text = "<div class=\"emp-alert " + (success ? "emp-alert-success" : "emp-alert-error") + "\">"
                + Server.HtmlEncode(text) + (success ? " <a href=\"" + ResolveUrl("~/Cart.aspx") + "\">View cart</a>" : "") + "</div>";
        }
    }
}
