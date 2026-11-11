<%@ Page Title="Orders" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Orders.aspx.cs" Inherits="SamsLiquorShop.Employee.Orders" %>
<%@ Register Src="~/Employee/CashierNav.ascx" TagPrefix="uc" TagName="CashierNav" %>
<%@ Register Src="~/Controls/SalesBrowser.ascx" TagPrefix="uc" TagName="SalesBrowser" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:CashierNav runat="server" />

        <div class="mgr-main">
            <uc:SalesBrowser ID="salesBrowser" runat="server" CashierMode="true" />
        </div>
    </section>
</asp:Content>
