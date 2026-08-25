<%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SamsLiquorShop.Login" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
  <section class="shell login-wrap">
    <div class="glass login-card">
      <p class="login-eyebrow">Sam's Liquor Shop</p>
      <h1 class="login-heading">Welcome back</h1>
      <p class="login-subtext">Log in to manage your orders, rewards, and account details.</p>

      <div class="login-field">
        <label for="<%= txtUsername.ClientID %>">Email or Username</label>
        <asp:TextBox ID="txtUsername" runat="server" ClientIDMode="Static" CssClass="login-input" placeholder="you@example.com" autocomplete="username"></asp:TextBox>
        <span class="field-error" id="usernameError"></span>
      </div>

      <div class="login-field">
        <label for="<%= txtPassword.ClientID %>">Password</label>
        <div class="login-password-wrap">
          <asp:TextBox ID="txtPassword" runat="server" ClientIDMode="Static" TextMode="Password" CssClass="login-input" placeholder="Enter your password" autocomplete="current-password"></asp:TextBox>
          <button type="button" class="login-eye-btn" onclick="togglePasswordVisibility()" aria-label="Show password">
            <span id="eyeIcon">&#128065;</span>
          </button>
        </div>
        <span class="field-error" id="passwordError"></span>
      </div>

      <div class="login-remember-row">
        <label class="login-remember-label">
          <asp:CheckBox ID="RememberMe" runat="server" ClientIDMode="Static" />
          Remember me
        </label>
        <a href="ForgotPassword.aspx" class="login-forgot">Forgot your password?</a>
      </div>

      <asp:Label ID="lblError" runat="server" CssClass="field-error login-server-error" Visible="false"></asp:Label>

      <asp:Button ID="btnLogin" runat="server" Text="Log In" CssClass="btn-primary login-submit" OnClick="btnLogin_Click" OnClientClick="return validateLoginForm();" />

      <div class="login-divider"><span>or</span></div>

      <button type="button" class="login-google-btn" onclick="alert('Google sign-in isn\'t connected yet — this is a placeholder for now.');">
        <svg width="18" height="18" viewBox="0 0 18 18" xmlns="http://www.w3.org/2000/svg">
          <path fill="#4285F4" d="M17.64 9.2c0-.64-.06-1.25-.16-1.84H9v3.48h4.84a4.14 4.14 0 0 1-1.8 2.72v2.26h2.92c1.7-1.57 2.68-3.88 2.68-6.62z"/>
          <path fill="#34A853" d="M9 18c2.43 0 4.47-.8 5.96-2.18l-2.92-2.26c-.81.54-1.84.86-3.04.86-2.34 0-4.32-1.58-5.03-3.7H.96v2.33A9 9 0 0 0 9 18z"/>
          <path fill="#FBBC05" d="M3.97 10.72A5.4 5.4 0 0 1 3.68 9c0-.6.1-1.18.29-1.72V4.95H.96A9 9 0 0 0 0 9c0 1.45.35 2.83.96 4.05l3.01-2.33z"/>
          <path fill="#EA4335" d="M9 3.58c1.32 0 2.51.45 3.44 1.35l2.58-2.58C13.46.89 11.43 0 9 0A9 9 0 0 0 .96 4.95l3.01 2.33C4.68 5.16 6.66 3.58 9 3.58z"/>
        </svg>
        Sign in with Google
      </button>

      <p class="login-register-text">Don't have an account? <a href="Account/Register">Register now</a></p>
    </div>
  </section>
</asp:Content>