using System.Web;

namespace SamsLiquorShop
{
    // Deliberately has no session access, so the heartbeat doesn't extend the session timeout.
    public class SessionBeacon : IHttpHandler
    {
        public bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpCookie cookie = context.Request.Cookies["ASP.NET_SessionId"];

            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                if (context.Request.QueryString["a"] == "close")
                    Helpers.SessionWatch.Close(cookie.Value, context.Request.UserAgent, context.Request.UserHostAddress);
                else
                    Helpers.SessionWatch.Alive(cookie.Value);
            }

            context.Response.StatusCode = 204;
        }
    }
}