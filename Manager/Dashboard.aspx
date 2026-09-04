<%@ Page Title="Manager Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SamsLiquorShop.Manager.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell home-section mgr-dash">

        <h1><asp:Literal runat="server" ID="litGreeting" /></h1>
        <p class="mgr-dash-subtext">Overview for the last <%= MonthsBack %> months</p>

        <!-- KPI CARDS -->
        <div class="mgr-kpi-grid">
            <div class="glass mgr-kpi-card">
                <p class="mgr-kpi-label">Revenue</p>
                <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litRevenue" /></p>
            </div>
            <div class="glass mgr-kpi-card">
                <p class="mgr-kpi-label">Transactions</p>
                <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litTransactions" /></p>
            </div>
            <div class="glass mgr-kpi-card">
                <p class="mgr-kpi-label">Avg. Basket Size</p>
                <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litAvgBasket" /></p>
            </div>
            <div class="glass mgr-kpi-card mgr-kpi-alert">
                <p class="mgr-kpi-label">Low Stock Items</p>
                <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litLowStock" /></p>
            </div>
        </div>

        <!-- CHARTS -->
        <div class="mgr-chart-grid">
            <div class="glass mgr-chart-card">
                <h3>Revenue by Month</h3>
                <canvas id="chartRevenue"></canvas>
            </div>
            <div class="glass mgr-chart-card">
                <h3>Top 5 Products (by units sold)</h3>
                <canvas id="chartTopProducts"></canvas>
            </div>
            <div class="glass mgr-chart-card">
                <h3>Sales by Category</h3>
                <canvas id="chartCategory"></canvas>
            </div>
            <div class="glass mgr-chart-card">
                <h3>Sales by Payment Method</h3>
                <canvas id="chartPayment"></canvas>
            </div>
        </div>

        <!-- TABLES -->
        <div class="mgr-table-grid">
            <div class="glass mgr-table-card">
                <h3>Recent Sales</h3>
                <asp:GridView ID="gvRecentSales" runat="server" AutoGenerateColumns="false" CssClass="mgr-table">
                    <Columns>
                        <asp:BoundField DataField="Sale_ID" HeaderText="Sale #" />
                        <asp:BoundField DataField="Sale_DateTime" HeaderText="Date" DataFormatString="{0:g}" />
                        <asp:BoundField DataField="CustomerName" HeaderText="Customer" />
                        <asp:BoundField DataField="EmployeeName" HeaderText="Cashier" />
                        <asp:BoundField DataField="Sale_TotalAmount" HeaderText="Total" DataFormatString="{0:C}" />
                    </Columns>
                </asp:GridView>
            </div>

            <div class="glass mgr-table-card">
                <h3>Top Customers</h3>
                <asp:GridView ID="gvTopCustomers" runat="server" AutoGenerateColumns="false" CssClass="mgr-table">
                    <Columns>
                        <asp:BoundField DataField="CustomerName" HeaderText="Customer" />
                        <asp:BoundField DataField="TotalSpent" HeaderText="Total Spent" DataFormatString="{0:C}" />
                        <asp:BoundField DataField="OrderCount" HeaderText="Orders" />
                    </Columns>
                </asp:GridView>
            </div>

            <div class="glass mgr-table-card">
                <h3>Active Discounts</h3>
                <asp:GridView ID="gvActiveDiscounts" runat="server" AutoGenerateColumns="false" CssClass="mgr-table">
                    <Columns>
                        <asp:BoundField DataField="Discount_Name" HeaderText="Name" />
                        <asp:BoundField DataField="Discount_Type" HeaderText="Type" />
                        <asp:BoundField DataField="Discount_Value" HeaderText="Value" />
                        <asp:BoundField DataField="Discount_EndDate" HeaderText="Ends" DataFormatString="{0:d}" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>

    </section>

    <script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/4.4.4/chart.umd.min.js"></script>
    <script>
        const revenueData = <%= RevenueChartJson %>;
        const topProductsData = <%= TopProductsChartJson %>;
        const categoryData = <%= CategoryChartJson %>;
        const paymentData = <%= PaymentChartJson %>;

        new Chart(document.getElementById('chartRevenue'), {
            type: 'line',
            data: {
                labels: revenueData.labels,
                datasets: [{
                    label: 'Revenue',
                    data: revenueData.values,
                    borderColor: '#0a84ff',
                    backgroundColor: 'rgba(10,132,255,0.1)',
                    fill: true,
                    tension: 0.3
                }]
            },
            options: { plugins: { legend: { display: false } } }
        });

        new Chart(document.getElementById('chartTopProducts'), {
            type: 'bar',
            data: {
                labels: topProductsData.labels,
                datasets: [{
                    label: 'Units Sold',
                    data: topProductsData.values,
                    backgroundColor: '#c9a24b'
                }]
            },
            options: { indexAxis: 'y', plugins: { legend: { display: false } } }
        });

        new Chart(document.getElementById('chartCategory'), {
            type: 'doughnut',
            data: {
                labels: categoryData.labels,
                datasets: [{
                    data: categoryData.values,
                    backgroundColor: ['#0a84ff', '#5ac8fa', '#c9a24b', '#a67f2e', '#6e6e73', '#1d1d1f']
                }]
            }
        });

        new Chart(document.getElementById('chartPayment'), {
            type: 'pie',
            data: {
                labels: paymentData.labels,
                datasets: [{
                    data: paymentData.values,
                    backgroundColor: ['#0a84ff', '#c9a24b', '#5ac8fa', '#a67f2e']
                }]
            }
        });
    </script>
</asp:Content>