using System;
using SamsLiquorShop.Helpers;

namespace SamsLiquorShop
{
    public partial class OrderConfirmation : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            try
            {
                object idObj = Session["CustomerID"];
                int customerId;
                if (Session["UserRole"] == null || idObj == null || !int.TryParse(idObj.ToString(), out customerId) || customerId <= 0)
                {
                    Response.Redirect("Login.aspx");
                    return;
                }

                int saleId;
                if (!int.TryParse(Request.QueryString["id"], out saleId) || saleId <= 0)
                {
                    ShowMissing("We couldn't find that order.");
                    return;
                }

                // Only the customer who placed the order can open it
                OnlineReceipt r = OnlineReceiptHelper.Load(saleId, customerId);
                if (r == null)
                {
                    ShowMissing("We couldn't find that order on your account.");
                    return;
                }

                litReceipt.Text = OnlineReceiptHelper.BuildHtml(r, true);
                litLead.Text = Server.HtmlEncode(r.IsDelivery
                    ? "We'll deliver your order on " + OnlineReceiptHelper.SlotText(r) + "."
                      + (string.IsNullOrWhiteSpace(r.CollectionCode) ? "" : " Your collection code is " + r.CollectionCode + " - give it to the driver when they arrive.")
                      + " A copy of this receipt has been emailed to " + r.Email + "."
                    : "Your order will be ready to collect on " + OnlineReceiptHelper.SlotText(r) + ". Your collection code is " + r.CollectionCode
                      + ". A copy of this receipt has been emailed to " + r.Email + ".");

                autoPrint.Visible = Request.QueryString["print"] == "1";
                pnlOk.Visible = true;
            }
            catch (System.Threading.ThreadAbortException) { throw; }   // Response.Redirect
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                ShowMissing("We couldn't load that order right now. Please try again, or look in My Orders.");
            }
        }

        private void ShowMissing(string message)
        {
            litMissing.Text = Server.HtmlEncode(message);
            pnlMissing.Visible = true;
            pnlOk.Visible = false;
        }
    }
}