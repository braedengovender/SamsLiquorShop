<%@ Page Title="My Wishlist" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Wishlist.aspx.cs" Inherits="SamsLiquorShop.Customer.Wishlist" %>
<%@ Register Src="~/Customer/CustomerNav.ascx" TagPrefix="uc" TagName="CustomerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:CustomerNav runat="server" />

        <div class="mgr-main">
            <div class="mgr-header-row">
                <h1>My Wishlist</h1>
                <p class="mgr-dash-subtext">Products you've saved. Tap the heart on any product in the shop to add it here.</p>
            </div>

            <asp:Literal ID="litMessage" runat="server" />

            <asp:Panel ID="pnlToolbar" runat="server" CssClass="ord-toolbar">
                <p class="ord-summary"><asp:Literal ID="litCount" runat="server" /></p>
                <asp:LinkButton ID="btnAddAll" runat="server" CssClass="btn-primary ord-reorder-btn" OnClick="btnAddAll_Click">Add all available to cart</asp:LinkButton>
            </asp:Panel>

            <asp:Repeater ID="rptWishlist" runat="server" OnItemCommand="rptWishlist_ItemCommand">
                <HeaderTemplate><div class="shop-grid"></HeaderTemplate>
                <ItemTemplate>
                    <div class="shop-card">
                        <div class="shop-card-image-wrap">
                            <img src='<%# ResolveUrl("~/ProductImage.ashx?id=" + Eval("Product_ID")) %>' alt='<%# Eval("Product_Name") %>' class="shop-card-image" />
                            <asp:LinkButton runat="server" CommandName="Remove" CommandArgument='<%# Eval("Product_ID") %>' CssClass="shop-heart-btn is-active" ToolTip="Remove from wishlist">&#9829;</asp:LinkButton>
                        </div>
                        <div class="shop-card-body">
                            <p class="shop-card-brand"><%# Eval("Product_Brand") %></p>
                            <p class="shop-card-name"><%# Eval("Product_Name") %></p>
                            <div class="shop-card-price-row">
                                <span runat="server" class="shop-card-strike" visible='<%# Convert.ToDecimal(Eval("DiscountedPrice")) < Convert.ToDecimal(Eval("Product_SellingPrice")) %>'><%# "R" + Convert.ToDecimal(Eval("Product_SellingPrice")).ToString("N2") %></span>
                                <span class="shop-card-price"><%# "R" + Convert.ToDecimal(Eval("DiscountedPrice")).ToString("N2") %></span>
                            </div>
                            <asp:LinkButton runat="server" CommandName="AddToCart" CommandArgument='<%# Eval("Product_ID") %>' CssClass="btn-primary shop-add-cart-btn" Enabled='<%# IsAvailable(Container.DataItem) %>'>
                                <%# IsAvailable(Container.DataItem) ? "Add to Cart" : "Out of Stock" %>
                            </asp:LinkButton>
                        </div>
                    </div>
                </ItemTemplate>
                <FooterTemplate></div></FooterTemplate>
            </asp:Repeater>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="mgr-table-card ord-empty">
                <p>Your wishlist is empty.</p>
                <a href="~/Shop.aspx" runat="server" class="btn-primary ord-reorder-btn">Browse the shop</a>
            </asp:Panel>
        </div>
    </section>
</asp:Content>
