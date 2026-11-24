using System.Collections.Generic;
using System.Web.SessionState;

namespace SamsLiquorShop.Helpers
{
    // Products the visitor has opened (newest first), kept in Session like the cart.
    // Recorded when a product pop-up opens on Shop or Specials; shown by Controls/RecentlyViewed.ascx.
    public static class RecentlyViewedHelper
    {
        private const string Key = "RecentlyViewed";
        public const int MaxItems = 10;

        public static void Add(HttpSessionState session, int productId)
        {
            if (session == null || productId <= 0) return;
            var list = Get(session);
            list.Remove(productId);
            list.Insert(0, productId);
            if (list.Count > MaxItems) list.RemoveRange(MaxItems, list.Count - MaxItems);
            session[Key] = list;
        }

        /// <summary>The live list (newest first). Never null.</summary>
        public static List<int> Get(HttpSessionState session)
        {
            var list = session?[Key] as List<int>;
            if (list == null)
            {
                list = new List<int>();
                if (session != null) session[Key] = list;
            }
            return list;
        }

        public static void Clear(HttpSessionState session)
        {
            session?.Remove(Key);
        }
    }
}
