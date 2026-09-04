using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SamsLiquorShop.Helpers;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SamsLiquorShop
{
    public partial class Cart : System.Web.UI.Page
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["GroupWst15ConnectionString"].ConnectionString;
        private const decimal FreeDeliveryThreshold = 500m;
        private const decimal DeliveryFee = 60m;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindCart();
            }
        }

        private void BindCart()
        {
            var cart = CartHelper.GetCart(Session);
            var saved = CartHelper.GetSavedForLater(Session);

            int itemCount = cart.Sum(c => c.Quantity);
            litItemCountSummary.Text = itemCount == 0 ? "" : itemCount + (itemCount == 1 ? " item" : " items") + " in your cart";

            if (cart.Count == 0)
            {
                pnlCartHasItems.Visible = false;
                pnlCartEmpty.Visible = true;
                LoadRecommended();
            }
            else
            {
                pnlCartHasItems.Visible = true;
                pnlCartEmpty.Visible = false;
                pnlRecommended.Visible = false;

                rptCart.DataSource = cart;
                rptCart.DataBind();

                BindSummary();
            }

            if (saved.Count > 0)
            {
                pnlSavedForLater.Visible = true;
                rptSaved.DataSource = saved;
                rptSaved.DataBind();
            }
            else
            {
                pnlSavedForLater.Visible = false;
            }
        }

        private void BindSummary()
        {
            decimal subtotal = CartHelper.GetCartTotal(Session);
            decimal deliveryFee = subtotal >= FreeDeliveryThreshold ? 0m : DeliveryFee;

            decimal discountPercent = 0m;
            string promoCode = Session["PromoCode"] as string;
            if (!string.IsNullOrEmpty(promoCode))
            {
                discountPercent = GetPromoDiscount(promoCode);
            }

            decimal discountAmount = Math.Round(subtotal * discountPercent / 100m, 2);
            decimal grandTotal = subtotal - discountAmount + deliveryFee;

            litSubtotal.Text = "R" + subtotal.ToString("N2");
            litDelivery.Text = deliveryFee == 0 ? "Free" : "R" + deliveryFee.ToString("N2");

            if (discountAmount > 0)
            {
                pnlDiscountLine.Visible = true;
                litDiscount.Text = "-R" + discountAmount.ToString("N2");
            }
            else
            {
                pnlDiscountLine.Visible = false;
            }

            litGrandTotal.Text = "R" + grandTotal.ToString("N2");
        }

        // Placeholder promo logic — swap this for a real Promo/Discount table lookup when you have one.
        private decimal GetPromoDiscount(string code)
        {
            switch (code.Trim().ToUpperInvariant())
            {
                case "SAVE10": return 10m;
                case "SAVE20": return 20m;
                default: return 0m;
            }
        }

        private void LoadRecommended()
        {
            string query = @"SELECT TOP 4 Product_ID, Product_Name, Product_SellingPrice 
                              FROM Product 
                              WHERE Product_Status = 'Active' AND Product_QuantityInStock > 0
                              ORDER BY NEWID()";

            using (SqlConnection conn = new SqlConnection(ConnStr))
            using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                pnlRecommended.Visible = dt.Rows.Count > 0;
                rptRecommended.DataSource = dt;
                rptRecommended.DataBind();
            }
        }

        protected void rptCart_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int productId = Convert.ToInt32(e.CommandArgument);
            var cart = CartHelper.GetCart(Session);
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            switch (e.CommandName)
            {
                case "Increment":
                    if (item != null) CartHelper.UpdateQuantity(Session, productId, item.Quantity + 1);
                    break;
                case "Decrement":
                    if (item != null) CartHelper.UpdateQuantity(Session, productId, item.Quantity - 1);
                    break;
                case "RemoveItem":
                    CartHelper.RemoveFromCart(Session, productId);
                    break;
                case "SaveForLater":
                    CartHelper.MoveToSavedForLater(Session, productId);
                    break;
            }

            BindCart();
        }

        protected void rptSaved_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int productId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "MoveToCart")
            {
                CartHelper.MoveToCart(Session, productId);
            }
            else if (e.CommandName == "RemoveSaved")
            {
                CartHelper.RemoveFromSavedForLater(Session, productId);
            }

            BindCart();
        }

        protected void rptRecommended_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "AddToCart")
            {
                int productId = Convert.ToInt32(e.CommandArgument);
                string query = "SELECT Product_Name, Product_SellingPrice FROM Product WHERE Product_ID = @Id";
                using (SqlConnection conn = new SqlConnection(ConnStr))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", productId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            CartHelper.AddToCart(Session, productId, reader["Product_Name"].ToString(), Convert.ToDecimal(reader["Product_SellingPrice"]));
                        }
                    }
                }
                BindCart();
            }
        }

        protected void btnApplyPromo_Click(object sender, EventArgs e)
        {
            string code = txtPromoCode.Text.Trim();
            decimal discount = GetPromoDiscount(code);

            if (discount > 0)
            {
                Session["PromoCode"] = code;
                litPromoMessage.Text = "<span class='field-hint valid'>Promo code applied: " + discount + "% off</span>";
            }
            else
            {
                Session["PromoCode"] = null;
                litPromoMessage.Text = "<span class='field-hint invalid'>That promo code isn't valid.</span>";
            }

            BindCart();
        }

        protected void btnCheckout_Click(object sender, EventArgs e)
        {
            Response.Redirect("Checkout.aspx");
        }
    }
}