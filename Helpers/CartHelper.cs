using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SamsLiquorShop.Helpers
{
    [Serializable]
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }

    public static class CartHelper
    {
        private const string CartSessionKey = "Cart";
        private const string WishlistSessionKey = "Wishlist";

        public static List<CartItem> GetCart(System.Web.SessionState.HttpSessionState session)
        {
            if (session[CartSessionKey] == null)
            {
                session[CartSessionKey] = new List<CartItem>();
            }
            return (List<CartItem>)session[CartSessionKey];
        }

        public static void AddToCart(System.Web.SessionState.HttpSessionState session, int productId, string name, decimal price, int quantity = 1)
        {
            var cart = GetCart(session);
            var existing = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem { ProductId = productId, ProductName = name, UnitPrice = price, Quantity = quantity });
            }
        }

        public static HashSet<int> GetWishlist(System.Web.SessionState.HttpSessionState session)
        {
            if (session[WishlistSessionKey] == null)
            {
                session[WishlistSessionKey] = new HashSet<int>();
            }
            return (HashSet<int>)session[WishlistSessionKey];
        }

        public static void ToggleWishlist(System.Web.SessionState.HttpSessionState session, int productId)
        {
            var wishlist = GetWishlist(session);
            if (wishlist.Contains(productId))
                wishlist.Remove(productId);
            else
                wishlist.Add(productId);
        }
    }
}