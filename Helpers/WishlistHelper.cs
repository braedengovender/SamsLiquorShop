using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.SessionState;

namespace SamsLiquorShop.Helpers
{
    // Keeps a logged-in customer's wishlist in the Wishlist table so it survives logging out.
    // Every method swallows SQL errors (returns false) so a missing table never breaks the Shop page.
    public static class WishlistHelper
    {
        private static string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        public static bool IsCustomer(HttpSessionState session)
        {
            return session["UserRole"] != null
                && session["UserRole"].ToString() == "Customer"
                && session["CustomerID"] != null;
        }

        // Replaces the session wishlist with what is stored for this customer
        public static void LoadIntoSession(HttpSessionState session)
        {
            if (!IsCustomer(session)) return;

            try
            {
                var ids = new HashSet<int>();
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand("SELECT Product_ID FROM Wishlist WHERE Customer_ID = @C", conn))
                {
                    cmd.Parameters.AddWithValue("@C", Convert.ToInt32(session["CustomerID"]));
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read()) ids.Add(Convert.ToInt32(r["Product_ID"]));
                    }
                }

                var wishlist = CartHelper.GetWishlist(session);
                wishlist.Clear();
                foreach (int id in ids) wishlist.Add(id);
            }
            catch (SqlException)
            {
                // table not created yet - keep using the session wishlist
            }
        }

        // After a heart is toggled in the session, mirror that change in the database (customers only)
        public static void Sync(HttpSessionState session, int productId)
        {
            if (!IsCustomer(session)) return;

            int customerId = Convert.ToInt32(session["CustomerID"]);
            if (CartHelper.GetWishlist(session).Contains(productId))
                Add(customerId, productId);
            else
                Remove(customerId, productId);
        }

        // Adds to the session wishlist and, for a logged-in customer, the database (e.g. "Move to wishlist" in the cart)
        public static void AddToWishlist(HttpSessionState session, int productId)
        {
            CartHelper.GetWishlist(session).Add(productId);
            if (IsCustomer(session)) Add(Convert.ToInt32(session["CustomerID"]), productId);
        }

        // At login: keep hearts a guest added before logging in, and bring back the saved ones
        public static void MergeAtLogin(HttpSessionState session)
        {
            if (!IsCustomer(session)) return;

            int customerId = Convert.ToInt32(session["CustomerID"]);
            foreach (int id in new List<int>(CartHelper.GetWishlist(session)))
                Add(customerId, id);

            LoadIntoSession(session);
        }

        public static bool Add(int customerId, int productId)
        {
            return Run(@"
                IF NOT EXISTS (SELECT 1 FROM Wishlist WHERE Customer_ID = @C AND Product_ID = @P)
                    INSERT INTO Wishlist (Customer_ID, Product_ID) VALUES (@C, @P)", customerId, productId);
        }

        public static bool Remove(int customerId, int productId)
        {
            return Run("DELETE FROM Wishlist WHERE Customer_ID = @C AND Product_ID = @P", customerId, productId);
        }

        private static bool Run(string sql, int customerId, int productId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@C", customerId);
                    cmd.Parameters.AddWithValue("@P", productId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }
    }
}
