using SamsLiquorShop.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SamsLiquorShop.Customer
{
    public partial class Orders : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private int CustomerId => Convert.ToInt32(Session["CustomerID"]);

        // Order lines for the orders currently on screen; filled by LoadOrders and read by rptOrders_ItemDataBound
        private DataTable _lines;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Customer")) return;

            if (!IsPostBack)
            {
                LoadOrders();
            }
        }

        protected void ddlRange_Changed(object sender, EventArgs e)
        {
            litMessage.Text = "";
            LoadOrders();
        }

        // ---------- LOAD ----------
        private void LoadOrders()
        {
            int days = int.Parse(ddlRange.SelectedValue);
            DateTime since = days > 0 ? DateTime.Now.AddDays(-days) : new DateTime(1753, 1, 1);

            DataTable orders = new DataTable();
            _lines = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT
                            s.Sale_ID,
                            'INV-' + RIGHT('000000' + CAST(s.Sale_ID AS VARCHAR(6)), 6) AS InvoiceNumber,
                            s.Sale_DateTime,
                            (SELECT ISNULL(SUM(SaleLine_Quantity), 0) FROM SaleLine WHERE Sale_ID = s.Sale_ID) AS ItemCount,
                            s.Sale_TotalAmount
                        FROM Sale s
                        WHERE s.Customer_ID = @CustomerId AND s.Sale_DateTime >= @Since
                        ORDER BY s.Sale_DateTime DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
                        cmd.Parameters.AddWithValue("@Since", since);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(orders);
                    }

                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT
                            sl.Sale_ID,
                            p.Product_Name,
                            sl.SaleLine_Quantity AS Quantity,
                            CASE WHEN sl.SaleLine_Quantity > 0 THEN sl.SaleLine_Subtotal / sl.SaleLine_Quantity ELSE 0 END AS UnitPrice,
                            sl.SaleLine_Subtotal AS Subtotal
                        FROM SaleLine sl
                        INNER JOIN Sale s ON sl.Sale_ID = s.Sale_ID
                        INNER JOIN Product p ON sl.Product_ID = p.Product_ID
                        WHERE s.Customer_ID = @CustomerId AND s.Sale_DateTime >= @Since
                        ORDER BY sl.Sale_ID, p.Product_Name", conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
                        cmd.Parameters.AddWithValue("@Since", since);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(_lines);
                    }
                }
            }
            catch (SqlException ex)
            {
                ShowMessage("Your orders could not be loaded: " + ex.Message, false);
                return;
            }

            decimal total = 0;
            foreach (DataRow r in orders.Rows) total += Convert.ToDecimal(r["Sale_TotalAmount"]);

            rptOrders.DataSource = orders;
            rptOrders.DataBind();

            bool any = orders.Rows.Count > 0;
            pnlEmpty.Visible = !any;
            litEmpty.Text = days > 0
                ? "You haven't placed any orders in this period."
                : "You haven't placed any orders yet.";
            litSummary.Text = any
                ? orders.Rows.Count + (orders.Rows.Count == 1 ? " order" : " orders") + " &middot; " + total.ToString("C") + " spent"
                : "";
        }

        protected void rptOrders_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            DataRowView order = (DataRowView)e.Item.DataItem;
            Repeater rptLines = (Repeater)e.Item.FindControl("rptLines");

            DataView lines = new DataView(_lines);
            lines.RowFilter = "Sale_ID = " + Convert.ToInt32(order["Sale_ID"]);
            rptLines.DataSource = lines;
            rptLines.DataBind();
        }

        // ---------- BUY AGAIN ----------
        protected void rptOrders_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Reorder") return;

            int saleId = Convert.ToInt32(e.CommandArgument);
            int added = 0, skipped = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    DataTable items = new DataTable();
                    // The Customer_ID check makes sure a customer can only re-order their own orders
                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT sl.Product_ID, SUM(sl.SaleLine_Quantity) AS Qty
                        FROM SaleLine sl
                        INNER JOIN Sale s ON sl.Sale_ID = s.Sale_ID
                        WHERE s.Sale_ID = @SaleId AND s.Customer_ID = @CustomerId
                        GROUP BY sl.Product_ID", conn))
                    {
                        cmd.Parameters.AddWithValue("@SaleId", saleId);
                        cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(items);
                    }

                    foreach (DataRow item in items.Rows)
                    {
                        int productId = Convert.ToInt32(item["Product_ID"]);
                        int wanted = Convert.ToInt32(item["Qty"]);

                        string name = null;
                        decimal shelfPrice = 0;
                        int stock = 0;
                        bool active = false;

                        using (SqlCommand cmd = new SqlCommand(@"
                            SELECT Product_Name, Product_SellingPrice, Product_QuantityInStock, Product_Status
                            FROM Product WHERE Product_ID = @Id", conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", productId);
                            using (SqlDataReader r = cmd.ExecuteReader())
                            {
                                if (r.Read())
                                {
                                    name = r["Product_Name"].ToString();
                                    shelfPrice = Convert.ToDecimal(r["Product_SellingPrice"]);
                                    stock = Convert.ToInt32(r["Product_QuantityInStock"]);
                                    active = r["Product_Status"].ToString().Trim() == "Active";
                                }
                            }
                        }

                        if (name == null || !active || stock <= 0)
                        {
                            skipped++;
                            continue;
                        }

                        // charge today's price (including any current discount), not what was paid last time
                        var prices = PricingHelper.GetPrices(ConnStr, new[] { productId });
                        decimal price = prices.ContainsKey(productId) ? prices[productId].FinalPrice : shelfPrice;

                        CartHelper.AddToCart(Session, productId, name, price, Math.Min(wanted, stock));
                        added++;
                    }
                }
            }
            catch (SqlException ex)
            {
                ShowMessage("We couldn't add those items to your cart: " + ex.Message, false);
                return;
            }

            if (added > 0 && skipped == 0)
            {
                Response.Redirect("~/Cart.aspx");
                return;
            }

            if (added == 0)
            {
                ShowMessage("None of the items from that order are currently available.", false);
            }
            else
            {
                ShowMessage(added + " item(s) added to your cart. " + skipped + " item(s) are no longer available.", true);
            }
        }

        private void ShowMessage(string text, bool success)
        {
            litMessage.Text = "<div class=\"emp-alert " + (success ? "emp-alert-success" : "emp-alert-error") + "\">"
                + Server.HtmlEncode(text) + (success ? " <a href=\"" + ResolveUrl("~/Cart.aspx") + "\">View cart</a>" : "") + "</div>";
        }
    }
}
