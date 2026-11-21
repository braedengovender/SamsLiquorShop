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

namespace SamsLiquorShop.Manager
{
    /// <summary>
    /// Manager page for discounts.
    /// The manager can view discounts (and the products each one applies to), add a discount,
    /// change a discount's END DATE only, and switch a discount on or off.
    /// Deleting discounts is deliberately not offered.
    ///
    /// A discount reaches a product in two ways (same rules as PricingHelper):
    ///   - directly, through Product.Discount_ID
    ///   - through the product's category, Category.Discount_ID
    /// A product's own running discount takes priority over its category's discount.
    /// </summary>
    public partial class Discounts : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private static readonly HashSet<string> AllowedSort = new HashSet<string>
        {
            "Discount_Name", "Discount_Value", "Discount_StartDate", "Discount_EndDate"
        };

        // ---------- DISCOUNT TABLE LAYOUT ----------
        // Read once from the database so the page matches the real column types
        // (for example whether Discount_Status is a bit (1/0) or text ("Active"/"Inactive")).
        private class DiscountSchema
        {
            public int NameMaxLength = 50;
            public bool StatusIsNumber = true;
            public string EndDateType = "datetime";
            public decimal MaxValue = 99999m;
            public string DescColumn;
            public int DescMaxLength = 255;
            public bool DescNullable = true;
        }

        private static DiscountSchema _schema;
        private static readonly object SchemaLock = new object();

        private DiscountSchema Schema
        {
            get
            {
                if (_schema != null) return _schema;
                lock (SchemaLock)
                {
                    if (_schema == null) _schema = ReadSchema();
                }
                return _schema;
            }
        }

        private DiscountSchema ReadSchema()
        {
            var s = new DiscountSchema();
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE, IS_NULLABLE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'Discount'
                ORDER BY ORDINAL_POSITION", conn))
            {
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string col = r["COLUMN_NAME"].ToString();
                        string type = r["DATA_TYPE"].ToString().ToLowerInvariant();
                        int maxLen = r["CHARACTER_MAXIMUM_LENGTH"] == DBNull.Value ? 0 : Convert.ToInt32(r["CHARACTER_MAXIMUM_LENGTH"]);

                        if (col.Equals("Discount_Name", StringComparison.OrdinalIgnoreCase))
                        {
                            if (maxLen > 0) s.NameMaxLength = maxLen;
                        }
                        else if (col.Equals("Discount_Status", StringComparison.OrdinalIgnoreCase))
                        {
                            s.StatusIsNumber = type == "bit" || type == "int" || type == "tinyint" || type == "smallint";
                        }
                        else if (col.Equals("Discount_EndDate", StringComparison.OrdinalIgnoreCase))
                        {
                            s.EndDateType = type;
                        }
                        else if (col.Equals("Discount_Value", StringComparison.OrdinalIgnoreCase))
                        {
                            if ((type == "decimal" || type == "numeric") && r["NUMERIC_PRECISION"] != DBNull.Value)
                            {
                                int p = Convert.ToInt32(r["NUMERIC_PRECISION"]);
                                int sc = r["NUMERIC_SCALE"] == DBNull.Value ? 0 : Convert.ToInt32(r["NUMERIC_SCALE"]);
                                if (p - sc < 10)
                                {
                                    s.MaxValue = (decimal)Math.Pow(10, p - sc) - (decimal)Math.Pow(10, -sc);
                                }
                            }
                        }
                        else if (s.DescColumn == null
                                 && col.IndexOf("desc", StringComparison.OrdinalIgnoreCase) >= 0
                                 && Regex.IsMatch(col, @"^\w+$"))
                        {
                            s.DescColumn = col;
                            s.DescMaxLength = maxLen > 0 ? maxLen : 1000;
                            s.DescNullable = r["IS_NULLABLE"].ToString() == "YES";
                        }
                    }
                }
            }
            return s;
        }

        // SQL condition that is true when a discount's status is "on"
        private string ActiveSql(string alias)
        {
            return Schema.StatusIsNumber
                ? alias + ".Discount_Status = 1"
                : "LTRIM(RTRIM(" + alias + ".Discount_Status)) IN ('Active', '1', 'true')";
        }

        private object StatusValue(bool active)
        {
            if (Schema.StatusIsNumber) return active ? 1 : 0;
            return active ? "Active" : "Inactive";
        }

        // The discount runs until the end of the chosen day
        private DateTime EndOfDay(DateTime date)
        {
            switch (Schema.EndDateType)
            {
                case "date": return date.Date;
                case "smalldatetime": return date.Date.AddHours(23).AddMinutes(59);
                default: return date.Date.AddDays(1).AddSeconds(-1);
            }
        }

        private string SortField
        {
            get { return (string)ViewState["SortField"] ?? "Discount_EndDate"; }
            set { ViewState["SortField"] = value; }
        }

        private string SortDir
        {
            get { return (string)ViewState["SortDir"] ?? "DESC"; }
            set { ViewState["SortDir"] = value; }
        }

        // Type and value of the discount whose products are being shown (used to work out sale prices)
        private string _viewType;
        private decimal _viewValue;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Manager")) return;

            txtSearch.Attributes["data-live-search"] = btnSearch.ClientID;

            try
            {
                txtName.MaxLength = Schema.NameMaxLength;
                phDescription.Visible = Schema.DescColumn != null;
            }
            catch (Exception ex)
            {
                ShowMessage("The discount table could not be read: " + ex.Message, false);
                return;
            }

            if (!IsPostBack)
            {
                BindDiscounts();
            }
        }

        // ---------- LIST ----------
        private void BindDiscounts()
        {
            string sort = AllowedSort.Contains(SortField) ? SortField : "Discount_EndDate";
            string dir = SortDir == "ASC" ? "ASC" : "DESC";

            string sql = @"
                SELECT d.Discount_ID, d.Discount_Name, d.Discount_Type, d.Discount_Value,
                       d.Discount_StartDate, d.Discount_EndDate, d.Discount_Status,
                       (SELECT COUNT(*) FROM Product p WHERE p.Discount_ID = d.Discount_ID) AS ProductCount,
                       STUFF((SELECT ', ' + c.Category_Name FROM Category c
                              WHERE c.Discount_ID = d.Discount_ID ORDER BY c.Category_Name
                              FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS CategoryNames
                FROM Discount d";

            var where = new List<string>();
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    string search = txtSearch.Text.Trim();
                    if (search.Length > 0)
                    {
                        where.Add("d.Discount_Name LIKE @Search");
                        cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                    }

                    switch (ddlStatusFilter.SelectedValue)
                    {
                        case "active": where.Add(ActiveSql("d")); break;
                        case "inactive": where.Add("NOT (" + ActiveSql("d") + ")"); break;
                        case "running": where.Add(ActiveSql("d") + " AND d.Discount_StartDate <= GETDATE() AND d.Discount_EndDate >= GETDATE()"); break;
                        case "scheduled": where.Add("d.Discount_StartDate > GETDATE()"); break;
                        case "ended": where.Add("d.Discount_EndDate < GETDATE()"); break;
                    }

                    if (where.Count > 0) sql += " WHERE " + string.Join(" AND ", where);
                    sql += " ORDER BY d." + sort + " " + dir + ", d.Discount_Name ASC";
                    cmd.CommandText = sql;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("The discounts could not be loaded: " + ex.Message, false);
            }

            gvDiscounts.DataSource = dt;
            gvDiscounts.DataBind();
            litCount.Text = dt.Rows.Count == 1 ? "1 discount" : dt.Rows.Count + " discounts";
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
                SortDir = SortDir == "ASC" ? "DESC" : "ASC";
            else
            {
                SortField = e.SortExpression;
                SortDir = "ASC";
            }
            gvDiscounts.PageIndex = 0;
            BindDiscounts();
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
                case "EditEnd": LoadEndDateForm(id); break;
                case "ToggleStatus": ToggleStatus(id); break;
            }
        }

        // ---------- ADD ----------
        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            litMessage.Text = "";
            pnlEdit.Visible = false;
            pnlProducts.Visible = false;

            txtName.Text = "";
            ddlType.SelectedIndex = 0;
            txtValue.Text = "";
            txtStart.Text = DateTime.Today.ToString("yyyy-MM-dd");
            txtEnd.Text = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd");
            txtDescription.Text = "";
            rblApplyTo.SelectedValue = "products";

            try
            {
                LoadApplyLists();
            }
            catch (Exception ex)
            {
                ShowMessage("The products and categories could not be loaded: " + ex.Message, false);
            }

            ShowApplyPanels();
            pnlAdd.Visible = true;
            ScrollTo("discAddCard", txtName.ClientID);
        }

        // Fills the category drop-down and the product tick list, showing any discount each already has
        private void LoadApplyLists()
        {
            ddlApplyCategory.Items.Clear();
            ddlApplyCategory.Items.Add(new ListItem("Select a category", ""));
            cblProducts.Items.Clear();

            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT c.Category_ID, c.Category_Name, d.Discount_Name
                    FROM Category c
                    LEFT JOIN Discount d ON d.Discount_ID = c.Discount_ID
                    ORDER BY c.Category_Name", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string text = r["Category_Name"].ToString();
                        if (r["Discount_Name"] != DBNull.Value) text += "  (currently: " + r["Discount_Name"] + ")";
                        ddlApplyCategory.Items.Add(new ListItem(text, r["Category_ID"].ToString()));
                    }
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT p.Product_ID, p.Product_Name, p.Product_Brand, p.Product_SizeML, p.Product_Status,
                           p.Product_SellingPrice, d.Discount_Name
                    FROM Product p
                    LEFT JOIN Discount d ON d.Discount_ID = p.Discount_ID
                    ORDER BY p.Product_Name, p.Product_SizeML", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string text = HttpUtility.HtmlEncode(r["Product_Name"].ToString());
                        string sub = SubLine(r["Product_Brand"], r["Product_SizeML"]);
                        if (sub.Length > 0) text += " <span class=\"emp-sub\" style=\"display:inline\">" + sub + "</span>";
                        text += " &middot; " + Convert.ToDecimal(r["Product_SellingPrice"]).ToString("C");
                        if (ProductStatusClass(r["Product_Status"]) != "active") text += " <em>(inactive)</em>";
                        if (r["Discount_Name"] != DBNull.Value)
                            text += " <em>(currently: " + HttpUtility.HtmlEncode(r["Discount_Name"].ToString()) + ")</em>";

                        cblProducts.Items.Add(new ListItem(text, r["Product_ID"].ToString()));
                    }
                }
            }
        }

        protected void rblApplyTo_Changed(object sender, EventArgs e)
        {
            ShowApplyPanels();
            pnlAdd.Visible = true;
        }

        private void ShowApplyPanels()
        {
            phApplyCategory.Visible = rblApplyTo.SelectedValue == "category";
            phApplyProducts.Visible = rblApplyTo.SelectedValue == "products";
        }

        protected void btnSaveNew_Click(object sender, EventArgs e)
        {
            pnlAdd.Visible = true;

            string name = Regex.Replace(txtName.Text ?? "", @"\s+", " ").Trim();
            string type = ddlType.SelectedValue == "Fixed" ? "Fixed" : "Percentage";
            string desc = Schema.DescColumn != null ? (txtDescription.Text ?? "").Trim() : null;
            string applyTo = rblApplyTo.SelectedValue;

            decimal value;
            DateTime start, end;
            Control bad = null;
            string error = null;

            bool valueOk = decimal.TryParse((txtValue.Text ?? "").Trim().Replace(',', '.'),
                NumberStyles.Number, CultureInfo.InvariantCulture, out value);
            bool startOk = DateTime.TryParseExact(txtStart.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out start);
            bool endOk = DateTime.TryParseExact(txtEnd.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out end);

            var productIds = cblProducts.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => Convert.ToInt32(i.Value)).ToList();
            int categoryId = 0;

            // ----- Checks -----
            if (name.Length < 2) { error = "Please enter a discount name (at least 2 characters)."; bad = txtName; }
            else if (name.Length > Schema.NameMaxLength) { error = "The discount name can be at most " + Schema.NameMaxLength + " characters."; bad = txtName; }
            else if (name.IndexOfAny(new[] { '<', '>' }) >= 0) { error = "The discount name cannot contain < or >."; bad = txtName; }
            else if (!valueOk || value <= 0) { error = "Please enter a discount value greater than 0."; bad = txtValue; }
            else if (decimal.Round(value, 2) != value) { error = "The value can have at most 2 decimal places."; bad = txtValue; }
            else if (type == "Percentage" && value > 100) { error = "A percentage discount can be at most 100%."; bad = txtValue; }
            else if (value > Schema.MaxValue) { error = "The value can be at most " + Schema.MaxValue.ToString("0.##") + "."; bad = txtValue; }
            else if (!startOk) { error = "Please choose a start date."; bad = txtStart; }
            else if (!endOk) { error = "Please choose an end date."; bad = txtEnd; }
            else if (end < start) { error = "The end date cannot be before the start date."; bad = txtEnd; }
            else if (end < DateTime.Today) { error = "The end date cannot be in the past."; bad = txtEnd; }
            else if (desc != null && desc.Length > Schema.DescMaxLength) { error = "The description can be at most " + Schema.DescMaxLength + " characters."; bad = txtDescription; }
            else if (applyTo == "category" && !int.TryParse(ddlApplyCategory.SelectedValue, out categoryId)) { error = "Please choose the category the discount applies to."; bad = ddlApplyCategory; }
            else if (applyTo == "products" && productIds.Count == 0) { error = "Please tick at least one product, or choose \"Nothing for now\"."; bad = txtName; }

            try
            {
                if (error == null && NameTaken(name))
                {
                    error = "A discount called \"" + name + "\" already exists.";
                    bad = txtName;
                }

                // A fixed amount must be smaller than the price of every product it covers
                if (error == null && type == "Fixed" && applyTo != "none")
                {
                    string cheapest = CheapestCovered(applyTo, categoryId, productIds, value);
                    if (cheapest != null)
                    {
                        error = "R" + value.ToString("0.00") + " off is more than the price of " + cheapest + ". Use a smaller amount or a percentage.";
                        bad = txtValue;
                    }
                }
            }
            catch (Exception ex)
            {
                error = "The discount could not be checked: " + ex.Message;
            }

            if (error != null)
            {
                ShowMessage(error, false);
                ShowApplyPanels();
                ScrollTo("discAddCard", bad != null ? bad.ClientID : null);
                return;
            }

            // ----- Save: the discount and what it applies to, together -----
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        int newId;
                        string cols = "Discount_Name, Discount_Type, Discount_Value, Discount_StartDate, Discount_EndDate, Discount_Status";
                        string vals = "@Name, @Type, @Value, @Start, @End, @Status";
                        if (Schema.DescColumn != null)
                        {
                            cols += ", [" + Schema.DescColumn + "]";
                            vals += ", @Desc";
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            "INSERT INTO Discount (" + cols + ") VALUES (" + vals + "); SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Name", name);
                            cmd.Parameters.AddWithValue("@Type", type);
                            cmd.Parameters.AddWithValue("@Value", value);
                            cmd.Parameters.AddWithValue("@Start", start.Date);
                            cmd.Parameters.AddWithValue("@End", EndOfDay(end));
                            cmd.Parameters.AddWithValue("@Status", StatusValue(true));
                            if (Schema.DescColumn != null)
                                cmd.Parameters.AddWithValue("@Desc", desc.Length == 0 && Schema.DescNullable ? (object)DBNull.Value : desc);
                            newId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        if (applyTo == "category")
                        {
                            using (SqlCommand cmd = new SqlCommand("UPDATE Category SET Discount_ID = @D WHERE Category_ID = @C", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@D", newId);
                                cmd.Parameters.AddWithValue("@C", categoryId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else if (applyTo == "products")
                        {
                            // ids are ints read from the list, so joining them into the query is safe
                            using (SqlCommand cmd = new SqlCommand(
                                "UPDATE Product SET Discount_ID = @D WHERE Product_ID IN (" + string.Join(",", productIds) + ")", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@D", newId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("The discount could not be saved: " + ex.Message, false);
                ShowApplyPanels();
                return;
            }

            pnlAdd.Visible = false;
            BindDiscounts();

            string where = applyTo == "category" ? " It applies to every product in " + ddlApplyCategory.SelectedItem.Text.Split(new[] { "  (" }, StringSplitOptions.None)[0] + "."
                         : applyTo == "products" ? " It applies to " + productIds.Count + (productIds.Count == 1 ? " product." : " products.")
                         : " It doesn't apply to any products yet. Choose it for a product in Manage Products.";
            ShowMessage(name + " was added." + where, true);
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

        // Returns "Product name (R xx)" for the cheapest covered product priced at or below the amount, or null
        private string CheapestCovered(string applyTo, int categoryId, List<int> productIds, decimal amount)
        {
            string sql = "SELECT TOP 1 Product_Name, Product_SellingPrice FROM Product WHERE Product_SellingPrice <= @Amount AND ";
            if (applyTo == "category") sql += "Category_ID = @C";
            else if (productIds.Count > 0) sql += "Product_ID IN (" + string.Join(",", productIds) + ")";
            else return null;
            sql += " ORDER BY Product_SellingPrice";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@C", categoryId);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return null;
                    return r["Product_Name"] + " (" + Convert.ToDecimal(r["Product_SellingPrice"]).ToString("C") + ")";
                }
            }
        }

        // ---------- EDIT: END DATE ONLY ----------
        private void LoadEndDateForm(int id)
        {
            pnlAdd.Visible = false;
            pnlProducts.Visible = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Discount_Name, Discount_Type, Discount_Value, Discount_StartDate, Discount_EndDate FROM Discount WHERE Discount_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            ShowMessage("That discount could not be found.", false);
                            pnlEdit.Visible = false;
                            return;
                        }

                        litEditName.Text = HttpUtility.HtmlEncode(r["Discount_Name"].ToString());
                        litEditValue.Text = ValueText(r["Discount_Type"], r["Discount_Value"]);
                        litEditStart.Text = r["Discount_StartDate"] == DBNull.Value ? "-" : Convert.ToDateTime(r["Discount_StartDate"]).ToString("d MMM yyyy");
                        txtEditEnd.Text = r["Discount_EndDate"] == DBNull.Value ? "" : Convert.ToDateTime(r["Discount_EndDate"]).ToString("yyyy-MM-dd");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("The discount could not be loaded: " + ex.Message, false);
                return;
            }

            hfEditId.Value = id.ToString();
            pnlEdit.Visible = true;
            ScrollTo("discEditCard", txtEditEnd.ClientID);
        }

        protected void btnSaveEnd_Click(object sender, EventArgs e)
        {
            pnlEdit.Visible = true;

            int id;
            if (!int.TryParse(hfEditId.Value, out id))
            {
                ShowMessage("Choose a discount to edit first.", false);
                pnlEdit.Visible = false;
                return;
            }

            DateTime end;
            if (!DateTime.TryParseExact(txtEditEnd.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out end))
            {
                ShowMessage("Please choose an end date.", false);
                ScrollTo("discEditCard", txtEditEnd.ClientID);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    DateTime? start = null;
                    string name;
                    using (SqlCommand cmd = new SqlCommand("SELECT Discount_Name, Discount_StartDate FROM Discount WHERE Discount_ID = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (!r.Read())
                            {
                                ShowMessage("That discount could not be found.", false);
                                pnlEdit.Visible = false;
                                return;
                            }
                            name = r["Discount_Name"].ToString();
                            if (r["Discount_StartDate"] != DBNull.Value) start = Convert.ToDateTime(r["Discount_StartDate"]);
                        }
                    }

                    if (start.HasValue && end.Date < start.Value.Date)
                    {
                        ShowMessage("The end date cannot be before the start date (" + start.Value.ToString("d MMM yyyy") + ").", false);
                        ScrollTo("discEditCard", txtEditEnd.ClientID);
                        return;
                    }

                    // Only the end date is changed
                    using (SqlCommand cmd = new SqlCommand("UPDATE Discount SET Discount_EndDate = @End WHERE Discount_ID = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@End", EndOfDay(end));
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }

                    pnlEdit.Visible = false;
                    hfEditId.Value = "";
                    BindDiscounts();
                    ShowMessage(name + " now ends on " + end.ToString("d MMM yyyy") + ".", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("The end date could not be saved: " + ex.Message, false);
            }
        }

        // ---------- ACTIVATE / DEACTIVATE ----------
        private void ToggleStatus(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    string name;
                    bool wasActive;
                    using (SqlCommand cmd = new SqlCommand("SELECT Discount_Name, Discount_Status FROM Discount WHERE Discount_ID = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (!r.Read())
                            {
                                ShowMessage("That discount could not be found.", false);
                                return;
                            }
                            name = r["Discount_Name"].ToString();
                            wasActive = IsActive(r["Discount_Status"]);
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand("UPDATE Discount SET Discount_Status = @S WHERE Discount_ID = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@S", StatusValue(!wasActive));
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }

                    BindDiscounts();
                    ShowMessage(wasActive
                        ? name + " was deactivated. Its products are back at their normal prices."
                        : name + " was reactivated. It applies again between its start and end dates.", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("The status could not be changed: " + ex.Message, false);
            }
        }

        // ---------- PRODUCTS IN A DISCOUNT ----------
        private void ShowProducts(int id)
        {
            pnlAdd.Visible = false;
            pnlEdit.Visible = false;

            DataTable dt = new DataTable();
            string name;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT Discount_Name, Discount_Type, Discount_Value FROM Discount WHERE Discount_ID = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (!r.Read())
                            {
                                ShowMessage("That discount could not be found.", false);
                                pnlProducts.Visible = false;
                                return;
                            }
                            name = r["Discount_Name"].ToString();
                            _viewType = r["Discount_Type"].ToString();
                            _viewValue = r["Discount_Value"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Discount_Value"]);
                        }
                    }

                    // Products with this discount directly, plus products in a category that has it.
                    // OverrideName = the product's own (different) discount, which takes priority over the category's.
                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT p.Product_ID, p.Product_Name, p.Product_Brand, p.Product_SizeML, p.Product_SellingPrice, p.Product_Status,
                               c.Category_Name,
                               CASE WHEN p.Discount_ID = @Id THEN 'Product' ELSE 'Category' END AS AppliedVia,
                               od.Discount_Name AS OverrideName
                        FROM Product p
                        INNER JOIN Category c ON c.Category_ID = p.Category_ID
                        LEFT JOIN Discount od ON od.Discount_ID = p.Discount_ID AND p.Discount_ID <> @Id
                        WHERE p.Discount_ID = @Id OR c.Discount_ID = @Id
                        ORDER BY c.Category_Name, p.Product_Name, p.Product_SizeML", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("The products could not be loaded: " + ex.Message, false);
                return;
            }

            litProductsName.Text = HttpUtility.HtmlEncode(name) + " &middot; " + ValueText(_viewType, _viewValue);

            int direct = dt.Select("AppliedVia = 'Product'").Length;
            int viaCategory = dt.Rows.Count - direct;
            var parts = new List<string>();
            if (direct > 0) parts.Add(direct + (direct == 1 ? " product chosen directly" : " products chosen directly"));
            if (viaCategory > 0) parts.Add(viaCategory + (viaCategory == 1 ? " product through its category" : " products through their category"));
            litProductsSummary.Text = dt.Rows.Count == 0
                ? "This discount isn't linked to any products or categories."
                : HttpUtility.HtmlEncode((dt.Rows.Count == 1 ? "1 product: " : dt.Rows.Count + " products: ") + string.Join(", ", parts) + ".");

            gvDiscountProducts.DataSource = dt;
            gvDiscountProducts.DataBind();
            pnlProducts.Visible = true;
            ScrollTo("discProductsCard", null);
        }

        protected void btnCloseProducts_Click(object sender, EventArgs e)
        {
            pnlProducts.Visible = false;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlAdd.Visible = false;
            pnlEdit.Visible = false;
            hfEditId.Value = "";
            litMessage.Text = "";
        }

        // ---------- DISPLAY HELPERS ----------
        protected bool IsActive(object status)
        {
            if (status == null || status == DBNull.Value) return false;
            string s = Convert.ToString(status).Trim();
            return s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("Active", StringComparison.OrdinalIgnoreCase);
        }

        // "15% off" or "R20.00 off"
        protected string ValueText(object type, object value)
        {
            if (value == null || value == DBNull.Value) return "-";
            decimal v = Convert.ToDecimal(value);
            return Convert.ToString(type) == "Percentage"
                ? v.ToString("0.##", CultureInfo.InvariantCulture) + "% off"
                : v.ToString("C") + " off";
        }

        // Whether the discount is actually running today (same rules as PricingHelper)
        protected string RunState(object status, object name, object start, object end)
        {
            if (!IsActive(status)) return "Switched off";

            DateTime now = DateTime.Now;
            if (start != DBNull.Value && now < Convert.ToDateTime(start)) return "Starts " + Convert.ToDateTime(start).ToString("d MMM");
            if (end != DBNull.Value && now > Convert.ToDateTime(end)) return "Ended " + Convert.ToDateTime(end).ToString("d MMM");

            if (Convert.ToString(name).IndexOf("wednesday", StringComparison.OrdinalIgnoreCase) >= 0)
                return now.DayOfWeek == DayOfWeek.Wednesday ? "Running today (Wednesdays only)" : "Wednesdays only";

            return "Running now";
        }

        // "Categories: Wine · 3 products" under the discount name
        protected string AppliesToText(object categoryNames, object productCount)
        {
            var parts = new List<string>();
            string cats = categoryNames == DBNull.Value ? "" : Convert.ToString(categoryNames);
            if (cats.Length > 0) parts.Add((cats.Contains(",") ? "Categories: " : "Category: ") + cats);

            int count = productCount == DBNull.Value ? 0 : Convert.ToInt32(productCount);
            if (count > 0) parts.Add(count == 1 ? "1 product" : count + " products");

            return HttpUtility.HtmlEncode(parts.Count == 0 ? "Not linked to any products" : string.Join(" · ", parts));
        }

        protected string AppliedText(object via, object overrideName)
        {
            if (Convert.ToString(via) == "Product") return "Chosen for this product";

            string text = "Through its category";
            if (overrideName != DBNull.Value && Convert.ToString(overrideName).Length > 0)
            {
                text += "<span class=\"emp-sub\">Its own discount, " + HttpUtility.HtmlEncode(Convert.ToString(overrideName))
                      + ", takes priority while that one is running</span>";
            }
            return text;
        }

        // Normal price crossed out, then the price with this discount
        protected string PriceText(object sellingPrice, object overrideName)
        {
            decimal price = Convert.ToDecimal(sellingPrice);
            decimal sale = _viewType == "Percentage"
                ? Math.Round(price - (price * _viewValue / 100m), 2)
                : Math.Max(0m, price - _viewValue);

            return "<span class=\"disc-strike\">" + price.ToString("C") + "</span><strong>" + sale.ToString("C") + "</strong>";
        }

        protected string ProductStatusClass(object status)
        {
            return (Convert.ToString(status) ?? "").Trim().ToLower() == "active" ? "active" : "inactive";
        }

        protected string ImageUrl(object id)
        {
            return ResolveUrl("~/ProductImage.ashx?id=" + Convert.ToInt32(id));
        }

        // "Brand · 750 ml"
        protected string SubLine(object brand, object sizeMl)
        {
            string b = (Convert.ToString(brand) ?? "").Trim();
            int s = sizeMl == null || sizeMl == DBNull.Value ? 0 : Convert.ToInt32(sizeMl);

            var parts = new List<string>();
            if (b.Length > 0) parts.Add(b);
            if (s > 0) parts.Add(s + " ml");
            return HttpUtility.HtmlEncode(string.Join(" · ", parts));
        }

        protected string DeactivateJs(object status, object name)
        {
            if (!IsActive(status)) return "";
            string n = HttpUtility.JavaScriptStringEncode(Convert.ToString(name) ?? "");
            return "return dashConfirm(this, 'Deactivate discount', '" + n
                + " will stop applying and its products go back to their normal prices. You can reactivate it at any time.', 'Deactivate', false);";
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
