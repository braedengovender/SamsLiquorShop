<%@ Page Title="Verify" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="VerifyOtp.aspx.cs" Inherits="SamsLiquorShop.VerifyOtp" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell login-wrap">
        <div class="glass login-card wide-card">
            <p class="login-eyebrow">Sam's Liquor Shop</p>
            <h1 class="login-heading">Check your email</h1>
            <p class="login-subtext">Enter the 6-digit code we sent you.</p>

            <span id="spanDemo" runat="server" class="field-error" visible="false"></span>

            <div class="login-field">
                <label for="txtCode">Login code</label>
                <asp:TextBox ID="txtCode" runat="server" ClientIDMode="Static" CssClass="login-input" MaxLength="6" autocomplete="one-time-code"></asp:TextBox>
            </div>

            <asp:Label ID="lblError" runat="server" CssClass="field-error login-server-error" Visible="false"></asp:Label>

            <asp:Button ID="btnVerify" runat="server" Text="Verify" CssClass="btn-primary login-submit" OnClick="btnVerify_Click" />

            <p class="login-register-text"><a href="Login.aspx">Back to login</a></p>
        </div>
    </section>
</asp:Content>