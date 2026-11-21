using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SamsLiquorShop.Helpers
{
    /// <summary>
    /// Reads a table's columns from INFORMATION_SCHEMA once, so a page can adapt to the real
    /// column names, sizes and types (the same idea as the Manage Categories page).
    /// </summary>
    public class TableSchema
    {
        public class Column
        {
            public string Name;
            public string DataType;      // e.g. varchar, nvarchar, bit, int, date, datetime
            public int MaxLength;        // 0 = not a text column, -1 = (max)
            public bool Nullable;
            public bool HasDefault;
            public bool IsIdentity;

            public bool IsText => DataType.IndexOf("char", StringComparison.OrdinalIgnoreCase) >= 0
                                  || DataType.Equals("text", StringComparison.OrdinalIgnoreCase)
                                  || DataType.Equals("ntext", StringComparison.OrdinalIgnoreCase);

            public bool IsNumericFlag => DataType.Equals("bit", StringComparison.OrdinalIgnoreCase)
                                         || DataType.Equals("int", StringComparison.OrdinalIgnoreCase)
                                         || DataType.Equals("tinyint", StringComparison.OrdinalIgnoreCase)
                                         || DataType.Equals("smallint", StringComparison.OrdinalIgnoreCase);

            public bool IsDateOnly => DataType.Equals("date", StringComparison.OrdinalIgnoreCase);

            /// <summary>Text length limit to apply in the form (falls back when the column is (max)).</summary>
            public int TextLimit(int fallback) => MaxLength > 0 ? MaxLength : fallback;
        }

        private static readonly ConcurrentDictionary<string, TableSchema> Cache =
            new ConcurrentDictionary<string, TableSchema>(StringComparer.OrdinalIgnoreCase);

        public string Table { get; private set; }
        public List<Column> Columns { get; private set; }

        // Re-read every few minutes so columns added by a SQL script show up without restarting the site
        private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(2);
        private DateTime _readAt;

        public static TableSchema Get(string connStr, string table)
        {
            TableSchema s;
            if (Cache.TryGetValue(table, out s) && DateTime.UtcNow - s._readAt < MaxAge) return s;
            s = Read(connStr, table);
            Cache[table] = s;
            return s;
        }

        /// <summary>Forget the cached layout (e.g. after the SQL script has added columns).</summary>
        public static void Forget(string table)
        {
            TableSchema ignored;
            Cache.TryRemove(table, out ignored);
        }

        private static TableSchema Read(string connStr, string table)
        {
            var s = new TableSchema { Table = table, Columns = new List<Column>(), _readAt = DateTime.UtcNow };
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT c.COLUMN_NAME, c.DATA_TYPE, c.CHARACTER_MAXIMUM_LENGTH, c.IS_NULLABLE,
                       CASE WHEN c.COLUMN_DEFAULT IS NULL THEN 0 ELSE 1 END AS HasDefault,
                       ISNULL(COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity'), 0) AS IsIdentity
                FROM INFORMATION_SCHEMA.COLUMNS c
                WHERE c.TABLE_NAME = @Table
                ORDER BY c.ORDINAL_POSITION", conn))
            {
                cmd.Parameters.AddWithValue("@Table", table);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        s.Columns.Add(new Column
                        {
                            Name = r["COLUMN_NAME"].ToString(),
                            DataType = r["DATA_TYPE"].ToString(),
                            MaxLength = r["CHARACTER_MAXIMUM_LENGTH"] == DBNull.Value ? 0 : Convert.ToInt32(r["CHARACTER_MAXIMUM_LENGTH"]),
                            Nullable = r["IS_NULLABLE"].ToString() == "YES",
                            HasDefault = Convert.ToInt32(r["HasDefault"]) == 1,
                            IsIdentity = Convert.ToInt32(r["IsIdentity"]) == 1
                        });
                    }
                }
            }

            if (s.Columns.Count == 0)
                throw new InvalidOperationException("The " + table + " table could not be found in the database.");

            return s;
        }

        public Column Find(string name)
        {
            return Columns.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>First column that exists from a list of possible names (e.g. Supplier_Phone / Supplier_PhoneNumber).</summary>
        public Column FindAny(params string[] names)
        {
            foreach (string n in names)
            {
                Column c = Find(n);
                if (c != null) return c;
            }
            return null;
        }

        /// <summary>
        /// Columns that must get a value on INSERT but that the page doesn't fill in
        /// (NOT NULL, no default, not an identity). Used to give a clear error instead of a SQL one.
        /// </summary>
        public List<string> UnfilledRequired(IEnumerable<string> filledColumns)
        {
            var filled = new HashSet<string>(filledColumns, StringComparer.OrdinalIgnoreCase);
            return Columns.Where(c => !c.Nullable && !c.HasDefault && !c.IsIdentity && !filled.Contains(c.Name))
                          .Select(c => c.Name).ToList();
        }

        /// <summary>Wraps a column name in [ ] for SQL. Names come from INFORMATION_SCHEMA, never from the user.</summary>
        public static string Q(string column)
        {
            return "[" + column.Replace("]", "]]") + "]";
        }
    }
}
