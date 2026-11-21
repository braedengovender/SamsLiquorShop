using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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
            if (e.CommandName != "EditCategory") return;

            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            litMessage.Text = "";
            LoadForEdit(id);
        }

        // ---------- ADD ----------
        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            litMessage.Text = "";
            hfCategoryId.Value = "";
            txtName.Text = "";
            txtDescription.Text = "";
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
