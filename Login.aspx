<%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SamsLiquorShop.Login" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
  <section class="shell login-wrap">
    <div class="glass login-card">
      <p class="login-eyebrow">Sam's Liquor Shop</p>
      <h1 class="login-heading">Log in</h1>

      <div class="form-group">
        <asp:TextBox ID="txtUsername" runat="server" placeholder="Username" CssClass="login-input" autocomplete="username"></asp:TextBox>
      </div>
      <div class="form-group">
        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Password" CssClass="login-input" autocomplete="new-password"></asp:TextBox>
      </div>

      <asp:Label ID="lblError" runat="server" CssClass="field-error" Visible="false"></asp:Label>

      <asp:Button ID="btnLogin" runat="server" Text="Log In" CssClass="btn-primary login-submit" OnClick="btnLogin_Click" />
    </div>
  </section>
</asp:Content>