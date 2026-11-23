<%@ Page Title="My Account" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Account.aspx.cs" Inherits="SamsLiquorShop.Manager.Account" %>
<%@ Register Src="~/Manager/ManagerNav.ascx" TagPrefix="uc" TagName="ManagerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:ManagerNav runat="server" />

        <div class="mgr-main">
            <div class="mgr-header-row">
                <h1>My Account</h1>
                <p class="mgr-dash-subtext">View your staff profile, update your contact details and change your password.</p>
            </div>

            <!-- PROFILE SUMMARY (read-only) -->
            <div class="mgr-kpi-grid">
                <div class="mgr-kpi-card">
                    <span class="dash-kpi-icon is-blue dash-icon-receipt" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">Employee ID</p>
                    <p class="mgr-kpi-value"><asp:Literal ID="litEmployeeId" runat="server" /></p>
                </div>
                <div class="mgr-kpi-card">
                    <span class="dash-kpi-icon is-amber dash-icon-cash" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">Role</p>
                    <p class="mgr-kpi-value"><asp:Literal ID="litRole" runat="server" /></p>
                </div>
                <div class="mgr-kpi-card">
                    <span class="dash-kpi-icon is-blue dash-icon-bag" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">Status</p>
                    <p class="mgr-kpi-value"><asp:Literal ID="litStatus" runat="server" /></p>
                </div>
                <div class="mgr-kpi-card">
                    <span class="dash-kpi-icon is-blue dash-icon-alert" aria-hidden="true"></span>
                    <p class="mgr-kpi-label">Hire date</p>
                    <p class="mgr-kpi-value"><asp:Literal ID="litHireDate" runat="server" /></p>
                </div>
            </div>

            <asp:Literal ID="litMessage" runat="server" />

            <!-- PERSONAL DETAILS -->
            <asp:Panel ID="pnlDetails" runat="server" DefaultButton="btnSaveDetails">
                <div class="mgr-table-card emp-form-card">
                    <div class="dash-card-head"><h3>Personal Details</h3></div>

                    <div class="emp-form-grid">
                        <div class="login-field">
                            <label for="<%= txtFirstName.ClientID %>">First name</label>
                            <asp:TextBox ID="txtFirstName" runat="server" CssClass="login-input" MaxLength="50" autocomplete="given-name" />
                        </div>
                        <div class="login-field">
                            <label for="<%= txtLastName.ClientID %>">Last name</label>
                            <asp:TextBox ID="txtLastName" runat="server" CssClass="login-input" MaxLength="50" autocomplete="family-name" />
                        </div>
                        <div class="login-field">
                            <label for="<%= txtIdNumber.ClientID %>">SA ID number</label>
                            <asp:TextBox ID="txtIdNumber" runat="server" CssClass="login-input" MaxLength="13" autocomplete="off" placeholder="13 digits" data-digits="1" />
                        </div>
                        <div class="login-field">
                            <label for="<%= txtPhone.ClientID %>">Phone number</label>
                            <asp:TextBox ID="txtPhone" runat="server" CssClass="login-input" MaxLength="12" autocomplete="tel" placeholder="e.g. 0821234567" data-phone="1" />
                            <span class="field-hint">For example 0821234567 or +27821234567.</span>
                        </div>
                        <div class="login-field">
                            <label for="<%= txtUsername.ClientID %>">Username</label>
                            <asp:TextBox ID="txtUsername" runat="server" CssClass="login-input" MaxLength="30" autocomplete="username" />
                            <span class="field-hint">3&ndash;30 letters, numbers, underscores or full stops.</span>
                        </div>
                        <div class="login-field">
                            <label for="<%= txtEmail.ClientID %>">Email address</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="login-input" MaxLength="100" autocomplete="email" />
                            <span class="field-hint">Your login codes are sent here.</span>
                        </div>
                    </div>

                    <p class="register-static-field">Your role, status and hire date can only be changed by another manager under Manage Employees.</p>

                    <div class="emp-form-actions">
                        <asp:Button ID="btnSaveDetails" runat="server" Text="Save Changes" CssClass="btn-primary" OnClick="btnSaveDetails_Click" />
                        <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn-ghost" OnClick="btnReset_Click" CausesValidation="false" />
                    </div>
                </div>
            </asp:Panel>

            <!-- CHANGE PASSWORD -->
            <asp:Panel ID="pnlPassword" runat="server" DefaultButton="btnChangePassword">
                <div class="mgr-table-card emp-form-card">
                    <div class="dash-card-head"><h3>Change Password</h3></div>

                    <div class="emp-form-grid">
                        <div class="login-field">
                            <label for="<%= txtCurrentPassword.ClientID %>">Current password</label>
                            <asp:TextBox ID="txtCurrentPassword" runat="server" CssClass="login-input" TextMode="Password" MaxLength="50" autocomplete="current-password" />
                        </div>
                        <div></div>
                        <div class="login-field">
                            <label for="<%= txtNewPassword.ClientID %>">New password</label>
                            <asp:TextBox ID="txtNewPassword" runat="server" CssClass="login-input" TextMode="Password" MaxLength="50" autocomplete="new-password" />
                            <span class="field-hint">8&ndash;50 characters with an upper-case letter, a lower-case letter, a number and a symbol.</span>
                        </div>
                        <div class="login-field">
                            <label for="<%= txtConfirmPassword.ClientID %>">Confirm new password</label>
                            <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="login-input" TextMode="Password" MaxLength="50" autocomplete="new-password" />
                        </div>
                    </div>

                    <div class="emp-form-actions">
                        <asp:Button ID="btnChangePassword" runat="server" Text="Update Password" CssClass="btn-primary" OnClick="btnChangePassword_Click" />
                    </div>
                </div>
            </asp:Panel>
        </div>
    </section>
</asp:Content>
