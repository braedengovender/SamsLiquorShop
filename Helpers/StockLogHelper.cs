using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;

namespace SamsLiquorShop.Helpers
{
    /// <summary>
    /// Keeps a plain-text record of every manual stock change: when it happened,
    /// which product, old and new quantity, how much it changed by, why, and who did it.
    /// The file is App_Data\StockChanges.log (IIS never serves App_Data, so it can't be
    /// opened from a web address).
    /// </summary>
    public static class StockLogHelper
    {
        private static readonly object FileLock = new object();

        public class StockLogEntry
        {
            public DateTime When { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int OldQuantity { get; set; }
            public int NewQuantity { get; set; }
            public int Change { get { return NewQuantity - OldQuantity; } }
            public string Reason { get; set; }
            public string ChangedBy { get; set; }
        }

        public static string LogPath
        {
            get { return HttpContext.Current.Server.MapPath("~/App_Data/StockChanges.log"); }
        }

        public static void Append(int productId, string productName, int oldQty, int newQty, string reason, string changedBy)
        {
            int change = newQty - oldQty;

            string line = string.Join(" | ", new[]
            {
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                "Product " + productId,
                Clean(productName),
                "Old: " + oldQty,
                "New: " + newQty,
                "Change: " + (change > 0 ? "+" : "") + change,
                "Reason: " + Clean(reason),
                "By: " + Clean(changedBy)
            });

            lock (FileLock)
            {
                string path = LogPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);
            }
        }

        // Whole file, for the "Download stock log" button. Null if nothing has been logged yet.
        public static byte[] ReadAllBytes()
        {
            lock (FileLock)
            {
                string path = LogPath;
                return File.Exists(path) ? File.ReadAllBytes(path) : null;
            }
        }

        // Newest first
        public static List<StockLogEntry> ReadForProduct(int productId, int max)
        {
            var result = new List<StockLogEntry>();

            string[] lines;
            lock (FileLock)
            {
                string path = LogPath;
                if (!File.Exists(path)) return result;
                lines = File.ReadAllLines(path, Encoding.UTF8);
            }

            for (int i = lines.Length - 1; i >= 0 && result.Count < max; i--)
            {
                StockLogEntry entry = Parse(lines[i]);
                if (entry != null && entry.ProductId == productId) result.Add(entry);
            }

            return result;
        }

        // Keeps one change on one line and stops the field separator appearing inside a value
        private static string Clean(string value)
        {
            return (value ?? "").Replace("\r", " ").Replace("\n", " ").Replace("|", "/").Trim();
        }

        private static string After(string text, string prefix)
        {
            return text.StartsWith(prefix, StringComparison.Ordinal) ? text.Substring(prefix.Length) : null;
        }

        private static StockLogEntry Parse(string line)
        {
            string[] p = line.Split(new[] { " | " }, StringSplitOptions.None);
            if (p.Length != 8) return null;

            string idText = After(p[1], "Product ");
            string oldText = After(p[3], "Old: ");
            string newText = After(p[4], "New: ");
            string reason = After(p[6], "Reason: ");
            string by = After(p[7], "By: ");
            if (idText == null || oldText == null || newText == null || reason == null || by == null) return null;

            DateTime when;
            int id, oldQty, newQty;
            if (!DateTime.TryParseExact(p[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out when)) return null;
            if (!int.TryParse(idText, out id) || !int.TryParse(oldText, out oldQty) || !int.TryParse(newText, out newQty)) return null;

            return new StockLogEntry
            {
                When = when,
                ProductId = id,
                ProductName = p[2],
                OldQuantity = oldQty,
                NewQuantity = newQty,
                Reason = reason,
                ChangedBy = by
            };
        }
    }
}