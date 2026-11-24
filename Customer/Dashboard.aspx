<%--<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SamsLiquorShop.Customer.Dashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal runat="server" ID="litGreeting" /></h1>
</asp:Content>--%>

<%@ Page Title="My Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SamsLiquorShop.Customer.Dashboard" %>
<%@ Register Src="~/Customer/CustomerNav.ascx" TagPrefix="uc" TagName="CustomerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:CustomerNav runat="server" />

        <div class="mgr-main">
            <div class="mgr-header-row">
                <h1>Overview</h1>
                <p class="mgr-dash-subtext"><asp:Literal runat="server" ID="litGreeting" /></p>
            </div>

            <%-- Shown once, straight after registering --%>
            <asp:Literal runat="server" ID="litWelcome" />

            <!-- 1. LOYALTY (1 point = R1 to redeem, 1 point earned per R10 — same rules as the POS) -->
            <div class="loyalty-wrap">
                <div class="loyalty-card" aria-label="Your loyalty card">
                    <div class="loyalty-card-top">
                        <span class="loyalty-card-brand">Sam's Liquor Shop</span>
                        <span class="loyalty-card-type">Loyalty Card</span>
                    </div>

                    <span class="loyalty-chip" aria-hidden="true"></span>

                    <div class="loyalty-card-points">
                        <strong><asp:Literal runat="server" ID="litLoyaltyPoints" /></strong>
                        <span>points</span>
                    </div>

                    <div class="loyalty-card-bottom">
                        <div class="loyalty-card-field">
                            <span class="loyalty-card-label">Card holder</span>
                            <span class="loyalty-card-name"><asp:Literal runat="server" ID="litCardName" /></span>
                        </div>
                        <div class="loyalty-card-field is-right">
                            <span class="loyalty-card-label">Cellphone</span>
                            <span class="loyalty-card-phone"><asp:Literal runat="server" ID="litCardPhone" /></span>
                        </div>
                    </div>
                </div>

                <div class="loyalty-info">
                    <p class="mgr-kpi-label">Your Loyalty Points</p>
                    <p class="loyalty-info-value">Worth <asp:Literal runat="server" ID="litPointsValue" /> off your next order</p>
                    <p class="loyalty-info-meta">You earn 1 point for every R10 you spend.</p>
                    <a href="~/Rewards.aspx" runat="server" class="btn-primary">How rewards work &rarr;</a>
                </div>
            </div>

            <!-- 2. HEADLINE NUMBERS -->
            <div class="mgr-kpi-grid">
                <div class="mgr-kpi-card">
                    <span class="dash-kpi-icon is-blue dash-icon-cash" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">Total Spent</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litTotalSpent" /></p>
                </div>
                <div class="mgr-kpi-card">
                    <span class="dash-kpi-icon is-amber dash-icon-receipt" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">Total Orders</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litOrderCount" /></p>
                </div>
                <div class="mgr-kpi-card">
                    <span class="dash-kpi-icon is-blue dash-icon-bag" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">Average Order</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litAvgOrder" /></p>
                </div>
            </div>

            <!-- 3. PROMO CODES THIS CUSTOMER CAN USE (same rules as checkout, see PromoHelper.GetAvailableOffers) -->
            <div class="mgr-table-card" id="offers" style="margin-bottom: 24px;">
                <div class="dash-card-head">
                    <h3>Your Offers</h3>
                    <span class="dash-card-meta">Enter a code at checkout. Each code can be used once.</span>
                </div>
                <asp:Literal runat="server" ID="litOffers" />
            </div>

            <!-- 4. RECENT ORDERS -->
            <div class="mgr-table-card">
                <div class="dash-card-head">
                    <h3>Recent Orders</h3>
                    <a href="~/Customer/Orders.aspx" runat="server" class="emp-action">View all orders &rarr;</a>
                </div>
                <asp:GridView ID="gvRecentOrders" runat="server" AutoGenerateColumns="false" CssClass="dash-table" GridLines="None"
                    EmptyDataText="You haven't placed any orders yet." EmptyDataRowStyle-CssClass="dash-table-empty" OnRowDataBound="gvRecentOrders_RowDataBound">
                    <Columns>
                            <asp:TemplateField HeaderText="Invoice #">
                            <ItemTemplate>
                                <a class="emp-action" href='<%# ResolveUrl("~/Customer/Orders.aspx") + "?order=" + Eval("Sale_ID") + "#order-" + Eval("Sale_ID") %>'><%# Eval("InvoiceNumber") %></a>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Sale_DateTime" HeaderText="Date" DataFormatString="{0:d MMM yyyy}" />
                        <asp:BoundField DataField="ItemCount" HeaderText="Items" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                        <asp:BoundField DataField="Sale_TotalAmount" HeaderText="Total" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                    </Columns>
                </asp:GridView>
            </div>

        </div>
    </section>

    <script>
        function copyOfferCode(btn) {
            var code = btn.getAttribute('data-code');
            function done(ok) {
                btn.textContent = ok ? 'Copied' : 'Copy failed';
                setTimeout(function () { btn.textContent = 'Copy code'; }, 1800);
            }
            if (navigator.clipboard && navigator.clipboard.writeText) {
                navigator.clipboard.writeText(code).then(function () { done(true); }, function () { done(false); });
            } else {
                done(false);
            }
        }
    </script>
</asp:Content>
