using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SamsLiquorShop.Helpers
{
    public static class SlotHelper
    {
        public const int DeliveryDaysAhead = 14;
        public const int CollectionDaysAhead = 7;
        public const int CollectionLeadHours = 2;

        public class Slot
        {
            public DateTime Start;
            public DateTime End;
            public string Label;   // e.g. "09:00 - 12:00"
            public string Value    // posted back by the page, e.g. "2026-11-24T09:00"
            {
                get { return Start.ToString("yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture); }
            }
        }

        // ---- Public holidays: the ONE place to edit. Verify against the official government list each year
        // (and add any one-off holidays such as election days).
        private static readonly HashSet<DateTime> PublicHolidays = new HashSet<DateTime>(new[]
        {
            // 2026
            new DateTime(2026,1,1),  new DateTime(2026,3,21), new DateTime(2026,4,3),  new DateTime(2026,4,6),
            new DateTime(2026,4,27), new DateTime(2026,5,1),  new DateTime(2026,6,16), new DateTime(2026,8,10),
            new DateTime(2026,9,24), new DateTime(2026,12,16),new DateTime(2026,12,25),new DateTime(2026,12,26),
            // 2027
            new DateTime(2027,1,1),  new DateTime(2027,3,22), new DateTime(2027,3,26), new DateTime(2027,3,29),
            new DateTime(2027,4,27), new DateTime(2027,5,1),  new DateTime(2027,6,16), new DateTime(2027,8,9),
            new DateTime(2027,9,24), new DateTime(2027,12,16),new DateTime(2027,12,27),new DateTime(2027,12,25)
        });

        private static TimeZoneInfo _tz;

        /// <summary>Current time in South Africa, whatever time zone the server is set to (Azure is UTC).</summary>
        public static DateTime Now()
        {
            try
            {
                if (_tz == null) _tz = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
                return TimeZoneInfo.ConvertTime(DateTime.UtcNow, _tz);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("SlotHelper.Now: " + ex);
                return DateTime.UtcNow.AddHours(2);   // SA has no daylight saving, so a fixed +2 is a safe fallback
            }
        }

        public static bool IsPublicHoliday(DateTime date)
        {
            return PublicHolidays.Contains(date.Date);
        }

        /// <summary>Opening and closing time for a date (Sun and holidays 10-15, Fri-Sat 09-20, otherwise 09-18).</summary>
        public static void GetHours(DateTime date, out TimeSpan open, out TimeSpan close)
        {
            if (date.DayOfWeek == DayOfWeek.Sunday || IsPublicHoliday(date))
            { open = TimeSpan.FromHours(10); close = TimeSpan.FromHours(15); }
            else if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday)
            { open = TimeSpan.FromHours(9); close = TimeSpan.FromHours(20); }
            else
            { open = TimeSpan.FromHours(9); close = TimeSpan.FromHours(18); }
        }

        private static Slot MakeSlot(DateTime date, int startHour, int endHour)
        {
            DateTime s = date.Date.AddHours(startHour);
            DateTime e = date.Date.AddHours(endHour);
            return new Slot { Start = s, End = e, Label = s.ToString("HH:mm") + " - " + e.ToString("HH:mm") };
        }

        /// <summary>Delivery windows for one date (3-hour windows, plus 18-20 on Fri/Sat).</summary>
        public static List<Slot> GetDeliveryWindows(DateTime date)
        {
            var list = new List<Slot>();
            bool sunOrHoliday = date.DayOfWeek == DayOfWeek.Sunday || IsPublicHoliday(date);

            if (sunOrHoliday)
            {
                list.Add(MakeSlot(date, 10, 13));
                list.Add(MakeSlot(date, 13, 15));
                return list;
            }

            list.Add(MakeSlot(date, 9, 12));
            list.Add(MakeSlot(date, 12, 15));
            list.Add(MakeSlot(date, 15, 18));
            if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday)
                list.Add(MakeSlot(date, 18, 20));
            return list;
        }

        /// <summary>All bookable delivery windows: from tomorrow up to 14 days ahead.</summary>
        public static List<Slot> GetDeliverySlots(DateTime now)
        {
            var list = new List<Slot>();
            for (int d = 1; d <= DeliveryDaysAhead; d++)
                list.AddRange(GetDeliveryWindows(now.Date.AddDays(d)));
            return list;
        }

        /// <summary>All bookable collection slots: 1-hour, earliest 2 hours from now, up to 7 days ahead, ending by closing time.</summary>
        public static List<Slot> GetCollectionSlots(DateTime now)
        {
            var list = new List<Slot>();
            DateTime earliest = now.AddHours(CollectionLeadHours);

            for (int d = 0; d <= CollectionDaysAhead; d++)
            {
                DateTime date = now.Date.AddDays(d);
                TimeSpan open, close;
                GetHours(date, out open, out close);

                for (int h = (int)open.TotalHours; h + 1 <= (int)close.TotalHours; h++)
                {
                    Slot s = MakeSlot(date, h, h + 1);
                    if (s.Start >= earliest) list.Add(s);
                }
            }
            return list;
        }

        /// <summary>Parses the posted slot value. False for blank or malformed input.</summary>
        public static bool TryParseSlot(string value, out DateTime start)
        {
            return DateTime.TryParseExact((value ?? "").Trim(), "yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture,
                                          DateTimeStyles.None, out start);
        }

        /// <summary>Server-side check that a posted delivery slot is still bookable. Use again inside PlaceOrder.</summary>
        public static bool IsValidDeliverySlot(DateTime start, DateTime now)
        {
            return GetDeliverySlots(now).Any(s => s.Start == start);
        }

        /// <summary>Server-side check that a posted collection slot is still bookable. graceMinutes forgives a customer who took a while at checkout.</summary>
        public static bool IsValidCollectionSlot(DateTime start, DateTime now, int graceMinutes = 10)
        {
            return GetCollectionSlots(now.AddMinutes(-Math.Abs(graceMinutes))).Any(s => s.Start == start);
        }
    }
}