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
                <h1>My Account</h1>
                <p class="mgr-dash-subtext"><asp:Literal runat="server" ID="litGreeting" /></p>
            </div>

            <!-- 1. LOYALTY (1 point = R1 to redeem, 1 point earned per R10 — same rules as the POS) -->
            <div class="mgr-spotlight-card dash-loyalty-card">
                <div>
                    <p class="mgr-kpi-label">Your Loyalty Points</p>
                    <p class="dash-loyalty-points"><asp:Literal runat="server" ID="litLoyaltyPoints" /></p>
                    <p class="dash-loyalty-meta">Worth <asp:Literal runat="server" ID="litPointsValue" /> off your next order &middot; you earn 1 point for every R10 you spend</p>
                </div>
                <a href="~/Rewards.aspx" runat="server" class="dash-loyalty-link">How rewards work &rarr;</a>
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

            <!-- 3. RECENT ORDERS + WHAT I BUY (charts are drawn by the chart script in Site.Master) -->
            <div class="dash-grid">
                <div class="mgr-table-card">
                    <div class="dash-card-head">
                        <h3>Recent Orders</h3>
                        <span class="dash-card-meta">latest 5</span>
                    </div>
                    <asp:GridView ID="gvRecentOrders" runat="server" AutoGenerateColumns="false" CssClass="dash-table" GridLines="None"
                        EmptyDataText="You haven't placed any orders yet." EmptyDataRowStyle-CssClass="dash-table-empty">
                        <Columns>
                            <asp:BoundField DataField="InvoiceNumber" HeaderText="Invoice #" ItemStyle-CssClass="is-muted" />
                            <asp:BoundField DataField="Sale_DateTime" HeaderText="Date" DataFormatString="{0:d MMM yyyy}" />
                            <asp:BoundField DataField="ItemCount" HeaderText="Items" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                            <asp:BoundField DataField="Sale_TotalAmount" HeaderText="Total" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                        </Columns>
                    </asp:GridView>
                </div>
                <div class="mgr-chart-card">
                    <div class="dash-card-head"><h3>What I Buy</h3></div>
                    <div class="dash-doughnut-row">
                        <div class="dash-doughnut-wrap"><canvas data-chart="<%: CategoryChartJson %>" data-empty="No orders yet" role="img" aria-label="Doughnut chart of your spending by product category"></canvas></div>
                        <ul class="dash-legend"></ul>
                    </div>
                </div>
            </div>

            <!-- 4. TRENDS -->
            <div class="dash-grid">
                <div class="mgr-chart-card">
                    <div class="dash-card-head">
                        <h3>My Spending</h3>
                        <span class="dash-card-meta">last 12 months</span>
                    </div>
                    <div class="mgr-chart-canvas-wrap"><canvas data-chart="<%: SpendingChartJson %>" data-empty="No orders in the last 12 months" role="img" aria-label="Column chart of your spending per month over the last 12 months"></canvas></div>
                </div>
                <div class="mgr-chart-card">
                    <div class="dash-card-head">
                        <h3>My Favourite Products</h3>
                        <span class="dash-card-meta">units bought</span>
                    </div>
                    <div class="mgr-chart-canvas-wrap"><canvas data-chart="<%: FavouritesChartJson %>" data-empty="No orders yet" role="img" aria-label="Horizontal bar chart of the products you have bought most often"></canvas></div>
                </div>
            </div>

        </div>
    </section>
</asp:Content>
