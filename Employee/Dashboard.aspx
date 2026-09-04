<%--<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SamsLiquorShop.Employee.Dashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell home-section">
      <h1><asp:Literal runat="server" ID="litGreeting" /></h1>
    </section>
</asp:Content>--%>

<%@ Page Title="Cashier Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SamsLiquorShop.Employee.Dashboard" %>
<%@ Register Src="~/Employee/CashierNav.ascx" TagPrefix="uc" TagName="CashierNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout">
        <uc:CashierNav runat="server" />

        <div class="mgr-main">
            <div class="mgr-header-row">
                <h1><asp:Literal runat="server" ID="litGreeting" /></h1>
                <p class="mgr-dash-subtext">Your activity this month</p>
            </div>

            <!-- SUMMARY CARDS -->
            <div class="mgr-kpi-grid">
                <div class="mgr-kpi-card">
                    <p class="mgr-kpi-label">My Transactions</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litMyTxCount" /></p>
                </div>
                <div class="mgr-kpi-card">
                    <p class="mgr-kpi-label">My Revenue</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litMyRevenue" /></p>
                </div>
                <div class="mgr-kpi-card">
                    <p class="mgr-kpi-label">Total Customers</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litTotalCustomers" /></p>
                </div>
                <div class="mgr-kpi-card mgr-kpi-alert">
                    <p class="mgr-kpi-label">Low Stock Items</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litLowStockCount" /></p>
                </div>
            </div>

            <!-- BEST CUSTOMER SPOTLIGHT -->
            <div class="mgr-spotlight-card">
                <p class="mgr-kpi-label">Best Customer (Store-wide, this month)</p>
                <asp:PlaceHolder runat="server" ID="phBestCustomer">
                    <div class="mgr-spotlight-body">
                        <p class="mgr-spotlight-name"><asp:Literal runat="server" ID="litBestCustomerName" /></p>
                        <div class="mgr-spotlight-stats">
                            <span><asp:Literal runat="server" ID="litBestCustomerSpend" /> spent</span>
                            <span><asp:Literal runat="server" ID="litBestCustomerOrders" /> orders</span>
                        </div>
                    </div>
                </asp:PlaceHolder>
            </div>

            <!-- TABLES -->
            <div class="mgr-table-grid">
                <div class="mgr-table-card">
                    <h3>My Recent Transactions</h3>
                    <asp:GridView ID="gvMyTransactions" runat="server" AutoGenerateColumns="false" CssClass="mgr-table">
                        <Columns>
                            <asp:BoundField DataField="InvoiceNumber" HeaderText="Invoice #" />
                            <asp:BoundField DataField="Sale_DateTime" HeaderText="Date" DataFormatString="{0:g}" />
                            <asp:BoundField DataField="CustomerName" HeaderText="Customer" />
                            <asp:BoundField DataField="Sale_TotalAmount" HeaderText="Total" DataFormatString="{0:C}" />
                        </Columns>
                    </asp:GridView>
                </div>

                <div class="mgr-table-card">
                    <h3>Low Stock Items</h3>
                    <asp:GridView ID="gvLowStock" runat="server" AutoGenerateColumns="false" CssClass="mgr-table">
                        <Columns>
                            <asp:TemplateField HeaderText="Product">
                                <ItemTemplate>
                                    <div class="cash-lowstock-row">
                                        <img class="cash-lowstock-thumb" src='<%# "/ProductImage.ashx?id=" + Eval("Product_ID") %>' alt="" />
                                        <span><%# Eval("Product_Name") %></span>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Product_QuantityInStock" HeaderText="In Stock" />
                            <asp:BoundField DataField="Product_ReorderQuantity" HeaderText="Reorder At" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

        </div>
    </section>
</asp:Content>
