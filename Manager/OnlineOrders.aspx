<%@ Page Title="Manage Online Orders" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="OnlineOrders.aspx.cs" Inherits="SamsLiquorShop.Manager.OnlineOrders" %>
<%@ Register Src="~/Manager/ManagerNav.ascx" TagPrefix="uc" TagName="ManagerNav" %>
<%@ Register Src="~/Controls/OnlineOrdersBrowser.ascx" TagPrefix="uc" TagName="OnlineOrdersBrowser" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:ManagerNav runat="server" />
        <div class="mgr-main">
            <uc:OnlineOrdersBrowser ID="ordersBrowser" runat="server" ManagerMode="true" />
        </div>
    </section>
</asp:Content>