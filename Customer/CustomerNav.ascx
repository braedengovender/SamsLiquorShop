<%--<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CustomerNav.ascx.cs" Inherits="SamsLiquorShop.Customer.CustomerNav" %>--%>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CustomerNav.ascx.cs" Inherits="SamsLiquorShop.Customer.CustomerNav" %>

<nav class="mgr-sidebar">
    <p class="mgr-sidebar-label">My Account</p>
    <ul class="mgr-sidebar-links">
        <li><a href="~/Customer/Dashboard.aspx" runat="server" id="navOverview">Overview</a></li>
        <li><a href="~/Customer/Orders.aspx" runat="server" id="navOrders">My Orders</a></li>
        <li><a href="~/Customer/Wishlist.aspx" runat="server" id="navWishlist">My Wishlist</a></li>
        <li class="mgr-sidebar-divider"></li>
        <li><a href="~/Customer/Account.aspx" runat="server" id="navAccount">Account Details</a></li>
    </ul>
</nav>
