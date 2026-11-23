using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;

namespace SamsLiquorShop
{
    public partial class Rewards : Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Matches the login-state pattern used in Site.Master.cs
            if (Session["UserRole"] == null) return;

            phSignedOut.Visible = false;
            phSignedIn.Visible = true;

            try
            {
                ShowBalance();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Rewards: could not load points balance: " + ex);
                litPoints.Text = "-";
                litWorth.Text = "points";
                litPointsNote.Text = "We couldn't load your balance right now. Please refresh the page in a moment.";
            }
        }

        private void ShowBalance()
        {
            // Only customer accounts have points (staff log in through the same session)
            string role = Convert.ToString(Session["UserRole"]).Trim();
            int customerId;
            if (!role.Equals("Customer", StringComparison.OrdinalIgnoreCase)
                || Session["CustomerID"] == null
                || !int.TryParse(Session["CustomerID"].ToString(), out customerId) || customerId <= 0)
            {
                litPoints.Text = "-";
                litWorth.Text = "staff account";
                litPointsNote.Text = "Loyalty points are earned on customer accounts. Log in with a customer account to see a balance.";
                return;
            }

            int balance;
            bool active;
            int orders;
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Customer_LoyaltyPointsBalance, Customer_Status FROM Customer WHERE Customer_ID = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", customerId);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            litPoints.Text = "-";
                            litWorth.Text = "points";
                            litPointsNote.Text = "We couldn't find your account. Please log in again.";
                            return;
                        }
                        balance = r.IsDBNull(0) ? 0 : Math.Max(0, Convert.ToInt32(r[0]));
                        active = !string.Equals(Convert.ToString(r[1]).Trim(), "Inactive", StringComparison.OrdinalIgnoreCase);
                    }
                }

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Sale WHERE Customer_ID = @Id AND Sale_Status <> 'Cancelled'", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", customerId);
                    orders = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            // 1 point = R1 when redeeming (same rule as the POS)
            litPoints.Text = balance.ToString("N0", CultureInfo.InvariantCulture);
            litWorth.Text = (balance == 1 ? "point" : "points") + " &middot; worth R" + balance.ToString("N0", CultureInfo.InvariantCulture);

            if (!active)
                litPointsNote.Text = "Your account is inactive, so new orders won't earn points. Speak to us in store if you think that's a mistake.";
            else if (balance == 0 && orders == 0)
                litPointsNote.Text = "Your balance updates after your first order.";
            else if (balance == 0)
                litPointsNote.Text = "You have no points to spend yet. You earn 1 point for every R10 you spend.";
            else
                litPointsNote.Text = "You can pay for a whole order with points at checkout when your balance covers the total.";
        }
    }
}