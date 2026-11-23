using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using SamsLiquorShop.Helpers;

namespace SamsLiquorShop.Controls
{
    /// <summary>Online order queue for staff. Used by Manager/OnlineOrders.aspx and Employee/OnlineOrders.aspx.</summary>
    public partial class OnlineOrdersBrowser : System.Web.UI.UserControl
    {
        /// <summary>Kept so existing pages still compile. Managers and cashiers currently get the same tools.</summary>
        public bool ManagerMode { get; set; }

        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private static readonly string[] DeliveryPath = { "Placed", "Packing", "Out for delivery", "Delivered" };
        private static readonly string[] CollectionPath = { "Placed", "Packing", "Ready for collection", "Collected" };

        // Every order with its current status (the latest history row) and whether it is still live
        private const string OrdersCte = @"
            WITH q AS (
                SELECT o.OnlineOrder_ID, s.Sale_ID,
                       'INV-' + RIGHT('000000' + CAST(s.Sale_ID AS VARCHAR(10)), 6) AS InvoiceNumber,
                       s.Sale_DateTime, o.OnlineOrder_Method AS Method,
                       LTRIM(RTRIM(o.OnlineOrder_FirstName + ' ' + o.OnlineOrder_LastName)) AS CustomerName,
                       o.OnlineOrder_PhoneNumber AS Phone, o.OnlineOrder_EmailAddress AS Email,
                       COALESCE(o.OnlineOrder_EstimatedDateTime, o.OnlineOrder_CollectionSlot) AS Slot,
                       o.OnlineOrder_CollectionCode AS CollectionCode, s.Sale_TotalAmount,
                       ISNULL((SELECT TOP 1 h.OnlineOrderStatusHistory_Status FROM OnlineOrderStatusHistory h
                               WHERE h.OnlineOrder_ID = o.OnlineOrder_ID ORDER BY h.OnlineOrderStatusHistory_ID DESC), 'Placed') AS CurrentStatus
                FROM OnlineOrder o INNER JOIN Sale s ON s.Sale_ID = o.Sale_ID
            ),
            r AS (
                SELECT q.*, CASE WHEN q.CurrentStatus IN ('Delivered', 'Collected', 'Cancelled', 'Expired') THEN 0 ELSE 1 END AS IsLive FROM q
            )";

        protected void Page_Load(object sender, EventArgs e)
        {
            txtSearch.Attributes["data-live-search"] = btnSearch.ClientID;
            if (!IsPostBack) BindAll();
        }

        // ---------- helpers ----------
        private int? EmployeeId()
        {
            object o = Session["EmployeeID"];
            int v;
            return o != null && int.TryParse(o.ToString(), out v) && v > 0 ? (int?)v : null;
        }

        private static string Clean(string s)
        {
            return Regex.Replace(s ?? "", @"[\x00-\x1F\x7F]+", " ").Trim();
        }

        private static string EscapeLike(string s)
        {
            return s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");
        }

        private static string Str(DataRow r, string col)
        {
            return r[col] == DBNull.Value ? "" : Convert.ToString(r[col]).Trim();
        }

        private void ShowMessage(string text, bool success)
        {
            litMessage.Text = "<div class=\"emp-alert " + (success ? "emp-alert-success" : "emp-alert-error") + "\" role=\"alert\">"
                + Server.HtmlEncode(text) + "</div>";
        }

        private void ScrollToDetail()
        {
            try
            {
                ScriptManager.RegisterStartupScript(upOrders, typeof(UpdatePanel), "obScroll",
                    "var f=document.getElementById('obDetailCard'); if(f){f.scrollIntoView({behavior:'smooth',block:'start'});}", true);
            }
            catch { /* scrolling is cosmetic */ }
        }

        // ---------- list ----------
        private void BindAll()
        {
            BindKpis();
            BindOrders();
        }

        private void BindKpis()
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand(OrdersCte + @"
                    SELECT ISNULL(SUM(CASE WHEN CurrentStatus = 'Placed' THEN 1 ELSE 0 END), 0),
                           ISNULL(SUM(IsLive), 0),
                           ISNULL(SUM(CASE WHEN IsLive = 1 AND CAST(Slot AS date) = @Today THEN 1 ELSE 0 END), 0),
                           ISNULL(SUM(CASE WHEN CurrentStatus = 'Ready for collection' THEN 1 ELSE 0 END), 0)
                    FROM r", conn))
                {
                    cmd.Parameters.AddWithValue("@Today", SlotHelper.Now().Date);
                    conn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            litKpiNew.Text = Convert.ToInt32(rd[0]).ToString("N0");
                            litKpiLive.Text = Convert.ToInt32(rd[1]).ToString("N0");
                            litKpiToday.Text = Convert.ToInt32(rd[2]).ToString("N0");
                            litKpiReady.Text = Convert.ToInt32(rd[3]).ToString("N0");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("OnlineOrdersBrowser.BindKpis: " + ex);
                litKpiNew.Text = litKpiLive.Text = litKpiToday.Text = litKpiReady.Text = "-";
            }
        }

        private void BindOrders()
        {
            var dt = new DataTable();
            try
            {
                var where = new StringBuilder(" WHERE 1 = 1");
                using (var conn = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.Parameters.AddWithValue("@Today", SlotHelper.Now().Date);

                    switch (ddlView.SelectedValue)
                    {
                        case "live": where.Append(" AND IsLive = 1"); break;
                        case "today": where.Append(" AND IsLive = 1 AND CAST(Slot AS date) = @Today"); break;
                        case "ready": where.Append(" AND CurrentStatus = 'Ready for collection'"); break;
                        case "done": where.Append(" AND CurrentStatus IN ('Delivered', 'Collected')"); break;
                        case "ended": where.Append(" AND CurrentStatus IN ('Cancelled', 'Expired')"); break;
                    }

                    string method = ddlMethod.SelectedValue;
                    if (method == "Delivery" || method == "Collection")
                    {
                        where.Append(" AND Method = @Method");
                        cmd.Parameters.AddWithValue("@Method", method);
                    }

                    string search = Clean(txtSearch.Text);
                    if (search.Length > 100) search = search.Substring(0, 100);
                    if (search.Length > 0)
                    {
                        where.Append(@" AND (InvoiceNumber LIKE @S ESCAPE '\' OR CustomerName LIKE @S ESCAPE '\' OR Phone LIKE @S ESCAPE '\'
                                         OR Email LIKE @S ESCAPE '\' OR CollectionCode LIKE @S ESCAPE '\')");
                        cmd.Parameters.AddWithValue("@S", "%" + EscapeLike(search) + "%");
                    }

                    // live orders first, soonest slot first; finished orders newest first
                    cmd.CommandText = OrdersCte + " SELECT * FROM r" + where
                        + " ORDER BY IsLive DESC, CASE WHEN IsLive = 1 THEN Slot END ASC, Sale_DateTime DESC";

                    using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("OnlineOrdersBrowser.BindOrders: " + ex);
                ShowMessage("The orders could not be loaded right now. Please try again.", false);
            }

            gvOrders.DataSource = dt;
            gvOrders.DataBind();
            litCount.Text = dt.Rows.Count == 1 ? "1 order" : dt.Rows.Count + " orders";
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            gvOrders.PageIndex = 0;
            BindOrders();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvOrders.PageIndex = 0;
            BindOrders();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlView.SelectedIndex = 0;
            ddlMethod.SelectedIndex = 0;
            gvOrders.PageIndex = 0;
            BindOrders();
        }

        protected void gvOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvOrders.PageIndex = e.NewPageIndex;
            BindOrders();
        }

        protected void gvOrders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "ManageOrder") return;
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id) || id <= 0) return;

            litMessage.Text = "";
            txtNote.Text = "";
            txtCode.Text = "";
            chkIdChecked.Checked = false;
            OpenDetail(id);
        }

        // ---------- display helpers used by the grid ----------
        protected string Money(object value)
        {
            decimal d;
            return value == null || value == DBNull.Value || !decimal.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out d)
                ? "-" : OrderHelper.Money(d);
        }

        protected string TypeText(object method)
        {
            return Convert.ToString(method).Trim().Equals("Delivery", StringComparison.OrdinalIgnoreCase) ? "Delivery" : "Click &amp; collect";
        }

        private static DateTime SlotEnd(bool delivery, DateTime slot)
        {
            if (!delivery) return slot.AddHours(1);
            var w = SlotHelper.GetDeliveryWindows(slot.Date).FirstOrDefault(x => x.Start == slot);
            return w != null ? w.End : slot.AddHours(3);
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

        protected string SlotShort(object method, object slot)
        {
            if (slot == null || slot == DBNull.Value) return "";
            bool delivery = Convert.ToString(method).Trim().Equals("Delivery", StringComparison.OrdinalIgnoreCase);
            return Server.HtmlEncode(SlotLabel(delivery, Convert.ToDateTime(slot)));
        }

        protected string StatusHtml(object status, object method, object slot)
        {
            string s = Convert.ToString(status).Trim();
            if (s.Length == 0) s = "Placed";
            string css;
            switch (s)
            {
                case "Delivered": case "Collected": css = "emp-badge-active"; break;
                case "Cancelled": case "Expired": css = "emp-badge-inactive"; break;
                case "Out for delivery": case "Ready for collection": css = "emp-badge-role"; break;
                default: css = "emp-badge-low"; break;
            }
            string html = "<span class=\"emp-badge " + css + "\">" + Server.HtmlEncode(s) + "</span>";

            if (OrderHelper.IsLive(s) && slot != null && slot != DBNull.Value)
            {
                bool delivery = Convert.ToString(method).Trim().Equals("Delivery", StringComparison.OrdinalIgnoreCase);
                if (SlotEnd(delivery, Convert.ToDateTime(slot)) < SlotHelper.Now())
                    html += " <span class=\"emp-badge emp-badge-inactive\">Overdue</span>";
            }
            return html;
        }

        // ---------- detail ----------
        private void OpenDetail(int orderId)
        {
            try
            {
                if (!LoadDetail(orderId))
                {
                    pnlDetail.Visible = false;
                    ShowMessage("That order could not be found.", false);
                    return;
                }
                pnlDetail.Visible = true;
                ScrollToDetail();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("OnlineOrdersBrowser.OpenDetail: " + ex);
                pnlDetail.Visible = false;
                ShowMessage("We couldn't load that order right now. Please try again.", false);
            }
        }

        private static void Row(StringBuilder sb, string label, string htmlValue)
        {
            sb.Append("<div><dt>").Append(label).Append("</dt><dd>").Append(htmlValue).Append("</dd></div>");
        }

        private bool LoadDetail(int orderId)
        {
            var head = new DataTable();
            var lines = new DataTable();
            var hist = new DataTable();

            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                using (var cmd = new SqlCommand(@"
                    SELECT o.OnlineOrder_ID, o.Sale_ID, o.OnlineOrder_Method, o.OnlineOrder_FirstName, o.OnlineOrder_LastName,
                           o.OnlineOrder_EmailAddress, o.OnlineOrder_PhoneNumber, o.OnlineOrder_UnitNumber, o.OnlineOrder_UnitName,
                           o.OnlineOrder_StreetNumber, o.OnlineOrder_StreetName, o.OnlineOrder_Suburb, o.OnlineOrder_City, o.OnlineOrder_PostalCode,
                           o.OnlineOrder_AdditionalInstructions, o.OnlineOrder_DeliveryFee,
                           COALESCE(o.OnlineOrder_EstimatedDateTime, o.OnlineOrder_CollectionSlot) AS Slot,
                           o.OnlineOrder_CollectionCode, o.OnlineOrder_CardLastFour, o.OnlineOrder_IDChecked,
                           s.Sale_DateTime, s.Sale_Subtotal, s.Sale_DiscountAmount, s.Sale_TotalAmount,
                           (SELECT ISNULL(SUM(p.Payment_LoyaltyPointsUsed), 0) FROM Payment p WHERE p.Sale_ID = s.Sale_ID) AS PointsUsed
                    FROM OnlineOrder o INNER JOIN Sale s ON s.Sale_ID = o.Sale_ID
                    WHERE o.OnlineOrder_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", orderId);
                    using (var da = new SqlDataAdapter(cmd)) da.Fill(head);
                }
                if (head.Rows.Count == 0) return false;

                using (var cmd = new SqlCommand(@"
                    SELECT p.Product_Name, sl.SaleLine_Quantity, sl.SaleLine_UnitPriceAfterDiscount, sl.SaleLine_Subtotal
                    FROM SaleLine sl INNER JOIN Product p ON p.Product_ID = sl.Product_ID
                    WHERE sl.Sale_ID = @Sale ORDER BY p.Product_Name", conn))
                {
                    cmd.Parameters.AddWithValue("@Sale", Convert.ToInt32(head.Rows[0]["Sale_ID"]));
                    using (var da = new SqlDataAdapter(cmd)) da.Fill(lines);
                }

                using (var cmd = new SqlCommand(@"
                    SELECT h.OnlineOrderStatusHistory_Status AS Status, h.OnlineOrderStatusHistory_DateTime AS ChangedAt,
                           h.OnlineOrderStatusHistory_Notes AS Notes,
                           LTRIM(RTRIM(ISNULL(e.Employee_FirstName, '') + ' ' + ISNULL(e.Employee_LastName, ''))) AS EmpName
                    FROM OnlineOrderStatusHistory h LEFT JOIN Employee e ON e.Employee_ID = h.Employee_ID
                    WHERE h.OnlineOrder_ID = @Id ORDER BY h.OnlineOrderStatusHistory_ID", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", orderId);
                    using (var da = new SqlDataAdapter(cmd)) da.Fill(hist);
                }
            }

            DataRow o = head.Rows[0];
            string method = Str(o, "OnlineOrder_Method");
            bool delivery = method.Equals("Delivery", StringComparison.OrdinalIgnoreCase);
            string current = hist.Rows.Count > 0 ? Str(hist.Rows[hist.Rows.Count - 1], "Status") : "Placed";
            string next = OrderHelper.GetNextStatus(method, current);
            bool canCancel = OrderHelper.CanCancel(current);
            bool live = OrderHelper.IsLive(current);
            bool idDone = o["OnlineOrder_IDChecked"] != DBNull.Value && Convert.ToBoolean(o["OnlineOrder_IDChecked"]);
            int pointsUsed = Convert.ToInt32(o["PointsUsed"]);
            string invoice = "INV-" + Convert.ToInt32(o["Sale_ID"]).ToString("000000");
            string packer = "";
            foreach (DataRow h in hist.Rows)
                if (Str(h, "Status") == OrderHelper.StPacking) packer = Str(h, "EmpName");

            Func<string, string> enc = Server.HtmlEncode;
            string slotText = o["Slot"] == DBNull.Value ? "To be confirmed" : SlotLabel(delivery, Convert.ToDateTime(o["Slot"]));

            // ----- progress -----
            var reached = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow h in hist.Rows) reached[Str(h, "Status")] = Convert.ToDateTime(h["ChangedAt"]);

            var pg = new StringBuilder();
            pg.Append("<div class=\"ob-box\"><div class=\"ob-box-head\"><h4>Progress</h4><span class=\"ob-meta\">")
              .Append(delivery ? "Delivery" : "Click &amp; collect").Append(" &middot; ").Append(enc(slotText)).Append("</span></div>");
            pg.Append("<ol class=\"ord-steps\">");
            foreach (string step in delivery ? DeliveryPath : CollectionPath)
            {
                bool done = reached.ContainsKey(step);
                bool isCurrent = done && step == current && live;
                pg.Append("<li class=\"").Append(done ? (isCurrent ? "is-current" : "is-done") : "").Append("\"><span class=\"ord-step-dot\"></span>")
                  .Append("<span class=\"ord-step-name\">").Append(enc(step)).Append("</span><span class=\"ord-step-time\">")
                  .Append(done ? reached[step].ToString("d MMM, HH:mm", CultureInfo.InvariantCulture) : "").Append("</span></li>");
            }
            pg.Append("</ol>");
            if (current == OrderHelper.StCancelled || current == OrderHelper.StExpired)
            {
                string why = hist.Rows.Count > 0 ? Str(hist.Rows[hist.Rows.Count - 1], "Notes") : "";
                pg.Append("<p class=\"ord-ended\"><strong>This order was ").Append(current.ToLowerInvariant()).Append(".</strong> ").Append(enc(why)).Append("</p>");
            }
            else if (!live)
            {
                pg.Append("<p class=\"field-hint\">This order is finished, so it can't be changed any more.</p>");
            }
            pg.Append("</div>");

            // ----- details: customer, delivery/collection, payment | items -----
            string phone = Str(o, "OnlineOrder_PhoneNumber");
            string phoneHref = Regex.Replace(phone, @"[^0-9+]", "");
            string email = Str(o, "OnlineOrder_EmailAddress");

            var d = new StringBuilder();
            d.Append("<div class=\"ob-grid\"><div class=\"ob-stack\">");

            d.Append("<div class=\"ob-box\"><h4>Customer</h4><dl class=\"ob-list\">");
            Row(d, "Name", "<strong>" + enc((Str(o, "OnlineOrder_FirstName") + " " + Str(o, "OnlineOrder_LastName")).Trim()) + "</strong>");
            Row(d, "Phone", phoneHref.Length >= 7 ? "<a class=\"sup-link\" href=\"tel:" + enc(phoneHref) + "\">" + enc(phone) + "</a>" : enc(phone));
            Row(d, "Email", Regex.IsMatch(email, @"^[A-Za-z0-9._%+\-']+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$")
                ? "<a class=\"sup-link\" href=\"mailto:" + enc(email) + "\">" + enc(email) + "</a>" : enc(email));
            d.Append("</dl></div>");

            if (delivery)
            {
                var parts = new List<string>();
                if (Str(o, "OnlineOrder_UnitNumber").Length > 0) parts.Add("Unit " + Str(o, "OnlineOrder_UnitNumber"));
                if (Str(o, "OnlineOrder_UnitName").Length > 0) parts.Add(Str(o, "OnlineOrder_UnitName"));
                string street = (Str(o, "OnlineOrder_StreetNumber") + " " + Str(o, "OnlineOrder_StreetName")).Trim();
                if (street.Length > 0) parts.Add(street);
                foreach (string c in new[] { "OnlineOrder_Suburb", "OnlineOrder_City", "OnlineOrder_PostalCode" })
                    if (Str(o, c).Length > 0) parts.Add(Str(o, c));

                d.Append("<div class=\"ob-box\"><h4>Deliver to</h4><dl class=\"ob-list\">");
                Row(d, "Address", enc(string.Join(", ", parts)));
                Row(d, "Window", enc(slotText));
                string ins = Str(o, "OnlineOrder_AdditionalInstructions");
                Row(d, "Instructions", ins.Length > 0 ? enc(ins) : "<span class=\"emp-sub\">None</span>");
                d.Append("</dl></div>");
            }
            else
            {
                d.Append("<div class=\"ob-box\"><h4>Collection</h4><dl class=\"ob-list\">");
                Row(d, "Slot", enc(slotText));
                Row(d, "Code", "<strong class=\"disc-code\">" + enc(Str(o, "OnlineOrder_CollectionCode")) + "</strong>");
                Row(d, "Where", "21 Coronation Road, Mithangar, Tongaat, 4399");
                d.Append("</dl></div>");
            }

            d.Append("<div class=\"ob-box\"><h4>Payment &amp; checks</h4><dl class=\"ob-list\">");
            Row(d, "Paid with", pointsUsed > 0 ? enc("Loyalty points (" + pointsUsed.ToString("N0", CultureInfo.InvariantCulture) + ")")
                : (Str(o, "OnlineOrder_CardLastFour").Length > 0 ? enc("Card ending " + Str(o, "OnlineOrder_CardLastFour")) : "-"));
            Row(d, "Order total", "<strong>" + enc(OrderHelper.Money(Convert.ToDecimal(o["Sale_TotalAmount"]))) + "</strong>");
            Row(d, "ID checked", idDone ? "<span class=\"emp-badge emp-badge-active\">Yes</span>" : "<span class=\"emp-badge emp-badge-low\">Not yet</span>");
            if (packer.Length > 0) Row(d, "Packed by", enc(packer));
            d.Append("</dl></div>");
            d.Append("</div>");   // end left stack

            d.Append("<div class=\"ob-box\"><h4>Items</h4><div class=\"ob-table\"><table class=\"dash-table\"><thead><tr><th>Product</th><th class=\"is-num\">Qty</th><th class=\"is-num\">Unit</th><th class=\"is-num\">Subtotal</th></tr></thead><tbody>");
            foreach (DataRow l in lines.Rows)
            {
                d.Append("<tr><td class=\"is-wrap\">").Append(enc(Str(l, "Product_Name"))).Append("</td><td class=\"is-num\">").Append(Convert.ToInt32(l["SaleLine_Quantity"]))
                 .Append("</td><td class=\"is-num\">").Append(enc(OrderHelper.Money(Convert.ToDecimal(l["SaleLine_UnitPriceAfterDiscount"]))))
                 .Append("</td><td class=\"is-num\">").Append(enc(OrderHelper.Money(Convert.ToDecimal(l["SaleLine_Subtotal"])))).Append("</td></tr>");
            }
            d.Append("</tbody></table></div>");
            d.Append("<dl class=\"sale-totals\"><dt>Subtotal</dt><dd>").Append(enc(OrderHelper.Money(Convert.ToDecimal(o["Sale_Subtotal"])))).Append("</dd>");
            decimal disc = o["Sale_DiscountAmount"] == DBNull.Value ? 0m : Convert.ToDecimal(o["Sale_DiscountAmount"]);
            if (disc > 0m) d.Append("<dt>Discounts</dt><dd>-").Append(enc(OrderHelper.Money(disc))).Append("</dd>");
            decimal fee = Convert.ToDecimal(o["OnlineOrder_DeliveryFee"]);
            d.Append("<dt>Delivery</dt><dd>").Append(fee > 0m ? enc(OrderHelper.Money(fee)) : "Free").Append("</dd>");
            d.Append("<dt class=\"is-total\">Total</dt><dd class=\"is-total\">").Append(enc(OrderHelper.Money(Convert.ToDecimal(o["Sale_TotalAmount"])))).Append("</dd></dl>");
            d.Append("</div></div>");   // end items box + grid

            // ----- history -----
            var hs = new StringBuilder();
            hs.Append("<div class=\"ob-box ob-gap\"><h4>Status history</h4><ul class=\"ord-history\">");
            for (int i = hist.Rows.Count - 1; i >= 0; i--)
            {
                DataRow h = hist.Rows[i];
                string by = Str(h, "EmpName");
                string note = Str(h, "Notes");
                hs.Append("<li><strong>").Append(enc(Str(h, "Status"))).Append("</strong><time>")
                  .Append(Convert.ToDateTime(h["ChangedAt"]).ToString("d MMM yyyy, HH:mm", CultureInfo.InvariantCulture)).Append("</time>");
                if (by.Length > 0 || note.Length > 0)
                    hs.Append("<em>").Append(by.Length > 0 ? "By " + enc(by) : "").Append(by.Length > 0 && note.Length > 0 ? " - " : "").Append(enc(note)).Append("</em>");
                hs.Append("</li>");
            }
            hs.Append("</ul></div>");

            litTitle.Text = enc(invoice);
            litHeadBadge.Text = StatusHtml(current, method, o["Slot"]);
            litProgress.Text = pg.ToString();
            litDetail.Text = d.ToString();
            litHistory.Text = hs.ToString();
            hfOrderId.Value = orderId.ToString(CultureInfo.InvariantCulture);
            hfNext.Value = next ?? "";

            // ----- update controls -----
            bool completing = next == OrderHelper.StDelivered || next == OrderHelper.StCollected;
            pnlActions.Visible = next != null || canCancel;
            btnAdvance.Visible = next != null;
            btnAdvance.Text = AdvanceLabel(next);
            btnCancelOrder.Visible = canCancel;
            pnlCode.Visible = next == OrderHelper.StCollected;
            pnlIdCheck.Visible = completing && !idDone;
            return true;
        }

        private static string AdvanceLabel(string next)
        {
            switch (next)
            {
                case OrderHelper.StPacking: return "Start packing";
                case OrderHelper.StOut: return "Mark out for delivery";
                case OrderHelper.StReady: return "Mark ready for collection";
                case OrderHelper.StDelivered: return "Mark delivered";
                case OrderHelper.StCollected: return "Mark collected";
                default: return "Next step";
            }
        }

        protected void btnCloseDetail_Click(object sender, EventArgs e)
        {
            pnlDetail.Visible = false;
            hfOrderId.Value = "";
            litMessage.Text = "";
        }

        // ---------- status changes ----------
        protected void btnAdvance_Click(object sender, EventArgs e)
        {
            int id = 0;
            try
            {
                if (!int.TryParse(hfOrderId.Value, out id) || id <= 0) { ShowMessage("Open an order first.", false); return; }

                int? emp = EmployeeId();
                if (!emp.HasValue) { ShowMessage("Your session has expired. Please log in again.", false); return; }

                string expected = hfNext.Value;
                string note = Clean(txtNote.Text);
                if (note.Length > 250) { ShowMessage("Notes can be at most 250 characters.", false); OpenDetail(id); return; }

                // Read the order fresh: someone else may have moved it since this page was drawn
                string method, code; bool idDone;
                if (!ReadBasics(id, out method, out code, out idDone)) { ShowMessage("That order could not be found.", false); pnlDetail.Visible = false; BindAll(); return; }

                string current = OrderHelper.GetCurrentStatus(id);
                string next = OrderHelper.GetNextStatus(method, current);
                if (next == null || next != expected)
                {
                    ShowMessage("This order was just updated by someone else (it is now '" + (current ?? "unknown") + "'). Please check it and try again.", false);
                    OpenDetail(id); BindAll();
                    return;
                }

                bool completing = next == OrderHelper.StDelivered || next == OrderHelper.StCollected;
                if (next == OrderHelper.StCollected)
                {
                    string typed = Clean(txtCode.Text);
                    if (typed.Length == 0) { ShowMessage("Please enter the customer's collection code.", false); OpenDetail(id); return; }
                    if (!string.Equals(typed, code, StringComparison.OrdinalIgnoreCase)) { ShowMessage("That collection code doesn't match this order.", false); OpenDetail(id); return; }
                }
                if (completing && !idDone && !chkIdChecked.Checked)
                {
                    ShowMessage("Please tick the box to confirm you checked the customer's ID (18 or older).", false);
                    OpenDetail(id);
                    return;
                }

                StatusResult r = OrderHelper.ChangeStatus(id, next, emp, note, chkIdChecked.Checked);
                if (r.Success)
                {
                    OrderEmailHelper.Notify(id, next, note);
                    ShowMessage("Order marked as " + next + ". The customer has been emailed.", true);
                    txtNote.Text = ""; txtCode.Text = ""; chkIdChecked.Checked = false;
                }
                else
                {
                    ShowMessage(r.Message, false);
                }
                OpenDetail(id);
                BindAll();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("OnlineOrdersBrowser.btnAdvance_Click: " + ex);
                ShowMessage("We couldn't update that order right now. Nothing was changed. Please try again.", false);
            }
        }

        protected void btnCancelOrder_Click(object sender, EventArgs e)
        {
            int id = 0;
            try
            {
                if (!int.TryParse(hfOrderId.Value, out id) || id <= 0) { ShowMessage("Open an order first.", false); return; }

                int? emp = EmployeeId();
                if (!emp.HasValue) { ShowMessage("Your session has expired. Please log in again.", false); return; }

                string reason = Clean(txtNote.Text);
                if (reason.Length < 3) { ShowMessage("Please type the reason for cancelling in the note box.", false); OpenDetail(id); return; }
                if (reason.Length > 250) { ShowMessage("The reason can be at most 250 characters.", false); OpenDetail(id); return; }

                StatusResult r = OrderHelper.ChangeStatus(id, OrderHelper.StCancelled, emp, reason);
                if (r.Success)
                {
                    OrderEmailHelper.Notify(id, OrderHelper.StCancelled, reason);
                    ShowMessage("Order cancelled. Stock and loyalty points were restored and the customer has been emailed.", true);
                    txtNote.Text = "";
                }
                else
                {
                    ShowMessage(r.Message, false);
                }
                OpenDetail(id);
                BindAll();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("OnlineOrdersBrowser.btnCancelOrder_Click: " + ex);
                ShowMessage("We couldn't cancel that order right now. Nothing was changed. Please try again.", false);
            }
        }

        private bool ReadBasics(int orderId, out string method, out string code, out bool idChecked)
        {
            method = ""; code = ""; idChecked = false;
            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("SELECT OnlineOrder_Method, OnlineOrder_CollectionCode, OnlineOrder_IDChecked FROM OnlineOrder WHERE OnlineOrder_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", orderId);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return false;
                    method = Convert.ToString(r[0]).Trim();
                    code = r.IsDBNull(1) ? "" : Convert.ToString(r[1]).Trim();
                    idChecked = !r.IsDBNull(2) && Convert.ToBoolean(r[2]);
                    return true;
                }
            }
        }
    }
}