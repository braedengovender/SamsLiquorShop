using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SamsLiquorShop.Helpers
{
    public static class ProductDetailHelper
    {
        public class ProductDetail
        {
            public int ProductId;
            public int CategoryId;
            public int Stock;
            public int RatingPct;
            public int RatingCount;
            public string Name;
            public string Brand;
            public string Category;
            public string Type;
            public string Origin;
            public string Flavour;
            public string Description;
            public string Ingredients;
            public string RatingText;
            public decimal ShelfPrice;
            public decimal FinalPrice;
            public decimal Abv;

            public bool OnSale => FinalPrice < ShelfPrice;
            public int DiscountPercent => (OnSale && ShelfPrice > 0m)
                ? Math.Max(1, (int)Math.Round((ShelfPrice - FinalPrice) / ShelfPrice * 100m, MidpointRounding.AwayFromZero))
                : 0;
        }

        private static readonly string[] OptionalColumns =
        {
            "Product_Description", "Product_Type", "Product_Origin",
            "Product_Flavour", "Product_Ingredients", "Product_AlcoholPercentage"
        };
        private static HashSet<string> _columns;

        private static HashSet<string> GetColumns(string connStr)
        {
            if (_columns != null) return _columns;

            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Product'", conn))
            {
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read()) set.Add(r.GetString(0));
            }
            _columns = set;
            return set;
        }

        private static string Text(DataRow r, string column)
        {
            return r.Table.Columns.Contains(column) ? Convert.ToString(r[column]) : "";
        }

        public static ProductDetail Get(string connStr, int productId)
        {
            var existing = GetColumns(connStr);
            string extra = string.Concat(OptionalColumns.Where(c => existing.Contains(c)).Select(c => ", p.[" + c + "]"));

            string query = "SELECT p.Product_ID, p.Product_Name, p.Product_Brand, p.Product_SellingPrice, p.Product_QuantityInStock, p.Category_ID, cat.Category_Name"
                + extra
                + " FROM Product p INNER JOIN Category cat ON p.Category_ID = cat.Category_ID"
                + " WHERE p.Product_ID = @Id AND p.Product_Status = 'Active'";

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                da.SelectCommand.Parameters.AddWithValue("@Id", productId);
                da.Fill(dt);
            }
            if (dt.Rows.Count == 0) return null;

            DataRow r = dt.Rows[0];
            decimal shelf = Convert.ToDecimal(r["Product_SellingPrice"]);
            var prices = PricingHelper.GetPrices(connStr, new[] { productId });
            decimal final = prices.ContainsKey(productId) ? prices[productId].FinalPrice : shelf;

            // Same temporary seeded rating the shop page uses
            var rng = new Random(productId * 7919 + 13);
            double rating = Math.Round(3.2 + rng.NextDouble() * 1.8, 1);

            return new ProductDetail
            {
                ProductId = productId,
                CategoryId = Convert.ToInt32(r["Category_ID"]),
                Stock = Convert.ToInt32(r["Product_QuantityInStock"]),
                Name = Convert.ToString(r["Product_Name"]),
                Brand = Convert.ToString(r["Product_Brand"]),
                Category = Convert.ToString(r["Category_Name"]),
                Type = Text(r, "Product_Type"),
                Origin = Text(r, "Product_Origin"),
                Flavour = Text(r, "Product_Flavour"),
                Description = Text(r, "Product_Description"),
                Ingredients = Text(r, "Product_Ingredients"),
                Abv = r.Table.Columns.Contains("Product_AlcoholPercentage") && r["Product_AlcoholPercentage"] != DBNull.Value
                    ? Convert.ToDecimal(r["Product_AlcoholPercentage"]) : 0m,
                ShelfPrice = shelf,
                FinalPrice = final,
                RatingText = rating.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
                RatingPct = (int)Math.Round(rating / 5.0 * 100.0),
                RatingCount = rng.Next(4, 160)
            };
        }
    }
}