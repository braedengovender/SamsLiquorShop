using System;
using System.Collections.Generic;
using System.Linq;
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
        }

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