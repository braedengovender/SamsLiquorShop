<%--<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SamsLiquorShop.Customer.Dashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal runat="server" ID="litGreeting" /></h1>
</asp:Content>--%>

<%@ Page Title="My Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SamsLiquorShop.Customer.Dashboard" %>
<%@ Register Src="~/Customer/CustomerNav.ascx" TagPrefix="uc" TagName="CustomerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout">
        <uc:CustomerNav runat="server" />

        <div class="mgr-main">
            <div class="mgr-header-row">
                <h1><asp:Literal runat="server" ID="litGreeting" /></h1>
                <p class="mgr-dash-subtext">Here's what's happening with your account</p>
            </div>

            <!-- SUMMARY CARDS -->
            <div class="mgr-kpi-grid">
                <div class="mgr-kpi-card">
                    <p class="mgr-kpi-label">Total Spent</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litTotalSpent" /></p>
                </div>
                <div class="mgr-kpi-card">
                    <p class="mgr-kpi-label">Total Orders</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litOrderCount" /></p>
                </div>
                <div class="mgr-kpi-card">
                    <p class="mgr-kpi-label">Loyalty Points</p>
                    <p class="mgr-kpi-value"><asp:Literal runat="server" ID="litLoyaltyPoints" /></p>
                </div>
            </div>

            <!-- RECENT ORDER -->
            <div class="mgr-spotlight-card">
                <p class="mgr-kpi-label">Most Recent Order</p>
                <asp:PlaceHolder runat="server" ID="phRecentOrder">
                    <div class="mgr-spotlight-body">
                        <p class="mgr-spotlight-name"><asp:Literal runat="server" ID="litRecentOrderDate" /></p>
                        <div class="mgr-spotlight-stats">
                            <span><asp:Literal runat="server" ID="litRecentOrderTotal" /></span>
                            <span><asp:Literal runat="server" ID="litRecentOrderItems" /> items</span>
                        </div>
                    </div>
                    <a href="Orders.aspx" class="btn-ghost cust-view-orders-link">View all orders &rarr;</a>
                </asp:PlaceHolder>
                <asp:Literal runat="server" ID="litNoOrders" Visible="false" Text="You haven't placed any orders yet." />
            </div>

        </div>
    </section>
</asp:Content>
