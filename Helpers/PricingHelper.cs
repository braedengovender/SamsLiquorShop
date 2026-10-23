using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SamsLiquorShop.Helpers
{
    public static class PricingHelper
    {
        public class PriceInfo
        {
            public decimal ShelfPrice;
            public decimal FinalPrice;
            public bool OnSale => FinalPrice < ShelfPrice;
        }

        public static Dictionary<int, PriceInfo> GetPrices(string connStr, IEnumerable<int> productIds)
        {
            var ids = productIds.Distinct().ToList();
            var result = new Dictionary<int, PriceInfo>();
            if (ids.Count == 0) return result;

            // ids are ints, so joining them into the query is safe
            string query = @"
                SELECT p.Product_ID, p.Product_SellingPrice,
                       p.Discount_ID AS ProductDiscountId, cat.Discount_ID AS CategoryDiscountId,
                       pd.Discount_Name AS ProductDiscountName, pd.Discount_Type AS ProductDiscountType, pd.Discount_Value AS ProductDiscountValue,
                       pd.Discount_Status AS ProductDiscountStatus, pd.Discount_StartDate AS ProductDiscountStart, pd.Discount_EndDate AS ProductDiscountEnd,
                       cd.Discount_Name AS CategoryDiscountName, cd.Discount_Type AS CategoryDiscountType, cd.Discount_Value AS CategoryDiscountValue,
                       cd.Discount_Status AS CategoryDiscountStatus, cd.Discount_StartDate AS CategoryDiscountStart, cd.Discount_EndDate AS CategoryDiscountEnd
                FROM Product p
                INNER JOIN Category cat ON p.Category_ID = cat.Category_ID
                LEFT JOIN Discount pd ON p.Discount_ID = pd.Discount_ID
                LEFT JOIN Discount cd ON cat.Discount_ID = cd.Discount_ID
                WHERE p.Product_ID IN (" + string.Join(",", ids) + ")";

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                da.Fill(dt);
            }

            foreach (DataRow row in dt.Rows)
            {
                decimal basePrice = Convert.ToDecimal(row["Product_SellingPrice"]);
                decimal finalPrice = basePrice;

                if (row["ProductDiscountId"] != DBNull.Value && IsActive(row, "Product"))
                    finalPrice = Apply(basePrice, row["ProductDiscountType"].ToString(), Convert.ToDecimal(row["ProductDiscountValue"]));
                else if (row["CategoryDiscountId"] != DBNull.Value && IsActive(row, "Category"))
                    finalPrice = Apply(basePrice, row["CategoryDiscountType"].ToString(), Convert.ToDecimal(row["CategoryDiscountValue"]));

                result[Convert.ToInt32(row["Product_ID"])] = new PriceInfo { ShelfPrice = basePrice, FinalPrice = finalPrice };
            }

            return result;
        }

        private static bool IsActive(DataRow row, string prefix)
        {
            object statusValue = row[prefix + "DiscountStatus"];
            if (statusValue == DBNull.Value) return false;

            string s = statusValue.ToString().Trim();
            bool active = s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("Active", StringComparison.OrdinalIgnoreCase);
            if (!active) return false;

            DateTime now = DateTime.Now;

            // Same rule as the POS: Wednesday promos only run on Wednesdays
            string promoName = Convert.ToString(row[prefix + "DiscountName"]);
            if (promoName.IndexOf("wednesday", StringComparison.OrdinalIgnoreCase) >= 0 && now.DayOfWeek != DayOfWeek.Wednesday)
                return false;

            return now >= Convert.ToDateTime(row[prefix + "DiscountStart"]) && now <= Convert.ToDateTime(row[prefix + "DiscountEnd"]);
        }

        private static decimal Apply(decimal basePrice, string type, decimal value)
        {
            if (type == "Percentage") return Math.Round(basePrice - (basePrice * value / 100m), 2);
            return Math.Max(0m, basePrice - value);
        }
    }
}