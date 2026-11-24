using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace SamsLiquorShop.Helpers
{
    // Tracks logged-in customers so we can email them when they log out,
    // close the browser/tab, or their session expires.
    public static class SessionWatch
    {
        private class Person { public string Email; public string Name; }
        private class Closing { public DateTime Due; public string UserAgent; public string Ip; }

        private static readonly ConcurrentDictionary<string, Person> people = new ConcurrentDictionary<string, Person>();
        private static readonly ConcurrentDictionary<string, Closing> closing = new ConcurrentDictionary<string, Closing>();
        private static readonly ConcurrentDictionary<string, bool> notified = new ConcurrentDictionary<string, bool>();

        // How long a page must stay silent after unloading before we call it "closed"
        private static readonly TimeSpan Grace = TimeSpan.FromMinutes(2);
        private static readonly Timer ticker = new Timer(Tick, null, 30000, 30000);

        public static void Register(string sessionId, string email, string name)
        {
            people[sessionId] = new Person { Email = email, Name = name };
            Closing c; bool b;
            closing.TryRemove(sessionId, out c);
            notified.TryRemove(sessionId, out b);
        }

        // The customer changed their email address: send later notices to the new one.
        public static void UpdateEmail(string sessionId, string email)
        {
            Person p;
            if (people.TryGetValue(sessionId, out p)) people[sessionId] = new Person { Email = email, Name = p.Name };
        }

        // Called on manual logout. Returns false if this session wasn't being watched.
        public static bool Unregister(string sessionId, out string email, out string name)
        {
            email = null; name = null;
            Closing c; bool b;
            closing.TryRemove(sessionId, out c);
            notified.TryRemove(sessionId, out b);
            Person p;
            if (!people.TryRemove(sessionId, out p)) return false;
            email = p.Email; name = p.Name;
            return true;
        }

        // A page unloaded (tab closed, navigated away, etc). Start the countdown.
        public static void Close(string sessionId, string userAgent, string ip)
        {
            if (!people.ContainsKey(sessionId)) return;
            closing.TryAdd(sessionId, new Closing { Due = DateTime.UtcNow.Add(Grace), UserAgent = userAgent, Ip = ip });
        }

        // A page is open and alive, so cancel any countdown.
        public static void Alive(string sessionId)
        {
            Closing c; bool b;
            closing.TryRemove(sessionId, out c);
            notified.TryRemove(sessionId, out b);
        }

        // Called from Global.asax Session_End
        public static void SessionEnded(string sessionId)
        {
            Person p;
            if (!people.TryRemove(sessionId, out p)) return;
            Closing c; bool wasNotified;
            closing.TryRemove(sessionId, out c);
            if (notified.TryRemove(sessionId, out wasNotified)) return; // already told them
            OtpHelper.SendLoginNotice(p.Email, p.Name, "expired", null, null);
        }

        private static void Tick(object state)
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                foreach (var kv in closing.ToArray())
                {
                    if (kv.Value.Due > now) continue;
                    Closing c;
                    if (!closing.TryRemove(kv.Key, out c)) continue;
                    Person p;
                    if (!people.TryGetValue(kv.Key, out p)) continue;
                    notified[kv.Key] = true;
                    OtpHelper.SendLoginNotice(p.Email, p.Name, "closed", c.UserAgent, c.Ip);
                }
            }
            catch { }
        }
    }
}