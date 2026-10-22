<%--<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SamsLiquorShop.Employee.Dashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell home-section">
      <h1><asp:Literal runat="server" ID="litGreeting" /></h1>
    </section>
</asp:Content>--%>

<%@ Page Title="Cashier Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SamsLiquorShop.Employee.Dashboard" %>
<%@ Register Src="~/Employee/CashierNav.ascx" TagPrefix="uc" TagName="CashierNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:CashierNav runat="server" />

        <div class="mgr-main">
            <div class="mgr-header-row">
                <h1>My Sales</h1>
                <p class="mgr-dash-subtext"><asp:Literal runat="server" ID="litGreeting" /> &middot; <%= MonthLabel %></p>
            </div>

            <!-- 1. HEADLINE NUMBERS -->
            <div class="mgr-kpi-grid">
                <div class="mgr-kpi-card">
                    <span class="dash-kpi-icon is-amber dash-icon-receipt" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">My Transactions</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litMyTxCount" /></p>
                    <asp:Literal runat="server" ID="litMyTxDelta" />
                </div>
                <div class="mgr-kpi-card">
                    <span class="dash-kpi-icon is-blue dash-icon-cash" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">My Revenue</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litMyRevenue" /></p>
                    <asp:Literal runat="server" ID="litMyRevenueDelta" />
                </div>
                <div class="mgr-kpi-card">
                    <span class="dash-kpi-icon is-blue dash-icon-bag" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">My Average Sale</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litMyAvgSale" /></p>
                    <asp:Literal runat="server" ID="litMyAvgSaleDelta" />
                </div>
                <div class="mgr-kpi-card mgr-kpi-alert">
                    <span class="dash-kpi-icon is-red dash-icon-alert" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">Low Stock Items</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litLowStockCount" /></p>
                    <span class="dash-delta is-flat">at or below reorder level</span>
                </div>
            </div>

            <!-- 2. MAIN TREND + NEEDS ATTENTION (charts are drawn by the chart script in Site.Master) -->
            <div class="dash-grid">
                <div class="mgr-chart-card dash-chart-tall">
                    <div class="dash-card-head">
                        <h3>My Daily Sales</h3>
                        <span class="dash-card-meta">this month</span>
                    </div>
                    <div class="mgr-chart-canvas-wrap"><canvas data-chart="<%: DailySalesChartJson %>" data-empty="No sales yet this month" role="img" aria-label="Column chart of your revenue for each day this month"></canvas></div>
                </div>
                <div class="mgr-chart-card">
                    <div class="dash-card-head">
                        <h3>Low Stock</h3>
                        <span class="dash-card-meta">stock / reorder level</span>
                    </div>
                    <asp:Repeater ID="rptLowStock" runat="server">
                        <HeaderTemplate><ul class="dash-stock-list"></HeaderTemplate>
                        <ItemTemplate>
                            <li class="dash-stock-item">
                                <div class="dash-stock-head">
                                    <span class="cash-lowstock-row">
                                        <img class="cash-lowstock-thumb" src='<%# "/ProductImage.ashx?id=" + Eval("Product_ID") %>' alt="" />
                                        <span><%#: Eval("Product_Name") %></span>
                                    </span>
                                    <span class="dash-stock-count"><%# Eval("Product_QuantityInStock") %> / <%# Eval("Product_ReorderQuantity") %></span>
                                </div>
                                <div class="dash-meter"><span class="dash-meter-fill <%# StockLevelClass(Eval("Product_QuantityInStock"), Eval("Product_ReorderQuantity")) %>" style="width: <%# StockPercent(Eval("Product_QuantityInStock"), Eval("Product_ReorderQuantity")) %>%"></span></div>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate></ul></FooterTemplate>
                    </asp:Repeater>
                    <asp:PlaceHolder runat="server" ID="phNoLowStock" Visible="false">
                        <p class="dash-empty">Everything is above its reorder level.</p>
                    </asp:PlaceHolder>
                </div>
            </div>

            <!-- 3. RECENT ACTIVITY + BREAKDOWN -->
            <div class="dash-grid">
                <div class="mgr-table-card">
                    <div class="dash-card-head">
                        <h3>My Recent Transactions</h3>
                        <span class="dash-card-meta">latest 10</span>
                    </div>
                    <asp:GridView ID="gvMyTransactions" runat="server" AutoGenerateColumns="false" CssClass="dash-table" GridLines="None"
                        EmptyDataText="You haven't processed any sales yet." EmptyDataRowStyle-CssClass="dash-table-empty">
                        <Columns>
                            <asp:BoundField DataField="InvoiceNumber" HeaderText="Invoice #" ItemStyle-CssClass="is-muted" />
                            <asp:BoundField DataField="Sale_DateTime" HeaderText="Date" DataFormatString="{0:d MMM yyyy, HH:mm}" />
                            <asp:TemplateField HeaderText="Customer">
                                <ItemTemplate><%# CustomerCell(Eval("CustomerName")) %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Sale_TotalAmount" HeaderText="Total" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                        </Columns>
                    </asp:GridView>
                </div>
                <div class="mgr-chart-card">
                    <div class="dash-card-head">
                        <h3>How My Customers Paid</h3>
                        <span class="dash-card-meta">this month</span>
                    </div>
                    <div class="dash-doughnut-row">
                        <div class="dash-doughnut-wrap"><canvas data-chart="<%: PaymentChartJson %>" data-empty="No sales yet this month" role="img" aria-label="Doughnut chart of your sales by payment method"></canvas></div>
                        <ul class="dash-legend"></ul>
                    </div>
                </div>
            </div>

            <div class="dash-grid">
                <div class="mgr-chart-card">
                    <div class="dash-card-head">
                        <h3>My Busiest Hours</h3>
                        <span class="dash-card-meta">transactions this month</span>
                    </div>
                    <div class="mgr-chart-canvas-wrap"><canvas data-chart="<%: HourlyChartJson %>" data-empty="No sales yet this month" role="img" aria-label="Column chart of your transactions by hour of day"></canvas></div>
                </div>
                <div class="mgr-chart-card">
                    <div class="dash-card-head">
                        <h3>Best Customer</h3>
                        <span class="dash-card-meta">store-wide, this month</span>
                    </div>
                    <asp:PlaceHolder runat="server" ID="phBestCustomer">
                        <p class="dash-stat-name"><asp:Literal runat="server" ID="litBestCustomerName" /></p>
                        <p class="dash-stat-meta"><asp:Literal runat="server" ID="litBestCustomerSpend" /> spent &middot; <asp:Literal runat="server" ID="litBestCustomerOrders" /> orders</p>
                    </asp:PlaceHolder>
                    <asp:PlaceHolder runat="server" ID="phNoBestCustomer" Visible="false">
                        <p class="dash-empty">No named customers have bought anything this month.</p>
                    </asp:PlaceHolder>
                </div>
            </div>

        </div>
    </section>
</asp:Content>
