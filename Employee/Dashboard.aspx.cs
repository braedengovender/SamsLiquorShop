using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SamsLiquorShop.Employee
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Cashier")) return;

            if (!IsPostBack)
            {
                litGreeting.Text = "Welcome back, " + Server.HtmlEncode(Session["UserName"].ToString());

                int employeeId = Convert.ToInt32(Session["EmployeeID"]);
                DateTime monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                LoadSummary(employeeId, monthStart);
                LoadBestCustomer(monthStart);
                LoadMyTransactions(employeeId);
                LoadLowStockItems();
            }
        }

        private void LoadSummary(int employeeId, DateTime monthStart)
        {
            using (SqlConnection conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                string myQuery = @"
                    SELECT 
                        ISNULL(SUM(Sale_TotalAmount), 0) AS MyRevenue,
                        COUNT(*) AS MyCount
                    FROM Sale
                    WHERE Employee_ID = @EmpId
                      AND Sale_DateTime >= @MonthStart";

                using (SqlCommand cmd = new SqlCommand(myQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpId", employeeId);
                    cmd.Parameters.AddWithValue("@MonthStart", monthStart);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            litMyRevenue.Text = Convert.ToDecimal(reader["MyRevenue"]).ToString("C");
                            litMyTxCount.Text = reader["MyCount"].ToString();
                        }
                    }
                }

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Customer", conn))
                {
                    litTotalCustomers.Text = cmd.ExecuteScalar().ToString();
                }

                string lowStockCountQuery = @"
                    SELECT COUNT(*) 
                    FROM Product 
                    WHERE Product_QuantityInStock <= Product_ReorderQuantity 
                      AND Product_Status = 'Active'";

                using (SqlCommand cmd = new SqlCommand(lowStockCountQuery, conn))
                {
                    litLowStockCount.Text = cmd.ExecuteScalar().ToString();
                }
            }
        }

        private void LoadBestCustomer(DateTime monthStart)
        {
            string query = @"
                SELECT TOP 1
                    CONCAT(c.Customer_FirstName, ' ', c.Customer_LastName) AS CustomerName,
                    SUM(s.Sale_TotalAmount) AS TotalSpent,
                    COUNT(s.Sale_ID) AS OrderCount
                FROM Sale s
                INNER JOIN Customer c ON s.Customer_ID = c.Customer_ID
                WHERE s.Sale_DateTime >= @MonthStart
                  AND s.Customer_ID <> 999999
                GROUP BY c.Customer_FirstName, c.Customer_LastName
                ORDER BY TotalSpent DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MonthStart", monthStart);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        litBestCustomerName.Text = Server.HtmlEncode(reader["CustomerName"].ToString());
                        litBestCustomerSpend.Text = Convert.ToDecimal(reader["TotalSpent"]).ToString("C");
                        litBestCustomerOrders.Text = reader["OrderCount"].ToString();
                        phBestCustomer.Visible = true;
                    }
                    else
                    {
                        phBestCustomer.Visible = false;
                    }
                }
            }
        }

        private void LoadMyTransactions(int employeeId)
        {
            string query = @"
                SELECT TOP 10
                    'INV-' + RIGHT('000000' + CAST(s.Sale_ID AS VARCHAR(6)), 6) AS InvoiceNumber,
                    s.Sale_DateTime,
                    ISNULL(CONCAT(c.Customer_FirstName, ' ', c.Customer_LastName), 'Walk-in Customer') AS CustomerName,
                    s.Sale_TotalAmount
                FROM Sale s
                LEFT JOIN Customer c ON s.Customer_ID = c.Customer_ID
                WHERE s.Employee_ID = @EmpId
                ORDER BY s.Sale_DateTime DESC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@EmpId", employeeId);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvMyTransactions.DataSource = dt;
                    gvMyTransactions.DataBind();
                }
            }
        }

        private void LoadLowStockItems()
        {
            string query = @"
                SELECT Product_ID, Product_Name, Product_QuantityInStock, Product_ReorderQuantity
                FROM Product
                WHERE Product_QuantityInStock <= Product_ReorderQuantity
                  AND Product_Status = 'Active'
                ORDER BY Product_QuantityInStock ASC";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvLowStock.DataSource = dt;
                gvLowStock.DataBind();
            }
        }
    }
}