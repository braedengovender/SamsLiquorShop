using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SamsLiquorShop.Helpers;

namespace SamsLiquorShop.Manager
{
    /// <summary>
    /// Manager page for product categories.
    /// The manager can view, add, and update a category's name and description.
    /// Deleting and deactivating categories is deliberately not offered.
    /// </summary>
    public partial class Categories : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private static readonly HashSet<string> AllowedSort = new HashSet<string> { "Category_Name", "ProductCount" };

        // ---------- CATEGORY TABLE LAYOUT ----------
        // The description column's exact name is read from the database once, so the page
        // works whether it is called Category_Description, Category_Desc, etc.
        private class CategorySchema
        {
            public int NameMaxLength = 50;
            public string DescColumn;          // null when the table has no description column
            public int DescMaxLength = 255;
            public bool DescNullable = true;
        }

        private static CategorySchema _schema;
        private static readonly object SchemaLock = new object();

        private CategorySchema Schema
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

        private CategorySchema ReadSchema()
        {
            var s = new CategorySchema();
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'Category'
                ORDER BY ORDINAL_POSITION", conn))
            {
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string col = r["COLUMN_NAME"].ToString();
                        int max = r["CHARACTER_MAXIMUM_LENGTH"] == DBNull.Value ? 0 : Convert.ToInt32(r["CHARACTER_MAXIMUM_LENGTH"]);
                        bool nullable = r["IS_NULLABLE"].ToString() == "YES";

                        if (col.Equals("Category_Name", StringComparison.OrdinalIgnoreCase))
                        {
                            if (max > 0) s.NameMaxLength = max;
                        }
                        else if (s.DescColumn == null
                                 && col.IndexOf("desc", StringComparison.OrdinalIgnoreCase) >= 0
                                 && Regex.IsMatch(col, @"^\w+$"))
                        {
                            s.DescColumn = col;
                            // -1 means nvarchar(max) / text: keep descriptions to a sensible length
                            s.DescMaxLength = max > 0 ? max : 1000;
                            s.DescNullable = nullable;
                        }
                    }
                }
            }
            return s;
        }

        private string SortField
        {
            get { return (string)ViewState["SortField"] ?? "Category_Name"; }
            set { ViewState["SortField"] = value; }
        }

        private string SortDir
        {
            get { return (string)ViewState["SortDir"] ?? "ASC"; }
            set { ViewState["SortDir"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Manager")) return;

            // Typing in the search box clicks Search after a short pause (handled in Site.Master)
            txtSearch.Attributes["data-live-search"] = btnSearch.ClientID;

            try
            {
                txtName.MaxLength = Schema.NameMaxLength;
                bool hasDesc = Schema.DescColumn != null;
                phDescription.Visible = hasDesc;
                gvCategories.Columns[1].Visible = hasDesc;
                if (hasDesc)
                {
                    litDescHint.Text = "Optional. Up to " + Schema.DescMaxLength + " characters.";
                }
            }
            catch (Exception ex)
            {
                ShowMessage("The category table could not be read: " + ex.Message, false);
                return;
            }

            if (!IsPostBack)
            {
                RefreshAll();
            }
        }

        private void RefreshAll()
        {
            BindCategories();
        }

        // ---------- LIST ----------
        private void BindCategories()
        {
            string descExpr = Schema.DescColumn != null
                ? "CAST(c.[" + Schema.DescColumn + "] AS NVARCHAR(MAX))"
                : "CAST(NULL AS NVARCHAR(MAX))";

            string sort = AllowedSort.Contains(SortField) ? SortField : "Category_Name";
            string dir = SortDir == "DESC" ? "DESC" : "ASC";

            string sql = @"
                SELECT c.Category_ID, c.Category_Name, " + descExpr + @" AS Category_Description,
                       (SELECT COUNT(*) FROM Product p WHERE p.Category_ID = c.Category_ID) AS ProductCount
                FROM Category c";

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
                        sql += " WHERE c.Category_Name LIKE @Search";
                        if (Schema.DescColumn != null) sql += " OR " + descExpr + " LIKE @Search";
                        cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                    }

                    sql += " ORDER BY " + sort + " " + dir + (sort != "Category_Name" ? ", c.Category_Name ASC" : "");
                    cmd.CommandText = sql;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("The categories could not be loaded: " + ex.Message, false);
            }

            gvCategories.DataSource = dt;
            gvCategories.DataBind();

            litCount.Text = dt.Rows.Count == 1 ? "1 category" : dt.Rows.Count + " categories";
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvCategories.PageIndex = 0;
            BindCategories();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            gvCategories.PageIndex = 0;
            BindCategories();
        }

        protected void gvCategories_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvCategories.PageIndex = e.NewPageIndex;
            BindCategories();
        }

        protected void gvCategories_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (!AllowedSort.Contains(e.SortExpression)) return;

            if (SortField == e.SortExpression)
            {
                SortDir = SortDir == "ASC" ? "DESC" : "ASC";
            }
            else
            {
                SortField = e.SortExpression;
                SortDir = "ASC";
            }
            gvCategories.PageIndex = 0;
            BindCategories();
        }

        protected void gvCategories_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EditCategory" && e.CommandName != "ViewProducts") return;

            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            litMessage.Text = "";
            if (e.CommandName == "ViewProducts") ShowCategoryProducts(id);
                        else LoadForEdit(id);
        }

        // ---------- ADD ----------
        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            litMessage.Text = "";
            hfCategoryId.Value = "";
            txtName.Text = "";
            txtDescription.Text = "";
            HideProducts();
            litFormTitle.Text = "Add category";
            litFormMeta.Text = "It will appear in the shop menu straight away.";
            btnSave.Text = "Add category";
            pnlForm.Visible = true;

            ScrollTo("catFormCard", txtName.ClientID);
        }

        // ---------- EDIT ----------
        private void LoadForEdit(int id)
        {
            string descExpr = Schema.DescColumn != null ? "CAST([" + Schema.DescColumn + "] AS NVARCHAR(MAX))" : "CAST(NULL AS NVARCHAR(MAX))";

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Category_Name, " + descExpr + " AS Category_Description, " +
                    "(SELECT COUNT(*) FROM Product p WHERE p.Category_ID = @Id) AS ProductCount " +
                    "FROM Category WHERE Category_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            ShowMessage("That category could not be found.", false);
                            pnlForm.Visible = false;
                            return;
                        }

                        txtName.Text = r["Category_Name"].ToString().Trim();
                        txtDescription.Text = r["Category_Description"] == DBNull.Value ? "" : r["Category_Description"].ToString().Trim();

                        int count = Convert.ToInt32(r["ProductCount"]);
                        litFormMeta.Text = count == 1 ? "Used by 1 product" : "Used by " + count + " products";
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("The category could not be loaded: " + ex.Message, false);
                return;
            }

            hfCategoryId.Value = id.ToString();
            HideProducts();
            litFormTitle.Text = "Edit category";
            btnSave.Text = "Save changes";
            pnlForm.Visible = true;

            ScrollTo("catFormCard", txtName.ClientID);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            hfCategoryId.Value = "";
            litMessage.Text = "";
        }

        // ---------- SAVE (add or update name / description only) ----------
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = 0;
            bool isNew = !int.TryParse(hfCategoryId.Value, out id);

            string name = Regex.Replace(txtName.Text ?? "", @"\s+", " ").Trim();
            string desc = Schema.DescColumn != null ? (txtDescription.Text ?? "").Trim() : null;

            // ----- Checks -----
            string error = null;
            Control badField = txtName;

            if (name.Length == 0)
                error = "Please enter a category name.";
            else if (name.Length < 2)
                error = "The category name must be at least 2 characters.";
            else if (name.Length > Schema.NameMaxLength)
                error = "The category name can be at most " + Schema.NameMaxLength + " characters.";
            else if (!Regex.IsMatch(name, @"[A-Za-z]"))
                error = "The category name must contain at least one letter.";
            else if (name.IndexOfAny(new[] { '<', '>' }) >= 0)
                error = "The category name cannot contain < or >.";
            else if (desc != null && desc.Length > Schema.DescMaxLength)
            {
                error = "The description can be at most " + Schema.DescMaxLength + " characters (it is " + desc.Length + ").";
                badField = txtDescription;
            }
            else if (desc != null && desc.IndexOfAny(new[] { '<', '>' }) >= 0)
            {
                error = "The description cannot contain < or >.";
                badField = txtDescription;
            }

            try
            {
                if (error == null && NameTaken(name, isNew ? 0 : id))
                    error = "A category called \"" + name + "\" already exists.";

                if (error != null)
                {
                    ShowMessage(error, false);
                    pnlForm.Visible = true;
                    ScrollTo("catFormCard", badField.ClientID);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.Parameters.AddWithValue("@Name", name);

                    bool hasDesc = Schema.DescColumn != null;
                    if (hasDesc)
                    {
                        object descValue = desc.Length == 0 && Schema.DescNullable ? (object)DBNull.Value : desc;
                        cmd.Parameters.AddWithValue("@Desc", descValue);
                    }

                    if (isNew)
                    {
                        cmd.CommandText = hasDesc
                            ? "INSERT INTO Category (Category_Name, [" + Schema.DescColumn + "]) VALUES (@Name, @Desc)"
                            : "INSERT INTO Category (Category_Name) VALUES (@Name)";
                    }
                    else
                    {
                        // Only the name and description are ever changed here
                        cmd.CommandText = hasDesc
                            ? "UPDATE Category SET Category_Name = @Name, [" + Schema.DescColumn + "] = @Desc WHERE Category_ID = @Id"
                            : "UPDATE Category SET Category_Name = @Name WHERE Category_ID = @Id";
                        cmd.Parameters.AddWithValue("@Id", id);
                    }

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    if (!isNew && rows == 0)
                    {
                        ShowMessage("That category could not be found. It may have been changed by someone else.", false);
                        pnlForm.Visible = false;
                        RefreshAll();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("The category could not be saved: " + ex.Message, false);
                pnlForm.Visible = true;
                return;
            }

            pnlForm.Visible = false;
            hfCategoryId.Value = "";
            RefreshAll();
            ShowMessage(name + (isNew ? " was added." : " was updated."), true);
        }

        // True when another category already uses this name (ignoring case and extra spaces)
        private bool NameTaken(string name, int excludeId)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Category WHERE UPPER(LTRIM(RTRIM(Category_Name))) = UPPER(@Name) AND Category_ID <> @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Id", excludeId);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ---------- CATEGORY PRODUCTS (click a category to see what's in it) ----------
        // Optional Product columns: only selected when they really exist, so the page still works without them
        private static readonly string[] OptionalProductColumns =
        {
            "Product_Description", "Product_Type", "Product_Origin",
            "Product_Flavour", "Product_Ingredients", "Product_AlcoholPercentage"
        };

        /// <summary>Category whose products are showing (0 = none open).</summary>
        private int ProductsCategoryId
        {
            get { return ViewState["ProductsCategoryId"] == null ? 0 : (int)ViewState["ProductsCategoryId"]; }
            set { ViewState["ProductsCategoryId"] = value; }
        }

        private string ProductsCategoryName
        {
            get { return (string)ViewState["ProductsCategoryName"] ?? ""; }
            set { ViewState["ProductsCategoryName"] = value; }
        }

        private static int SafeInt(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            try { return Convert.ToInt32(value, CultureInfo.InvariantCulture); }
            catch { return 0; }
        }

        private static decimal SafeDecimal(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            try { return Convert.ToDecimal(value, CultureInfo.InvariantCulture); }
            catch { return 0m; }
        }

        private void HideProducts()
        {
            pnlDetail.Visible = false;
            pnlProducts.Visible = false;
            ProductsCategoryId = 0;
            ProductsCategoryName = "";
        }

        private bool TryLoadCategoryHeader(int id, out string name, out string description)
        {
            name = "";
            description = "";

            string descExpr = Schema.DescColumn != null
                ? "CAST([" + Schema.DescColumn + "] AS NVARCHAR(MAX))"
                : "CAST(NULL AS NVARCHAR(MAX))";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT Category_Name, " + descExpr + " AS Category_Description FROM Category WHERE Category_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return false;
                    name = Convert.ToString(r["Category_Name"]).Trim();
                    description = r["Category_Description"] == DBNull.Value ? "" : Convert.ToString(r["Category_Description"]).Trim();
                    return true;
                }
            }
        }

        /// <summary>
        /// All products in a category (or just one of them), including inactive ones, with the price the
        /// customer actually pays. Prices come from PricingHelper so they match the Shop page.
        /// </summary>
        private DataTable LoadCategoryProducts(int categoryId, int? onlyProductId, out bool pricingFailed)
        {
            pricingFailed = false;

            TableSchema productSchema = TableSchema.Get(ConnStr, "Product");
            var optional = OptionalProductColumns
                .Where(c => productSchema.Find(c) != null)
                .Select(c => "p." + TableSchema.Q(c))
                .ToList();
            string optionalSql = optional.Count > 0 ? ", " + string.Join(", ", optional) : "";

            string where = "p.Category_ID = @Cat";
            if (onlyProductId.HasValue) where += " AND p.Product_ID = @Pid";

            string sql = @"
                SELECT p.Product_ID, p.Product_Name, p.Product_Brand, p.Product_SizeML, p.Product_SellingPrice,
                       p.Product_QuantityInStock, p.Product_ReorderQuantity, p.Product_Status" + optionalSql + @"
                FROM Product p
                WHERE " + where + @"
                ORDER BY CASE WHEN p.Product_Status = 'Active' THEN 0 ELSE 1 END, p.Product_Name, p.Product_SizeML";

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Cat", categoryId);
                if (onlyProductId.HasValue) cmd.Parameters.AddWithValue("@Pid", onlyProductId.Value);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }

            // Every optional column exists from here on, so the rest of the code never has to check
            foreach (string c in OptionalProductColumns)
            {
                if (!dt.Columns.Contains(c)) dt.Columns.Add(c, typeof(string));
            }

            dt.Columns.Add("DiscountedPrice", typeof(decimal));
            dt.Columns.Add("IsOnSale", typeof(bool));
            dt.Columns.Add("DiscountPercent", typeof(int));

            // If discounts can't be read, show shelf prices and tell the manager instead of failing the whole view
            var prices = new Dictionary<int, PricingHelper.PriceInfo>();
            try
            {
                prices = PricingHelper.GetPrices(ConnStr, dt.Rows.Cast<DataRow>().Select(x => SafeInt(x["Product_ID"])));
            }
            catch
            {
                pricingFailed = true;
            }

            foreach (DataRow row in dt.Rows)
            {
                decimal shelfPrice = SafeDecimal(row["Product_SellingPrice"]);
                decimal finalPrice = shelfPrice;

                PricingHelper.PriceInfo info;
                if (prices.TryGetValue(SafeInt(row["Product_ID"]), out info)) finalPrice = info.FinalPrice;

                bool onSale = finalPrice < shelfPrice;
                row["DiscountedPrice"] = finalPrice;
                row["IsOnSale"] = onSale;
                row["DiscountPercent"] = (onSale && shelfPrice > 0m)
                    ? Math.Max(1, (int)Math.Round((shelfPrice - finalPrice) / shelfPrice * 100m, MidpointRounding.AwayFromZero))
                    : 0;
            }

            return dt;
        }

        private void ShowCategoryProducts(int categoryId)
        {
            // Only one panel open at a time
            pnlForm.Visible = false;
            hfCategoryId.Value = "";
            pnlDetail.Visible = false;

            if (categoryId <= 0)
            {
                HideProducts();
                ShowMessage("That category could not be found.", false);
                return;
            }

            string categoryName, categoryDesc;
            DataTable products;
            bool pricingFailed;

            try
            {
                if (!TryLoadCategoryHeader(categoryId, out categoryName, out categoryDesc))
                {
                    HideProducts();
                    ShowMessage("That category could not be found. It may have been changed by someone else.", false);
                    RefreshAll();
                    return;
                }

                products = LoadCategoryProducts(categoryId, null, out pricingFailed);
            }
            catch (Exception ex)
            {
                HideProducts();
                ShowMessage("The products for this category could not be loaded: " + ex.Message, false);
                return;
            }

            int active = 0, inactive = 0, low = 0, outOfStock = 0;
            foreach (DataRow r in products.Rows)
            {
                if (IsActiveProduct(r["Product_Status"])) active++; else inactive++;

                int stock = SafeInt(r["Product_QuantityInStock"]);
                if (stock <= 0) outOfStock++;
                else if (stock <= SafeInt(r["Product_ReorderQuantity"])) low++;
            }

            ProductsCategoryId = categoryId;
            ProductsCategoryName = categoryName;

            litProdTitle.Text = Server.HtmlEncode(categoryName);
            litProdDesc.Text = categoryDesc.Length > 0
                ? "<p class=\"field-hint disc-note\">" + Server.HtmlEncode(categoryDesc) + "</p>"
                : "";
            litProdTotal.Text = products.Rows.Count.ToString("N0");
            litProdActive.Text = active.ToString("N0");
            litProdInactive.Text = inactive.ToString("N0");
            litProdLow.Text = low.ToString("N0");
            litProdOut.Text = outOfStock.ToString("N0");

            string note = "";
            if (inactive == 1) note = "1 product is inactive, so customers can't see it in the shop.";
            else if (inactive > 1) note = inactive + " products are inactive, so customers can't see them in the shop.";
            if (pricingFailed) note += (note.Length > 0 ? " " : "") + "Discounts couldn't be checked just now, so shelf prices are shown.";
            litProdNote.Text = note.Length > 0 ? "<p class=\"field-hint disc-note\">" + Server.HtmlEncode(note) + "</p>" : "";

            try
            {
                phProdEmpty.Visible = products.Rows.Count == 0;
                rptCatProducts.Visible = products.Rows.Count > 0;
                rptCatProducts.DataSource = products;
                rptCatProducts.DataBind();
            }
            catch (Exception ex)
            {
                HideProducts();
                ShowMessage("The products could not be displayed: " + ex.Message, false);
                return;
            }

            pnlProducts.Visible = true;
            ScrollToTop();
        }

        protected void rptCatProducts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "ViewProduct") return;

            int productId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out productId) || productId <= 0)
            {
                ShowMessage("That product could not be found.", false);
                ScrollToTop();
                return;
            }

            litMessage.Text = "";
            ShowProductDetail(productId);
        }

        private void ShowProductDetail(int productId)
        {
            int categoryId = ProductsCategoryId;
            if (categoryId <= 0)
            {
                pnlDetail.Visible = false;
                ShowMessage("Open a category first to see its products.", false);
                ScrollToTop();
                return;
            }

            try
            {
                bool pricingFailed;
                DataTable t = LoadCategoryProducts(categoryId, productId, out pricingFailed);

                if (t.Rows.Count == 0)
                {
                    // Removed or moved since the list was drawn: refresh the list so it matches the database
                    ShowCategoryProducts(categoryId);
                    ShowMessage("That product could not be found in this category. It may have been changed by someone else.", false);
                    return;
                }

                DataRow r = t.Rows[0];
                string name = Convert.ToString(r["Product_Name"]).Trim();
                bool onSale = (bool)r["IsOnSale"];
                int stock = SafeInt(r["Product_QuantityInStock"]);
                int reorder = SafeInt(r["Product_ReorderQuantity"]);

                imgDetail.Src = "/ProductImage.ashx?id=" + productId;
                imgDetail.Alt = name;

                litDetailBrand.Text = Convert.ToString(r["Product_Brand"]).Trim();
                litDetailName.Text = name;
                litDetailCategory.Text = ProductsCategoryName;

                spnDetailBadge.Visible = onSale;
                spnDetailBadge.InnerText = "-" + r["DiscountPercent"] + "%";
                spnDetailOld.Visible = onSale;
                spnDetailOld.InnerText = Money(r["Product_SellingPrice"]);
                spnDetailPrice.InnerText = Money(r["DiscountedPrice"]);
                spnDetailPrice.Attributes["class"] = onSale ? "shop-card-price is-sale" : "shop-card-price";

                string desc = Convert.ToString(r["Product_Description"]);
                litDetailDesc.Text = string.IsNullOrWhiteSpace(desc) ? "No description available yet." : desc.Trim();

                var facts = new StringBuilder();
                Action<string, string> add = (label, value) =>
                {
                    if (!string.IsNullOrWhiteSpace(value))
                        facts.Append("<div><dt>").Append(Server.HtmlEncode(label)).Append("</dt><dd>")
                             .Append(Server.HtmlEncode(value.Trim())).Append("</dd></div>");
                };

                decimal abv = SafeDecimal(r["Product_AlcoholPercentage"]);
                int sizeMl = SafeInt(r["Product_SizeML"]);

                add("Type", Convert.ToString(r["Product_Type"]));
                add("Origin", Convert.ToString(r["Product_Origin"]));
                add("Flavour", Convert.ToString(r["Product_Flavour"]));
                add("Alcohol", abv > 0m ? abv.ToString("0.##", CultureInfo.InvariantCulture) + "%" : null);
                add("Size", sizeMl > 0 ? sizeMl.ToString("N0") + " ml" : null);
                add("In stock", stock.ToString("N0"));
                add("Reorder level", reorder.ToString("N0"));
                add("Status", Convert.ToString(r["Product_Status"]));
                add("Price note", pricingFailed ? "Discounts couldn't be checked, so the shelf price is shown." : null);
                litDetailFacts.Text = facts.Length > 0 ? "<dl class=\"pd-facts\">" + facts + "</dl>" : "";

                string ingredients = Convert.ToString(r["Product_Ingredients"]);
                litDetailIngredients.Text = string.IsNullOrWhiteSpace(ingredients)
                    ? ""
                    : "<p class=\"pd-ingredients\"><strong>Ingredients</strong> " + Server.HtmlEncode(ingredients.Trim()) + "</p>";

                pnlDetail.Visible = true;
                KeepScrollPosition();
            }
            catch (Exception ex)
            {
                pnlDetail.Visible = false;
                ShowMessage("The product details could not be loaded: " + ex.Message, false);
                ScrollToTop();
            }
        }

        protected void btnCloseProducts_Click(object sender, EventArgs e)
        {
            HideProducts();
            ScrollToTop();
        }

        protected void btnCloseDetail_Click(object sender, EventArgs e)
        {
            pnlDetail.Visible = false;
            KeepScrollPosition();
        }

        // Used by the product cards in the markup
        protected string Money(object value)
        {
            return "R" + SafeDecimal(value).ToString("N2");
        }

        protected bool IsActiveProduct(object status)
        {
            return string.Equals(Convert.ToString(status).Trim(), "Active", StringComparison.OrdinalIgnoreCase);
        }

        protected string StockBadge(object stock, object reorderLevel)
        {
            int qty = SafeInt(stock);
            if (qty <= 0) return "<span class=\"emp-badge emp-badge-inactive\">Out of stock</span>";
            if (qty <= SafeInt(reorderLevel)) return "<span class=\"emp-badge emp-badge-low\">Low stock: " + qty.ToString("N0") + " left</span>";
            return "<span class=\"emp-badge emp-badge-active\">" + qty.ToString("N0") + " in stock</span>";
        }

        protected string StatusBadge(object status)
        {
            string text = Convert.ToString(status).Trim();
            if (text.Length == 0) text = "Unknown";
            string css = IsActiveProduct(status) ? "emp-badge-active" : "emp-badge-inactive";
            return "<span class=\"emp-badge " + css + "\">" + Server.HtmlEncode(text) + "</span>";
        }

        protected string SizeBadge(object size)
        {
            int ml = SafeInt(size);
            return ml > 0 ? "<span class=\"emp-sub\">" + ml.ToString("N0") + " ml</span>" : "";
        }

        // ---------- DISPLAY HELPERS ----------
        protected string DescriptionText(object value)
        {
            string text = value == null || value == DBNull.Value ? "" : value.ToString().Trim();
            if (text.Length == 0) return "<span class=\"emp-sub\">No description</span>";
            if (text.Length > 140) text = text.Substring(0, 137).TrimEnd() + "...";
            return HttpUtility.HtmlEncode(text);
        }

        private void ShowMessage(string text, bool success)
        {
            litMessage.Text = "<div class=\"emp-alert " + (success ? "emp-alert-success" : "emp-alert-error") + "\">"
                + Server.HtmlEncode(text) + "</div>";
        }

        /// <summary>Goes to the top of the page (used when a different category's products open).</summary>
        private void ScrollToTop()
        {
            RegisterScroll("scrollTop", "if (window.dashScroll) { dashScroll('top'); }");
        }

        /// <summary>Stays where the page is (used when the product pop-up opens or closes).</summary>
        private void KeepScrollPosition()
        {
            RegisterScroll("scrollKeep", "if (window.dashScroll) { dashScroll('keep'); }");
        }

        private void RegisterScroll(string key, string js)
        {
            try
            {
                ScriptManager.RegisterStartupScript(this, GetType(), key, js, true);
            }
            catch
            {
                // Scrolling is cosmetic: never let it break the page
            }
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
