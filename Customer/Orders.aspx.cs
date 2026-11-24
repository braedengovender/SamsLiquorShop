using SamsLiquorShop.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace SamsLiquorShop.Customer
{
    public partial class Orders : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private int CustomerId => Convert.ToInt32(Session["CustomerID"]);

        // Order lines for the orders currently on screen; filled by LoadOrders and read by rptOrders_ItemDataBound
        private DataTable _lines;

        // Status history rows of the online orders currently on screen
        private DataTable _history;

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
        // The order opened from the dashboard (Orders.aspx?order=123) starts expanded
        protected string OpenAttr(object saleId)
        {
            return Request.QueryString["order"] == Convert.ToString(saleId) ? "open" : "";
        }
        // ---------- LOAD ----------
        private void LoadOrders()
        {
            int days = int.Parse(ddlRange.SelectedValue);
            DateTime since = days > 0 ? SlotHelper.Now().AddDays(-days) : new DateTime(1753, 1, 1);

            DataTable orders = new DataTable();
            _lines = new DataTable();
            _history = new DataTable();

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
                            s.Sale_TotalAmount,
                            o.OnlineOrder_ID,
                            o.OnlineOrder_Method AS OrderMethod,
                            COALESCE(o.OnlineOrder_EstimatedDateTime, o.OnlineOrder_CollectionSlot) AS OrderSlot,
                            o.OnlineOrder_CollectionCode AS CollectionCode,
                            (SELECT TOP 1 h.OnlineOrderStatusHistory_Status FROM OnlineOrderStatusHistory h
                             WHERE h.OnlineOrder_ID = o.OnlineOrder_ID ORDER BY h.OnlineOrderStatusHistory_ID DESC) AS OrderStatus
                        FROM Sale s
                        LEFT JOIN OnlineOrder o ON o.Sale_ID = s.Sale_ID
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
                            sl.Product_ID,
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

                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT h.OnlineOrder_ID,
                               h.OnlineOrderStatusHistory_Status AS Status,
                               h.OnlineOrderStatusHistory_DateTime AS ChangedAt,
                               h.OnlineOrderStatusHistory_Notes AS Notes
                        FROM OnlineOrderStatusHistory h
                        INNER JOIN OnlineOrder o ON o.OnlineOrder_ID = h.OnlineOrder_ID
                        INNER JOIN Sale s ON s.Sale_ID = o.Sale_ID
                        WHERE s.Customer_ID = @CustomerId AND s.Sale_DateTime >= @Since
                        ORDER BY h.OnlineOrder_ID, h.OnlineOrderStatusHistory_ID", conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerId", CustomerId);
                        cmd.Parameters.AddWithValue("@Since", since);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(_history);
                    }
                }
            }
            catch (SqlException ex)
            {
                ShowMessage("Your orders could not be loaded: " + ex.Message, false);
                return;
            }

            decimal total = 0;
            int live = 0;
            orders.Columns.Add("IsLive", typeof(bool));
            foreach (DataRow r in orders.Rows)
            {
                string st = r["OrderStatus"] == DBNull.Value ? "" : Convert.ToString(r["OrderStatus"]).Trim();
                bool isLive = r["OnlineOrder_ID"] != DBNull.Value && IsLiveStatus(st);
                r["IsLive"] = isLive;
                if (isLive) live++;

                bool reversed = st == "Cancelled" || st == "Expired";
                if (!reversed) total += Convert.ToDecimal(r["Sale_TotalAmount"]);
            }

            // orders still on their way come first, newest first within each group
            orders.DefaultView.Sort = "IsLive DESC, Sale_DateTime DESC";
            rptOrders.DataSource = orders.DefaultView;
            rptOrders.DataBind();

            bool any = orders.Rows.Count > 0;
            pnlEmpty.Visible = !any;
            litEmpty.Text = days > 0
                ? "You haven't placed any orders in this period."
                : "You haven't placed any orders yet.";
            litSummary.Text = any
                ? orders.Rows.Count + (orders.Rows.Count == 1 ? " order" : " orders") + " &middot; " + total.ToString("C") + " spent"
                  + (live > 0 ? " &middot; <strong style=\"color:var(--accent);\">" + live + " in progress</strong>" : "")
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

            Literal litTimeline = (Literal)e.Item.FindControl("litTimeline");
            if (litTimeline != null)
                litTimeline.Text = order["OnlineOrder_ID"] == DBNull.Value ? "" : BuildTimeline(order);
        }

        // ---------- ONLINE ORDER STATUS ----------
        private static readonly string[] DeliveryPath = { "Placed", "Packing", "Out for delivery", "Delivered" };
        private static readonly string[] CollectionPath = { "Placed", "Packing", "Ready for collection", "Collected" };

        protected static bool IsLiveStatus(string status)
        {
            string s = (status ?? "").Trim();
            return s.Length > 0 && s != "Delivered" && s != "Collected" && s != "Cancelled" && s != "Expired";
        }

        protected string CardClass(object isLive)
        {
            return (isLive is bool && (bool)isLive) ? "mgr-table-card ord-card is-live" : "mgr-table-card ord-card";
        }

        protected string OpenAttribute(object isLive)
        {
            return (isLive is bool && (bool)isLive) ? "open" : "";
        }

        protected string StatusPill(object status)
        {
            string s = status == null || status == DBNull.Value ? "" : Convert.ToString(status).Trim();
            if (s.Length == 0) return "";
            string css = IsLiveStatus(s) ? "is-live" : (s == "Cancelled" || s == "Expired") ? "is-bad" : "is-done";
            return "<span class=\"ord-pill " + css + "\">" + Server.HtmlEncode(s) + "</span>";
        }

        protected string ReceiptHref(object saleId, object onlineOrderId, bool print)
        {
            int id = Convert.ToInt32(saleId);
            string href = (onlineOrderId != null && onlineOrderId != DBNull.Value)
                ? ResolveUrl("~/OrderConfirmation.aspx?id=" + id)
                : "Receipt.aspx?sale=" + id;
            return href + (print ? "&print=1" : "");
        }

        private static string SlotLabel(bool delivery, DateTime s)
        {
            string day = s.ToString("ddd d MMM", CultureInfo.InvariantCulture);
            if (delivery)
            {
                var w = SlotHelper.GetDeliveryWindows(s.Date).FirstOrDefault(x => x.Start == s);
                return day + ", " + (w != null ? w.Label : s.ToString("HH:mm", CultureInfo.InvariantCulture));
            }
            return day + ", " + s.ToString("HH:mm", CultureInfo.InvariantCulture) + " - " + s.AddHours(1).ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        private string BuildTimeline(DataRowView order)
        {
            try
            {
                int orderId = Convert.ToInt32(order["OnlineOrder_ID"]);
                string method = Convert.ToString(order["OrderMethod"]).Trim();
                string current = Convert.ToString(order["OrderStatus"]).Trim();
                bool delivery = method.Equals("Delivery", StringComparison.OrdinalIgnoreCase);
                string[] path = delivery ? DeliveryPath : CollectionPath;
                bool live = IsLiveStatus(current);
                bool ended = current == "Cancelled" || current == "Expired";

                var rows = new List<DataRowView>();
                DataView hv = new DataView(_history) { RowFilter = "OnlineOrder_ID = " + orderId };
                foreach (DataRowView h in hv) rows.Add(h);

                var reached = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
                foreach (var h in rows) reached[Convert.ToString(h["Status"]).Trim()] = Convert.ToDateTime(h["ChangedAt"]);

                var sb = new StringBuilder();
                sb.Append("<div class=\"ord-track\"><div class=\"ord-track-head\"><strong>Order progress</strong>");
                if (order["OrderSlot"] != DBNull.Value)
                    sb.Append("<span>").Append(delivery ? "Delivery window: " : "Collection slot: ")
                      .Append(Server.HtmlEncode(SlotLabel(delivery, Convert.ToDateTime(order["OrderSlot"])))).Append("</span>");
                sb.Append("</div><ol class=\"ord-steps\">");

                foreach (string step in path)
                {
                    bool done = reached.ContainsKey(step);
                    bool isCurrent = step == current && live;
                    sb.Append("<li class=\"").Append(done ? (isCurrent ? "is-current" : "is-done") : "").Append("\">")
                      .Append("<span class=\"ord-step-dot\"></span><span class=\"ord-step-name\">").Append(Server.HtmlEncode(step)).Append("</span>")
                      .Append("<span class=\"ord-step-time\">")
                      .Append(done ? reached[step].ToString("d MMM, HH:mm", CultureInfo.InvariantCulture) : "")
                      .Append("</span></li>");
                }
                sb.Append("</ol>");

                if (ended)
                {
                    string note = rows.Count > 0 ? Convert.ToString(rows[rows.Count - 1]["Notes"]).Trim() : "";
                    sb.Append("<p class=\"ord-ended\"><strong>This order was ").Append(current.ToLowerInvariant()).Append(".</strong> ")
                      .Append(Server.HtmlEncode(note)).Append(" Any points used were returned to your account.</p>");
                }

                string code = Convert.ToString(order["CollectionCode"]).Trim();
                if (!delivery && live && code.Length > 0)
                    sb.Append("<p class=\"ord-code\">Collection code: <strong>").Append(Server.HtmlEncode(code)).Append("</strong> &middot; bring it and your ID.</p>");

                sb.Append("<ul class=\"ord-history\">");
                for (int i = rows.Count - 1; i >= 0; i--)
                {
                    string note = Convert.ToString(rows[i]["Notes"]).Trim();
                    sb.Append("<li><strong>").Append(Server.HtmlEncode(Convert.ToString(rows[i]["Status"]).Trim())).Append("</strong>")
                      .Append("<time>").Append(Convert.ToDateTime(rows[i]["ChangedAt"]).ToString("d MMM yyyy, HH:mm", CultureInfo.InvariantCulture)).Append("</time>");
                    if (note.Length > 0) sb.Append("<em>").Append(Server.HtmlEncode(note)).Append("</em>");
                    sb.Append("</li>");
                }
                sb.Append("</ul></div>");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Orders.BuildTimeline: " + ex);
                return "";   // the order list still works without the timeline
            }
        }

        // ---------- BUY AGAIN ----------
        protected void rptOrders_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            // ---------- EMAIL RECEIPT (receipt built by Helpers/ReceiptHelper.cs) ----------
            if (e.CommandName == "EmailReceipt")
            {
                string message;
                bool sent = ReceiptHelper.Email(ConnStr, Convert.ToInt32(e.CommandArgument), CustomerId, out message);
                litMessage.Text = "<div class=\"emp-alert " + (sent ? "emp-alert-success" : "emp-alert-error") + "\">"
                    + Server.HtmlEncode(message) + "</div>";
                return;
            }

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
