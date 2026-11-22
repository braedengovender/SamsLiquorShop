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
    /// Manager page for suppliers.
    /// - View suppliers and the products each one supplies (Product.Supplier_ID)
    /// - Add a supplier (address search uses the same OpenStreetMap lookup as Register.aspx)
    /// - Edit a supplier's details
    /// - Deactivate / reactivate (status change)
    /// Deleting suppliers is deliberately not offered.
    ///
    /// The Supplier table's columns are read from the database, so the page works with the
    /// existing column names (e.g. Supplier_Phone or Supplier_PhoneNumber). Run
    /// Database/AddSupplierColumns.sql once to add any contact/address columns that are missing.
    /// </summary>
    public partial class Suppliers : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        // ---------- FIELDS THE PAGE KNOWS ABOUT ----------
        // Each field lists the column names it may have in the Supplier table (first match wins).
        private class Field
        {
            public string Key, Label;
            public string[] Names;
            public bool Required;
            public int Fallback;   // length limit when the column is (max)
        }

        private static readonly Field[] Fields =
        {
            new Field { Key = "ContactPerson", Label = "Contact person", Fallback = 100, Names = new[] { "Supplier_ContactPerson", "Supplier_ContactName", "Supplier_Contact" } },
            new Field { Key = "Phone", Label = "Phone number", Required = true, Fallback = 15, Names = new[] { "Supplier_PhoneNumber", "Supplier_Phone", "Supplier_ContactNumber", "Supplier_TelephoneNumber", "Supplier_Telephone", "Supplier_CellNumber", "Supplier_Tel" } },
            new Field { Key = "Email", Label = "Email address", Required = true, Fallback = 100, Names = new[] { "Supplier_EmailAddress", "Supplier_Email" } },
            new Field { Key = "UnitNumber", Label = "Unit number", Fallback = 20, Names = new[] { "Supplier_UnitNumber" } },
            new Field { Key = "UnitName", Label = "Unit / complex name", Fallback = 100, Names = new[] { "Supplier_UnitName" } },
            new Field { Key = "StreetNumber", Label = "Street number", Required = true, Fallback = 20, Names = new[] { "Supplier_StreetNumber" } },
            new Field { Key = "StreetName", Label = "Street name", Required = true, Fallback = 100, Names = new[] { "Supplier_StreetName" } },
            new Field { Key = "Suburb", Label = "Suburb", Required = true, Fallback = 100, Names = new[] { "Supplier_Suburb" } },
            new Field { Key = "City", Label = "City", Required = true, Fallback = 100, Names = new[] { "Supplier_City" } },
            new Field { Key = "PostalCode", Label = "Postal code", Required = true, Fallback = 4, Names = new[] { "Supplier_PostalCode", "Supplier_PostCode", "Supplier_ZipCode" } },
            new Field { Key = "Province", Label = "Province", Required = true, Fallback = 30, Names = new[] { "Supplier_Province" } },
            // Older single-column address: kept in step with the address fields when both exist
            new Field { Key = "Address", Label = "Address", Fallback = 255, Names = new[] { "Supplier_Address", "Supplier_PhysicalAddress", "Supplier_StreetAddress" } },
            new Field { Key = "Status", Label = "Status", Fallback = 20, Names = new[] { "Supplier_Status" } }
        };

        private static readonly string[] AddressPartKeys = { "UnitNumber", "UnitName", "StreetNumber", "StreetName", "Suburb", "City", "PostalCode", "Province" };

        private static readonly HashSet<string> AllowedSort = new HashSet<string> { "Supplier_Name", "ProductCount", "StatusSort" };

        private TableSchema Schema => TableSchema.Get(ConnStr, "Supplier");

        private TableSchema.Column Col(string key)
        {
            Field f = Fields.First(x => x.Key == key);
            return Schema.FindAny(f.Names);
        }

        private bool Has(string key) => Col(key) != null;

        protected bool HasStatusColumn => Has("Status");

        private bool HasAddressParts => AddressPartKeys.Any(k => Has(k));

        private TableSchema.Column StatusCol => Col("Status");
        private object ActiveValue => StatusCol != null && StatusCol.IsNumericFlag ? (object)1 : "Active";
        private object InactiveValue => StatusCol != null && StatusCol.IsNumericFlag ? (object)0 : "Inactive";

        /// <summary>Form control for each field.</summary>
        private Dictionary<string, WebControl> Inputs => new Dictionary<string, WebControl>
        {
            { "ContactPerson", txtContact }, { "Phone", txtPhone }, { "Email", txtEmail },
            { "UnitNumber", supUnitNumber }, { "UnitName", supUnitName }, { "StreetNumber", supStreetNumber },
            { "StreetName", supStreetName }, { "Suburb", supSuburb }, { "City", supCity },
            { "PostalCode", supPostalCode }, { "Province", supProvince }, { "Address", supAddressLine }
        };

        /// <summary>Wrapper shown/hidden for each field depending on whether its column exists.</summary>
        private Dictionary<string, PlaceHolder> Wrappers => new Dictionary<string, PlaceHolder>
        {
            { "ContactPerson", phContact }, { "Phone", phPhone }, { "Email", phEmail },
            { "UnitNumber", phUnitNumber }, { "UnitName", phUnitName }, { "StreetNumber", phStreetNumber },
            { "StreetName", phStreetName }, { "Suburb", phSuburb }, { "City", phCity },
            { "PostalCode", phPostalCode }, { "Province", phProvince }, { "Address", phAddressLine }
        };

        private string SortField
        {
            get { return (string)ViewState["SortField"] ?? "Supplier_Name"; }
            set { ViewState["SortField"] = value; }
        }

        private string SortDir
        {
            get { return (string)ViewState["SortDir"] ?? "ASC"; }
            set { ViewState["SortDir"] = value; }
        }

        private int ProductsSupplierId
        {
            get { return ViewState["ProductsSupplierId"] == null ? 0 : (int)ViewState["ProductsSupplierId"]; }
            set { ViewState["ProductsSupplierId"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Manager")) return;

            txtSearch.Attributes["data-live-search"] = btnSearch.ClientID;

            try
            {
                if (Schema.Find("Supplier_ID") == null || Schema.Find("Supplier_Name") == null)
                {
                    ShowMessage("The Supplier table needs Supplier_ID and Supplier_Name columns.", false);
                    btnAddNew.Visible = false;
                    return;
                }
                ConfigureForm();
            }
            catch (Exception ex)
            {
                ShowMessage("The supplier table could not be read: " + ex.Message, false);
                btnAddNew.Visible = false;
                return;
            }

            if (!IsPostBack)
            {
                RefreshAll();

                int viewId;
                if (int.TryParse(Request.QueryString["view"], out viewId)) ShowProducts(viewId);
            }
        }

        /// <summary>Shows only the fields the Supplier table has, and sets their length limits.</summary>
        private void ConfigureForm()
        {
            txtName.MaxLength = Schema.Find("Supplier_Name").TextLimit(100);

            foreach (var pair in Wrappers)
            {
                TableSchema.Column c = Col(pair.Key);
                bool show = c != null;
                // The single address line is only needed when the table has no separate address columns
                if (pair.Key == "Address") show = c != null && !HasAddressParts;
                pair.Value.Visible = show;

                TextBox tb = Inputs[pair.Key] as TextBox;
                if (tb != null && c != null && c.IsText)
                {
                    int limit = c.TextLimit(Fields.First(f => f.Key == pair.Key).Fallback);
                    // Postal codes and phone numbers keep their own tighter limits
                    if (pair.Key == "PostalCode") limit = Math.Min(limit, 4);
                    if (pair.Key == "Phone") limit = Math.Min(limit, 12);
                    tb.MaxLength = limit;
                }
            }
            phAddressSection.Visible = HasAddressParts || Has("Address");

            // Tell the manager which columns are missing so they know to run the script
            var missing = Fields.Where(f => f.Key != "Address" && !Has(f.Key)).Select(f => f.Label).ToList();
            litSchemaNote.Text = missing.Count == 0 ? "" :
                "<div class=\"emp-alert emp-alert-error\">The Supplier table doesn't have columns for: "
                + Server.HtmlEncode(string.Join(", ", missing))
                + ". Run Database/AddSupplierColumns.sql once to add them; until then those fields are hidden.</div>";
        }

        private void RefreshAll()
        {
            BindSuppliers();
        }

        // ---------- SELECT HELPERS ----------
        /// <summary>SELECT list giving every field a fixed alias (F_Phone, F_City, ...), NULL when the column doesn't exist.</summary>
        private string FieldSelects(string alias)
        {
            return string.Join(", ", Fields.Select(f =>
            {
                TableSchema.Column c = Col(f.Key);
                string expr = c == null ? "CAST(NULL AS NVARCHAR(MAX))" : "CAST(" + alias + TableSchema.Q(c.Name) + " AS NVARCHAR(MAX))";
                return expr + " AS F_" + f.Key;
            }));
        }

        private static bool IsActiveValueOn(object value)
        {
            if (value == null || value == DBNull.Value) return true; // no status recorded = treated as active
            string s = value.ToString().Trim();
            return s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("Active", StringComparison.OrdinalIgnoreCase);
        }

        // ---------- LIST ----------
        private DataTable LoadSuppliers(string search)
        {
            string statusExpr = HasStatusColumn ? "s." + TableSchema.Q(StatusCol.Name) : "NULL";
            string activeTest = HasStatusColumn
                ? (StatusCol.IsNumericFlag ? statusExpr + " = 1" : "LTRIM(RTRIM(CAST(" + statusExpr + " AS NVARCHAR(50)))) = 'Active'")
                : "1 = 1";

            string sql = @"
                SELECT s.Supplier_ID, s.Supplier_Name, " + FieldSelects("s.") + @",
                       CASE WHEN " + statusExpr + " IS NULL OR " + activeTest + @" THEN 0 ELSE 1 END AS StatusSort,
                       (SELECT COUNT(*) FROM Product p WHERE p.Supplier_ID = s.Supplier_ID) AS ProductCount,
                       (SELECT COUNT(*) FROM Product p WHERE p.Supplier_ID = s.Supplier_ID AND p.Product_Status = 'Active') AS ActiveProductCount
                FROM Supplier s";

            var where = new List<string>();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;

                search = (search ?? "").Trim();
                if (search.Length > 0)
                {
                    var parts = new List<string> { "s.Supplier_Name LIKE @Search" };
                    foreach (string key in new[] { "ContactPerson", "Phone", "Email", "City", "Suburb", "Address" })
                    {
                        TableSchema.Column c = Col(key);
                        if (c != null) parts.Add("CAST(s." + TableSchema.Q(c.Name) + " AS NVARCHAR(MAX)) LIKE @Search");
                    }
                    where.Add("(" + string.Join(" OR ", parts) + ")");
                    cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                }

                if (where.Count > 0) sql += " WHERE " + string.Join(" AND ", where);

                string sort = AllowedSort.Contains(SortField) ? SortField : "Supplier_Name";
                string dir = SortDir == "DESC" ? "DESC" : "ASC";
                sql += " ORDER BY " + sort + " " + dir + (sort != "Supplier_Name" ? ", Supplier_Name ASC" : "");
                cmd.CommandText = sql;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }
            return dt;
        }

        private void BindSuppliers()
        {
            DataTable all, everything;
            try
            {
                string search = txtSearch.Text.Trim();
                all = LoadSuppliers(search);
                everything = search.Length == 0 ? all : LoadSuppliers("");
            }
            catch (Exception ex)
            {
                ShowMessage("The suppliers could not be loaded: " + ex.Message, false);
                return;
            }

            // Summary cards always describe every supplier
            int active = everything.Rows.Cast<DataRow>().Count(r => IsActiveValueOn(r["F_Status"]));
            int products = everything.Rows.Cast<DataRow>().Sum(r => Convert.ToInt32(r["ProductCount"]));
            litKpiTotal.Text = everything.Rows.Count.ToString("N0");
            litKpiActive.Text = active.ToString("N0");
            litKpiInactive.Text = (everything.Rows.Count - active).ToString("N0");
            litKpiProducts.Text = products.ToString("N0");

            DataTable shown = all;
            string filter = ddlStatusFilter.SelectedValue;
            if (filter.Length > 0)
            {
                shown = all.Clone();
                foreach (DataRow r in all.Rows)
                    if (StatusClass(r["F_Status"]) == filter) shown.ImportRow(r);
            }

            gvSuppliers.DataSource = shown;
            gvSuppliers.DataBind();
            litCount.Text = shown.Rows.Count == 1 ? "1 supplier" : shown.Rows.Count + " suppliers";
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvSuppliers.PageIndex = 0;
            BindSuppliers();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlStatusFilter.SelectedIndex = 0;
            gvSuppliers.PageIndex = 0;
            BindSuppliers();
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            gvSuppliers.PageIndex = 0;
            BindSuppliers();
        }

        protected void gvSuppliers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvSuppliers.PageIndex = e.NewPageIndex;
            BindSuppliers();
        }

        protected void gvSuppliers_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (!AllowedSort.Contains(e.SortExpression)) return;

            if (SortField == e.SortExpression)
            {
                SortDir = SortDir == "ASC" ? "DESC" : "ASC";
            }
            else
            {
                SortField = e.SortExpression;
                SortDir = e.SortExpression == "ProductCount" ? "DESC" : "ASC";
            }
            gvSuppliers.PageIndex = 0;
            BindSuppliers();
        }

        protected void gvSuppliers_RowDataBound(object sender, GridViewRowEventArgs e)
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

        protected void gvSuppliers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "ViewProducts" && e.CommandName != "EditSupplier" && e.CommandName != "ToggleStatus") return;

            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;
            litMessage.Text = "";

            switch (e.CommandName)
            {
                case "ViewProducts": ShowProducts(id); break;
                case "EditSupplier": LoadForEdit(id); break;
                case "ToggleStatus": ToggleStatus(id); break;
            }
        }

        private DataRow LoadSupplier(int id)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT s.Supplier_ID, s.Supplier_Name, " + FieldSelects("s.") + " FROM Supplier s WHERE s.Supplier_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
            }
            return dt.Rows.Count == 0 ? null : dt.Rows[0];
        }

        // ---------- VIEW PRODUCTS (click a supplier to see what they supply) ----------
        // Optional Product columns: only selected when they really exist, so the page still works without them
        private static readonly string[] OptionalProductColumns =
        {
            "Product_Description", "Product_Type", "Product_Origin",
            "Product_Flavour", "Product_Ingredients", "Product_AlcoholPercentage"
        };

        // Stricter than the save check: only plain addresses become clickable mailto: links
        private static readonly Regex LinkableEmail = new Regex(@"^[A-Za-z0-9._%+\-']+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$", RegexOptions.Compiled);

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
            ProductsSupplierId = 0;
        }

        /// <summary>Closes the product view, shows the message and scrolls up so it is seen.</summary>
        private void FailProducts(string message)
        {
            HideProducts();
            ShowMessage(message, false);
            ScrollToTop();
        }

        /// <summary>
        /// Every product from a supplier (or just one of them), including inactive ones, with the price the
        /// customer actually pays. Prices come from PricingHelper so they match the Shop page.
        /// </summary>
        private DataTable LoadSupplierProducts(int supplierId, int? onlyProductId, out bool pricingFailed)
        {
            pricingFailed = false;

            TableSchema productSchema = TableSchema.Get(ConnStr, "Product");
            var optional = OptionalProductColumns
                .Where(c => productSchema.Find(c) != null)
                .Select(c => "p." + TableSchema.Q(c))
                .ToList();
            string optionalSql = optional.Count > 0 ? ", " + string.Join(", ", optional) : "";

            string where = "p.Supplier_ID = @Sup";
            if (onlyProductId.HasValue) where += " AND p.Product_ID = @Pid";

            string sql = @"
                SELECT p.Product_ID, p.Product_Name, ISNULL(p.Product_Brand, '') AS Product_Brand, p.Product_SizeML,
                       ISNULL(c.Category_Name, '') AS Category_Name, p.Product_CostPrice, p.Product_SellingPrice,
                       p.Product_QuantityInStock, p.Product_ReorderQuantity, p.Product_Status" + optionalSql + @"
                FROM Product p
                LEFT JOIN Category c ON c.Category_ID = p.Category_ID
                WHERE " + where + @"
                ORDER BY CASE WHEN p.Product_Status = 'Active' THEN 0 ELSE 1 END, p.Product_Name, p.Product_SizeML";

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Sup", supplierId);
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

        private void ShowProducts(int id)
        {
            // Only one panel open at a time
            pnlForm.Visible = false;
            hfSupplierId.Value = "";
            pnlDetail.Visible = false;

            if (id <= 0)
            {
                FailProducts("That supplier could not be found.");
                return;
            }

            DataRow s;
            DataTable products;
            bool pricingFailed;

            try
            {
                s = LoadSupplier(id);
                if (s == null)
                {
                    FailProducts("That supplier could not be found. It may have been changed by someone else.");
                    RefreshAll();
                    return;
                }

                products = LoadSupplierProducts(id, null, out pricingFailed);
            }
            catch (Exception ex)
            {
                FailProducts("The products for this supplier could not be loaded: " + ex.Message);
                return;
            }

            int low = 0, inactive = 0;
            decimal value = 0m;
            foreach (DataRow r in products.Rows)
            {
                if (!IsActiveProduct(r["Product_Status"])) { inactive++; continue; }

                int stock = SafeInt(r["Product_QuantityInStock"]);
                if (stock <= SafeInt(r["Product_ReorderQuantity"])) low++;
                if (stock > 0) value += stock * SafeDecimal(r["Product_CostPrice"]);
            }

            ProductsSupplierId = id;
            litProdTitle.Text = Server.HtmlEncode(Convert.ToString(s["Supplier_Name"]).Trim());
            litProdContactCards.Text = ContactCards(s);
            litProdCount.Text = products.Rows.Count.ToString("N0");
            litProdLow.Text = low.ToString("N0");
            litProdValue.Text = value.ToString("C");

            string statusClass = StatusClass(s["F_Status"]);
            litProdStatus.Text = "<span class=\"emp-badge emp-badge-" + statusClass + "\">"
                + (statusClass == "active" ? "Active" : "Inactive") + "</span>";

            var hints = new List<string>();
            if (inactive == 1) hints.Add("1 product is inactive, so customers can't see it in the shop.");
            else if (inactive > 1) hints.Add(inactive + " products are inactive, so customers can't see them in the shop.");
            if (pricingFailed) hints.Add("Discounts couldn't be checked just now, so shelf prices are shown.");
            litProdAddress.Text = string.Concat(hints.Select(h => "<p class=\"field-hint disc-note\">" + Server.HtmlEncode(h) + "</p>"));

            try
            {
                phProdEmpty.Visible = products.Rows.Count == 0;
                rptSupProducts.Visible = products.Rows.Count > 0;
                rptSupProducts.DataSource = products;
                rptSupProducts.DataBind();
            }
            catch (Exception ex)
            {
                FailProducts("The products could not be displayed: " + ex.Message);
                return;
            }

            pnlProducts.Visible = true;
            ScrollToTop();
        }

        protected void rptSupProducts_ItemCommand(object source, RepeaterCommandEventArgs e)
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
            int supplierId = ProductsSupplierId;
            if (supplierId <= 0)
            {
                pnlDetail.Visible = false;
                ShowMessage("Open a supplier first to see their products.", false);
                ScrollToTop();
                return;
            }

            try
            {
                bool pricingFailed;
                DataTable t = LoadSupplierProducts(supplierId, productId, out pricingFailed);

                if (t.Rows.Count == 0)
                {
                    // Removed or moved to another supplier since the list was drawn: refresh the list to match the database
                    ShowProducts(supplierId);
                    ShowMessage("That product could not be found for this supplier. It may have been changed by someone else.", false);
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
                litDetailCategory.Text = Convert.ToString(r["Category_Name"]).Trim();

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
                add("Cost price", Money(r["Product_CostPrice"]));
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
        }

        protected void btnCloseDetail_Click(object sender, EventArgs e)
        {
            pnlDetail.Visible = false;
            KeepScrollPosition();
        }

        // ---------- CONTACT DETAILS (click to call / email) ----------
        // A numeric phone column drops the leading 0, so put it back; keeps digits and a leading + only
        private static string PhoneDigits(string phone)
        {
            string d = Regex.Replace(phone ?? "", @"(?!^\+)[^0-9]", "");
            if (d.Length == 9 && d[0] != '0') d = "0" + d;
            return d;
        }

        /// <summary>Number for a tel: link (South African numbers become +27...). Empty when it doesn't look like a phone number.</summary>
        private static string PhoneHref(string phone)
        {
            string d = PhoneDigits(phone);
            if (Regex.IsMatch(d, @"^0[1-9][0-9]{8}$")) return "+27" + d.Substring(1);
            if (Regex.IsMatch(d, @"^\+27[1-9][0-9]{8}$")) return d;
            if (Regex.IsMatch(d, @"^\+?[0-9]{7,15}$")) return d;
            return "";
        }

        private static string PhoneDisplay(string phone)
        {
            string d = PhoneDigits(phone);
            if (Regex.IsMatch(d, @"^0[1-9][0-9]{8}$")) return d.Substring(0, 3) + " " + d.Substring(3, 3) + " " + d.Substring(6);
            if (Regex.IsMatch(d, @"^\+27[1-9][0-9]{8}$")) return "+27 " + d.Substring(3, 2) + " " + d.Substring(5, 3) + " " + d.Substring(8);
            return Clean(phone);
        }

        /// <summary>
        /// The contact row above the product cards: four cards (contact person, call, email, address)
        /// built the same way as the Products / Status cards.
        /// </summary>
        private string ContactCards(DataRow s)
        {
            try
            {
                string person = Text(s, "ContactPerson");
                string phone = Text(s, "Phone");
                string email = Text(s, "Email");
                string address = FullAddress(s);

                string callHtml = "";
                if (phone.Length > 0)
                {
                    string href = PhoneHref(phone);
                    string shown = Server.HtmlEncode(PhoneDisplay(phone));
                    callHtml = href.Length > 0
                        ? "<a class=\"sup-link\" href=\"tel:" + HttpUtility.HtmlAttributeEncode(href) + "\" title=\"Call this supplier\">" + shown + "</a>"
                        : shown;
                }

                string emailHtml = "";
                if (email.Length > 0)
                {
                    string shown = Server.HtmlEncode(email);
                    emailHtml = LinkableEmail.IsMatch(email)
                        ? "<a class=\"sup-link\" href=\"mailto:" + HttpUtility.HtmlAttributeEncode(email) + "\" title=\"Email this supplier\">" + shown + "</a>"
                        : shown;
                }

                return "<div class=\"cust-history-stats\">"
                    + ContactCard("Contact person", person.Length > 0 ? Server.HtmlEncode(person) : "")
                    + ContactCard("Call", callHtml)
                    + ContactCard("Email", emailHtml)
                    + ContactCard("Address", address.Length > 0 ? Server.HtmlEncode(address) : "")
                    + "</div>";
            }
            catch (Exception)
            {
                return "<p class=\"field-hint disc-note\">The contact details could not be displayed. Use Edit to check them.</p>";
            }
        }

        /// <summary>One contact card; valueHtml must already be encoded. Empty values show "Not recorded".</summary>
        private static string ContactCard(string label, string valueHtml)
        {
            return "<div><span class=\"mgr-kpi-label\">" + label + "</span><strong style=\"overflow-wrap:anywhere;\">"
                + (string.IsNullOrEmpty(valueHtml) ? "<span class=\"emp-sub\">Not recorded</span>" : valueHtml)
                + "</strong></div>";
        }

        /// <summary>Phone and email for the supplier list, as links.</summary>
        protected string ContactLinks(object phone, object email)
        {
            string p = phone == null || phone == DBNull.Value ? "" : Clean(Convert.ToString(phone));
            string m = email == null || email == DBNull.Value ? "" : Clean(Convert.ToString(email));

            var sb = new StringBuilder();

            if (p.Length > 0)
            {
                string href = PhoneHref(p);
                string shown = Server.HtmlEncode(PhoneDisplay(p));
                sb.Append(href.Length > 0
                    ? "<a class=\"sup-link\" href=\"tel:" + HttpUtility.HtmlAttributeEncode(href) + "\">" + shown + "</a>"
                    : shown);
            }

            if (m.Length > 0)
            {
                string shown = Server.HtmlEncode(m);
                sb.Append("<span class=\"emp-sub\">")
                  .Append(LinkableEmail.IsMatch(m)
                      ? "<a class=\"sup-link\" href=\"mailto:" + HttpUtility.HtmlAttributeEncode(m) + "\">" + shown + "</a>"
                      : shown)
                  .Append("</span>");
            }

            return sb.Length == 0 ? "<span class=\"emp-sub\">-</span>" : sb.ToString();
        }

        // ---------- PRODUCT CARD HELPERS (used by the markup) ----------
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

        protected string CardSub(object size, object category)
        {
            int ml = SafeInt(size);
            string cat = Convert.ToString(category).Trim();
            string text = ml > 0 && cat.Length > 0 ? ml.ToString("N0") + " ml · " + cat
                        : ml > 0 ? ml.ToString("N0") + " ml"
                        : cat;
            return text.Length == 0 ? "" : "<span class=\"emp-sub\">" + Server.HtmlEncode(text) + "</span>";
        }

        // ---------- ADD ----------
        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            litMessage.Text = "";
            pnlProducts.Visible = false;
            hfSupplierId.Value = "";
            ClearForm();

            HideProducts();
            litFormTitle.Text = "Add supplier";
            litFormMeta.Text = HasStatusColumn ? "Starts as Active" : "";
            btnSave.Text = "Add supplier";
            pnlForm.Visible = true;

            ScrollTo("supFormCard", txtName.ClientID);
        }

        private void ClearForm()
        {
            txtName.Text = "";
            foreach (WebControl c in Inputs.Values)
            {
                TextBox tb = c as TextBox;
                if (tb != null) tb.Text = "";
            }
            supProvince.SelectedIndex = 0;
        }

        // ---------- EDIT ----------
        private void LoadForEdit(int id)
        {
            pnlProducts.Visible = false;

            DataRow s;
            try
            {
                s = LoadSupplier(id);
            }
            catch (Exception ex)
            {
                ShowMessage("The supplier could not be loaded: " + ex.Message, false);
                return;
            }
            if (s == null)
            {
                ShowMessage("That supplier could not be found.", false);
                pnlForm.Visible = false;
                return;
            }

            ClearForm();
            txtName.Text = Convert.ToString(s["Supplier_Name"]).Trim();
            foreach (var pair in Inputs)
            {
                string v = Text(s, pair.Key);
                if (pair.Key == "PostalCode" && Regex.IsMatch(v, @"^\d{1,3}$")) v = v.PadLeft(4, '0'); // numeric column dropped a leading 0
                if (pair.Key == "Province")
                {
                    ListItem item = supProvince.Items.Cast<ListItem>()
                        .FirstOrDefault(i => i.Value.Length > 0 && i.Value.Equals(v, StringComparison.OrdinalIgnoreCase));
                    if (item == null && v.Length > 0)
                    {
                        // Keep an unexpected stored value selectable rather than losing it
                        item = new ListItem(v, v);
                        supProvince.Items.Add(item);
                    }
                    supProvince.SelectedIndex = item == null ? 0 : supProvince.Items.IndexOf(item);
                }
                else
                {
                    ((TextBox)pair.Value).Text = v;
                }
            }

            hfSupplierId.Value = id.ToString();
            HideProducts();
            litFormTitle.Text = "Edit supplier: " + Server.HtmlEncode(txtName.Text);
            litFormMeta.Text = StatusClass(s["F_Status"]) == "active" ? "Active" : "Inactive";
            btnSave.Text = "Save changes";
            pnlForm.Visible = true;

            ScrollTo("supFormCard", txtName.ClientID);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            hfSupplierId.Value = "";
            litMessage.Text = "";
        }

        // ---------- SAVE (add or update) ----------
        private static string Clean(string s)
        {
            return Regex.Replace(s ?? "", @"\s+", " ").Trim();
        }

        private string FormValue(string key)
        {
            if (key == "Province") return supProvince.SelectedValue;
            return Clean(((TextBox)Inputs[key]).Text);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id;
            bool isNew = !int.TryParse(hfSupplierId.Value, out id);

            string name = Clean(txtName.Text);
            var values = new Dictionary<string, string>();
            foreach (Field f in Fields)
            {
                if (f.Key == "Status" || !Has(f.Key)) continue;
                if (f.Key == "Address" && HasAddressParts) continue; // built from the parts below
                values[f.Key] = FormValue(f.Key);
            }
            if (values.ContainsKey("Phone")) values["Phone"] = Regex.Replace(values["Phone"], @"(?!^\+)[^0-9]", "");
            if (values.ContainsKey("Email")) values["Email"] = values["Email"].ToLowerInvariant();

            // ----- Checks -----
            Control bad;
            string error = Validate(name, values, out bad);

            try
            {
                if (error == null && NameTaken(name, isNew ? 0 : id))
                {
                    error = "A supplier called \"" + name + "\" already exists.";
                    bad = txtName;
                }
            }
            catch (Exception ex)
            {
                error = "The supplier could not be checked: " + ex.Message;
            }

            if (error != null)
            {
                ShowFormError(error, bad);
                return;
            }

            // Older single address column: keep it filled from the separate fields
            if (Has("Address") && HasAddressParts) values["Address"] = ComposeAddress(values);

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tx = conn.BeginTransaction())
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.Transaction = tx;

                        var cols = new List<string> { "Supplier_Name" };
                        var pars = new List<string> { "@Name" };
                        cmd.Parameters.AddWithValue("@Name", name);

                        int n = 0;
                        foreach (var pair in values)
                        {
                            TableSchema.Column c = Col(pair.Key);
                            string p = "@F" + (n++);
                            cols.Add(c.Name);
                            pars.Add(p);
                            cmd.Parameters.AddWithValue(p, DbValue(c, pair.Value));
                        }

                        if (isNew)
                        {
                            if (HasStatusColumn)
                            {
                                cols.Add(StatusCol.Name);
                                pars.Add("@Status");
                                cmd.Parameters.AddWithValue("@Status", ActiveValue);
                            }

                            bool manualId = !Schema.Find("Supplier_ID").IsIdentity;
                            if (manualId) { cols.Add("Supplier_ID"); pars.Add("@NewId"); }

                            List<string> unfilled = Schema.UnfilledRequired(cols);
                            if (unfilled.Count > 0)
                            {
                                tx.Rollback();
                                ShowFormError("The Supplier table needs a value for " + string.Join(", ", unfilled)
                                    + ", which this page doesn't fill in. Run Database/AddSupplierColumns.sql or give that column a default value.", txtName);
                                return;
                            }

                            cmd.CommandText = (manualId ? "DECLARE @NewId INT = (SELECT ISNULL(MAX(Supplier_ID), 0) + 1 FROM Supplier WITH (UPDLOCK, HOLDLOCK)); " : "")
                                + "INSERT INTO Supplier (" + string.Join(", ", cols.Select(c => TableSchema.Q(c))) + ") VALUES (" + string.Join(", ", pars) + ")";
                        }
                        else
                        {
                            // The status is never changed here: that is what Deactivate / Reactivate is for
                            var sets = cols.Select((c, i) => TableSchema.Q(c) + " = " + pars[i]);
                            cmd.CommandText = "UPDATE Supplier SET " + string.Join(", ", sets) + " WHERE Supplier_ID = @Id";
                            cmd.Parameters.AddWithValue("@Id", id);
                        }

                        int rows = cmd.ExecuteNonQuery();
                        if (!isNew && rows == 0)
                        {
                            tx.Rollback();
                            ShowMessage("That supplier could not be found. It may have been changed by someone else.", false);
                            pnlForm.Visible = false;
                            RefreshAll();
                            return;
                        }
                        tx.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowFormError("The supplier could not be saved: " + ex.Message, txtName);
                return;
            }

            pnlForm.Visible = false;
            hfSupplierId.Value = "";
            RefreshAll();
            if (!isNew && pnlProducts.Visible && ProductsSupplierId == id) ShowProducts(id);
            ShowMessage(name + (isNew ? " was added. You can now choose it as the supplier on products in Manage Products." : " was updated."), true);
        }

        /// <summary>Value for the database: blank optional values become NULL where allowed.</summary>
        private static object DbValue(TableSchema.Column c, string value)
        {
            if (string.IsNullOrEmpty(value)) return c.Nullable ? (object)DBNull.Value : "";
            if (!c.IsText)
            {
                // Numeric phone / postal code columns
                long number;
                if (long.TryParse(value.TrimStart('+'), out number)) return number;
            }
            return value;
        }

        private string Validate(string name, Dictionary<string, string> v, out Control bad)
        {
            bad = txtName;
            int nameMax = Schema.Find("Supplier_Name").TextLimit(100);

            if (name.Length == 0) return "Please enter the supplier's name.";
            if (name.Length < 2) return "The supplier name must be at least 2 characters.";
            if (name.Length > nameMax) return "The supplier name can be at most " + nameMax + " characters.";
            if (!Regex.IsMatch(name, @"[A-Za-z]")) return "The supplier name must contain at least one letter.";
            if (name.IndexOfAny(new[] { '<', '>' }) >= 0) return "The supplier name cannot contain < or >.";

            // Required fields and length limits for every field the table has
            foreach (Field f in Fields)
            {
                string value;
                if (!v.TryGetValue(f.Key, out value)) continue;
                bad = Inputs[f.Key];

                if (f.Required && value.Length == 0)
                    return f.Key == "Province" ? "Please select a province." : "Please enter the " + f.Label.ToLower() + ".";

                TableSchema.Column c = Col(f.Key);
                int max = c.IsText ? c.TextLimit(f.Fallback) : f.Fallback;
                if (value.Length > max) return "The " + f.Label.ToLower() + " can be at most " + max + " characters.";
                if (value.IndexOfAny(new[] { '<', '>' }) >= 0) return "The " + f.Label.ToLower() + " cannot contain < or >.";
            }

            string s;
            if (v.TryGetValue("ContactPerson", out s) && s.Length > 0 && !Regex.IsMatch(s, @"^[A-Za-zÀ-ÿ' .\-]{2,}$"))
            { bad = txtContact; return "The contact person's name can only contain letters, spaces, hyphens and apostrophes."; }

            if (v.TryGetValue("Phone", out s) && s.Length > 0 && !Regex.IsMatch(s, @"^(\+27|0)[1-9][0-9]{8}$"))
            { bad = txtPhone; return "The phone number must look like 0311234567 or +27311234567."; }

            if (v.TryGetValue("Email", out s) && s.Length > 0 && !Regex.IsMatch(s, @"^[^\s@<>]+@[^\s@<>]+\.[A-Za-z]{2,}$"))
            { bad = txtEmail; return "Please enter a valid email address, e.g. orders@supplier.co.za."; }

            if (v.TryGetValue("StreetNumber", out s) && s.Length > 0 && !Regex.IsMatch(s, @"^[0-9]+[A-Za-z]?(\s?-\s?[0-9]+[A-Za-z]?)?$"))
            { bad = supStreetNumber; return "The street number must be a number, e.g. 12, 12A or 12-14."; }

            if (v.TryGetValue("PostalCode", out s) && s.Length > 0 && !Regex.IsMatch(s, @"^\d{4}$"))
            { bad = supPostalCode; return "The postal code must be 4 digits."; }

            foreach (string key in new[] { "Suburb", "City" })
            {
                if (v.TryGetValue(key, out s) && s.Length > 0 && !Regex.IsMatch(s, @"[A-Za-z]"))
                { bad = Inputs[key]; return "The " + key.ToLower() + " must contain letters."; }
            }

            bad = null;
            return null;
        }

        private bool NameTaken(string name, int excludeId)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Supplier WHERE UPPER(LTRIM(RTRIM(Supplier_Name))) = UPPER(@Name) AND Supplier_ID <> @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Id", excludeId);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ---------- DEACTIVATE / REACTIVATE ----------
        private void ToggleStatus(int id)
        {
            if (!HasStatusColumn)
            {
                ShowMessage("The Supplier table has no status column. Run Database/AddSupplierColumns.sql first.", false);
                return;
            }

            try
            {
                DataRow s = LoadSupplier(id);
                if (s == null)
                {
                    ShowMessage("That supplier could not be found.", false);
                    RefreshAll();
                    return;
                }

                string name = Convert.ToString(s["Supplier_Name"]).Trim();
                bool turningOn = !IsActiveValueOn(s["F_Status"]);

                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Supplier SET " + TableSchema.Q(StatusCol.Name) + " = @Status WHERE Supplier_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Status", turningOn ? ActiveValue : InactiveValue);
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowMessage(turningOn
                    ? name + " is active again."
                    : name + " was deactivated. Their products stay in the shop; they will show as \"(inactive)\" in the supplier list in Manage Products.", true);
            }
            catch (Exception ex)
            {
                ShowMessage("The supplier status could not be changed: " + ex.Message, false);
            }

            if (pnlProducts.Visible && ProductsSupplierId == id) ShowProducts(id);
            RefreshAll();
        }

        // ---------- DISPLAY HELPERS ----------
        private static string Text(DataRow r, string key)
        {
            object v = r["F_" + key];
            return v == DBNull.Value ? "" : Clean(v.ToString());
        }

        private static string ComposeAddress(Dictionary<string, string> v)
        {
            Func<string, string> g = k => v.ContainsKey(k) ? v[k] : "";
            var parts = new List<string>();
            string unit = (g("UnitNumber").Length > 0 ? "Unit " + g("UnitNumber") + " " : "") + g("UnitName");
            if (unit.Trim().Length > 0) parts.Add(unit.Trim());
            string street = (g("StreetNumber") + " " + g("StreetName")).Trim();
            if (street.Length > 0) parts.Add(street);
            foreach (string k in new[] { "Suburb", "City", "PostalCode", "Province" })
                if (g(k).Length > 0) parts.Add(g(k));
            return string.Join(", ", parts);
        }

        private string FullAddress(DataRow s)
        {
            if (HasAddressParts)
            {
                var v = AddressPartKeys.ToDictionary(k => k, k => Text(s, k));
                string composed = ComposeAddress(v);
                if (composed.Length > 0) return composed;
            }
            return Text(s, "Address");
        }

        protected string LocationText(object city, object province, object address)
        {
            string c = city == DBNull.Value ? "" : Convert.ToString(city).Trim();
            string p = province == DBNull.Value ? "" : Convert.ToString(province).Trim();
            string text = c.Length > 0 && p.Length > 0 ? c + ", " + p : c + p;
            if (text.Length == 0 && address != DBNull.Value) text = Convert.ToString(address).Trim();
            if (text.Length > 60) text = text.Substring(0, 57).TrimEnd() + "...";
            return text.Length == 0 ? "-" : text;
        }

        protected string SubText(object value)
        {
            string text = value == null || value == DBNull.Value ? "" : value.ToString().Trim();
            if (text.Length == 0) return "";
            return "<span class=\"emp-sub\">" + HttpUtility.HtmlEncode(text) + "</span>";
        }

        protected string ProductSub(object brand, object size)
        {
            string b = Convert.ToString(brand).Trim();
            string s = size == null || size == DBNull.Value ? "" : Convert.ToString(size) + " ml";
            return b.Length > 0 && s.Length > 0 ? b + " · " + s : b + s;
        }

        protected string StockNote(object stock, object reorder)
        {
            int st = Convert.ToInt32(stock);
            int re = reorder == null || reorder == DBNull.Value ? 0 : Convert.ToInt32(reorder);
            if (st <= 0) return "<span class=\"emp-sub\">Out of stock</span>";
            if (st <= re) return "<span class=\"emp-sub\">Low - reorder</span>";
            return "";
        }

        protected string StatusClass(object status)
        {
            return IsActiveValueOn(status) ? "active" : "inactive";
        }

        protected string DeactivateJs(object status, object name, object activeProducts)
        {
            if (StatusClass(status) != "active") return "";
            string n = HttpUtility.JavaScriptStringEncode(Convert.ToString(name).Trim());
            int count = Convert.ToInt32(activeProducts);
            string extra = count > 0
                ? " Their " + count + " active product" + (count == 1 ? "" : "s") + " will stay in the shop."
                : "";
            return "return dashConfirm(this, 'Deactivate supplier', '" + n
                + " will be marked inactive." + HttpUtility.JavaScriptStringEncode(extra) + " You can reactivate them at any time.', 'Deactivate', false);";
        }

        private void ShowFormError(string text, Control field)
        {
            ShowMessage(text, false);
            pnlForm.Visible = true;
            ScrollTo("supFormCard", field != null ? field.ClientID : null);
        }

        private void ShowMessage(string text, bool success)
        {
            litMessage.Text = "<div class=\"emp-alert " + (success ? "emp-alert-success" : "emp-alert-error") + "\">"
                + Server.HtmlEncode(text) + "</div>";
        }

        /// <summary>Goes to the top of the page (used when a different supplier's products open).</summary>
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
