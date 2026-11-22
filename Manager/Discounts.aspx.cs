using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SamsLiquorShop.Helpers;

namespace SamsLiquorShop.Manager
{
    /// <summary>
    /// Manager page for discounts (promotions).
    /// - View all discounts and the products each one applies to
    /// - Add a new discount
    /// - Edit an existing discount: ONLY the end date can change
    /// - Deactivate / reactivate (status change)
    /// Deleting discounts is deliberately not offered.
    /// Promo codes (Discount_Code, Discount_MinSpend, Discount_Rule) are typed in the cart and
    /// checked by PromoHelper; they come off the whole order rather than individual products.
    /// A discount reaches a product either directly (Product.Discount_ID) or through
    /// the product's category (Category.Discount_ID). A product's own discount wins over its category's.
    /// </summary>
    public partial class Discounts : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private static readonly HashSet<string> AllowedSort = new HashSet<string>
        {
            "Discount_Name", "Discount_Value", "Discount_StartDate", "Discount_EndDate", "ProductCount", "Discount_Code"
        };

        /// <summary>Rules PromoHelper understands. Any other value would make the code unusable.</summary>
        private static readonly string[] KnownRules = { "Student", "FirstOrder" };

        private static readonly Regex CodeFormat = new Regex("^[A-Z0-9_-]{3,20}$");

        private static readonly string[] RequiredColumns =
        {
            "Discount_ID", "Discount_Name", "Discount_Type", "Discount_Value",
            "Discount_Status", "Discount_StartDate", "Discount_EndDate"
        };

        // ---------- TABLE LAYOUT (read from the database) ----------
        private TableSchema Schema => TableSchema.Get(ConnStr, "Discount");

        private TableSchema.Column StatusCol => Schema.Find("Discount_Status");
        private TableSchema.Column EndCol => Schema.Find("Discount_EndDate");
        private TableSchema.Column StartCol => Schema.Find("Discount_StartDate");
        private TableSchema.Column ValueCol => Schema.Find("Discount_Value");

        // New promo columns (optional, so the page still works on a database without them)
        private TableSchema.Column CodeCol => Schema.Find("Discount_Code");
        private TableSchema.Column MinSpendCol => Schema.Find("Discount_MinSpend");
        private TableSchema.Column RuleCol => Schema.Find("Discount_Rule");
        private bool HasPromoCols => CodeCol != null && MinSpendCol != null && RuleCol != null;

        /// <summary>Optional description column (Discount_Description, Discount_Desc, ...), or null.</summary>
        private TableSchema.Column DescCol
        {
            get
            {
                return Schema.Columns.FirstOrDefault(c =>
                    c.Name.StartsWith("Discount_", StringComparison.OrdinalIgnoreCase)
                    && c.Name.IndexOf("desc", StringComparison.OrdinalIgnoreCase) >= 0
                    && c.IsText && Regex.IsMatch(c.Name, @"^\w+$"));
            }
        }

        private object ActiveValue => StatusCol.IsNumericFlag ? (object)1 : "Active";
        private object InactiveValue => StatusCol.IsNumericFlag ? (object)0 : "Inactive";

        private string SortField
        {
            get { return (string)ViewState["SortField"] ?? "Discount_StartDate"; }
            set { ViewState["SortField"] = value; }
        }

        private string SortDir
        {
            get { return (string)ViewState["SortDir"] ?? "DESC"; }
            set { ViewState["SortDir"] = value; }
        }

        /// <summary>Discount whose products are showing (kept so paging the product list works).</summary>
        private int ProductsDiscountId
        {
            get { return ViewState["ProductsDiscountId"] == null ? 0 : (int)ViewState["ProductsDiscountId"]; }
            set { ViewState["ProductsDiscountId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Manager")) return;

            txtSearch.Attributes["data-live-search"] = btnSearch.ClientID;

            try
            {
                var missing = RequiredColumns.Where(c => Schema.Find(c) == null).ToList();
                if (missing.Count > 0)
                {
                    ShowMessage("The Discount table is missing these columns: " + string.Join(", ", missing) + ".", false);
                    btnAddNew.Visible = false;
                    return;
                }

                txtName.MaxLength = Schema.Find("Discount_Name").TextLimit(100);
                phDescription.Visible = DescCol != null;
                if (DescCol != null) txtDescription.MaxLength = DescCol.TextLimit(500);

                phPromoFields.Visible = HasPromoCols;
                phRoPromo.Visible = HasPromoCols;
                if (HasPromoCols) txtCode.MaxLength = Math.Min(20, CodeCol.TextLimit(20));
                if (!HasPromoCols)
                {
                    ListItem codeItem = ddlStatusFilter.Items.FindByValue("code");
                    ListItem noCodeItem = ddlStatusFilter.Items.FindByValue("nocode");
                    if (codeItem != null) ddlStatusFilter.Items.Remove(codeItem);
                    if (noCodeItem != null) ddlStatusFilter.Items.Remove(noCodeItem);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("The discount table could not be read: " + ex.Message, false);
                btnAddNew.Visible = false;
                return;
            }

            if (!IsPostBack)
            {
                RefreshAll();

                // Allow linking straight to a discount's products, e.g. Discounts.aspx?view=3
                int viewId;
                if (int.TryParse(Request.QueryString["view"], out viewId)) ShowProducts(viewId);
            }
        }

        private void RefreshAll()
        {
            BindDiscounts();
        }

        // ---------- STATUS RULES (same rules as Shop.aspx / PricingHelper) ----------
        private static bool IsActiveValueOn(object value)
        {
            if (value == null || value == DBNull.Value) return false;
            string s = value.ToString().Trim();
            return s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("Active", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsWednesdayPromo(string name)
        {
            return (name ?? "").IndexOf("wednesday", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private class StatusInfo
        {
            public string Label, Css, Key, Note = "";
        }

        private static StatusInfo GetStatus(object statusValue, object startValue, object endValue, string name)
        {
            DateTime now = DateTime.Now;
            DateTime? start = startValue == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(startValue);
            DateTime? end = endValue == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(endValue);

            if (!IsActiveValueOn(statusValue))
                return new StatusInfo { Label = "Inactive", Css = "inactive", Key = "inactive" };
            if (start.HasValue && now < start.Value)
                return new StatusInfo { Label = "Scheduled", Css = "low", Key = "scheduled", Note = "Starts in " + DaysText((start.Value.Date - now.Date).Days) };
            if (end.HasValue && now > end.Value)
                return new StatusInfo { Label = "Ended", Css = "ended", Key = "ended" };

            var info = new StatusInfo { Label = "Running", Css = "active", Key = "running" };
            if (end.HasValue)
            {
                int left = (end.Value.Date - now.Date).Days;
                info.Note = left == 0 ? "Last day today" : DaysText(left) + " left";
            }
            if (IsWednesdayPromo(name))
                info.Note = "Wednesdays only" + (now.DayOfWeek == DayOfWeek.Wednesday ? " (on today)" : " (off today)");
            return info;
        }

        private static string DaysText(int days)
        {
            return days == 1 ? "1 day" : days + " days";
        }

        // ---------- LIST ----------
        private string DescSelect(string alias)
        {
            return DescCol != null
                ? "CAST(" + alias + TableSchema.Q(DescCol.Name) + " AS NVARCHAR(MAX))"
                : "CAST(NULL AS NVARCHAR(MAX))";
        }

        /// <summary>SELECT list for the promo columns (NULLs when a column doesn't exist yet).</summary>
        private string PromoSelect(string alias)
        {
            string code = CodeCol != null ? "CAST(" + alias + TableSchema.Q(CodeCol.Name) + " AS NVARCHAR(50))" : "CAST(NULL AS NVARCHAR(50))";
            string min = MinSpendCol != null ? "CAST(" + alias + TableSchema.Q(MinSpendCol.Name) + " AS DECIMAL(18,2))" : "CAST(NULL AS DECIMAL(18,2))";
            string rule = RuleCol != null ? "CAST(" + alias + TableSchema.Q(RuleCol.Name) + " AS NVARCHAR(50))" : "CAST(NULL AS NVARCHAR(50))";
            return code + " AS Discount_Code, " + min + " AS Discount_MinSpend, " + rule + " AS Discount_Rule";
        }

        private DataTable LoadAllDiscounts(string search)
        {
            string sort = AllowedSort.Contains(SortField) ? SortField : "Discount_StartDate";
            if (sort == "Discount_Code" && CodeCol == null) sort = "Discount_StartDate";
            string dir = SortDir == "ASC" ? "ASC" : "DESC";

            string sql = @"
                SELECT d.Discount_ID, d.Discount_Name, d.Discount_Type, d.Discount_Value, d.Discount_Status,
                       d.Discount_StartDate, d.Discount_EndDate, " + DescSelect("d.") + @" AS Discount_Description,
                       " + PromoSelect("d.") + @",
                       (SELECT COUNT(*) FROM Product p
                        INNER JOIN Category c ON c.Category_ID = p.Category_ID
                        WHERE p.Discount_ID = d.Discount_ID
                           OR (c.Discount_ID = d.Discount_ID AND p.Discount_ID IS NULL)) AS ProductCount,
                       (SELECT COUNT(*) FROM Category c WHERE c.Discount_ID = d.Discount_ID) AS CategoryCount
                FROM Discount d";

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;
                search = (search ?? "").Trim();
                if (search.Length > 0)
                {
                    sql += " WHERE d.Discount_Name LIKE @Search";
                    if (DescCol != null) sql += " OR " + DescSelect("d.") + " LIKE @Search";
                    if (CodeCol != null) sql += " OR d." + TableSchema.Q(CodeCol.Name) + " LIKE @Search";
                    cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                }
                sql += " ORDER BY " + sort + " " + dir + ", d.Discount_Name ASC";
                cmd.CommandText = sql;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            dt.Columns.Add("StatusLabel", typeof(string));
            dt.Columns.Add("StatusClass", typeof(string));
            dt.Columns.Add("StatusKey", typeof(string));
            dt.Columns.Add("StatusNote", typeof(string));
            dt.Columns.Add("IsActiveFlag", typeof(bool));

            foreach (DataRow r in dt.Rows)
            {
                StatusInfo s = GetStatus(r["Discount_Status"], r["Discount_StartDate"], r["Discount_EndDate"], Convert.ToString(r["Discount_Name"]));
                r["StatusLabel"] = s.Label;
                r["StatusClass"] = s.Css;
                r["StatusKey"] = s.Key;
                r["StatusNote"] = s.Note;
                r["IsActiveFlag"] = IsActiveValueOn(r["Discount_Status"]);
            }
            return dt;
        }

        private void BindDiscounts()
        {
            DataTable all;
            int total, running, scheduled, off;
            try
            {
                string search = txtSearch.Text.Trim();
                all = LoadAllDiscounts(search);

                // Summary cards ignore the search so they always describe the whole shop
                CountStatuses(search.Length == 0 ? all : LoadAllDiscounts(""), out total, out running, out scheduled, out off);
            }
            catch (Exception ex)
            {
                ShowMessage("The discounts could not be loaded: " + ex.Message, false);
                return;
            }
            litKpiTotal.Text = total.ToString("N0");
            litKpiRunning.Text = running.ToString("N0");
            litKpiScheduled.Text = scheduled.ToString("N0");
            litKpiOff.Text = off.ToString("N0");

            DataTable shown = all;
            string filter = ddlStatusFilter.SelectedValue;
            if (filter.Length > 0)
            {
                shown = all.Clone();
                foreach (DataRow r in all.Rows)
                {
                    bool hasCode = Convert.ToString(r["Discount_Code"]).Trim().Length > 0;
                    bool match = filter == "code" ? hasCode
                               : filter == "nocode" ? !hasCode
                               : (string)r["StatusKey"] == filter;
                    if (match) shown.ImportRow(r);
                }
            }

            gvDiscounts.DataSource = shown;
            gvDiscounts.DataBind();
            litCount.Text = shown.Rows.Count == 1 ? "1 discount" : shown.Rows.Count + " discounts";
        }

        private static void CountStatuses(DataTable dt, out int total, out int running, out int scheduled, out int off)
        {
            total = dt.Rows.Count;
            running = scheduled = off = 0;
            foreach (DataRow r in dt.Rows)
            {
                string key = (string)r["StatusKey"];
                if (key == "running") running++;
                else if (key == "scheduled") scheduled++;
                else off++;
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvDiscounts.PageIndex = 0;
            BindDiscounts();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlStatusFilter.SelectedIndex = 0;
            gvDiscounts.PageIndex = 0;
            BindDiscounts();
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            gvDiscounts.PageIndex = 0;
            BindDiscounts();
        }

        protected void gvDiscounts_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvDiscounts.PageIndex = e.NewPageIndex;
            BindDiscounts();
        }

        protected void gvDiscounts_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (!AllowedSort.Contains(e.SortExpression)) return;

            if (SortField == e.SortExpression)
            {
                SortDir = SortDir == "ASC" ? "DESC" : "ASC";
            }
            else
            {
                SortField = e.SortExpression;
                SortDir = e.SortExpression == "Discount_Name" ? "ASC" : "DESC";
            }
            gvDiscounts.PageIndex = 0;
            BindDiscounts();
        }

        protected void gvDiscounts_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.Header) return;

            foreach (TableCell cell in e.Row.Cells)
            {
                if (cell.Controls.Count == 0) continue;
                LinkButton lb = cell.Controls[0] as LinkButton;
                if (lb != null && lb.CommandArgument == SortField)
                {
                    lb.Text += (SortDir == "ASC") ? " ▲" : " ▼";
                }
            }
        }

        protected void gvDiscounts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "ViewProducts" && e.CommandName != "EditEnd" && e.CommandName != "ToggleStatus") return;

            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;
            litMessage.Text = "";

            switch (e.CommandName)
            {
                case "ViewProducts": ShowProducts(id); break;
                case "EditEnd": LoadForEdit(id); break;
                case "ToggleStatus": ToggleStatus(id); break;
            }
        }

        // ---------- VIEW PRODUCTS ----------
        private void ShowProducts(int id)
        {
            pnlForm.Visible = false;

            DataRow d = LoadDiscount(id);
            if (d == null)
            {
                ShowMessage("That discount could not be found.", false);
                pnlProducts.Visible = false;
                return;
            }

            string type = Convert.ToString(d["Discount_Type"]).Trim();
            decimal value = Convert.ToDecimal(d["Discount_Value"]);
            string name = Convert.ToString(d["Discount_Name"]);
            StatusInfo status = GetStatus(d["Discount_Status"], d["Discount_StartDate"], d["Discount_EndDate"], name);

            DataTable products = new DataTable();
            List<string> categoryNames = new List<string>();
            int overridden = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    // Products linked directly, plus products in a linked category that have no discount of their own
                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT p.Product_ID, p.Product_Name, ISNULL(p.Product_Brand, '') AS Product_Brand, p.Product_SizeML,
                               c.Category_Name, p.Product_SellingPrice, p.Product_QuantityInStock, p.Product_Status,
                               CASE WHEN p.Discount_ID = @Id THEN 'This product' ELSE 'Category: ' + c.Category_Name END AS AppliedVia
                        FROM Product p
                        INNER JOIN Category c ON c.Category_ID = p.Category_ID
                        WHERE p.Discount_ID = @Id
                           OR (c.Discount_ID = @Id AND p.Discount_ID IS NULL)
                        ORDER BY CASE WHEN p.Product_Status = 'Active' THEN 0 ELSE 1 END, p.Product_Name, p.Product_SizeML", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(products);
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT Category_Name FROM Category WHERE Discount_ID = @Id ORDER BY Category_Name", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader r = cmd.ExecuteReader())
                            while (r.Read()) categoryNames.Add(r["Category_Name"].ToString().Trim());
                    }

                    // Products in a linked category that carry a different discount of their own (that one takes priority)
                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT COUNT(*) FROM Product p INNER JOIN Category c ON c.Category_ID = p.Category_ID
                        WHERE c.Discount_ID = @Id AND p.Discount_ID IS NOT NULL AND p.Discount_ID <> @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        overridden = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("The products for this discount could not be loaded: " + ex.Message, false);
                return;
            }

            products.Columns.Add("DiscountedPrice", typeof(decimal));
            foreach (DataRow r in products.Rows)
                r["DiscountedPrice"] = ApplyDiscount(Convert.ToDecimal(r["Product_SellingPrice"]), type, value);

            ProductsDiscountId = id;
            litProdTitle.Text = Server.HtmlEncode(name);
            litProdAmount.Text = Server.HtmlEncode(AmountText(type, value));
            litProdPeriod.Text = Server.HtmlEncode(DateText(d["Discount_StartDate"]) + " to " + DateText(d["Discount_EndDate"]));
            litProdStatus.Text = "<span class=\"emp-badge emp-badge-" + status.Css + "\">" + Server.HtmlEncode(status.Label) + "</span>";
            litProdCount.Text = products.Rows.Count.ToString("N0");
            litProdCategories.Text = categoryNames.Count == 0 ? "None" : Server.HtmlEncode(string.Join(", ", categoryNames));

            string code = Convert.ToString(d["Discount_Code"]).Trim();
            litProdCode.Text = code.Length == 0 ? "None" : Server.HtmlEncode(code + PromoConditions(d["Discount_MinSpend"], d["Discount_Rule"], " (", ")"));

            string note = "";
            if (code.Length > 0)
            {
                note = "This is a promo code: customers enter " + code + " in the cart and it comes off the whole order"
                     + (products.Rows.Count > 0 ? ". The products below are ALSO linked to it, so they get the discount twice. Unlink them in Manage Products." : ", so it doesn't need any products linked.");
            }
            if (overridden > 0)
            {
                note += (note.Length > 0 ? " " : "") + (overridden == 1
                    ? "1 product in the linked categories has its own discount, which takes priority, so it isn't listed."
                    : overridden + " products in the linked categories have their own discount, which takes priority, so they aren't listed.");
            }
            if (status.Key != "running")
            {
                note += (note.Length > 0 ? " " : "") + "This discount isn't running right now, so customers currently pay the shelf price.";
            }
            litProdNote.Text = note.Length > 0 ? "<p class=\"field-hint disc-note\">" + Server.HtmlEncode(note) + "</p>" : "";

            gvProducts.PageIndex = 0;
            gvProducts.DataSource = products;
            gvProducts.DataBind();
            pnlProducts.Visible = true;

            ScrollTo("discProductsCard", null);
        }

        protected void gvProducts_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            int id = ProductsDiscountId;
            if (id == 0) return;
            ShowProducts(id);
            gvProducts.PageIndex = e.NewPageIndex;
            gvProducts.DataBind();
        }

        protected void btnCloseProducts_Click(object sender, EventArgs e)
        {
            pnlProducts.Visible = false;
            ProductsDiscountId = 0;
        }

        private static decimal ApplyDiscount(decimal basePrice, string type, decimal value)
        {
            if (type == "Percentage") return Math.Round(basePrice - (basePrice * value / 100m), 2);
            return Math.Max(0m, basePrice - value);
        }

        private DataRow LoadDiscount(int id)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT Discount_ID, Discount_Name, Discount_Type, Discount_Value, Discount_Status,
                       Discount_StartDate, Discount_EndDate, " + PromoSelect("") + @"
                FROM Discount WHERE Discount_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        // ---------- ADD ----------
        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            litMessage.Text = "";
            pnlProducts.Visible = false;
            hfDiscountId.Value = "";

            txtName.Text = "";
            txtDescription.Text = "";
            txtValue.Text = "";
            ddlType.SelectedIndex = 0;
            txtCode.Text = "";
            txtMinSpend.Text = "";
            ddlRule.SelectedIndex = 0;
            txtStartDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            txtEndDate.Text = DateTime.Today.AddDays(30).ToString("yyyy-MM-dd");
            txtStartDate.Attributes["min"] = DateTime.Today.ToString("yyyy-MM-dd");
            txtEndDate.Attributes["min"] = DateTime.Today.ToString("yyyy-MM-dd");

            SetFormMode(true);
            litFormTitle.Text = "Add discount";
            litFormMeta.Text = "Starts as Active";
            btnSave.Text = "Add discount";
            pnlForm.Visible = true;

            ScrollTo("discFormCard", txtName.ClientID);
        }

        private void SetFormMode(bool isNew)
        {
            phAddFields.Visible = isNew;
            phAddStatus.Visible = isNew;
            phReadOnly.Visible = !isNew;
        }

        // ---------- EDIT (end date only) ----------
        private void LoadForEdit(int id)
        {
            pnlProducts.Visible = false;

            DataRow d;
            try
            {
                d = LoadDiscount(id);
            }
            catch (Exception ex)
            {
                ShowMessage("The discount could not be loaded: " + ex.Message, false);
                return;
            }
            if (d == null)
            {
                ShowMessage("That discount could not be found.", false);
                pnlForm.Visible = false;
                return;
            }

            string name = Convert.ToString(d["Discount_Name"]).Trim();
            StatusInfo status = GetStatus(d["Discount_Status"], d["Discount_StartDate"], d["Discount_EndDate"], name);

            litRoName.Text = Server.HtmlEncode(name);
            litRoAmount.Text = Server.HtmlEncode(AmountText(d["Discount_Type"], d["Discount_Value"]));
            litRoStart.Text = Server.HtmlEncode(DateText(d["Discount_StartDate"]));
            litRoStatus.Text = "<span class=\"emp-badge emp-badge-" + status.Css + "\">" + Server.HtmlEncode(status.Label) + "</span>";

            string roCode = Convert.ToString(d["Discount_Code"]).Trim();
            litRoCode.Text = roCode.Length == 0 ? "None (product discount)" : Server.HtmlEncode(roCode);
            litRoMinSpend.Text = d["Discount_MinSpend"] == DBNull.Value ? "None" : Server.HtmlEncode(Convert.ToDecimal(d["Discount_MinSpend"]).ToString("C"));
            litRoRule.Text = Server.HtmlEncode(RuleText(d["Discount_Rule"]));

            txtEndDate.Text = d["Discount_EndDate"] == DBNull.Value ? "" : Convert.ToDateTime(d["Discount_EndDate"]).ToString("yyyy-MM-dd");
            DateTime minEnd = MinAllowedEnd(d["Discount_StartDate"]);
            txtEndDate.Attributes["min"] = minEnd.ToString("yyyy-MM-dd");

            hfDiscountId.Value = id.ToString();
            SetFormMode(false);
            litFormTitle.Text = "Change end date";
            litFormMeta.Text = "Currently ends " + Server.HtmlEncode(DateText(d["Discount_EndDate"]));
            btnSave.Text = "Save end date";
            pnlForm.Visible = true;

            ScrollTo("discFormCard", txtEndDate.ClientID);
        }

        /// <summary>The end date can't be before the start date, or before today.</summary>
        private static DateTime MinAllowedEnd(object startValue)
        {
            DateTime min = DateTime.Today;
            if (startValue != DBNull.Value)
            {
                DateTime start = Convert.ToDateTime(startValue).Date;
                if (start > min) min = start;
            }
            return min;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            hfDiscountId.Value = "";
            litMessage.Text = "";
        }

        // ---------- SAVE ----------
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id;
            if (int.TryParse(hfDiscountId.Value, out id))
                SaveEndDate(id);
            else
                AddDiscount();
        }

        private static bool TryParseDate(string text, out DateTime date)
        {
            return DateTime.TryParseExact((text ?? "").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        /// <summary>The value stored for an end date: the last moment of that day (date-only columns just keep the date).</summary>
        private object EndValueFor(DateTime endDate)
        {
            if (EndCol.IsDateOnly) return endDate.Date;
            // smalldatetime rounds seconds, so stop at 23:59 there
            if (EndCol.DataType.Equals("smalldatetime", StringComparison.OrdinalIgnoreCase))
                return endDate.Date.AddHours(23).AddMinutes(59);
            return endDate.Date.AddDays(1).AddSeconds(-1);
        }

        private void SaveEndDate(int id)
        {
            DataRow d;
            try
            {
                d = LoadDiscount(id);
            }
            catch (Exception ex)
            {
                ShowFormError("The discount could not be loaded: " + ex.Message, txtEndDate);
                return;
            }
            if (d == null)
            {
                ShowMessage("That discount could not be found. It may have been changed by someone else.", false);
                pnlForm.Visible = false;
                RefreshAll();
                return;
            }

            DateTime end;
            if (!TryParseDate(txtEndDate.Text, out end))
            {
                ShowFormError("Please choose a valid end date.", txtEndDate);
                return;
            }

            DateTime minEnd = MinAllowedEnd(d["Discount_StartDate"]);
            if (end < minEnd)
            {
                ShowFormError(minEnd == DateTime.Today
                    ? "The end date can't be in the past. To stop the discount now, use Deactivate instead."
                    : "The end date can't be before the start date (" + DateText(d["Discount_StartDate"]) + ").", txtEndDate);
                return;
            }
            if (end > DateTime.Today.AddYears(5))
            {
                ShowFormError("The end date can be at most 5 years from today.", txtEndDate);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(
                    // Only the end date is ever changed for an existing discount
                    "UPDATE Discount SET Discount_EndDate = @End WHERE Discount_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@End", EndValueFor(end));
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        ShowMessage("That discount could not be found. It may have been changed by someone else.", false);
                        pnlForm.Visible = false;
                        RefreshAll();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowFormError("The end date could not be saved: " + ex.Message, txtEndDate);
                return;
            }

            pnlForm.Visible = false;
            hfDiscountId.Value = "";
            RefreshAll();

            string msg = Convert.ToString(d["Discount_Name"]).Trim() + " now ends on " + end.ToString("d MMM yyyy") + ".";
            if (!IsActiveValueOn(d["Discount_Status"])) msg += " It is still inactive: use Reactivate to switch it back on.";
            ShowMessage(msg, true);
        }

        /// <summary>Label used for rand-off discounts: whatever the existing data uses, else "Fixed".</summary>
        private string FixedTypeLabel()
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 1 LTRIM(RTRIM(Discount_Type)) FROM Discount WHERE Discount_Type IS NOT NULL AND LTRIM(RTRIM(Discount_Type)) <> 'Percentage' GROUP BY LTRIM(RTRIM(Discount_Type)) ORDER BY COUNT(*) DESC", conn))
            {
                conn.Open();
                object v = cmd.ExecuteScalar();
                string label = v == null || v == DBNull.Value ? "" : v.ToString();
                return label.Length > 0 ? label : "Fixed";
            }
        }

        private void AddDiscount()
        {
            SetFormMode(true);

            string name = Regex.Replace(txtName.Text ?? "", @"\s+", " ").Trim();
            string desc = DescCol != null ? (txtDescription.Text ?? "").Trim() : null;
            bool isPercent = ddlType.SelectedValue == "Percentage";
            int nameMax = Schema.Find("Discount_Name").TextLimit(100);

            // ----- Name -----
            if (name.Length == 0) { ShowFormError("Please enter a discount name.", txtName); return; }
            if (name.Length < 3) { ShowFormError("The discount name must be at least 3 characters.", txtName); return; }
            if (name.Length > nameMax) { ShowFormError("The discount name can be at most " + nameMax + " characters.", txtName); return; }
            if (!Regex.IsMatch(name, @"[A-Za-z]")) { ShowFormError("The discount name must contain at least one letter.", txtName); return; }
            if (name.IndexOfAny(new[] { '<', '>' }) >= 0) { ShowFormError("The discount name cannot contain < or >.", txtName); return; }

            if (desc != null)
            {
                int descMax = DescCol.TextLimit(500);
                if (desc.Length > descMax) { ShowFormError("The description can be at most " + descMax + " characters.", txtDescription); return; }
                if (desc.IndexOfAny(new[] { '<', '>' }) >= 0) { ShowFormError("The description cannot contain < or >.", txtDescription); return; }
            }

            // ----- Value -----
            string valueText = (txtValue.Text ?? "").Trim().Replace(",", ".").TrimStart('R', 'r').TrimEnd('%').Trim();
            decimal value;
            if (valueText.Length == 0) { ShowFormError("Please enter the discount value.", txtValue); return; }
            if (!decimal.TryParse(valueText, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
            { ShowFormError("The discount value must be a number, e.g. 15 or 49.99.", txtValue); return; }
            if (decimal.Round(value, 2) != value) { ShowFormError("The discount value can have at most 2 decimal places.", txtValue); return; }

            bool wholeOnly = ValueCol.DataType.IndexOf("int", StringComparison.OrdinalIgnoreCase) >= 0;
            if (wholeOnly && value != decimal.Truncate(value)) { ShowFormError("The discount value must be a whole number.", txtValue); return; }

            if (isPercent)
            {
                if (value < 1 || value > 99) { ShowFormError("A percentage discount must be between 1% and 99%.", txtValue); return; }
            }
            else
            {
                if (value <= 0) { ShowFormError("A rand discount must be more than R0.", txtValue); return; }
                if (value > 10000) { ShowFormError("A rand discount can be at most R10 000.", txtValue); return; }
            }

            // ----- Promo code, minimum spend and rule -----
            string code = "";
            decimal? minSpend = null;
            string rule = "";
            if (HasPromoCols)
            {
                code = Regex.Replace(txtCode.Text ?? "", @"\s+", "").ToUpperInvariant();
                string minText = (txtMinSpend.Text ?? "").Trim().Replace(",", ".").TrimStart('R', 'r').Trim().Replace(" ", "");
                rule = ddlRule.SelectedValue ?? "";
                int codeMax = Math.Min(20, CodeCol.TextLimit(20));

                if (code.Length > 0)
                {
                    if (code.Length < 3 || code.Length > codeMax)
                    { ShowFormError("The promo code must be 3 to " + codeMax + " characters.", txtCode); return; }
                    if (!CodeFormat.IsMatch(code))
                    { ShowFormError("The promo code can only use letters, numbers, - and _ (no spaces).", txtCode); return; }
                }

                if (minText.Length > 0)
                {
                    decimal m;
                    if (!decimal.TryParse(minText, NumberStyles.Number, CultureInfo.InvariantCulture, out m))
                    { ShowFormError("The minimum spend must be a number, e.g. 500.", txtMinSpend); return; }
                    if (decimal.Round(m, 2) != m) { ShowFormError("The minimum spend can have at most 2 decimal places.", txtMinSpend); return; }
                    if (m <= 0) { ShowFormError("The minimum spend must be more than R0, or leave it empty.", txtMinSpend); return; }
                    if (m > 100000) { ShowFormError("The minimum spend can be at most R100 000.", txtMinSpend); return; }
                    minSpend = m;
                }

                if (rule.Length > 0 && !KnownRules.Contains(rule))
                { ShowFormError("Please choose who can use this discount.", ddlRule); return; }

                if (code.Length == 0 && (minSpend.HasValue || rule.Length > 0))
                {
                    ShowFormError("A minimum spend or a Student / First order rule only works with a promo code. Enter a code, or clear those fields.", txtCode);
                    return;
                }

                if (code.Length > 0 && !isPercent && minSpend.HasValue && value >= minSpend.Value)
                {
                    ShowFormError("For a rand-off code the minimum spend must be more than the discount (R" + value.ToString("N2") + ").", txtMinSpend);
                    return;
                }
            }

            // ----- Dates -----
            DateTime start, end;
            if (!TryParseDate(txtStartDate.Text, out start)) { ShowFormError("Please choose a valid start date.", txtStartDate); return; }
            if (!TryParseDate(txtEndDate.Text, out end)) { ShowFormError("Please choose a valid end date.", txtEndDate); return; }
            if (start < DateTime.Today) { ShowFormError("The start date can't be in the past.", txtStartDate); return; }
            if (start > DateTime.Today.AddYears(2)) { ShowFormError("The start date can be at most 2 years from today.", txtStartDate); return; }
            if (end < start) { ShowFormError("The end date can't be before the start date.", txtEndDate); return; }
            if (end > start.AddYears(5)) { ShowFormError("A discount can run for at most 5 years.", txtEndDate); return; }

            try
            {
                if (NameTaken(name)) { ShowFormError("A discount called \"" + name + "\" already exists.", txtName); return; }
                if (code.Length > 0 && CodeTaken(code)) { ShowFormError("The promo code " + code + " is already used by another discount.", txtCode); return; }
                if (rule.Length > 0 && RuleTaken(rule, start, end))
                {
                    ShowFormError("Another active " + RuleText(rule).ToLowerInvariant() + " code already runs during these dates. Customers are only shown one, so deactivate the old one first.", ddlRule);
                    return;
                }

                var cols = new List<string> { "Discount_Name", "Discount_Type", "Discount_Value", "Discount_Status", "Discount_StartDate", "Discount_EndDate" };
                var pars = new List<string> { "@Name", "@Type", "@Value", "@Status", "@Start", "@End" };
                if (DescCol != null) { cols.Add(DescCol.Name); pars.Add("@Desc"); }
                if (HasPromoCols)
                {
                    cols.Add(CodeCol.Name); pars.Add("@Code");
                    cols.Add(MinSpendCol.Name); pars.Add("@MinSpend");
                    cols.Add(RuleCol.Name); pars.Add("@Rule");
                }

                TableSchema.Column idCol = Schema.Find("Discount_ID");
                bool manualId = !idCol.IsIdentity;
                if (manualId) { cols.Add("Discount_ID"); pars.Add("@NewId"); }

                List<string> unfilled = Schema.UnfilledRequired(cols);
                if (unfilled.Count > 0)
                {
                    ShowFormError("The Discount table needs a value for " + string.Join(", ", unfilled) + ", which this page doesn't fill in. Ask whoever set up the database to give it a default value.", txtName);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tx = conn.BeginTransaction())
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.Transaction = tx;

                        string sql = "";
                        if (manualId)
                            sql = "DECLARE @NewId INT = (SELECT ISNULL(MAX(Discount_ID), 0) + 1 FROM Discount WITH (UPDLOCK, HOLDLOCK)); ";

                        sql += "INSERT INTO Discount (" + string.Join(", ", cols.Select(c => TableSchema.Q(c))) + ") VALUES ("
                             + string.Join(", ", pars) + ")";
                        cmd.CommandText = sql;

                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Type", isPercent ? "Percentage" : FixedTypeLabel());
                        cmd.Parameters.AddWithValue("@Value", value);
                        cmd.Parameters.AddWithValue("@Status", ActiveValue);
                        cmd.Parameters.AddWithValue("@Start", start.Date);
                        cmd.Parameters.AddWithValue("@End", EndValueFor(end));
                        if (DescCol != null)
                            cmd.Parameters.AddWithValue("@Desc", desc.Length == 0 && DescCol.Nullable ? (object)DBNull.Value : desc);
                        if (HasPromoCols)
                        {
                            cmd.Parameters.AddWithValue("@Code", code.Length == 0 ? (object)DBNull.Value : code);
                            cmd.Parameters.Add("@MinSpend", SqlDbType.Decimal).Value = minSpend.HasValue ? (object)minSpend.Value : DBNull.Value;
                            cmd.Parameters["@MinSpend"].Precision = 18;
                            cmd.Parameters["@MinSpend"].Scale = 2;
                            cmd.Parameters.AddWithValue("@Rule", rule.Length == 0 ? (object)DBNull.Value : rule);
                        }

                        cmd.ExecuteNonQuery();
                        tx.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowFormError("The discount could not be saved: " + ex.Message, txtName);
                return;
            }

            pnlForm.Visible = false;
            RefreshAll();
            ShowMessage(code.Length > 0
                ? name + " was added. Customers can use the code " + code + " in their cart from " + start.ToString("d MMM yyyy") + "."
                : name + " was added. Link products to it in Manage Products using the Discount drop-down.", true);
        }

        private bool NameTaken(string name)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Discount WHERE UPPER(LTRIM(RTRIM(Discount_Name))) = UPPER(@Name)", conn))
            {
                cmd.Parameters.AddWithValue("@Name", name);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool CodeTaken(string code)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Discount WHERE UPPER(LTRIM(RTRIM(" + TableSchema.Q(CodeCol.Name) + "))) = @Code", conn))
            {
                cmd.Parameters.AddWithValue("@Code", code);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        /// <summary>True when another active discount with the same rule overlaps these dates.</summary>
        private bool RuleTaken(string rule, DateTime start, DateTime end)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT Discount_Status, Discount_StartDate, Discount_EndDate FROM Discount WHERE LTRIM(RTRIM(" + TableSchema.Q(RuleCol.Name) + ")) = @Rule", conn))
            {
                cmd.Parameters.AddWithValue("@Rule", rule);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        if (!IsActiveValueOn(r["Discount_Status"])) continue;
                        DateTime s = r["Discount_StartDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["Discount_StartDate"]).Date;
                        DateTime e = r["Discount_EndDate"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(r["Discount_EndDate"]).Date;
                        if (s <= end.Date && e >= start.Date) return true;
                    }
                }
            }
            return false;
        }

        // ---------- DEACTIVATE / REACTIVATE ----------
        private void ToggleStatus(int id)
        {
            try
            {
                DataRow d = LoadDiscount(id);
                if (d == null)
                {
                    ShowMessage("That discount could not be found.", false);
                    RefreshAll();
                    return;
                }

                string name = Convert.ToString(d["Discount_Name"]).Trim();
                bool turningOn = !IsActiveValueOn(d["Discount_Status"]);

                if (turningOn && d["Discount_EndDate"] != DBNull.Value && Convert.ToDateTime(d["Discount_EndDate"]) < DateTime.Now)
                {
                    ShowMessage(name + " ended on " + DateText(d["Discount_EndDate"]) + ". Change its end date first, then reactivate it.", false);
                    RefreshAll();
                    return;
                }

                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand("UPDATE Discount SET Discount_Status = @Status WHERE Discount_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Status", turningOn ? ActiveValue : InactiveValue);
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowMessage(turningOn
                    ? name + " is active again."
                    : name + " was deactivated. Customers now pay the normal price on its products.", true);
            }
            catch (Exception ex)
            {
                ShowMessage("The discount status could not be changed: " + ex.Message, false);
            }

            // Keep the product view in step if it's open for this discount
            if (pnlProducts.Visible && ProductsDiscountId == id) ShowProducts(id);
            RefreshAll();
        }

        // ---------- DISPLAY HELPERS ----------
        protected string AmountText(object type, object value)
        {
            if (value == null || value == DBNull.Value) return "";
            decimal v = Convert.ToDecimal(value);
            if (Convert.ToString(type).Trim() == "Percentage")
                return v.ToString("0.##", CultureInfo.InvariantCulture) + "% off";
            return v.ToString("C") + " off";
        }

        protected string CodeText(object code)
        {
            string c = code == null || code == DBNull.Value ? "" : code.ToString().Trim();
            if (c.Length == 0) return "<span class=\"emp-sub\">No code</span>";
            return "<strong class=\"disc-code\">" + HttpUtility.HtmlEncode(c) + "</strong>";
        }

        protected string PromoSub(object minSpend, object rule)
        {
            string text = PromoConditions(minSpend, rule, "", "");
            return text.Length == 0 ? "" : "<span class=\"emp-sub\">" + HttpUtility.HtmlEncode(text) + "</span>";
        }

        /// <summary>"Min spend R500.00 · Students only", wrapped in prefix/suffix when there's anything to say.</summary>
        private static string PromoConditions(object minSpend, object rule, string prefix, string suffix)
        {
            var parts = new List<string>();
            if (minSpend != null && minSpend != DBNull.Value) parts.Add("Min spend " + Convert.ToDecimal(minSpend).ToString("C"));
            string r = rule == null || rule == DBNull.Value ? "" : rule.ToString().Trim();
            if (r.Length > 0) parts.Add(RuleText(r));
            return parts.Count == 0 ? "" : prefix + string.Join(" · ", parts) + suffix;
        }

        protected static string RuleText(object rule)
        {
            string r = rule == null || rule == DBNull.Value ? "" : rule.ToString().Trim();
            if (r.Length == 0) return "Anyone";
            if (r.Equals("Student", StringComparison.OrdinalIgnoreCase)) return "Students only";
            if (r.Equals("FirstOrder", StringComparison.OrdinalIgnoreCase)) return "First order only";
            return r + " (unknown rule: code won't work)";
        }

        protected string AppliesText(object code, object productCount, object categoryCount)
        {
            int n = Convert.ToInt32(productCount);
            bool hasCode = code != null && code != DBNull.Value && code.ToString().Trim().Length > 0;
            if (hasCode && n == 0) return "Whole order<span class=\"emp-sub\">via promo code</span>";
            string text = n + " product" + (n == 1 ? "" : "s") + CategorySub(categoryCount);
            if (hasCode) text += "<span class=\"emp-sub\">+ whole order via code</span>";
            return text;
        }

        protected string DateText(object value)
        {
            if (value == null || value == DBNull.Value) return "No date";
            return Convert.ToDateTime(value).ToString("d MMM yyyy");
        }

        protected string SubText(object value)
        {
            string text = value == null || value == DBNull.Value ? "" : value.ToString().Trim();
            if (text.Length == 0) return "";
            if (text.Length > 100) text = text.Substring(0, 97).TrimEnd() + "...";
            return "<span class=\"emp-sub\">" + HttpUtility.HtmlEncode(text) + "</span>";
        }

        protected string CategorySub(object count)
        {
            int n = Convert.ToInt32(count);
            if (n == 0) return "";
            return "<span class=\"emp-sub\">incl. " + (n == 1 ? "1 whole category" : n + " whole categories") + "</span>";
        }

        protected string ProductSub(object brand, object size)
        {
            string b = Convert.ToString(brand).Trim();
            string s = size == null || size == DBNull.Value ? "" : Convert.ToString(size) + " ml";
            return b.Length > 0 && s.Length > 0 ? b + " · " + s : b + s;
        }

        protected string ActiveClass(object status)
        {
            return IsActiveValueOn(status) ? "active" : "inactive";
        }

        protected string DeactivateJs(object isActive, object name)
        {
            if (!(isActive is bool) || !(bool)isActive) return "";
            string n = HttpUtility.JavaScriptStringEncode(Convert.ToString(name).Trim());
            return "return dashConfirm(this, 'Deactivate discount', '" + n
                + " will stop applying straight away and customers will pay the normal price. You can reactivate it later.', 'Deactivate', false);";
        }

        private void ShowFormError(string text, Control field)
        {
            ShowMessage(text, false);
            pnlForm.Visible = true;
            ScrollTo("discFormCard", field != null ? field.ClientID : null);
        }

        private void ShowMessage(string text, bool success)
        {
            litMessage.Text = "<div class=\"emp-alert " + (success ? "emp-alert-success" : "emp-alert-error") + "\">"
                + Server.HtmlEncode(text) + "</div>";
        }

        private void ScrollTo(string elementId, string focusClientId)
        {
            string js = "var f=document.getElementById('" + elementId + "'); if(f){f.scrollIntoView({behavior:'smooth',block:'start'});}";
            if (!string.IsNullOrEmpty(focusClientId))
            {
                js += " var i=document.getElementById('" + focusClientId + "'); if(i){setTimeout(function(){i.focus();},350);}";
            }
            ScriptManager.RegisterStartupScript(this, GetType(), "scrollTo", js, true);
        }
    }
}
