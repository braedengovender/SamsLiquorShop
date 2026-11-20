using SamsLiquorShop.Helpers;
using System;
using System.Configuration;

namespace SamsLiquorShop.Customer
{
    // Printable / downloadable / emailable receipt for one of the logged-in customer's purchases: Receipt.aspx?sale=64
    public partial class Receipt : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;

        private int CustomerId => Convert.ToInt32(Session["CustomerID"]);

        private int SaleId
        {
            get
            {
                int id;
                return int.TryParse(Request.QueryString["sale"], out id) ? id : 0;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthHelper.RequireRole(this, "Customer")) return;

            if (!IsPostBack)
            {
                ShowReceipt();
            }
        }

        private void ShowReceipt()
        {
            ReceiptHelper.Receipt receipt = SaleId > 0 ? ReceiptHelper.Load(ConnStr, SaleId, CustomerId) : null;

            if (receipt == null)
            {
                // Same message whether the sale doesn't exist or belongs to someone else
                litReceipt.Text = "";
                phActions.Visible = false;
                phPdfTip.Visible = false;
                ShowMessage("Receipt not found. It may not belong to your account.", false);
                return;
            }

            // The browser suggests the page title as the PDF file name
            Page.Title = "Receipt " + receipt.InvoiceNumber;
            litReceipt.Text = ReceiptHelper.ToHtml(receipt);

            if (!IsPostBack && Request.QueryString["print"] == "1")
            {
                receiptPrint.Attributes["data-autoprint"] = "1";
            }
        }

        protected void btnEmail_Click(object sender, EventArgs e)
        {
            string message;
            bool sent = ReceiptHelper.Email(ConnStr, SaleId, CustomerId, out message);
            ShowMessage(message, sent);
            ShowReceipt();
        }

        private void ShowMessage(string text, bool success)
        {
            litMessage.Text = "<div class=\"emp-alert " + (success ? "emp-alert-success" : "emp-alert-error") + "\">"
                + Server.HtmlEncode(text) + "</div>";
        }
    }
}
