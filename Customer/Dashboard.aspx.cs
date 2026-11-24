using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using SamsLiquorShop.Helpers;

namespace SamsLiquorShop.Customer
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Customer")) return;

            if (!IsPostBack)
            {
                litGreeting.Text = "Welcome back, " + Server.HtmlEncode(Session["UserName"].ToString());

                int customerId = Convert.ToInt32(Session["CustomerID"]);

                LoadSpendingSummary(customerId);
                LoadLoyaltyPoints(customerId);
                LoadRecentOrders(customerId);
                LoadOffers(customerId);
            }
        }

        // Promo codes this customer can use right now. Uses the same checks as checkout, so nothing
        // is shown that they've already used, that is for students only, or that is first-order only.
        private void LoadOffers(int customerId)
        {
            var offers = NotificationEmailHelper.OrderOffers(PromoHelper.GetAvailableOffers(customerId)).ToList();

            bool justRegistered = Session["ShowWelcomeOffer"] is bool && (bool)Session["ShowWelcomeOffer"];
            Session.Remove("ShowWelcomeOffer");   // one-time notice
            if (justRegistered)
            {
                var welcome = offers.FirstOrDefault(o => string.Equals(o.Rule, "FirstOrder", StringComparison.OrdinalIgnoreCase));
                string text = welcome != null
                    ? "Welcome to Sam's Liquor Shop! Your new-customer discount is ready: use code <strong>" + Server.HtmlEncode(welcome.Code)
                      + "</strong> for " + Server.HtmlEncode(welcome.Description) + " your first order. We've emailed it to you too."
                    : "Welcome to Sam's Liquor Shop! Your account is ready.";
                litWelcome.Text = "<div class=\"dash-notice is-welcome\" role=\"status\"><span>" + text + "</span>"
                    + (welcome != null ? "<a href=\"#offers\">See your offers</a>" : "") + "</div>";
            }

            if (offers.Count == 0)
            {
                litOffers.Text = "<p class=\"dash-table-empty\">You don't have any promo codes right now. Keep an eye on our <a href=\""
                    + ResolveUrl("~/Specials.aspx") + "\">specials</a>.</p>";
                return;
            }

            var sb = new StringBuilder("<div class=\"offer-grid\">");
            foreach (var o in offers)
            {
                string tag = string.Equals(o.Rule, "FirstOrder", StringComparison.OrdinalIgnoreCase) ? "New customer"
                    : string.Equals(o.Rule, "Student", StringComparison.OrdinalIgnoreCase) ? "Student" : "Promo";

                var conditions = new List<string>();
                if (string.Equals(o.Rule, "FirstOrder", StringComparison.OrdinalIgnoreCase)) conditions.Add("First order only");
                if (string.Equals(o.Rule, "Student", StringComparison.OrdinalIgnoreCase)) conditions.Add("For .ac.za email addresses");
                if (o.MinSpend.HasValue && o.MinSpend.Value > 0) conditions.Add("Spend at least R" + o.MinSpend.Value.ToString("N2"));
                conditions.Add("Valid until " + o.EndDate.ToString("d MMM yyyy"));

                sb.Append("<div class=\"offer-card\">")
                  .Append("<span class=\"offer-tag\">").Append(tag).Append("</span>")
                  .Append("<p class=\"offer-value\">").Append(Server.HtmlEncode(o.Description)).Append("</p>");
                if (o.Name.Length > 0) sb.Append("<p class=\"offer-name\">").Append(Server.HtmlEncode(o.Name)).Append("</p>");
                sb.Append("<div class=\"offer-code-row\"><code class=\"offer-code\">").Append(Server.HtmlEncode(o.Code)).Append("</code>")
                  .Append("<button type=\"button\" class=\"suggest-password-copy\" data-code=\"").Append(Server.HtmlEncode(o.Code))
                  .Append("\" onclick=\"copyOfferCode(this)\">Copy code</button></div>")
                  .Append("<p class=\"offer-terms\">").Append(Server.HtmlEncode(string.Join(" · ", conditions))).Append("</p>")
                  .Append("</div>");
            }
            sb.Append("</div>");
            litOffers.Text = sb.ToString();
        }

        private void LoadSpendingSummary(int customerId)
        {
            string query = @"
                SELECT
                    ISNULL(SUM(Sale_TotalAmount), 0) AS TotalSpent,
                    COUNT(*) AS OrderCount
                FROM Sale
                WHERE Customer_ID = @CustomerId";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        decimal totalSpent = Convert.ToDecimal(reader["TotalSpent"]);
                        int orderCount = Convert.ToInt32(reader["OrderCount"]);

                        litTotalSpent.Text = totalSpent.ToString("C");
                        litOrderCount.Text = orderCount.ToString();
                        litAvgOrder.Text = (orderCount > 0 ? totalSpent / orderCount : 0).ToString("C");
                    }
                }
            }
        }

        // Points redeem at 1 point = R1 (mirrors the POS loyalty rules)
        private void LoadLoyaltyPoints(int customerId)
        {
            string query = @"SELECT Customer_LoyaltyPointsBalance, Customer_FirstName, Customer_LastName, Customer_PhoneNumber
                             FROM Customer WHERE Customer_ID = @CustomerId";

            int points = 0;
            string name = "";
            string phone = "";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        points = r.IsDBNull(0) ? 0 : Convert.ToInt32(r[0]);
                        name = (Convert.ToString(r[1]) + " " + Convert.ToString(r[2])).Trim();
                        phone = Convert.ToString(r[3]);
                    }
                }
            }

            litLoyaltyPoints.Text = points.ToString("N0");
            litPointsValue.Text = ((decimal)points).ToString("C");
            litCardName.Text = Server.HtmlEncode(name);
            litCardPhone.Text = Server.HtmlEncode(FormatPhone(phone));
        }

        // 0824055932 becomes 082 405 5932; anything else is shown as stored
        private static string FormatPhone(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            string digits = new string(raw.Where(char.IsDigit).ToArray());
            if (digits.Length == 10 && digits[0] == '0')
                return digits.Substring(0, 3) + " " + digits.Substring(3, 3) + " " + digits.Substring(6);
            return raw.Trim();
        }
        // Clicking a recent order opens it on the My Orders page
        protected void gvRecentOrders_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            DataRowView row = (DataRowView)e.Row.DataItem;
            string saleId = Convert.ToString(row["Sale_ID"]);
            string url = ResolveUrl("~/Customer/Orders.aspx") + "?order=" + saleId + "#order-" + saleId;

            e.Row.Style["cursor"] = "pointer";
            e.Row.ToolTip = "View this order";
            e.Row.Attributes["onclick"] = "window.location.href='" + url + "';";
        }
        private void LoadRecentOrders(int customerId)
        {
            string query = @"
                SELECT TOP 5
                      s.Sale_ID,
                    'INV-' + RIGHT('000000' + CAST(s.Sale_ID AS VARCHAR(6)), 6) AS InvoiceNumber,
                    s.Sale_DateTime,
                    (SELECT ISNULL(SUM(SaleLine_Quantity), 0) FROM SaleLine WHERE Sale_ID = s.Sale_ID) AS ItemCount,
                    s.Sale_TotalAmount
                FROM Sale s
                WHERE s.Customer_ID = @CustomerId
                ORDER BY s.Sale_DateTime DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvRecentOrders.DataSource = dt;
                    gvRecentOrders.DataBind();
                }
            }
        }
    }
}
