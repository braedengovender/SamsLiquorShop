<%--<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Cart.aspx.cs" Inherits="SamsLiquorShop.Cart" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
</asp:Content>--%>

<%@ Page Title="Your Cart" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Cart.aspx.cs" Inherits="SamsLiquorShop.Cart" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell">
        <h1 class="login-heading" style="margin: 32px 0 8px;">Your Cart</h1>
        <p class="login-subtext" style="margin: 0 0 8px;"><asp:Literal ID="litItemCountSummary" runat="server" /></p>

        <asp:UpdatePanel ID="upnlCart" runat="server">
            <ContentTemplate>

                <asp:Panel runat="server" ID="pnlCartEmpty" Visible="false">
                    <div class="glass cart-empty">
                        <p>Your cart is empty.</p>
                        <a href="Shop.aspx" class="btn-primary">Continue Shopping</a>
                    </div>
                </asp:Panel>

                <asp:Panel runat="server" ID="pnlCartHasItems">
                    <div class="cart-layout">
                        <div class="cart-items-col">

                            <asp:Repeater ID="rptCart" runat="server" OnItemCommand="rptCart_ItemCommand">
                                <ItemTemplate>
                                    <div class="glass cart-item-card">
                                        <img src='<%# "/ProductImage.ashx?id=" + Eval("ProductId") %>' alt='<%# Eval("ProductName") %>' class="cart-item-image" />
                                        <div class="cart-item-details">
                                            <p class="cart-item-name"><%# Eval("ProductName") %></p>
                                            <p class="cart-item-price">R<%# Convert.ToDecimal(Eval("UnitPrice")).ToString("N2") %> each</p>
                                            <div class="cart-item-actions-row">
                                                <asp:LinkButton runat="server" CommandName="SaveForLater" CommandArgument='<%# Eval("ProductId") %>'>Save for later</asp:LinkButton>
                                                <asp:LinkButton runat="server" CommandName="RemoveItem" CommandArgument='<%# Eval("ProductId") %>' CssClass="danger-link">Remove</asp:LinkButton>
                                            </div>
                                        </div>
                                        <div class="cart-item-side">
                                            <div class="qty-stepper">
                                                <asp:LinkButton runat="server" CommandName="Decrement" CommandArgument='<%# Eval("ProductId") %>'>&minus;</asp:LinkButton>
                                                <asp:TextBox runat="server" CssClass="qty-stepper-input" Text='<%# Eval("Quantity") %>' ReadOnly="true" />
                                                <asp:LinkButton runat="server" CommandName="Increment" CommandArgument='<%# Eval("ProductId") %>'>+</asp:LinkButton>
                                            </div>
                                            <span class="cart-item-line-total">R<%# (Convert.ToDecimal(Eval("UnitPrice")) * Convert.ToInt32(Eval("Quantity"))).ToString("N2") %></span>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>

                            <asp:Panel runat="server" ID="pnlSavedForLater" Visible="false" CssClass="cart-saved-section">
                                <h3 class="cart-section-heading">Saved for later</h3>
                                <asp:Repeater ID="rptSaved" runat="server" OnItemCommand="rptSaved_ItemCommand">
                                    <ItemTemplate>
                                        <div class="glass cart-item-card cart-item-card-saved">
                                            <img src='<%# "/ProductImage.ashx?id=" + Eval("ProductId") %>' alt='<%# Eval("ProductName") %>' class="cart-item-image" />
                                            <div class="cart-item-details">
                                                <p class="cart-item-name"><%# Eval("ProductName") %></p>
                                                <p class="cart-item-price">R<%# Convert.ToDecimal(Eval("UnitPrice")).ToString("N2") %></p>
                                                <div class="cart-item-actions-row">
                                                    <asp:LinkButton runat="server" CommandName="MoveToCart" CommandArgument='<%# Eval("ProductId") %>'>Move to cart</asp:LinkButton>
                                                    <asp:LinkButton runat="server" CommandName="RemoveSaved" CommandArgument='<%# Eval("ProductId") %>' CssClass="danger-link">Remove</asp:LinkButton>
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </asp:Panel>

                        </div>

                        <aside class="cart-summary-sidebar">
                            <div class="glass cart-summary-card">
                                <h3>Order Summary</h3>
                                <div class="cart-summary-line">
                                    <span>Subtotal</span>
                                    <span><asp:Literal ID="litSubtotal" runat="server" /></span>
                                </div>
                                <div class="cart-summary-line">
                                    <span>Estimated Delivery</span>
                                    <span><asp:Literal ID="litDelivery" runat="server" /></span>
                                </div>
                                <asp:Panel runat="server" ID="pnlDiscountLine" Visible="false" CssClass="cart-summary-line cart-summary-discount">
                                    <span>Discount</span>
                                    <span><asp:Literal ID="litDiscount" runat="server" /></span>
                                </asp:Panel>

                                <div class="cart-summary-promo">
                                    <asp:TextBox ID="txtPromoCode" runat="server" CssClass="login-input" placeholder="Promo code" />
                                    <asp:Button ID="btnApplyPromo" runat="server" Text="Apply" CssClass="btn-ghost" OnClick="btnApplyPromo_Click" />
                                </div>
                                <asp:Literal ID="litPromoMessage" runat="server" />

                                <div class="cart-summary-divider"></div>
                                <div class="cart-summary-line cart-summary-total-line">
                                    <span>Total</span>
                                    <span><asp:Literal ID="litGrandTotal" runat="server" /></span>
                                </div>

                                <asp:LinkButton ID="btnCheckout" runat="server" CssClass="btn-primary cart-checkout-btn" OnClick="btnCheckout_Click">Proceed to Checkout</asp:LinkButton>
                                <p class="cart-summary-secure"><span class="lock-icon">&#128274;</span> Secure checkout</p>
                            </div>
                        </aside>
                    </div>
                </asp:Panel>

                <asp:Panel runat="server" ID="pnlRecommended" Visible="false" CssClass="home-section">
                    <div class="section-header">
                        <h2>You might like</h2>
                    </div>
                    <div class="ribbon">
                        <asp:Repeater ID="rptRecommended" runat="server" OnItemCommand="rptRecommended_ItemCommand">
                            <ItemTemplate>
                                <div class="card-surface product-card">
                                    <img src='<%# "/ProductImage.ashx?id=" + Eval("Product_ID") %>' alt='<%# Eval("Product_Name") %>' />
                                    <p class="product-name"><%# Eval("Product_Name") %></p>
                                    <p class="product-price">R<%# Convert.ToDecimal(Eval("Product_SellingPrice")).ToString("N2") %></p>
                                    <asp:LinkButton runat="server" CommandName="AddToCart" CommandArgument='<%# Eval("Product_ID") %>' CssClass="btn-primary shop-add-cart-btn">Add to Cart</asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>

            </ContentTemplate>
        </asp:UpdatePanel>

        <asp:UpdateProgress ID="updCartProgress" runat="server" AssociatedUpdatePanelID="upnlCart">
            <ProgressTemplate>
                <div class="loading-overlay">
                    <div class="loading-spinner"></div>
                    <p>Updating cart&hellip;</p>
                </div>
            </ProgressTemplate>
        </asp:UpdateProgress>
    </section>
</asp:Content>