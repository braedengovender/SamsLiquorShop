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
                                        <div class="cart-item-image-wrap">
                                            <asp:LinkButton runat="server" CommandName="ViewProduct" CommandArgument='<%# Eval("ProductId") %>' CssClass="cart-item-image-link" ToolTip="View details">
                                                <img src='<%# "/ProductImage.ashx?id=" + Eval("ProductId") %>' alt='<%# Eval("ProductName") %>' class="cart-item-image" />
                                            </asp:LinkButton>
                                            <span class="cart-qty-badge" title="Quantity in cart"><%# Convert.ToInt32(Eval("Quantity")) > 99 ? "99+" : Eval("Quantity").ToString() %></span>
                                        </div>
                                        <div class="cart-item-details">
                                            <asp:LinkButton runat="server" CommandName="ViewProduct" CommandArgument='<%# Eval("ProductId") %>' CssClass="cart-item-name cart-item-name-link"><%# Eval("ProductName") %></asp:LinkButton>
                                            <p class="cart-item-price"><%# CartPriceHtml(Eval("ShelfPrice"), Eval("UnitPrice"), " each") %></p>
                                            <div class="cart-item-actions-row">
                                                <asp:LinkButton runat="server" CommandName="MoveToWishlist" CommandArgument='<%# Eval("ProductId") %>'>Move to wishlist</asp:LinkButton>
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
                                                <p class="cart-item-price"><%# CartPriceHtml(Eval("ShelfPrice"), Eval("UnitPrice"), "") %></p>
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

                                <asp:Literal ID="litPointsInfo" runat="server" />

                                <asp:LinkButton ID="btnCheckout" runat="server" CssClass="btn-primary cart-checkout-btn" OnClick="btnCheckout_Click">Proceed to Checkout</asp:LinkButton>

                                <div class="cart-secure">
                                    <p class="cart-secure-title"><span class="lock-icon">&#128274;</span> Secure checkout</p>
                                    <ul class="cart-secure-list">
                                        <li>Many ways to pay</li>
                                        <li>Fast, reliable delivery</li>
                                    </ul>
                                    <p class="cart-secure-note">Placing an item in your shopping cart does not reserve that item or price. We only reserve stock for your order once payment is received. Delivery dates shown are estimates and will be confirmed at checkout.</p>
                                </div>
                            </div>
                        </aside>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlDetail" runat="server" Visible="false" CssClass="pd-overlay">
                    <div class="pd-card" role="dialog" aria-modal="true">
                        <asp:LinkButton ID="btnCloseDetail" runat="server" CssClass="pd-close" OnClick="btnCloseDetail_Click" aria-label="Close" Text="&times;" />
                        <div class="pd-image-wrap">
                            <img id="imgDetail" runat="server" class="pd-image" alt="" />
                            <span id="spnDetailBadge" runat="server" class="shop-sale-badge" visible="false"></span>
                        </div>
                        <div class="pd-body">
                            <p class="pd-brand"><asp:Literal ID="litDetailBrand" runat="server" Mode="Encode" /></p>
                            <h2 class="pd-name"><asp:Literal ID="litDetailName" runat="server" Mode="Encode" /></h2>
                            <asp:Literal ID="litDetailRating" runat="server" />
                            <div><asp:LinkButton ID="lnkDetailCategory" runat="server" CssClass="pd-category" OnClick="lnkDetailCategory_Click" /></div>
                            <div class="shop-card-price-row pd-price-row">
                                <span id="spnDetailOld" runat="server" class="shop-card-strike" visible="false"></span>
                                <span id="spnDetailPrice" runat="server" class="shop-card-price"></span>
                            </div>
                            <p class="pd-desc"><asp:Literal ID="litDetailDesc" runat="server" Mode="Encode" /></p>
                            <asp:Literal ID="litDetailFacts" runat="server" />
                            <asp:Literal ID="litDetailIngredients" runat="server" />
                            <asp:Literal ID="litDetailMsg" runat="server" />
                            <div class="pd-actions">
                                <asp:LinkButton ID="btnDetailAdd" runat="server" CssClass="btn-primary shop-add-cart-btn pd-add" OnClick="btnDetailAdd_Click" />
                                <asp:LinkButton ID="btnDetailWish" runat="server" CssClass="btn-ghost pd-wish" OnClick="btnDetailWish_Click" />
                            </div>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel runat="server" ID="pnlLoginPrompt" Visible="false" CssClass="cart-modal-overlay">
                    <div class="cart-modal-card" role="dialog" aria-modal="true">
                        <asp:LinkButton ID="btnLoginPromptClose" runat="server" CssClass="pd-close" OnClick="btnClosePrompt_Click" aria-label="Close" Text="&times;" />
                        <h3>Log in to check out</h3>
                        <p>Log in or create an account to check out, earn loyalty points and use them on your order.</p>
                        <div class="cart-modal-actions">
                            <a href="Login.aspx?returnUrl=Cart.aspx" class="btn-primary">Log in</a>
                            <a href="Register.aspx" class="btn-ghost cart-modal-secondary">Sign up</a>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel runat="server" ID="pnlPointsPrompt" Visible="false" CssClass="cart-modal-overlay">
                    <div class="cart-modal-card" role="dialog" aria-modal="true">
                        <asp:LinkButton ID="btnPointsPromptClose" runat="server" CssClass="pd-close" OnClick="btnClosePrompt_Click" aria-label="Close" Text="&times;" />
                        <h3>Use your loyalty points?</h3>
                        <p><asp:Literal ID="litPointsPromptText" runat="server" /></p>
                        <div class="cart-modal-actions">
                            <asp:LinkButton ID="btnUsePoints" runat="server" CssClass="btn-primary" OnClick="btnUsePoints_Click">Use points - get this order free</asp:LinkButton>
                            <asp:LinkButton ID="btnPayNormally" runat="server" CssClass="btn-ghost cart-modal-secondary" OnClick="btnPayNormally_Click">No, pay normally</asp:LinkButton>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel runat="server" ID="pnlRecommended" Visible="false" CssClass="home-section cart-recs">
                    <div class="section-header">
                        <h2><asp:Literal ID="litRecommendedHeading" runat="server" /></h2>
                    </div>
                    <div class="ribbon">
                        <asp:Repeater ID="rptRecommended" runat="server" OnItemCommand="rptRecommended_ItemCommand">
                            <ItemTemplate>
                                <div class="card-surface product-card">
                                    <img src='<%# "/ProductImage.ashx?id=" + Eval("Product_ID") %>' alt='<%# Eval("Product_Name") %>' />
                                    <p class="product-name"><%# Eval("Product_Name") %></p>
                                    <p class="rec-reason"><%# Eval("Reason") %></p>
                                    <%# (bool)Eval("IsOnSale") ? "<p class='product-price-old'>R" + Convert.ToDecimal(Eval("Product_SellingPrice")).ToString("N2") + "</p>" : "" %>
                                    <p class="product-price">R<%# Convert.ToDecimal(Eval("DiscountedPrice")).ToString("N2") %></p>
                                    <asp:LinkButton runat="server" CommandName="AddToCart" CommandArgument='<%# Eval("Product_ID") %>' CssClass="btn-primary shop-add-cart-btn">Add to Cart</asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>

            </ContentTemplate>
        </asp:UpdatePanel>

        <script type="text/javascript">
            (function () {
                function overlay() { return document.querySelector('.pd-overlay, .cart-modal-overlay'); }
                function lockScroll() { document.body.style.overflow = overlay() ? 'hidden' : ''; }
                function closeOverlay() { var c = document.querySelector('.pd-close'); if (c) c.click(); }

                document.addEventListener('DOMContentLoaded', function () {
                    lockScroll();
                    if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(lockScroll);
                    }
                    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') closeOverlay(); });
                    document.addEventListener('click', function (e) {
                        var c = e.target.classList;
                        if (c && (c.contains('pd-overlay') || c.contains('cart-modal-overlay'))) closeOverlay();
                    });
                });
            })();
        </script>

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