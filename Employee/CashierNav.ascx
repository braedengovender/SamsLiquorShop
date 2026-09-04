<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CashierNav.ascx.cs" Inherits="SamsLiquorShop.Employee.CashierNav" %>

<nav class="mgr-sidebar">
    <p class="mgr-sidebar-label">Cashier</p>
    <ul class="mgr-sidebar-links">
        <li><a href="~/Employee/Dashboard.aspx" runat="server" id="navOverview">Overview</a></li>
        <li><a href="~/Employee/Orders.aspx" runat="server" id="navOrders">Orders</a></li>
        <li><a href="~/Employee/Products.aspx" runat="server" id="navProducts">View Products</a></li>
        <li class="mgr-sidebar-divider"></li>
        <li><a href="~/Employee/Account.aspx" runat="server" id="navAccount">My Account</a></li>
    </ul>
</nav>
