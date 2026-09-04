<%--<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ManagerNav.ascx.cs" Inherits="SamsLiquorShop.Manager.ManagerNav" %>--%>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ManagerNav.ascx.cs" Inherits="SamsLiquorShop.Manager.ManagerNav" %>
<nav class="mgr-sidebar">
    <p class="mgr-sidebar-label">Manager</p>
    <ul class="mgr-sidebar-links">
        <li><a href="~/Manager/Dashboard.aspx" runat="server" id="navOverview">Overview</a></li>
        <li><a href="~/Manager/Orders.aspx" runat="server" id="navOrders">Manage Orders</a></li>
        <li><a href="~/Manager/Products.aspx" runat="server" id="navProducts">Manage Products</a></li>
        <li><a href="~/Manager/Employees.aspx" runat="server" id="navEmployees">Manage Employees</a></li>
        <li><a href="~/Manager/Customers.aspx" runat="server" id="navCustomers">Manage Customers</a></li>
        <li><a href="~/Manager/Discounts.aspx" runat="server" id="navDiscounts">Manage Discounts</a></li>
        <li class="mgr-sidebar-divider"></li>
        <li><a href="~/Manager/Account.aspx" runat="server" id="navAccount">My Account</a></li>
    </ul>
</nav>
