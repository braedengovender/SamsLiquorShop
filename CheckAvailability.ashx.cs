using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using SamsLiquorShop.Helpers;

namespace SamsLiquorShop
{
    // Live "is this username / email free?" check for Register.aspx and the staff My Account pages.
    // GET CheckAvailability.ashx?type=email|username&value=...
    // Returns {"available":true|false,"message":"..."}. Saving always re-checks on the server anyway.
    // When someone is logged in, their own account is ignored, so keeping their current value isn't "taken".
    public class CheckAvailability : IHttpHandler, IReadOnlySessionState
    {
        private const int MaxChecksPerMinute = 60;

        private static readonly Regex EmailPattern = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
        private static readonly Regex CustomerUsernamePattern = new Regex(@"^[a-zA-Z0-9_]{4,30}$");
        private static readonly Regex StaffUsernamePattern = new Regex(@"^[A-Za-z0-9_.]{3,30}$");   // same as Manage Employees

        public bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            string type = (context.Request.QueryString["type"] ?? "").ToLowerInvariant();
            string value = (context.Request.QueryString["value"] ?? "").Trim();

            // Who is asking (if anyone)
            string role = context.Session != null ? Convert.ToString(context.Session["UserRole"]) : "";
            bool staff = role == "Manager" || role == "Cashier";
            int? customerId = role == "Customer" ? ToId(context.Session["CustomerID"]) : null;
            int? employeeId = staff ? ToId(context.Session["EmployeeID"]) : null;
            bool loggedIn = customerId.HasValue || employeeId.HasValue;

            // Managers editing someone else (Manage Customers / Manage Employees): ignore that record instead of their own.
            // &scope=customer|employee&for=<id>, with no id for a new record.
            if (role == "Manager")
            {
                string scope = (context.Request.QueryString["scope"] ?? "").ToLowerInvariant();
                int? forId = ToId(context.Request.QueryString["for"]);
                if (scope == "customer") { customerId = forId; employeeId = null; }
                else if (scope == "employee") { employeeId = forId; customerId = null; }
            }

            if (TooManyChecks(context.Request.UserHostAddress))
            {
                Write(context, null, "Too many checks. We'll confirm when you save.");
                return;
            }

            if (type == "email")
            {
                if (value.Length == 0 || value.Length > 100 || !EmailPattern.IsMatch(value))
                {
                    Write(context, null, "");
                    return;
                }
            }
            else if (type == "username")
            {
                if (!(staff ? StaffUsernamePattern : CustomerUsernamePattern).IsMatch(value))
                {
                    Write(context, null, "");
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = 400;
                return;
            }

            bool taken;
            try
            {
                taken = AccountHelper.LoginNameTaken(value, customerId, employeeId);
            }
            catch (Exception)
            {
                Write(context, null, "");
                return;
            }

            if (type == "email")
                Write(context, !taken, !taken ? "Email is available."
                    : loggedIn ? "That email address is already in use by another account."
                               : "An account with this email already exists. Try logging in instead.");
            else
                Write(context, !taken, taken ? "That username is already taken." : "Username is available.");
        }

        private static int? ToId(object o)
        {
            int id;
            return int.TryParse(Convert.ToString(o), out id) && id > 0 ? id : (int?)null;
        }

        // Simple per-IP limit so the endpoint can't be used to scan lots of addresses quickly.
        private static bool TooManyChecks(string ip)
        {
            string key = "AvailChecks:" + (ip ?? "unknown");
            var cache = HttpRuntime.Cache;
            lock (CounterLock)
            {
                var counter = cache[key] as Counter;
                if (counter == null)
                {
                    counter = new Counter();
                    cache.Insert(key, counter, null, DateTime.UtcNow.AddMinutes(1), Cache.NoSlidingExpiration);
                }
                if (counter.Count >= MaxChecksPerMinute) return true;
                counter.Count++;
                return false;
            }
        }

        private static readonly object CounterLock = new object();
        private class Counter { public int Count; }

        // available: true / false, or null when there's nothing to say (bad format, error).
        private static void Write(HttpContext context, bool? available, string message)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(new { available, message }));
        }
    }
}
