<%@ Page Title="Manage Online Orders" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="OnlineOrders.aspx.cs" Inherits="SamsLiquorShop.Employee.OnlineOrders" %>
<%@ Register Src="~/Employee/CashierNav.ascx" TagPrefix="uc" TagName="CashierNav" %>
<%@ Register Src="~/Controls/OnlineOrdersBrowser.ascx" TagPrefix="uc" TagName="OnlineOrdersBrowser" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:CashierNav runat="server" />
        <div class="mgr-main">
            <uc:OnlineOrdersBrowser ID="ordersBrowser" runat="server" ManagerMode="false" />
        </div>
    </section>
</asp:Content>