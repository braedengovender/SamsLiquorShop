using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace SamsLiquorShop
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Background threads (e.g. Session_End emails) also get South African formatting.
            CultureInfo.DefaultThreadCurrentCulture = ShopCulture;
            CultureInfo.DefaultThreadCurrentUICulture = ShopCulture;
        }

        // Prices use "C" / {0:C} all over the site, which follows the server's culture. Azure servers are en-US,
        // so prices showed "$". Pin every request to South Africa (Rand) whatever the host machine is set to.
        // Separators are fixed to "R1,234.56" so the look matches the "R" + N2 strings used elsewhere and doesn't
        // change with the host's Windows version (newer Windows en-ZA uses a comma decimal and space grouping).
        internal static readonly CultureInfo ShopCulture = CreateShopCulture();

        private static CultureInfo CreateShopCulture()
        {
            var c = (CultureInfo)CultureInfo.GetCultureInfo("en-ZA").Clone();
            var n = c.NumberFormat;
            n.CurrencySymbol = "R";
            n.CurrencyDecimalSeparator = ".";
            n.CurrencyGroupSeparator = ",";
            n.CurrencyPositivePattern = 0;   // R1,234.56
            n.CurrencyNegativePattern = 1;   // -R1,234.56
            n.NumberDecimalSeparator = ".";
            n.NumberGroupSeparator = ",";
            n.PercentDecimalSeparator = ".";
            n.PercentGroupSeparator = ",";
            return CultureInfo.ReadOnly(c);
        }

        private static void ApplyShopCulture()
        {
            Thread.CurrentThread.CurrentCulture = ShopCulture;
            Thread.CurrentThread.CurrentUICulture = ShopCulture;
        }

        void Application_BeginRequest(object sender, EventArgs e) { ApplyShopCulture(); }

        // Set again just before the page/handler runs, in case the pipeline moved the request to another thread.
        void Application_PreRequestHandlerExecute(object sender, EventArgs e) { ApplyShopCulture(); }

        // A missing .aspx raises its 404 before session state is loaded. If customErrors drew NotFound.aspx inside
        // that request, the header would always look logged out. Redirecting makes it a normal request with the session.
        void Application_Error(object sender, EventArgs e)
        {
            var http = Server.GetLastError() as HttpException;
            if (http == null || http.GetHttpCode() != 404) return;   // everything else still goes to Error.aspx
            if (Request.Path.EndsWith("/NotFound.aspx", StringComparison.OrdinalIgnoreCase)) return;

            Server.ClearError();
            Response.Redirect("~/NotFound.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        void Session_End(object sender, EventArgs e)
        {
            // Fires when a session times out (InProc session mode only)
            Helpers.SessionWatch.SessionEnded(Session.SessionID);
        }
    }
}