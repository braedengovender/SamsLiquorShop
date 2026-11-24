using System;
using System.Globalization;
using SamsLiquorShop.Helpers;

namespace SamsLiquorShop.Controls
{
    public partial class BlackFridayBanner : System.Web.UI.UserControl
    {
        // The banner shows until the end of this day (South Africa time), then disappears on its own.
        // The 10% itself is a Discount row on the Wine category (Manager > Discounts), so keep the dates in step.
        public static readonly DateTime LastDay = new DateTime(2026, 11, 30);

        // Wines that are on sale: same filters as ticking "Wine" and "On sale" on the shop page
        public const string ShopUrl = "~/Shop.aspx?cat=wine&sale=1";

        /// <summary>"Card" (default): a card in the page. "Bar": a thin strip stuck to the bottom of the window, with an x to close it.</summary>
        public string Layout { get; set; }

        public static bool IsLive => SlotHelper.Now().Date <= LastDay.Date;

        protected string StorageKey => "bfBannerClosed-" + LastDay.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        protected string EndText => LastDay.ToString("dddd d MMMM", CultureInfo.GetCultureInfo("en-ZA"));

        protected void Page_Load(object sender, EventArgs e)
        {
            bool bar = string.Equals(Layout, "Bar", StringComparison.OrdinalIgnoreCase);
            phBanner.Visible = IsLive;
            phCard.Visible = !bar;
            phBar.Visible = bar;
        }
    }
}
