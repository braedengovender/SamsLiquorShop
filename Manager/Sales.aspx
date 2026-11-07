<%@ Page Title="Manage Sales" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Sales.aspx.cs" Inherits="SamsLiquorShop.Manager.Sales" %>
<%@ Register Src="~/Manager/ManagerNav.ascx" TagPrefix="uc" TagName="ManagerNav" %>
<%@ Register Src="~/Controls/SalesBrowser.ascx" TagPrefix="uc" TagName="SalesBrowser" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:ManagerNav runat="server" />

        <div class="mgr-main">
            <uc:SalesBrowser ID="salesBrowser" runat="server" />
        </div>
    </section>
</asp:Content>
