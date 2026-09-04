using SamsLiquorShop.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SamsLiquorShop
{
    public partial class Shop : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCategories();

                // Support the old ?cat= query string temporarily by matching category ID directly
                string catParam = Request.QueryString["cat"];
                if (!string.IsNullOrEmpty(catParam) && int.TryParse(catParam, out int catId))
                {
                    foreach (ListItem item in cblCategories.Items)
                    {
                        if (item.Value == catId.ToString())
                        {
                            item.Selected = true;
                        }
                    }
                }

                LoadProducts();
            }
        }

        private void LoadCategories()
        {
            string query = "SELECT Category_ID, Category_Name FROM Category ORDER BY Category_Name";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                cblCategories.DataSource = dt;
                cblCategories.DataTextField = "Category_Name";
                cblCategories.DataValueField = "Category_ID";
                cblCategories.DataBind();
            }
        }

        private void LoadProducts()
        {
            var whereClauses = new List<string> { "p.Product_Status = 'Active'" };
            var parameters = new List<SqlParameter>();

            // Search
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                whereClauses.Add("(p.Product_Name LIKE @Search OR p.Product_Brand LIKE @Search OR p.Product_Description LIKE @Search)");
                parameters.Add(new SqlParameter("@Search", "%" + txtSearch.Text.Trim() + "%"));
            }

            // Category filter
            var selectedCategoryIds = new List<string>();
            foreach (ListItem item in cblCategories.Items)
            {
                if (item.Selected) selectedCategoryIds.Add(item.Value);
            }
            if (selectedCategoryIds.Count > 0)
            {
                whereClauses.Add("p.Category_ID IN (" + string.Join(",", selectedCategoryIds) + ")");
            }

            // Price range
            if (decimal.TryParse(txtMinPrice.Text, out decimal minPrice))
            {
                whereClauses.Add("p.Product_SellingPrice >= @MinPrice");
                parameters.Add(new SqlParameter("@MinPrice", minPrice));
            }
            if (decimal.TryParse(txtMaxPrice.Text, out decimal maxPrice))
            {
                whereClauses.Add("p.Product_SellingPrice <= @MaxPrice");
                parameters.Add(new SqlParameter("@MaxPrice", maxPrice));
            }

            // Sort
            string orderBy = "p.Product_Name ASC";
            switch (ddlSort.SelectedValue)
            {
                case "price_asc": orderBy = "p.Product_SellingPrice ASC"; break;
                case "price_desc": orderBy = "p.Product_SellingPrice DESC"; break;
                case "name_asc": orderBy = "p.Product_Name ASC"; break;
                case "featured": orderBy = "p.Product_ID ASC"; break;
            }

            string query = $@"
                SELECT 
                    p.Product_ID, p.Product_Name, p.Product_Brand, p.Product_SellingPrice, 
                    p.Product_QuantityInStock, p.Category_ID, p.Discount_ID AS ProductDiscountId,
                    cat.Discount_ID AS CategoryDiscountId,
                    pd.Discount_Type AS ProductDiscountType, pd.Discount_Value AS ProductDiscountValue, 
                    pd.Discount_Status AS ProductDiscountStatus, pd.Discount_StartDate AS ProductDiscountStart, pd.Discount_EndDate AS ProductDiscountEnd,
                    cd.Discount_Type AS CategoryDiscountType, cd.Discount_Value AS CategoryDiscountValue,
                    cd.Discount_Status AS CategoryDiscountStatus, cd.Discount_StartDate AS CategoryDiscountStart, cd.Discount_EndDate AS CategoryDiscountEnd
                FROM Product p
                INNER JOIN Category cat ON p.Category_ID = cat.Category_ID
                LEFT JOIN Discount pd ON p.Discount_ID = pd.Discount_ID
                LEFT JOIN Discount cd ON cat.Discount_ID = cd.Discount_ID
                WHERE {string.Join(" AND ", whereClauses)}
                ORDER BY {orderBy}";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddRange(parameters.ToArray());
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Compute effective discounted price per row (product's own discount takes priority over category's)
                    dt.Columns.Add("DiscountedPrice", typeof(decimal));
                    foreach (DataRow row in dt.Rows)
                    {
                        decimal basePrice = Convert.ToDecimal(row["Product_SellingPrice"]);
                        decimal finalPrice = basePrice;

                        bool hasProductDiscount = row["ProductDiscountId"] != DBNull.Value && IsDiscountActive(row, "Product");
                        bool hasCategoryDiscount = row["CategoryDiscountId"] != DBNull.Value && IsDiscountActive(row, "Category");

                        if (hasProductDiscount)
                        {
                            finalPrice = ApplyDiscount(basePrice, row["ProductDiscountType"].ToString(), Convert.ToDecimal(row["ProductDiscountValue"]));
                        }
                        else if (hasCategoryDiscount)
                        {
                            finalPrice = ApplyDiscount(basePrice, row["CategoryDiscountType"].ToString(), Convert.ToDecimal(row["CategoryDiscountValue"]));
                        }

                        row["DiscountedPrice"] = finalPrice;
                    }

                    rptProducts.DataSource = dt;
                    rptProducts.DataBind();

                    litResultCount.Text = dt.Rows.Count + (dt.Rows.Count == 1 ? " product found" : " products found");
                    litNoResults.Visible = dt.Rows.Count == 0;
                }
            }
        }

        private bool IsDiscountActive(DataRow row, string prefix)
        {
            object statusValue = row[prefix + "DiscountStatus"];
            if (statusValue == DBNull.Value) return false;

            string statusStr = statusValue.ToString().Trim();
            bool isActive = statusStr == "1"
                || statusStr.Equals("true", StringComparison.OrdinalIgnoreCase)
                || statusStr.Equals("Active", StringComparison.OrdinalIgnoreCase);

            if (!isActive) return false;

            DateTime start = Convert.ToDateTime(row[prefix + "DiscountStart"]);
            DateTime end = Convert.ToDateTime(row[prefix + "DiscountEnd"]);
            DateTime now = DateTime.Now;

            return now >= start && now <= end;
        }

        private decimal ApplyDiscount(decimal basePrice, string discountType, decimal discountValue)
        {
            if (discountType == "Percentage")
            {
                return Math.Round(basePrice - (basePrice * discountValue / 100m), 2);
            }
            else // Fixed
            {
                decimal result = basePrice - discountValue;
                return result < 0 ? 0 : result;
            }
        }

        protected bool IsWishlisted(int productId)
        {
            return CartHelper.GetWishlist(Session).Contains(productId);
        }

        protected void rptProducts_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int productId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "AddToCart")
            {
                string query = "SELECT Product_Name, Product_SellingPrice FROM Product WHERE Product_ID = @Id";
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", productId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string productName = reader["Product_Name"].ToString();
                            CartHelper.AddToCart(Session, productId, productName, Convert.ToDecimal(reader["Product_SellingPrice"]));

                            litCartMessage.Text = "<div class='shop-cart-toast'>" + Server.HtmlEncode(productName) + " added to cart.</div>";
                            litCartMessage.Visible = true;
                        }
                    }
                }
            }
            else if (e.CommandName == "ToggleWishlist")
            {
                CartHelper.ToggleWishlist(Session, productId);
            }

            LoadProducts();
        }

        protected void btnSearch_Click(object sender, EventArgs e) => LoadProducts();
        protected void cblCategories_Changed(object sender, EventArgs e) => LoadProducts();
        protected void ddlSort_Changed(object sender, EventArgs e) => LoadProducts();
        protected void btnApplyPrice_Click(object sender, EventArgs e) => LoadProducts();

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            txtMinPrice.Text = "";
            txtMaxPrice.Text = "";
            ddlSort.SelectedValue = "featured";
            foreach (ListItem item in cblCategories.Items) item.Selected = false;
            LoadProducts();
        }
    }
}