using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SamsLiquorShop.Helpers;

namespace SamsLiquorShop.Manager
{
    public partial class Products : System.Web.UI.Page
    {
        private const int MaxImageBytes = 2 * 1024 * 1024;

        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private static readonly HashSet<string> AllowedSort = new HashSet<string>
        {
            "Product_Name", "Category_Name", "Product_SellingPrice",
            "Product_QuantityInStock", "Product_ReorderQuantity", "Product_Status"
        };

        // "add" = the reason only makes sense when stock goes up, "remove" = only when it goes down
        private static readonly Dictionary<string, string> ReasonDirection = new Dictionary<string, string>
        {
            { "Delivery received", "add" },
            { "Returned by customer", "add" },
            { "Stock count correction", "any" },
            { "Damaged / broken", "remove" },
            { "Expired / spoiled", "remove" },
            { "Theft / loss", "remove" },
            { "Returned to supplier", "remove" },
            { "Other", "any" }
        };

        // Everything the manager typed into the product form, already checked
        private class ProductInput
        {
            public string Name, Brand, Type, Flavour, Origin, Ingredients, Description, Barcode, Status;
            public int CategoryId, SupplierId, SizeMl, Reorder, InitialStock;
            public int? DiscountId;
            public decimal? Abv;
            public decimal Cost, Price;
            public byte[] ImageBytes;
            public bool RemoveImage;
        }

        private string SortField
        {
            get { return (string)ViewState["SortField"] ?? "Product_Name"; }
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

            // The picture upload is posted from a form that was first drawn without it,
            // so the form must be multipart from the start.
            Page.Form.Enctype = "multipart/form-data";

            // Typing in the search box clicks Search after a short pause (handled in Site.Master)
            txtSearch.Attributes["data-live-search"] = btnSearch.ClientID;

            if (!IsPostBack)
            {
                LoadLookups();
                RefreshAll();
            }
        }

        private void RefreshAll()
        {
            LoadKpis();
            BindProducts();
        }

        // Fills the category, supplier and discount drop-downs
        private void LoadLookups()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add(new ListItem("All categories", ""));
            ddlCategory.Items.Clear();
            ddlCategory.Items.Add(new ListItem("Select a category", ""));
            ddlSupplier.Items.Clear();
            ddlSupplier.Items.Add(new ListItem("Select a supplier", ""));
            ddlDiscount.Items.Clear();
            ddlDiscount.Items.Add(new ListItem("No discount", ""));

            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT Category_ID, Category_Name FROM Category ORDER BY Category_Name", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string id = r["Category_ID"].ToString();
                        string name = r["Category_Name"].ToString();
                        ddlCategoryFilter.Items.Add(new ListItem(name, id));
                        ddlCategory.Items.Add(new ListItem(name, id));
                    }
                }

                using (SqlCommand cmd = new SqlCommand("SELECT Supplier_ID, Supplier_Name, Supplier_Status FROM Supplier ORDER BY Supplier_Name", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        bool active = Convert.ToString(r["Supplier_Status"]).Trim().Equals("Active", StringComparison.OrdinalIgnoreCase);
                        ddlSupplier.Items.Add(new ListItem(r["Supplier_Name"].ToString() + (active ? "" : " (inactive)"), r["Supplier_ID"].ToString()));
                    }
                }

                using (SqlCommand cmd = new SqlCommand("SELECT Discount_ID, Discount_Name FROM Discount ORDER BY Discount_Name", conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        ddlDiscount.Items.Add(new ListItem(r["Discount_Name"].ToString(), r["Discount_ID"].ToString()));
                    }
                }
            }
        }

        // ---------- READ ----------
        private DataTable LoadProductTable()
        {
            var where = new List<string> { "1 = 1" };

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = conn;

                string search = txtSearch.Text.Trim();
                if (search.Length > 0)
                {
                    where.Add(@"(p.Product_Name LIKE @Search OR p.Product_Brand LIKE @Search
                                 OR c.Category_Name LIKE @Search OR p.Product_BarcodeNumber LIKE @Search)");
                    cmd.Parameters.AddWithValue("@Search", "%" + search + "%");
                }

                int categoryId;
                if (int.TryParse(ddlCategoryFilter.SelectedValue, out categoryId))
                {
                    where.Add("p.Category_ID = @CategoryId");
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId);
                }

                if (ddlStatusFilter.SelectedValue != "")
                {
                    where.Add("p.Product_Status = @Status");
                    cmd.Parameters.AddWithValue("@Status", ddlStatusFilter.SelectedValue);
                }

                switch (ddlStockFilter.SelectedValue)
                {
                    case "in": where.Add("p.Product_QuantityInStock > p.Product_ReorderQuantity"); break;
                    case "low": where.Add("p.Product_QuantityInStock > 0 AND p.Product_QuantityInStock <= p.Product_ReorderQuantity"); break;
                    case "out": where.Add("p.Product_QuantityInStock <= 0"); break;
                }

                string sortField = AllowedSort.Contains(SortField) ? SortField : "Product_Name";
                string dir = SortDir == "DESC" ? "DESC" : "ASC";
                // Tie-breakers keep the order stable, but a column can't appear twice in ORDER BY
                string orderBy = sortField + " " + dir;
                if (sortField != "Product_Name") orderBy += ", Product_Name";
                orderBy += ", Product_ID";

                // UsageCount = how many sale lines and purchase order lines use the product
                // (a product that has been used can be deactivated but not deleted)
                cmd.CommandText = @"
                    SELECT * FROM (
                        SELECT p.Product_ID, p.Product_Name, ISNULL(p.Product_Brand, '') AS Product_Brand,
                               p.Product_SizeML, p.Product_BarcodeNumber,
                               ISNULL(c.Category_Name, 'Uncategorised') AS Category_Name,
                               ISNULL(s.Supplier_Name, '') AS Supplier_Name,
                               p.Product_CostPrice, p.Product_SellingPrice, p.Product_QuantityInStock,
                               p.Product_ReorderQuantity, p.Product_Status,
                               (SELECT COUNT(*) FROM SaleLine sl WHERE sl.Product_ID = p.Product_ID)
                             + (SELECT COUNT(*) FROM PurchaseOrderLine pl WHERE pl.Product_ID = p.Product_ID) AS UsageCount
                        FROM Product p
                        LEFT JOIN Category c ON c.Category_ID = p.Category_ID
                        LEFT JOIN Supplier s ON s.Supplier_ID = p.Supplier_ID
                        WHERE " + string.Join(" AND ", where) + @"
                    ) t
                                        ORDER BY " + orderBy; 

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        private void BindProducts()
        {
            DataTable dt = LoadProductTable();
            int total = dt.Rows.Count;

            int pages = (int)Math.Ceiling(total / (double)gvProducts.PageSize);
            if (pages > 0 && gvProducts.PageIndex >= pages) gvProducts.PageIndex = pages - 1;

            gvProducts.DataSource = dt;
            gvProducts.DataBind();

            if (total == 0)
            {
                litCount.Text = "No products found";
            }
            else
            {
                int start = gvProducts.PageIndex * gvProducts.PageSize + 1;
                int end = Math.Min(start + gvProducts.PageSize - 1, total);
                litCount.Text = "Showing " + start + "\u2013" + end + " of " + total + (total == 1 ? " product" : " products");
            }
        }

        private void LoadKpis()
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(*),
                       ISNULL(SUM(CASE WHEN Product_Status = 'Active' THEN 1 ELSE 0 END), 0),
                       ISNULL(SUM(CASE WHEN Product_Status = 'Active' AND Product_QuantityInStock > 0
                                        AND Product_QuantityInStock <= Product_ReorderQuantity THEN 1 ELSE 0 END), 0),
                       ISNULL(SUM(CASE WHEN Product_Status = 'Active' AND Product_QuantityInStock <= 0 THEN 1 ELSE 0 END), 0),
                       ISNULL(SUM(CASE WHEN Product_Status = 'Active' THEN Product_QuantityInStock * Product_CostPrice ELSE 0 END), 0)
                FROM Product", conn))
            {
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        litKpiTotal.Text = r.GetInt32(0).ToString();
                        litKpiActive.Text = r.GetInt32(1).ToString();
                        litKpiLow.Text = r.GetInt32(2).ToString();
                        litKpiOut.Text = r.GetInt32(3).ToString();
                        litKpiValue.Text = r.GetDecimal(4).ToString("C", CultureInfo.CurrentCulture);
                    }
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvProducts.PageIndex = 0;
            BindProducts();
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            gvProducts.PageIndex = 0;
            BindProducts();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlCategoryFilter.SelectedIndex = 0;
            ddlStatusFilter.SelectedIndex = 0;
            ddlStockFilter.SelectedIndex = 0;
            gvProducts.PageIndex = 0;
            BindProducts();
        }

        protected void gvProducts_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvProducts.PageIndex = e.NewPageIndex;
            BindProducts();
        }

        protected void gvProducts_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (SortField == e.SortExpression)
            {
                SortDir = (SortDir == "ASC") ? "DESC" : "ASC";
            }
            else
            {
                SortField = e.SortExpression;
                SortDir = "ASC";
            }
            gvProducts.PageIndex = 0;
            BindProducts();
        }

        // Adds an up/down arrow to the header of the column being sorted
        protected void gvProducts_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.Header) return;

            foreach (TableCell cell in e.Row.Cells)
            {
                if (cell.Controls.Count == 0) continue;
                LinkButton lb = cell.Controls[0] as LinkButton;
                if (lb != null && lb.CommandArgument == SortField)
                {
                    lb.Text += (SortDir == "ASC") ? " \u25B2" : " \u25BC";
                }
            }
        }

        // ---------- Helpers used inside the grid markup ----------
        protected string ImageUrl(object id)
        {
            return ResolveUrl("~/ProductImage.ashx?id=" + Convert.ToInt32(id));
        }

        // "Brand · 750 ml"
        protected string SubLine(object brand, object sizeMl)
        {
            string b = (Convert.ToString(brand) ?? "").Trim();
            int s = Convert.ToInt32(sizeMl);

            var parts = new List<string>();
            if (b.Length > 0) parts.Add(b);
            if (s > 0) parts.Add(s + " ml");
            return Server.HtmlEncode(string.Join(" \u00B7 ", parts));
        }

        protected string StatusClass(object status)
        {
            return (Convert.ToString(status) ?? "").Trim().ToLower() == "active" ? "active" : "inactive";
        }

        // "active" (green) = healthy, "low" (amber), "inactive" (red) = out of stock
        protected string StockClass(object qty, object reorder)
        {
            int q = Convert.ToInt32(qty);
            int r = Convert.ToInt32(reorder);
            if (q <= 0) return "inactive";
            return q <= r ? "low" : "active";
        }

        protected string StockLabel(object qty, object reorder)
        {
            string cls = StockClass(qty, reorder);
            return cls == "inactive" ? "Out" : (cls == "low" ? "Low" : "In stock");
        }

        protected string ChangeText(object change)
        {
            int c = Convert.ToInt32(change);
            return "<span class=\"" + (c > 0 ? "emp-change-up" : "emp-change-down") + "\">" + (c > 0 ? "+" : "") + c + "</span>";
        }

        protected string DeactivateJs(object status, object name)
        {
            if (StatusClass(status) != "active") return "";

            string n = HttpUtility.JavaScriptStringEncode(Convert.ToString(name) ?? "");
            return "return dashConfirm(this, 'Deactivate product', '" + n
                + " will disappear from the shop and can no longer be bought. You can reactivate it at any time.', 'Deactivate', false);";
        }

        protected string DeleteJs(object name)
        {
            string n = HttpUtility.JavaScriptStringEncode(Convert.ToString(name) ?? "");
            return "return dashConfirm(this, 'Delete product', 'Permanently delete " + n
                + "? This cannot be undone.', 'Delete', true);";
        }

        // ---------- ROW BUTTONS ----------
        protected void gvProducts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EditProduct" && e.CommandName != "AdjustStock" && e.CommandName != "StockHistory"
                && e.CommandName != "ToggleProduct" && e.CommandName != "DeleteProduct") return;

            int id = Convert.ToInt32(e.CommandArgument);
            litMessage.Text = "";

            switch (e.CommandName)
            {
                case "EditProduct": LoadForEdit(id); break;
                case "AdjustStock": LoadStockForm(id); break;
                case "StockHistory": ShowHistory(id); break;
                case "ToggleProduct": ToggleStatus(id); break;
                case "DeleteProduct": DeleteProduct(id); break;
            }
        }

        // ---------- ADD ----------
        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            litMessage.Text = "";
            pnlStock.Visible = false;
            pnlHistory.Visible = false;

            ClearProductForm();
            hfProductId.Value = "";
            litFormTitle.Text = "Add product";
            litFormMeta.Text = "You can add a picture after saving, using Edit.";
            phInitialStock.Visible = true;
            phStockNote.Visible = false;
            phImage.Visible = false;
            pnlForm.Visible = true;

            ScrollTo("prodFormCard", txtName.ClientID);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
            ClearProductForm();
        }

        // ---------- EDIT (load into form) ----------
        private void LoadForEdit(int id)
        {
            pnlStock.Visible = false;
            pnlHistory.Visible = false;

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT Product_ID, Category_ID, Supplier_ID, Discount_ID, Product_Name, Product_Description,
                       Product_Brand, Product_Type, Product_Flavour, Product_AlcoholPercentage, Product_OriginRegion,
                       Product_Ingredients, Product_SizeML, Product_BarcodeNumber, Product_SellingPrice,
                       Product_CostPrice, Product_QuantityInStock, Product_ReorderQuantity, Product_Status,
                       CASE WHEN Product_Image IS NULL THEN 0 ELSE 1 END AS HasImage
                FROM Product WHERE Product_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        ShowMessage("That product could not be found.", false);
                        return;
                    }

                    ClearProductForm();
                    hfProductId.Value = id.ToString();

                    txtName.Text = Val(r, "Product_Name");
                    txtBrand.Text = Val(r, "Product_Brand");
                    txtType.Text = Val(r, "Product_Type");
                    SelectValue(ddlCategory, Val(r, "Category_ID"));
                    SelectValue(ddlSupplier, Val(r, "Supplier_ID"));
                    SelectValue(ddlDiscount, Val(r, "Discount_ID"));
                    txtSize.Text = Val(r, "Product_SizeML");
                    txtAlcohol.Text = r["Product_AlcoholPercentage"] == DBNull.Value
                        ? "" : Convert.ToDecimal(r["Product_AlcoholPercentage"]).ToString("0.##", CultureInfo.InvariantCulture);
                    txtFlavour.Text = Val(r, "Product_Flavour");
                    txtOrigin.Text = Val(r, "Product_OriginRegion");
                    txtBarcode.Text = Val(r, "Product_BarcodeNumber");
                    txtCost.Text = Convert.ToDecimal(r["Product_CostPrice"]).ToString("0.00", CultureInfo.InvariantCulture);
                    txtPrice.Text = Convert.ToDecimal(r["Product_SellingPrice"]).ToString("0.00", CultureInfo.InvariantCulture);
                    txtReorder.Text = Val(r, "Product_ReorderQuantity");
                    SelectValue(ddlStatus, Val(r, "Product_Status"));
                    txtIngredients.Text = Val(r, "Product_Ingredients");
                    txtDescription.Text = Val(r, "Product_Description");

                    litStockNote.Text = Convert.ToInt32(r["Product_QuantityInStock"])
                        + " in stock. To change it, use <strong>Stock</strong> on the product list so the change is logged.";

                    bool hasImage = Convert.ToInt32(r["HasImage"]) == 1;
                    litImagePreview.Text = hasImage
                        ? "<img class=\"prod-preview\" alt=\"Current picture\" src=\"" + ResolveUrl("~/ProductImage.ashx?id=" + id + "&v=" + DateTime.Now.Ticks) + "\" />"
                        : "<span class=\"prod-preview prod-preview-empty\">No picture yet</span>";
                    chkRemoveImage.Visible = hasImage;
                    chkRemoveImage.Checked = false;
                }
            }

            litFormTitle.Text = "Edit product";
            litFormMeta.Text = "";
            phInitialStock.Visible = false;
            phStockNote.Visible = true;
            phImage.Visible = true;
            pnlForm.Visible = true;

            ScrollTo("prodFormCard", txtName.ClientID);
        }

        // ---------- SAVE (INSERT or UPDATE) ----------
        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id;
            int.TryParse(hfProductId.Value, out id);
            bool isNew = id <= 0;

            ResetInvalid();

            string error;
            WebControl badField;
            ProductInput p = ReadAndValidate(isNew, id, out error, out badField);

            if (error != null)
            {
                ShowMessage(error, false);
                pnlForm.Visible = true;
                if (badField != null) badField.CssClass = "login-input invalid";
                ScrollTo("prodFormCard", badField != null ? badField.ClientID : null);
                return;
            }

            string warning = null;

            try
            {
                if (isNew)
                {
                    int newId = InsertProduct(p);

                    if (p.InitialStock > 0)
                    {
                        try
                        {
                            StockLogHelper.Append(newId, p.Name, 0, p.InitialStock, "Initial stock - product added", WhoIsChanging());
                        }
                        catch (Exception ex)
                        {
                            warning = " The starting stock could not be written to the log file (" + ex.Message + ").";
                        }
                    }
                }
                else
                {
                    UpdateProduct(id, p);
                }
            }
            catch (SqlException ex)
            {
                ShowMessage("The product could not be saved: " + ex.Message, false);
                pnlForm.Visible = true;
                return;
            }

            pnlForm.Visible = false;
            ClearProductForm();
            ShowMessage(p.Name + (isNew ? " was added." : " was updated.") + (warning ?? ""), warning == null);
            RefreshAll();
        }

        // Reads and checks every field. On a problem it returns null, sets the message and
        // points badField at the box to highlight.
        private ProductInput ReadAndValidate(bool isNew, int id, out string error, out WebControl badField)
        {
            error = null;
            badField = null;

            var p = new ProductInput();

            // ----- Name, brand, type -----
            p.Name = txtName.Text.Trim();
            if (p.Name.Length == 0) { badField = txtName; error = "Enter the product name."; return null; }
            if (p.Name.Length > 100) { badField = txtName; error = "The product name can be at most 100 characters."; return null; }

            p.Brand = txtBrand.Text.Trim();
            if (p.Brand.Length > 50) { badField = txtBrand; error = "The brand can be at most 50 characters."; return null; }

            p.Type = txtType.Text.Trim();
            if (p.Type.Length > 50) { badField = txtType; error = "The type can be at most 50 characters."; return null; }

            // ----- Category, supplier, discount -----
            if (!int.TryParse(ddlCategory.SelectedValue, out p.CategoryId))
            { badField = ddlCategory; error = "Please choose a category."; return null; }

            if (!int.TryParse(ddlSupplier.SelectedValue, out p.SupplierId))
            { badField = ddlSupplier; error = "Please choose a supplier."; return null; }

            int discountId;
            p.DiscountId = int.TryParse(ddlDiscount.SelectedValue, out discountId) ? (int?)discountId : null;

            // ----- Size and alcohol -----
            if (!int.TryParse(txtSize.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out p.SizeMl) || p.SizeMl > 20000)
            { badField = txtSize; error = "Enter the size in millilitres (0 to 20 000). Use 0 for items without a volume."; return null; }

            string abvText = txtAlcohol.Text.Trim();
            if (abvText.Length > 0)
            {
                decimal abv;
                if (!TryParseDecimal(abvText, out abv) || abv < 0 || abv > 100)
                { badField = txtAlcohol; error = "Alcohol % must be a number from 0 to 100, with at most two decimals."; return null; }
                p.Abv = abv;
            }

            // ----- Flavour, origin, ingredients, description -----
            p.Flavour = txtFlavour.Text.Trim();
            if (p.Flavour.Length > 100) { badField = txtFlavour; error = "The flavour can be at most 100 characters."; return null; }

            p.Origin = txtOrigin.Text.Trim();
            if (p.Origin.Length > 100) { badField = txtOrigin; error = "The origin region can be at most 100 characters."; return null; }

            p.Ingredients = txtIngredients.Text.Trim();
            if (p.Ingredients.Length > 255) { badField = txtIngredients; error = "The ingredients can be at most 255 characters."; return null; }

            p.Description = txtDescription.Text.Trim();
            if (p.Description.Length > 1000) { badField = txtDescription; error = "The description can be at most 1000 characters."; return null; }

            // ----- Barcode -----
            p.Barcode = txtBarcode.Text.Trim();
            if (!Regex.IsMatch(p.Barcode, @"^[0-9]{13}$"))
            { badField = txtBarcode; error = "The barcode must be exactly 13 digits."; return null; }

            // ----- Prices -----
            if (!TryParseDecimal(txtCost.Text.Trim(), out p.Cost) || p.Cost <= 0 || p.Cost > 1000000m)
            { badField = txtCost; error = "Enter the cost price as an amount above 0 (for example 129.50)."; return null; }

            if (!TryParseDecimal(txtPrice.Text.Trim(), out p.Price) || p.Price <= 0 || p.Price > 1000000m)
            { badField = txtPrice; error = "Enter the selling price as an amount above 0 (for example 189.99)."; return null; }

            if (p.Price < p.Cost)
            {
                badField = txtPrice;
                error = "The selling price (R" + p.Price.ToString("0.00", CultureInfo.InvariantCulture)
                    + ") is lower than the cost price (R" + p.Cost.ToString("0.00", CultureInfo.InvariantCulture) + ").";
                return null;
            }

            // ----- Stock -----
            if (!int.TryParse(txtReorder.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out p.Reorder) || p.Reorder > 100000)
            { badField = txtReorder; error = "Enter the reorder level as a whole number (0 to 100 000)."; return null; }

            if (isNew)
            {
                if (!int.TryParse(txtInitialStock.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out p.InitialStock) || p.InitialStock > 100000)
                { badField = txtInitialStock; error = "Enter the starting stock as a whole number (0 to 100 000)."; return null; }
            }

            p.Status = ddlStatus.SelectedValue;

            // ----- Picture (edit only) -----
            if (!isNew)
            {
                p.RemoveImage = chkRemoveImage.Visible && chkRemoveImage.Checked;

                if (fuImage.HasFile)
                {
                    if (fuImage.PostedFile.ContentLength > MaxImageBytes)
                    { badField = fuImage; error = "That picture is larger than 2 MB. Please choose a smaller one."; return null; }

                    byte[] bytes = fuImage.FileBytes;
                    if (!IsSupportedImage(bytes))
                    { badField = fuImage; error = "Please choose a JPG, PNG, GIF or WEBP picture."; return null; }

                    p.ImageBytes = bytes;
                    p.RemoveImage = false;
                }
            }

            // ----- Duplicates (only checked when the value is new or has changed) -----
            string origName = null, origBarcode = null;
            int origSize = -1;
            if (!isNew)
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Product_Name, Product_SizeML, Product_BarcodeNumber FROM Product WHERE Product_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            origName = r["Product_Name"].ToString();
                            origSize = Convert.ToInt32(r["Product_SizeML"]);
                            origBarcode = r["Product_BarcodeNumber"].ToString();
                        }
                    }
                }
            }

            if (isNew || !SameText(origBarcode, p.Barcode))
            {
                if (BarcodeExists(p.Barcode, id))
                { badField = txtBarcode; error = "That barcode already belongs to another product."; return null; }
            }

            if (isNew || !SameText(origName, p.Name) || origSize != p.SizeMl)
            {
                if (NameSizeExists(p.Name, p.SizeMl, id))
                { badField = txtName; error = "A product with this name and size already exists."; return null; }
            }

            return p;
        }

        private static bool SameText(string a, string b)
        {
            return string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
        }

        // Accepts "129.5" and "129,50"; no more than two decimals
        private static bool TryParseDecimal(string text, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;

            string t = text.Trim().Replace(',', '.');
            if (!decimal.TryParse(t, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value)) return false;
            return Math.Round(value, 2) == value;
        }

        private bool BarcodeExists(string barcode, int excludeId)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Product WHERE Product_BarcodeNumber = @V AND Product_ID <> @Id", conn))
            {
                cmd.Parameters.AddWithValue("@V", barcode);
                cmd.Parameters.AddWithValue("@Id", excludeId);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool NameSizeExists(string name, int sizeMl, int excludeId)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Product WHERE Product_Name = @Name AND Product_SizeML = @Size AND Product_ID <> @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Size", sizeMl);
                cmd.Parameters.AddWithValue("@Id", excludeId);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // JPG, PNG, GIF or WEBP, judged by the file's first bytes (not its name)
        private static bool IsSupportedImage(byte[] b)
        {
            if (b == null || b.Length < 12) return false;

            bool jpg = b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF;
            bool png = b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47;
            bool gif = b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x38;
            bool webp = b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46
                        && b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50;

            return jpg || png || gif || webp;
        }

        // Parameters shared by INSERT and UPDATE
        private static void AddProductParameters(SqlCommand cmd, ProductInput p)
        {
            cmd.Parameters.AddWithValue("@CategoryId", p.CategoryId);
            cmd.Parameters.AddWithValue("@SupplierId", p.SupplierId);
            cmd.Parameters.AddWithValue("@DiscountId", p.DiscountId.HasValue ? (object)p.DiscountId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Name", p.Name);
            cmd.Parameters.AddWithValue("@Description", NullIfBlank(p.Description));
            cmd.Parameters.AddWithValue("@Brand", NullIfBlank(p.Brand));
            cmd.Parameters.AddWithValue("@Type", NullIfBlank(p.Type));
            cmd.Parameters.AddWithValue("@Flavour", NullIfBlank(p.Flavour));
            cmd.Parameters.AddWithValue("@Abv", p.Abv.HasValue ? (object)p.Abv.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Origin", NullIfBlank(p.Origin));
            cmd.Parameters.AddWithValue("@Ingredients", NullIfBlank(p.Ingredients));
            cmd.Parameters.AddWithValue("@SizeMl", p.SizeMl);
            cmd.Parameters.AddWithValue("@Barcode", p.Barcode);
            cmd.Parameters.AddWithValue("@Price", p.Price);
            cmd.Parameters.AddWithValue("@Cost", p.Cost);
            cmd.Parameters.AddWithValue("@Reorder", p.Reorder);
            cmd.Parameters.AddWithValue("@Status", p.Status);
        }

        private int InsertProduct(ProductInput p)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Product
                    (Category_ID, Supplier_ID, Discount_ID, Product_Name, Product_Description, Product_Brand,
                     Product_Type, Product_Flavour, Product_AlcoholPercentage, Product_OriginRegion,
                     Product_Ingredients, Product_SizeML, Product_BarcodeNumber, Product_SellingPrice,
                     Product_CostPrice, Product_QuantityInStock, Product_ReorderQuantity, Product_Status)
                VALUES
                    (@CategoryId, @SupplierId, @DiscountId, @Name, @Description, @Brand,
                     @Type, @Flavour, @Abv, @Origin,
                     @Ingredients, @SizeMl, @Barcode, @Price,
                     @Cost, @Stock, @Reorder, @Status);
                SELECT CAST(SCOPE_IDENTITY() AS int);", conn))
            {
                AddProductParameters(cmd, p);
                cmd.Parameters.AddWithValue("@Stock", p.InitialStock);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Stock is never changed here: that only happens through the logged Stock form
        private void UpdateProduct(int id, ProductInput p)
        {
            string imageSql = p.ImageBytes != null ? ", Product_Image = @Image"
                            : (p.RemoveImage ? ", Product_Image = NULL" : "");

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE Product
                SET Category_ID = @CategoryId, Supplier_ID = @SupplierId, Discount_ID = @DiscountId,
                    Product_Name = @Name, Product_Description = @Description, Product_Brand = @Brand,
                    Product_Type = @Type, Product_Flavour = @Flavour, Product_AlcoholPercentage = @Abv,
                    Product_OriginRegion = @Origin, Product_Ingredients = @Ingredients, Product_SizeML = @SizeMl,
                    Product_BarcodeNumber = @Barcode, Product_SellingPrice = @Price, Product_CostPrice = @Cost,
                    Product_ReorderQuantity = @Reorder, Product_Status = @Status" + imageSql + @"
                WHERE Product_ID = @Id", conn))
            {
                AddProductParameters(cmd, p);
                cmd.Parameters.AddWithValue("@Id", id);
                if (p.ImageBytes != null)
                {
                    cmd.Parameters.Add("@Image", SqlDbType.VarBinary, -1).Value = p.ImageBytes;
                }

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ---------- ADJUST STOCK ----------
        private void LoadStockForm(int id)
        {
            pnlForm.Visible = false;
            pnlHistory.Visible = false;

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT Product_Name, Product_QuantityInStock, Product_ReorderQuantity
                FROM Product WHERE Product_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        ShowMessage("That product could not be found.", false);
                        return;
                    }

                    int stock = Convert.ToInt32(r["Product_QuantityInStock"]);
                    hfProductId.Value = id.ToString();
                    hfCurrentStock.Value = stock.ToString();
                    litStockTitle.Text = "Adjust stock: " + Server.HtmlEncode(r["Product_Name"].ToString());
                    litStockCurrent.Text = "Currently " + stock + " in stock \u00B7 reorder at " + Convert.ToInt32(r["Product_ReorderQuantity"]);
                }
            }

            ClearStockForm();
            pnlStock.Visible = true;
            ScrollTo("prodStockCard", ddlChangeType.ClientID);
        }

        protected void btnCancelStock_Click(object sender, EventArgs e)
        {
            pnlStock.Visible = false;
            ClearStockForm();
        }

        protected void btnSaveStock_Click(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(hfProductId.Value, out id) || id <= 0)
            {
                ShowMessage("Choose a product to adjust first.", false);
                return;
            }

            ResetStockInvalid();

            int quantity;
            string category, reason;
            WebControl badField;
            string error = ValidateStockForm(out quantity, out category, out reason, out badField);

            string productName = null;
            int oldQty = 0, newQty = 0;

            if (error == null)
            {
                error = ApplyStockChange(id, ddlChangeType.SelectedValue, quantity, category,
                                         out productName, out oldQty, out newQty, out badField);
            }

            if (error != null)
            {
                ShowMessage(error, false);
                pnlStock.Visible = true;
                if (badField != null) badField.CssClass = "login-input invalid";
                ScrollTo("prodStockCard", badField != null ? badField.ClientID : null);
                return;
            }

            // The database change is committed; now write the text-file record
            string logWarning = null;
            try
            {
                StockLogHelper.Append(id, productName, oldQty, newQty, reason, WhoIsChanging());
            }
            catch (Exception ex)
            {
                logWarning = " The change could not be written to the log file (" + ex.Message + ").";
            }

            int change = newQty - oldQty;
            string summary = productName + ": " + oldQty + " \u2192 " + newQty + " (" + (change > 0 ? "+" : "") + change + ").";

            pnlStock.Visible = false;
            pnlHistory.Visible = false;
            ClearStockForm();
            ShowMessage("Stock updated. " + summary + (logWarning ?? ""), logWarning == null);
            RefreshAll();
        }

        // Checks what the manager typed. Returns an error message, or null when the form is fine.
        private string ValidateStockForm(out int quantity, out string category, out string reason, out WebControl badField)
        {
            quantity = 0;
            category = ddlReason.SelectedValue;
            reason = "";
            badField = null;

            string type = ddlChangeType.SelectedValue;
            string qtyText = txtQuantity.Text.Trim();

            if (qtyText.Length == 0)
            {
                badField = txtQuantity;
                return "Enter a quantity.";
            }
            if (!int.TryParse(qtyText, NumberStyles.None, CultureInfo.InvariantCulture, out quantity))
            {
                badField = txtQuantity;
                return "The quantity must be a whole number.";
            }
            if (type != "set" && quantity < 1)
            {
                badField = txtQuantity;
                return "The quantity must be at least 1.";
            }
            if (quantity > 100000)
            {
                badField = txtQuantity;
                return "That quantity is too large (maximum 100 000).";
            }

            if (category.Length == 0)
            {
                badField = ddlReason;
                return "Please choose a reason for the change.";
            }

            string notes = txtNotes.Text.Trim();
            if (category == "Other" && notes.Length < 3)
            {
                badField = txtNotes;
                return "Please describe the reason (at least 3 characters).";
            }

            reason = category + (notes.Length > 0 ? " - " + notes : "");
            return null;
        }

        // Reads the current stock with a lock, applies the change, and commits.
        // Returns an error message, or null on success.
        private string ApplyStockChange(int id, string type, int quantity, string reasonCategory,
                                        out string productName, out int oldQty, out int newQty, out WebControl badField)
        {
            productName = null;
            oldQty = 0;
            newQty = 0;
            badField = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        // UPDLOCK stops a sale changing this row between our read and our write
                        using (SqlCommand read = new SqlCommand(
                            "SELECT Product_Name, Product_QuantityInStock FROM Product WITH (UPDLOCK, ROWLOCK) WHERE Product_ID = @Id", conn, tx))
                        {
                            read.Parameters.AddWithValue("@Id", id);
                            using (SqlDataReader r = read.ExecuteReader())
                            {
                                if (!r.Read())
                                {
                                    r.Close();
                                    tx.Rollback();
                                    return "That product could not be found.";
                                }
                                productName = r["Product_Name"].ToString();
                                oldQty = Convert.ToInt32(r["Product_QuantityInStock"]);
                            }
                        }

                        if (type == "add")
                        {
                            newQty = oldQty + quantity;
                        }
                        else if (type == "remove")
                        {
                            if (quantity > oldQty)
                            {
                                tx.Rollback();
                                badField = txtQuantity;
                                return "Only " + oldQty + " in stock \u2013 you can't remove " + quantity + ".";
                            }
                            newQty = oldQty - quantity;
                        }
                        else
                        {
                            newQty = quantity;
                        }

                        int delta = newQty - oldQty;
                        if (delta == 0)
                        {
                            tx.Rollback();
                            badField = txtQuantity;
                            return "Stock is already " + oldQty + " \u2013 nothing to change.";
                        }

                        string direction;
                        if (ReasonDirection.TryGetValue(reasonCategory, out direction))
                        {
                            if (direction == "add" && delta < 0)
                            {
                                tx.Rollback();
                                badField = ddlReason;
                                return "\"" + reasonCategory + "\" can only increase stock. Choose a different reason or change type.";
                            }
                            if (direction == "remove" && delta > 0)
                            {
                                tx.Rollback();
                                badField = ddlReason;
                                return "\"" + reasonCategory + "\" can only reduce stock. Choose a different reason or change type.";
                            }
                        }

                        using (SqlCommand update = new SqlCommand(
                            "UPDATE Product SET Product_QuantityInStock = @New WHERE Product_ID = @Id", conn, tx))
                        {
                            update.Parameters.AddWithValue("@New", newQty);
                            update.Parameters.AddWithValue("@Id", id);
                            update.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                }
            }
            catch (SqlException ex)
            {
                return "The stock could not be updated: " + ex.Message;
            }

            return null;
        }

        // ---------- STOCK HISTORY ----------
        private void ShowHistory(int id)
        {
            pnlForm.Visible = false;
            pnlStock.Visible = false;

            string name = null;
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand("SELECT Product_Name FROM Product WHERE Product_ID = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                object o = cmd.ExecuteScalar();
                if (o != null && o != DBNull.Value) name = o.ToString();
            }

            if (name == null)
            {
                ShowMessage("That product could not be found.", false);
                return;
            }

            try
            {
                gvStockLog.DataSource = StockLogHelper.ReadForProduct(id, 50);
                gvStockLog.DataBind();
            }
            catch (Exception ex)
            {
                ShowMessage("The stock log could not be read: " + ex.Message, false);
                return;
            }

            litHistoryName.Text = Server.HtmlEncode(name);
            pnlHistory.Visible = true;
            ScrollTo("prodHistoryCard", null);
        }

        protected void btnCloseHistory_Click(object sender, EventArgs e)
        {
            pnlHistory.Visible = false;
        }

        // ---------- DEACTIVATE / REACTIVATE ----------
        private void ToggleStatus(int id)
        {
            string current = null;
            string name = null;

            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                using (SqlCommand read = new SqlCommand("SELECT Product_Name, Product_Status FROM Product WHERE Product_ID = @Id", conn))
                {
                    read.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader r = read.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            name = r["Product_Name"].ToString();
                            current = r["Product_Status"].ToString().Trim();
                        }
                    }
                }

                if (current == null)
                {
                    ShowMessage("That product could not be found.", false);
                    return;
                }

                bool wasActive = current.Equals("Active", StringComparison.OrdinalIgnoreCase);
                string newStatus = wasActive ? "Inactive" : "Active";

                try
                {
                    using (SqlCommand update = new SqlCommand("UPDATE Product SET Product_Status = @Status WHERE Product_ID = @Id", conn))
                    {
                        update.Parameters.AddWithValue("@Status", newStatus);
                        update.Parameters.AddWithValue("@Id", id);
                        update.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    ShowMessage("The status could not be changed: " + ex.Message, false);
                    return;
                }

                ShowMessage(name + (wasActive ? " was deactivated and no longer appears in the shop." : " was reactivated and is back in the shop."), true);
            }

            RefreshAll();
        }

        // ---------- DELETE (only if the product was never sold or ordered) ----------
        private void DeleteProduct(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        using (SqlCommand cmd = new SqlCommand(@"
                            SELECT (SELECT COUNT(*) FROM SaleLine WHERE Product_ID = @Id)
                                 + (SELECT COUNT(*) FROM PurchaseOrderLine WHERE Product_ID = @Id)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            {
                                tx.Rollback();
                                ShowMessage("This product has sales or purchase orders on record and cannot be deleted. Use Deactivate instead.", false);
                                RefreshAll();
                                return;
                            }
                        }

                        // The Wishlist table is optional (added by Database\CreateWishlistTable.sql)
                        using (SqlCommand cmd = new SqlCommand(
                            "IF OBJECT_ID('dbo.Wishlist', 'U') IS NOT NULL DELETE FROM dbo.Wishlist WHERE Product_ID = @Id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand("DELETE FROM Product WHERE Product_ID = @Id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                }
                ShowMessage("Product deleted.", true);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // foreign key conflict
                    ShowMessage("This product is linked to other records and cannot be deleted. Use Deactivate instead.", false);
                else
                    ShowMessage("The product could not be deleted: " + ex.Message, false);
            }

            pnlForm.Visible = false;
            pnlStock.Visible = false;
            pnlHistory.Visible = false;
            RefreshAll();
        }

        // ---------- EXPORT / DOWNLOAD ----------
        // Export PDF / Export Excel: the products matching the current filters
        protected void btnExport_Command(object sender, CommandEventArgs e)
        {
            DataTable dt = LoadProductTable();

            var table = new ExportTable("Products", "Stock list")
                .Col("ID", ExportKind.Integer)
                .Col("Name")
                .Col("Brand")
                .Col("Category")
                .Col("Supplier")
                .Col("Size (ml)", ExportKind.Integer)
                .Col("Barcode")
                .Col("Cost Price", ExportKind.Money)
                .Col("Selling Price", ExportKind.Money)
                .Col("Stock", ExportKind.Integer)
                .Col("Reorder At", ExportKind.Integer)
                .Col("Stock Level")
                .Col("Status");

            foreach (DataRow r in dt.Rows)
            {
                table.Add(
                    r["Product_ID"],
                    r["Product_Name"],
                    r["Product_Brand"],
                    r["Category_Name"],
                    r["Supplier_Name"],
                    r["Product_SizeML"],
                    Convert.ToString(r["Product_BarcodeNumber"]),
                    r["Product_CostPrice"],
                    r["Product_SellingPrice"],
                    r["Product_QuantityInStock"],
                    r["Product_ReorderQuantity"],
                    StockLabel(r["Product_QuantityInStock"], r["Product_ReorderQuantity"]),
                    r["Product_Status"]);
            }

            ExportHelper.Send(Response, table, "products", e.CommandArgument.ToString());
        }

        protected void btnDownloadLog_Click(object sender, EventArgs e)
        {
            byte[] bytes;
            try
            {
                bytes = StockLogHelper.ReadAllBytes();
            }
            catch (Exception ex)
            {
                ShowMessage("The stock log could not be read: " + ex.Message, false);
                return;
            }

            if (bytes == null || bytes.Length == 0)
            {
                ShowMessage("No stock changes have been logged yet.", false);
                return;
            }

            Response.Clear();
            Response.ContentType = "text/plain";
            Response.AddHeader("Content-Disposition", "attachment; filename=StockChanges-" + DateTime.Today.ToString("yyyyMMdd") + ".txt");
            Response.BinaryWrite(bytes);
            Response.End();
        }

        // Quotes a CSV value and defuses spreadsheet formulas (cells starting with = + - @)
        private static string Csv(object value)
        {
            string s = Convert.ToString(value) ?? "";
            if (s.Length > 0 && "=+-@".IndexOf(s[0]) >= 0 && !Regex.IsMatch(s, @"^\+?[0-9]+$"))
                s = "'" + s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        // ---------- HELPERS ----------
        private string WhoIsChanging()
        {
            return (Convert.ToString(Session["UserName"]) + " (employee #" + Convert.ToString(Session["EmployeeID"]) + ")").Trim();
        }

        private static string Val(SqlDataReader r, string column)
        {
            object o = r[column];
            return o == DBNull.Value ? "" : o.ToString().Trim();
        }

        private static object NullIfBlank(string value)
        {
            string v = (value ?? "").Trim();
            return v.Length == 0 ? (object)DBNull.Value : v;
        }

        private static void SelectValue(DropDownList ddl, string value)
        {
            ddl.ClearSelection();
            ListItem item = ddl.Items.FindByValue(value);
            if (item != null) item.Selected = true;
        }

        private void ClearProductForm()
        {
            ResetInvalid();
            hfProductId.Value = "";
            txtName.Text = "";
            txtBrand.Text = "";
            txtType.Text = "";
            ddlCategory.SelectedIndex = 0;
            ddlSupplier.SelectedIndex = 0;
            ddlDiscount.SelectedIndex = 0;
            txtSize.Text = "";
            txtAlcohol.Text = "";
            txtFlavour.Text = "";
            txtOrigin.Text = "";
            txtBarcode.Text = "";
            txtCost.Text = "";
            txtPrice.Text = "";
            txtReorder.Text = "";
            txtInitialStock.Text = "0";
            ddlStatus.SelectedIndex = 0;
            txtIngredients.Text = "";
            txtDescription.Text = "";
            litImagePreview.Text = "";
            chkRemoveImage.Checked = false;
        }

        private void ClearStockForm()
        {
            ResetStockInvalid();
            ddlChangeType.SelectedIndex = 0;
            txtQuantity.Text = "";
            ddlReason.SelectedIndex = 0;
            txtNotes.Text = "";
        }

        private void ResetInvalid()
        {
            foreach (WebControl c in new WebControl[]
            {
                txtName, txtBrand, txtType, ddlCategory, ddlSupplier, ddlDiscount, txtSize, txtAlcohol,
                txtFlavour, txtOrigin, txtBarcode, txtCost, txtPrice, txtReorder, txtInitialStock,
                ddlStatus, txtIngredients, txtDescription, fuImage
            })
            {
                c.CssClass = "login-input";
            }
        }

        private void ResetStockInvalid()
        {
            ddlChangeType.CssClass = "login-input";
            txtQuantity.CssClass = "login-input";
            ddlReason.CssClass = "login-input";
            txtNotes.CssClass = "login-input";
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