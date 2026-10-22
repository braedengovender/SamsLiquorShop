<%@ Page Title="Manager Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SamsLiquorShop.Manager.Dashboard" %>
<%@ Register Src="~/Manager/ManagerNav.ascx" TagPrefix="uc" TagName="ManagerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:ManagerNav runat="server" />

        <div class="mgr-main">
            <div class="mgr-header-row dash-header-row">
                <div>
                    <h1>Store Overview</h1>
                    <p class="mgr-dash-subtext"><asp:Literal runat="server" ID="litGreeting" /> &middot; last <%= RangeLabel %></p>
                </div>
                <nav class="dash-range" aria-label="Date range">
                    <a href="?range=30d" class="dash-range-chip<%= RangeKey == "30d" ? " is-active" : "" %>">30 days</a>
                    <a href="?range=3m" class="dash-range-chip<%= RangeKey == "3m" ? " is-active" : "" %>">3 months</a>
                    <a href="?range=12m" class="dash-range-chip<%= RangeKey == "12m" ? " is-active" : "" %>">12 months</a>
                </nav>
            </div>

            <!-- 1. HEADLINE NUMBERS -->
            <div class="mgr-kpi-grid">
                <div class="mgr-kpi-card">
                    <span class="dash-kpi-icon is-blue dash-icon-cash" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">Revenue</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litRevenue" /></p>
                    <asp:Literal runat="server" ID="litRevenueDelta" />
                </div>
                <div class="mgr-kpi-card">
                    <span class="dash-kpi-icon is-amber dash-icon-receipt" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">Transactions</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litTransactions" /></p>
                    <asp:Literal runat="server" ID="litTransactionsDelta" />
                </div>
                <div class="mgr-kpi-card">
                    <span class="dash-kpi-icon is-blue dash-icon-bag" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">Avg. Basket Size</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litAvgBasket" /></p>
                    <asp:Literal runat="server" ID="litAvgBasketDelta" />
                </div>
                <div class="mgr-kpi-card mgr-kpi-alert">
                    <span class="dash-kpi-icon is-red dash-icon-alert" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">Low Stock Items</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litLowStock" /></p>
                    <span class="dash-delta is-flat">at or below reorder level</span>
                </div>
            </div>

            <!-- 2. MAIN TREND + NEEDS ATTENTION (charts are drawn by the chart script in Site.Master) -->
            <div class="dash-grid">
                <div class="mgr-chart-card dash-chart-tall">
                    <div class="dash-card-head">
                        <h3>Revenue</h3>
                        <span class="dash-card-meta"><%= RevenueBucketLabel %></span>
                    </div>
                    <div class="mgr-chart-canvas-wrap"><canvas data-chart="<%: RevenueChartJson %>" role="img" aria-label="Column chart of revenue over the selected period"></canvas></div>
                </div>
                <div class="mgr-chart-card">
                    <div class="dash-card-head">
                        <h3>Needs Restocking</h3>
                        <span class="dash-card-meta">stock / reorder level</span>
                    </div>
                    <asp:Repeater ID="rptLowStock" runat="server">
                        <HeaderTemplate><ul class="dash-stock-list"></HeaderTemplate>
                        <ItemTemplate>
                            <li class="dash-stock-item">
                                <div class="dash-stock-head">
                                    <span><%#: Eval("Product_Name") %></span>
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

            <!-- 3. BREAKDOWNS -->
            <div class="dash-grid-3">
                <div class="mgr-chart-card">
                    <div class="dash-card-head"><h3>Sales by Category</h3></div>
                    <div class="mgr-chart-canvas-wrap"><canvas data-chart="<%: CategoryChartJson %>" role="img" aria-label="Horizontal bar chart of revenue by category"></canvas></div>
                </div>
                <div class="mgr-chart-card">
                    <div class="dash-card-head"><h3>Payment Method</h3></div>
                    <div class="dash-doughnut-row">
                        <div class="dash-doughnut-wrap"><canvas data-chart="<%: PaymentChartJson %>" role="img" aria-label="Doughnut chart of revenue by payment method"></canvas></div>
                        <ul class="dash-legend"></ul>
                    </div>
                </div>
                <div class="mgr-chart-card">
                    <div class="dash-card-head">
                        <h3>Top 5 Products</h3>
                        <span class="dash-card-meta">units sold</span>
                    </div>
                    <div class="mgr-chart-canvas-wrap"><canvas data-chart="<%: TopProductsChartJson %>" role="img" aria-label="Horizontal bar chart of the five best-selling products by units"></canvas></div>
                </div>
            </div>

            <div class="dash-grid-2">
                <div class="mgr-chart-card">
                    <div class="dash-card-head"><h3>Revenue by Cashier</h3></div>
                    <div class="mgr-chart-canvas-wrap"><canvas data-chart="<%: CashierChartJson %>" role="img" aria-label="Horizontal bar chart of revenue by cashier"></canvas></div>
                </div>
                <div class="mgr-chart-card">
                    <div class="dash-card-head">
                        <h3>Busiest Hours</h3>
                        <span class="dash-card-meta">transactions</span>
                    </div>
                    <div class="mgr-chart-canvas-wrap"><canvas data-chart="<%: HourlyChartJson %>" role="img" aria-label="Column chart of transactions by hour of day"></canvas></div>
                </div>
            </div>

            <!-- 4. DETAIL TABLES -->
            <div class="dash-grid">
                <div class="mgr-table-card">
                    <div class="dash-card-head">
                        <h3>Recent Sales</h3>
                        <span class="dash-card-meta">latest 15, all time</span>
                    </div>
                    <asp:GridView ID="gvRecentSales" runat="server" AutoGenerateColumns="false" CssClass="dash-table" GridLines="None"
                        EmptyDataText="No sales recorded yet." EmptyDataRowStyle-CssClass="dash-table-empty">
                        <Columns>
                            <asp:BoundField DataField="Sale_ID" HeaderText="Sale #" ItemStyle-CssClass="is-muted" />
                            <asp:BoundField DataField="Sale_DateTime" HeaderText="Date" DataFormatString="{0:d MMM yyyy, HH:mm}" />
                            <asp:TemplateField HeaderText="Customer">
                                <ItemTemplate><%# CustomerCell(Eval("CustomerName")) %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="EmployeeName" HeaderText="Cashier" />
                            <asp:BoundField DataField="Sale_TotalAmount" HeaderText="Total" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                        </Columns>
                    </asp:GridView>
                </div>

                <div class="dash-stack">
                    <div class="mgr-table-card">
                        <div class="dash-card-head">
                            <h3>Top Customers</h3>
                            <span class="dash-card-meta">last <%= RangeLabel %></span>
                        </div>
                        <asp:GridView ID="gvTopCustomers" runat="server" AutoGenerateColumns="false" CssClass="dash-table" GridLines="None"
                            EmptyDataRowStyle-CssClass="dash-table-empty">
                            <Columns>
                                <asp:BoundField DataField="CustomerName" HeaderText="Customer" ItemStyle-CssClass="is-wrap" />
                                <asp:BoundField DataField="OrderCount" HeaderText="Orders" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                <asp:BoundField DataField="TotalSpent" HeaderText="Spent" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                            </Columns>
                        </asp:GridView>
                    </div>

                    <div class="mgr-table-card">
                        <div class="dash-card-head"><h3>Active Discounts</h3></div>
                        <asp:GridView ID="gvActiveDiscounts" runat="server" AutoGenerateColumns="false" CssClass="dash-table" GridLines="None"
                            EmptyDataText="No discounts are running right now." EmptyDataRowStyle-CssClass="dash-table-empty">
                            <Columns>
                                <asp:BoundField DataField="Discount_Name" HeaderText="Name" ItemStyle-CssClass="is-wrap" />
                                <asp:TemplateField HeaderText="Value" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num">
                                    <ItemTemplate><%# DiscountValue(Eval("Discount_Type"), Eval("Discount_Value")) %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Discount_EndDate" HeaderText="Ends" DataFormatString="{0:d MMM yyyy}" ItemStyle-CssClass="is-num is-muted" HeaderStyle-CssClass="is-num" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
