<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RecentlyViewed.ascx.cs" Inherits="SamsLiquorShop.Controls.RecentlyViewed" %>
<%-- "Recently viewed" strip: the products this visitor opened this session (see Helpers/RecentlyViewedHelper.cs).
     Hidden when there's nothing to show. Each card opens the product on the shop page. --%>
<asp:PlaceHolder ID="phRecent" runat="server" Visible="false">
<style>
    .rv-section .section-header { display: flex; align-items: baseline; justify-content: space-between; gap: 12px; }
    .rv-clear { font-size: .85rem; font-weight: 600; color: var(--text-secondary); text-decoration: none; }
    .rv-clear:hover { color: var(--accent); }
    .rv-row { display: flex; gap: 16px; overflow-x: auto; scroll-snap-type: x mandatory; padding: 4px 2px 14px; -webkit-overflow-scrolling: touch; }
    .rv-row .product-card { position: relative; flex: 0 0 170px; }
    .rv-row .product-card img { height: 130px; object-fit: contain; background: #fff; }
    .rv-row .product-name { display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; min-height: 2.5em; }
    .rv-row .product-price-old { margin: 0; font-size: .78rem; color: var(--text-secondary); text-decoration: line-through; }
</style>
<section class="shell home-section rv-section" aria-labelledby="<%= ClientID %>_title">
    <div class="section-header">
        <h2 id="<%= ClientID %>_title"><%: Title %></h2>
        <asp:LinkButton ID="btnClear" runat="server" CssClass="rv-clear" CausesValidation="false" OnClick="btnClear_Click">Clear history</asp:LinkButton>
    </div>
    <div class="rv-row">
        <asp:Literal ID="litCards" runat="server" />
    </div>
</section>
</asp:PlaceHolder>
