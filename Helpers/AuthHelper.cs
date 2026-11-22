using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Configuration; 
using System.Data.SqlClient;

namespace SamsLiquorShop
{
    public class AuthHelper
    {
        public static bool RequireRole(Page page, string requiredRole)
        {
            var session = HttpContext.Current.Session;
            if (session["UserRole"] == null || session["UserRole"].ToString() != requiredRole)
            {
                page.Response.Redirect("~/Login.aspx");
                return false;
            }

            // Staff pages: the account must still be Active and still hold this role
            if ((requiredRole == "Manager" || requiredRole == "Cashier") && session["EmployeeID"] != null)
            {
                string cs = ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;
                using (var conn = new SqlConnection(cs))
                using (var cmd = new SqlCommand(
                    "SELECT Employee_Role FROM Employee WHERE Employee_ID = @Id AND Employee_Status = 'Active'", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", Convert.ToInt32(session["EmployeeID"]));
                    conn.Open();
                    object role = cmd.ExecuteScalar();
                    if (role == null || role.ToString().Trim() != requiredRole)
                    {
                        session.Abandon();
                        page.Response.Redirect("~/Login.aspx");
                        return false;
                    }
                }
            }

            // Customer pages: the profile must still be Active (it may have been deactivated in another session)
            if (requiredRole == "Customer" && session["CustomerID"] != null)
            {
                string cs = ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;
                using (var conn = new SqlConnection(cs))
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Customer WHERE Customer_ID = @Id AND Customer_Status = 'Active'", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", Convert.ToInt32(session["CustomerID"]));
                    conn.Open();
                    if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                    {
                        session.Abandon();
                        page.Response.Redirect("~/Login.aspx?account=deactivated");
                        return false;
                    }
                }
            }
            return true;
        }
    }
}