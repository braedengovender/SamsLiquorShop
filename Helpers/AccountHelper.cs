using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace SamsLiquorShop.Helpers
{
    // Shared account checks used by Register.aspx, the account/staff/customer forms, the newsletter
    // signup and the live CheckAvailability.ashx endpoint.
    public static class AccountHelper
    {
        private static string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        public const int MaxNameLength = 50;

        // First names and surnames: letters, with a single space, hyphen or apostrophe between them
        // ("Mary Anne", "Smith-Jones", "O'Brien", "van der Merwe"). No numbers or other symbols.
        // Matches data-letters / isValidPersonName in Site.Master.
        private static readonly Regex NamePattern = new Regex(@"^[A-Za-z]+([ '-][A-Za-z]+)*$", RegexOptions.Compiled);

        public static bool IsValidName(string value)
        {
            value = (value ?? "").Trim();
            return value.Length > 0 && value.Length <= MaxNameLength && NamePattern.IsMatch(value);
        }

        // Null when valid, otherwise a message such as "First name can only contain letters, spaces, hyphens and apostrophes."
        public static string NameError(string value, string label)
        {
            value = (value ?? "").Trim();
            if (value.Length == 0) return label + " is required.";
            if (value.Length > MaxNameLength) return label + " can be at most " + MaxNameLength + " characters.";
            if (!NamePattern.IsMatch(value)) return label + " can only contain letters, spaces, hyphens and apostrophes.";
            return null;
        }

        // Login accepts a username OR an email, so neither may collide with
        // another customer's or employee's username or email.
        public static bool LoginNameTaken(string value)
        {
            return LoginNameTaken(value, null, null);
        }

        // Same check, but ignores the account that is editing itself, so keeping your own
        // username or email doesn't count as "taken".
        public static bool LoginNameTaken(string value, int? exceptCustomerId, int? exceptEmployeeId)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT
                    (SELECT COUNT(*) FROM Customer WHERE (Customer_Username = @V OR Customer_EmailAddress = @V)
                                                     AND (@C IS NULL OR Customer_ID <> @C))
                  + (SELECT COUNT(*) FROM Employee WHERE (Employee_Username = @V OR Employee_EmailAddress = @V)
                                                     AND (@E IS NULL OR Employee_ID <> @E))", conn))
            {
                cmd.Parameters.AddWithValue("@V", value);
                cmd.Parameters.Add("@C", System.Data.SqlDbType.Int).Value = (object)exceptCustomerId ?? DBNull.Value;
                cmd.Parameters.Add("@E", System.Data.SqlDbType.Int).Value = (object)exceptEmployeeId ?? DBNull.Value;
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }
}
