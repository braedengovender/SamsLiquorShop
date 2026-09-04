<%--<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Shop.aspx.cs" Inherits="SamsLiquorShop.Shop" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
</asp:Content>--%>

<%@ Page Title="Shop" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Shop.aspx.cs" Inherits="SamsLiquorShop.Shop" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="upnlShop" runat="server">
        <ContentTemplate>
            <section class="shell shop-layout">

                <!-- FILTER SIDEBAR -->
                <aside class="shop-filters">
                    <div class="shop-filter-block">
                        <h3>Search</h3>
                        <div class="shop-search-row">
                            <asp:TextBox ID="txtSearch" runat="server" CssClass="login-input" placeholder="Search products..." />
                            <asp:Button ID="btnSearch" runat="server" Text="Go" CssClass="btn-primary shop-search-btn" OnClick="btnSearch_Click" />
                        </div>
                    </div>

                    <div class="shop-filter-block">
                        <h3>Category</h3>
                        <asp:CheckBoxList ID="cblCategories" runat="server" CssClass="shop-category-list" RepeatLayout="UnorderedList" RepeatDirection="Vertical" AutoPostBack="true" OnSelectedIndexChanged="cblCategories_Changed" />
                    </div>

                    <div class="shop-filter-block">
                        <h3>Price Range</h3>
                        <div class="shop-price-row">
                            <asp:TextBox ID="txtMinPrice" runat="server" CssClass="login-input shop-price-input" placeholder="Min" TextMode="Number" />
                            <span>&ndash;</span>
                            <asp:TextBox ID="txtMaxPrice" runat="server" CssClass="login-input shop-price-input" placeholder="Max" TextMode="Number" />
                        </div>
                        <asp:Button ID="btnApplyPrice" runat="server" Text="Apply" CssClass="btn-ghost shop-apply-price-btn" OnClick="btnApplyPrice_Click" />
                    </div>

                    <asp:LinkButton ID="btnClearFilters" runat="server" CssClass="shop-clear-link" OnClick="btnClearFilters_Click">Clear all filters</asp:LinkButton>
                </aside>

                <!-- PRODUCT GRID -->
                <div class="shop-main">
                    <div class="shop-toolbar">
                        <p class="shop-result-count"><asp:Literal runat="server" ID="litResultCount" /></p>
                        <asp:DropDownList ID="ddlSort" runat="server" CssClass="shop-sort-select" AutoPostBack="true" OnSelectedIndexChanged="ddlSort_Changed">
                            <asp:ListItem Text="Sort: Featured" Value="featured" />
                            <asp:ListItem Text="Price: Low to High" Value="price_asc" />
                            <asp:ListItem Text="Price: High to Low" Value="price_desc" />
                            <asp:ListItem Text="Name: A-Z" Value="name_asc" />
                        </asp:DropDownList>
                    </div>

                    <asp:Literal runat="server" ID="litCartMessage" Visible="false" />

                    <asp:Repeater ID="rptProducts" runat="server" OnItemCommand="rptProducts_ItemCommand">
                        <HeaderTemplate><div class="shop-grid"></HeaderTemplate>
                        <ItemTemplate>
                            <div class="shop-card">
                                <div class="shop-card-image-wrap">
                                    <img src='<%# "/ProductImage.ashx?id=" + Eval("Product_ID") %>' alt='<%# Eval("Product_Name") %>' class="shop-card-image" />
                                    <asp:LinkButton runat="server" CommandName="ToggleWishlist" CommandArgument='<%# Eval("Product_ID") %>' CssClass='<%# "shop-heart-btn " + (IsWishlisted((int)Eval("Product_ID")) ? "is-active" : "") %>'>&#9825;</asp:LinkButton>
                                    <asp:Literal runat="server" Visible='<%# Convert.ToDecimal(Eval("DiscountedPrice")) < Convert.ToDecimal(Eval("Product_SellingPrice")) %>' Text="Sale" ID="litSaleBadge" ></asp:Literal>
                                </div>
                                <div class="shop-card-body">
                                    <p class="shop-card-brand"><%# Eval("Product_Brand") %></p>
                                    <p class="shop-card-name"><%# Eval("Product_Name") %></p>
                                    <div class="shop-card-price-row">
                                        <span runat="server" class="shop-card-strike" Visible='<%# Convert.ToDecimal(Eval("DiscountedPrice")) < Convert.ToDecimal(Eval("Product_SellingPrice")) %>'><%# "R" + Convert.ToDecimal(Eval("Product_SellingPrice")).ToString("N2") %></span>
                                        <span class="shop-card-price"><%# "R" + Convert.ToDecimal(Eval("DiscountedPrice")).ToString("N2") %></span>
                                    </div>
                                    <asp:LinkButton runat="server" CommandName="AddToCart" CommandArgument='<%# Eval("Product_ID") %>' CssClass="btn-primary shop-add-cart-btn" Enabled='<%# Convert.ToInt32(Eval("Product_QuantityInStock")) > 0 %>'>
                                        <%# Convert.ToInt32(Eval("Product_QuantityInStock")) > 0 ? "Add to Cart" : "Out of Stock" %>
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                        <FooterTemplate></div></FooterTemplate>
                    </asp:Repeater>

                    <p runat="server" id="litNoResults" visible="false" class="shop-no-results">No products match your filters. Try adjusting your search.</p>
                </div>

            </section>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdateProgress ID="updShopProgress" runat="server" AssociatedUpdatePanelID="upnlShop">
        <ProgressTemplate>
            <div class="loading-overlay">
                <div class="loading-spinner"></div>
                <p>Updating&hellip;</p>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
</asp:Content>
