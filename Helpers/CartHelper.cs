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
        public decimal ShelfPrice { get; set; }   // original price before any discount
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

            // logged-in customers: keep the database wishlist in step
            WishlistHelper.Sync(session, productId);
        }

        public static void UpdateQuantity(System.Web.SessionState.HttpSessionState session, int productId, int quantity)
        {
            var cart = GetCart(session);
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item == null) return;

            if (quantity <= 0)
                cart.Remove(item);
            else
                item.Quantity = quantity;
        }

        public static void RemoveFromCart(System.Web.SessionState.HttpSessionState session, int productId)
        {
            var cart = GetCart(session);
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null) cart.Remove(item);
        }

        public static void ClearCart(System.Web.SessionState.HttpSessionState session)
        {
            session[CartSessionKey] = new List<CartItem>();
        }

        public static int GetCartCount(System.Web.SessionState.HttpSessionState session) =>
            GetCart(session).Sum(c => c.Quantity);

        public static decimal GetCartTotal(System.Web.SessionState.HttpSessionState session) =>
            GetCart(session).Sum(c => c.UnitPrice * c.Quantity);

        private const string SavedForLaterSessionKey = "SavedForLater";

        public static List<CartItem> GetSavedForLater(System.Web.SessionState.HttpSessionState session)
        {
            if (session[SavedForLaterSessionKey] == null)
            {
                session[SavedForLaterSessionKey] = new List<CartItem>();
            }
            return (List<CartItem>)session[SavedForLaterSessionKey];
        }

        public static void MoveToSavedForLater(System.Web.SessionState.HttpSessionState session, int productId)
        {
            var cart = GetCart(session);
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item == null) return;

            cart.Remove(item);

            var saved = GetSavedForLater(session);
            var existing = saved.FirstOrDefault(c => c.ProductId == productId);
            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                saved.Add(item);
        }

        public static void MoveToCart(System.Web.SessionState.HttpSessionState session, int productId)
        {
            var saved = GetSavedForLater(session);
            var item = saved.FirstOrDefault(c => c.ProductId == productId);
            if (item == null) return;

            saved.Remove(item);
            AddToCart(session, item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);
        }

        public static void RemoveFromSavedForLater(System.Web.SessionState.HttpSessionState session, int productId)
        {
            var saved = GetSavedForLater(session);
            var item = saved.FirstOrDefault(c => c.ProductId == productId);
            if (item != null) saved.Remove(item);
        }
    }
}