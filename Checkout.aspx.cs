using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using SamsLiquorShop.Helpers;

namespace SamsLiquorShop
{
    public partial class Checkout : System.Web.UI.Page
    {
        private Quote _quote;   // worked out once per request; cleared whenever something that changes the total changes

        // ---------- page life cycle ----------

        protected void Page_Load(object sender, EventArgs e)
        {
            litMessage.Text = "";
            try
            {
                if (Session["UserRole"] == null)
                {
                    Response.Redirect("Login.aspx?returnUrl=Checkout.aspx");
                    return;
                }
                if (!CurrentCustomerId().HasValue)
                {
                    ShowBlocked("Checkout is only available to customer accounts. Please log in with a customer account.");
                    return;
                }
                if (!IsPostBack) InitPage();
            }
            catch (System.Threading.ThreadAbortException) { throw; }   // Response.Redirect
            catch (Exception ex)
            {
                LogError(ex);
                ShowBlocked("We couldn't load the checkout page. Please go back to your cart and try again.");
            }
        }

        private void InitPage()
        {
            if (CartHelper.GetCart(Session).Count == 0)
            {
                Response.Redirect("Cart.aspx");
                return;
            }

            ViewState["Token"] = Guid.NewGuid().ToString("N");   // guards against double submits
            txtCity.Text = OrderHelper.FixedCity;
            txtProvince.Text = OrderHelper.FixedProvince;
            txtPromo.Text = Session["PromoCode"] as string ?? "";

            CustomerProfile p = OrderHelper.GetCustomerProfile(CurrentCustomerId().Value);
            if (p != null)
            {
                txtFirstName.Text = p.FirstName;
                txtLastName.Text = p.LastName;
                txtEmail.Text = p.Email;
                txtPhone.Text = p.Phone;
                txtUnitNumber.Text = p.UnitNumber;
                txtUnitName.Text = p.UnitName;
                txtStreetNumber.Text = p.StreetNumber;
                txtStreetName.Text = p.StreetName;
                txtSuburb.Text = p.Suburb;
                txtPostalCode.Text = p.PostalCode;
            }
            else
            {
                AddMessage("We couldn't load your saved details, so please fill them in below.", true);
            }

            // Delivery needs R250 of goods: start on collection when the cart is below that
            Quote q = LoadQuote(OrderHelper.MethodDelivery, false, Session["PromoCode"] as string);
            rblMethod.SelectedValue = (q.Ok && !q.DeliveryAllowed) ? OrderHelper.MethodCollection : OrderHelper.MethodDelivery;

            BindSlots();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!pnlForm.Visible) return;
            try
            {
                bool delivery = rblMethod.SelectedValue == OrderHelper.MethodDelivery;
                pnlAddress.Visible = delivery;
                pnlCollectNote.Visible = !delivery;
                litStepTitle.Text = delivery ? "Delivery address and time" : "Collection time";

                Quote q = GetQuote();

                // A code that stopped being valid is dropped (same as the cart); a database hiccup keeps it
                string stored = Session["PromoCode"] as string;
                if (q.Ok && !string.IsNullOrEmpty(stored) && !q.PromoValid && !q.PromoError)
                {
                    Session["PromoCode"] = null;
                    txtPromo.Text = "";
                    litPromoMsg.Text = "<span class='field-hint invalid'>" + Server.HtmlEncode(q.PromoMessage ?? "That promo code is no longer valid.") + " The code was removed.</span>";
                }
                else if (q.Ok && q.PromoValid && string.IsNullOrEmpty(litPromoMsg.Text))
                {
                    litPromoMsg.Text = "<span class='field-hint valid'>" + Server.HtmlEncode(q.PromoMessage) + "</span>";
                }

                BindPoints(q);
                chkUsePoints.Checked = q.Ok && q.UsePoints;
                pnlCard.Visible = q.Ok && !q.UsePoints && q.Total > 0m;

                BindSummary(q);
                ViewState["ExpectedTotal"] = q.Ok ? (object)q.Total : null;
                btnPlaceOrder.Enabled = q.Ok && q.DeliveryAllowed;       // delivery needs R250 of goods; collection has no minimum
                btnSwitchCollect.Visible = q.Ok && !q.DeliveryAllowed;
            }
            catch (Exception ex)
            {
                LogError(ex);
                litSummary.Text = "";
                btnPlaceOrder.Enabled = false;
                AddMessage("We couldn't calculate your totals right now. Please refresh the page.", true);
            }
        }

        // ---------- events ----------

        protected void rblMethod_Changed(object sender, EventArgs e)
        {
            BindSlots();
            InvalidateQuote();
        }

        protected void btnSwitchCollect_Click(object sender, EventArgs e)
        {
            rblMethod.SelectedValue = OrderHelper.MethodCollection;
            BindSlots();
            InvalidateQuote();
        }

        protected void chkUsePoints_Changed(object sender, EventArgs e)
        {
            InvalidateQuote();
        }

        protected void btnApplyPromo_Click(object sender, EventArgs e)
        {
            try
            {
                string code = (txtPromo.Text ?? "").Trim();
                if (code.Length == 0)
                {
                    litPromoMsg.Text = "<span class='field-hint invalid'>Please enter a promo code.</span>";
                    return;
                }
                if (!Regex.IsMatch(code, "^[A-Za-z0-9_-]{1,20}$"))
                {
                    litPromoMsg.Text = "<span class='field-hint invalid'>Promo codes only contain letters, numbers, - and _.</span>";
                    return;
                }

                Quote q = LoadQuote(rblMethod.SelectedValue, chkUsePoints.Checked, code);
                if (!q.Ok)
                {
                    litPromoMsg.Text = "<span class='field-hint invalid'>" + Server.HtmlEncode(q.Error) + "</span>";
                }
                else if (q.PromoValid)
                {
                    Session["PromoCode"] = q.PromoCode;
                    litPromoMsg.Text = "<span class='field-hint valid'>" + Server.HtmlEncode(q.PromoMessage) + "</span>";
                }
                else
                {
                    if (!q.PromoError) Session["PromoCode"] = null;   // a wrong code replaces the old one, a database hiccup doesn't
                    litPromoMsg.Text = "<span class='field-hint invalid'>" + Server.HtmlEncode(q.PromoMessage ?? "That promo code isn't valid.") + "</span>";
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                litPromoMsg.Text = "<span class='field-hint invalid'>We couldn't check that promo code right now. Please try again.</span>";
            }
            InvalidateQuote();
        }

        protected void btnRemovePromo_Click(object sender, EventArgs e)
        {
            Session["PromoCode"] = null;
            txtPromo.Text = "";
            litPromoMsg.Text = "<span class='field-hint'>Promo code removed.</span>";
            InvalidateQuote();
        }

        protected void btnPlaceOrder_Click(object sender, EventArgs e)
        {
            try { PlaceOrderCore(); }
            catch (System.Threading.ThreadAbortException) { throw; }   // Response.Redirect
            catch (Exception ex)
            {
                LogError(ex);
                AddMessage("We couldn't place your order right now. You have not been charged. Please try again in a moment.", true);
            }
        }

        private void PlaceOrderCore()
        {
            int? customerId = CurrentCustomerId();
            if (!customerId.HasValue)
            {
                Response.Redirect("Login.aspx?returnUrl=Checkout.aspx");
                return;
            }

            // Double-click / double-submit guard
            string token = ViewState["Token"] as string;
            if (string.IsNullOrEmpty(token))
            {
                AddMessage("This page has expired. Please refresh it and try again.", true);
                return;
            }
            HashSet<string> used = UsedTokens();
            if (used.Contains(token))
            {
                RedirectToLastOrder();   // this exact order was already placed
                return;
            }

            var cart = CartHelper.GetCart(Session);
            if (cart.Count == 0)
            {
                Response.Redirect("Cart.aspx");
                return;
            }

            OrderRequest req = BuildRequest(customerId.Value, cart);

            List<string> errors;
            if (!OrderHelper.ValidateRequest(req, out errors))
            {
                foreach (string err in errors) AddMessage(err, true);
                BindSlots();   // refresh in case the chosen slot just expired
                return;
            }

            OrderResult result = OrderHelper.PlaceOrder(req);

            // Card details are never kept
            req.CardName = req.CardNumber = req.CardExpiry = req.CardCvv = null;
            txtCardName.Text = "";
            txtCardNumber.Text = "";
            txtCardExpiry.Text = "";
            txtCardCvv.Text = "";

            if (!result.Success)
            {
                AddMessage(result.Message, true);
                if (result.CartChanged)
                    litMessage.Text += "<div class='field-hint'><a href='Cart.aspx'>Go back to your cart</a></div>";
                BindSlots();
                InvalidateQuote();
                return;
            }

            used.Add(token);
            Session["LastSaleId"] = result.SaleId;
            CartHelper.ClearCart(Session);
            Session["PromoCode"] = null;

            SendReceipt(result.SaleId, customerId.Value);

            Response.Redirect("OrderConfirmation.aspx?id=" + result.SaleId);
        }

        // ---------- building things ----------

        private OrderRequest BuildRequest(int customerId, List<CartItem> cart)
        {
            var req = new OrderRequest
            {
                CustomerId = customerId,
                Lines = cart.Select(c => new OrderLine { ProductId = c.ProductId, Quantity = c.Quantity }).ToList(),
                Method = rblMethod.SelectedValue,
                FirstName = txtFirstName.Text,
                LastName = txtLastName.Text,
                Email = txtEmail.Text,
                Phone = txtPhone.Text,
                UnitNumber = txtUnitNumber.Text,
                UnitName = txtUnitName.Text,
                StreetNumber = txtStreetNumber.Text,
                StreetName = txtStreetName.Text,
                Suburb = txtSuburb.Text,
                PostalCode = txtPostalCode.Text,
                Instructions = txtInstructions.Text,
                PromoCode = Session["PromoCode"] as string,
                UsePoints = chkUsePoints.Checked,
                CardName = txtCardName.Text,
                CardNumber = txtCardNumber.Text,
                CardExpiry = txtCardExpiry.Text,
                CardCvv = txtCardCvv.Text,
                AgeConfirmed = chkAge.Checked
            };

            DateTime slot;
            if (SlotHelper.TryParseSlot(ddlSlot.SelectedValue, out slot)) req.SlotStart = slot;

            object expected = ViewState["ExpectedTotal"];
            if (expected is decimal)
            {
                req.ExpectedTotal = (decimal)expected;
                req.CheckExpectedTotal = true;
            }
            return req;
        }

        private Quote LoadQuote(string method, bool usePoints, string promoCode)
        {
            var cart = CartHelper.GetCart(Session);
            var lines = cart.Select(c => new OrderLine { ProductId = c.ProductId, Quantity = c.Quantity }).ToList();
            var names = new Dictionary<int, string>();
            foreach (var c in cart) names[c.ProductId] = c.ProductName;

            return OrderHelper.GetQuote(CurrentCustomerId().Value, lines, names, method, promoCode, usePoints);
        }

        private Quote GetQuote()
        {
            if (_quote == null)
                _quote = LoadQuote(rblMethod.SelectedValue, chkUsePoints.Checked, Session["PromoCode"] as string);
            return _quote;
        }

        private void InvalidateQuote() { _quote = null; }

        private void BindSlots()
        {
            bool delivery = rblMethod.SelectedValue == OrderHelper.MethodDelivery;
            DateTime now = SlotHelper.Now();
            List<SlotHelper.Slot> slots = delivery ? SlotHelper.GetDeliverySlots(now) : SlotHelper.GetCollectionSlots(now);

            string keep = ddlSlot.SelectedValue;
            ddlSlot.Items.Clear();
            ddlSlot.Items.Add(new ListItem("Select a time slot", ""));
            foreach (var s in slots)
                ddlSlot.Items.Add(new ListItem(s.Start.ToString("ddd d MMM", CultureInfo.InvariantCulture) + ", " + s.Label, s.Value));

            if (!string.IsNullOrEmpty(keep) && ddlSlot.Items.FindByValue(keep) != null) ddlSlot.SelectedValue = keep;

            if (delivery)
                litSlotHint.Text = "<p class='co-note'>Choose a delivery window. You can book from tomorrow, up to " + SlotHelper.DeliveryDaysAhead + " days ahead.</p>";
            else if (slots.Count == 0)
                litSlotHint.Text = "<p class='field-hint invalid'>No collection slots are available right now. Please try again later or choose delivery.</p>";
            else
                litSlotHint.Text = "<p class='co-note'>Collection slots are 1 hour long. The earliest is " + SlotHelper.CollectionLeadHours + " hours from now.</p>";
        }

        private void BindPoints(Quote q)
        {
            if (!q.Ok || !q.CustomerActive || q.PointsBalance <= 0 || q.Total <= 0m)
            {
                pnlPoints.Visible = false;
                return;
            }

            pnlPoints.Visible = true;
            if (q.CanUsePoints)
            {
                chkUsePoints.Visible = true;
                chkUsePoints.Text = "Use " + q.PointsNeeded.ToString("N0") + " of my " + q.PointsBalance.ToString("N0")
                                  + " loyalty points to pay for this order (no card needed, no points earned)";
                litPointsNote.Text = "";
            }
            else
            {
                chkUsePoints.Visible = false;
                litPointsNote.Text = "<p class='field-hint'>You have " + q.PointsBalance.ToString("N0") + " points. You need " + q.PointsNeeded.ToString("N0")
                                   + " to pay for this order with points.</p>";
            }
        }

        private void BindSummary(Quote q)
        {
            if (!q.Ok)
            {
                litSummary.Text = "";
                AddMessage(q.Error ?? "We couldn't load your order.", true);
                return;
            }

            var sb = new StringBuilder();
            foreach (var l in q.Lines)
            {
                sb.Append("<div class='cart-summary-line'><span>").Append(Server.HtmlEncode(l.Name)).Append(" &times; ").Append(l.Quantity)
                  .Append("</span><span>").Append(OrderHelper.Money(l.LineTotal)).Append("</span></div>");
            }
            sb.Append("<div class='cart-summary-divider'></div>");

            sb.Append("<div class='cart-summary-line'><span>Items</span><span>").Append(OrderHelper.Money(q.GoodsAfterItems)).Append("</span></div>");
            if (q.ItemDiscount > 0m)
                sb.Append("<p class='co-note'>Includes ").Append(OrderHelper.Money(q.ItemDiscount)).Append(" of product discounts.</p>");
            if (q.PromoAmount > 0m)
                sb.Append("<div class='cart-summary-line cart-summary-discount'><span>Promo (").Append(Server.HtmlEncode(q.PromoCode)).Append(")</span><span>-")
                  .Append(OrderHelper.Money(q.PromoAmount)).Append("</span></div>");

            bool delivery = rblMethod.SelectedValue == OrderHelper.MethodDelivery;
            sb.Append("<div class='cart-summary-line'><span>").Append(delivery ? "Delivery" : "Collection").Append("</span><span>")
              .Append(q.DeliveryFee > 0m ? OrderHelper.Money(q.DeliveryFee) : "Free").Append("</span></div>");
            if (delivery && q.DeliveryAllowed && q.DeliveryFee > 0m)
                sb.Append("<p class='co-note'>Add ").Append(OrderHelper.Money(OrderHelper.FreeDeliveryThreshold - q.GoodsAfterItems)).Append(" more for free delivery.</p>");

            sb.Append("<div class='cart-summary-divider'></div>");
            sb.Append("<div class='cart-summary-line cart-summary-total-line'><span>Total</span><span>").Append(OrderHelper.Money(q.Total)).Append("</span></div>");
            sb.Append("<p class='co-note'>Includes 15% VAT of ").Append(OrderHelper.Money(q.Vat)).Append("</p>");

            if (q.UsePoints)
                sb.Append("<p class='co-note'>Paid with ").Append(q.PointsNeeded.ToString("N0")).Append(" loyalty points.</p>");
            else if (q.PointsToEarn > 0)
                sb.Append("<p class='co-note'>You'll earn <strong>").Append(q.PointsToEarn.ToString("N0")).Append("</strong> loyalty points on this order.</p>");

            if (!q.DeliveryAllowed && !string.IsNullOrEmpty(q.DeliveryMessage))
                sb.Append("<p class='field-hint invalid' role='note'>").Append(Server.HtmlEncode(q.DeliveryMessage)).Append("</p>");

            litSummary.Text = sb.ToString();
        }

        // ---------- small helpers ----------

        private int? CurrentCustomerId()
        {
            if (Session["UserRole"] == null) return null;
            object id = Session["CustomerID"];   // same key the Cart page reads (set at login from PendingKey)
            int value;
            return id != null && int.TryParse(id.ToString(), out value) && value > 0 ? (int?)value : null;
        }

        private HashSet<string> UsedTokens()
        {
            var set = Session["Checkout.UsedTokens"] as HashSet<string>;
            if (set == null)
            {
                set = new HashSet<string>();
                Session["Checkout.UsedTokens"] = set;
            }
            return set;
        }

        private void RedirectToLastOrder()
        {
            object last = Session["LastSaleId"];
            int id;
            if (last != null && int.TryParse(last.ToString(), out id) && id > 0)
                Response.Redirect("OrderConfirmation.aspx?id=" + id);
            else
                Response.Redirect("Cart.aspx");
        }

        private void SendReceipt(int saleId, int customerId)
        {
            try
            {
                OnlineReceipt r = OnlineReceiptHelper.Load(saleId, customerId);
                if (r == null || string.IsNullOrWhiteSpace(r.Email)) return;

                string html = OnlineReceiptHelper.BuildHtml(r);
                string to = r.Email;
                string name = (r.FirstName + " " + r.LastName).Trim();
                string subject = "Your Sam's Liquor Shop order INV-" + saleId.ToString("000000");

                // In the background so the customer never waits on email. A failed email never affects the order.
                Task.Run(() =>
                {
                    if (!OtpHelper.SendEmail(to, name, subject, html))
                        System.Diagnostics.Trace.TraceWarning("Receipt email for sale " + saleId + " was not sent (demo mode or mail error).");
                });
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private void AddMessage(string text, bool isError)
        {
            litMessage.Text += "<div class='field-hint " + (isError ? "invalid" : "valid") + "' role='alert'>" + Server.HtmlEncode(text) + "</div>";
        }

        private void ShowBlocked(string text)
        {
            pnlForm.Visible = false;
            AddMessage(text, true);
            litMessage.Text += "<p><a href='Cart.aspx' class='btn-primary'>Back to cart</a></p>";
        }

        private void LogError(Exception ex)
        {
            System.Diagnostics.Trace.TraceError(ex.ToString());
        }
    }
}