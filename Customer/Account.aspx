<%@ Page Title="Account Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Account.aspx.cs" Inherits="SamsLiquorShop.Customer.Account" %>
<%@ Register Src="~/Customer/CustomerNav.ascx" TagPrefix="uc" TagName="CustomerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:CustomerNav runat="server" />

        <div class="mgr-main">
            <div class="mgr-header-row">
                <h1>Account Details</h1>
                <p class="mgr-dash-subtext">Update your personal details, delivery address and password.</p>
            </div>

            <asp:Literal ID="litMessage" runat="server" />

            <!-- PERSONAL + ADDRESS DETAILS -->
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
                            <span class="field-hint">You must be 18 or older.</span>
                        </div>
                        <div class="login-field">
                            <label for="<%= txtPhone.ClientID %>">Phone number</label>
                            <asp:TextBox ID="txtPhone" runat="server" CssClass="login-input" MaxLength="12" autocomplete="tel" placeholder="e.g. 0821234567" data-phone="1" />
                            <span class="field-hint">For example 0821234567 or +27821234567.</span>
                        </div>
                        <div class="login-field">
                            <label for="<%= txtUsername.ClientID %>">Username</label>
                            <asp:TextBox ID="txtUsername" runat="server" CssClass="login-input" MaxLength="30" autocomplete="username" />
                            <span class="field-hint">4&ndash;30 letters, numbers or underscores.</span>
                        </div>
                        <div class="login-field">
                            <label for="<%= txtEmail.ClientID %>">Email address</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="login-input" MaxLength="100" autocomplete="email" />
                        </div>
                    </div>
                </div>

                <div class="mgr-table-card emp-form-card">
                    <div class="dash-card-head"><h3>Residential Address</h3></div>

                    <div class="emp-form-grid">
                        <div class="login-field">
                            <label for="<%= txtUnitNumber.ClientID %>">Unit number <span class="register-optional">(optional)</span></label>
                            <asp:TextBox ID="txtUnitNumber" runat="server" CssClass="login-input" MaxLength="20" autocomplete="off" />
                        </div>
                        <div class="login-field">
                            <label for="<%= txtUnitName.ClientID %>">Unit / complex name <span class="register-optional">(optional)</span></label>
                            <asp:TextBox ID="txtUnitName" runat="server" CssClass="login-input" MaxLength="50" autocomplete="off" />
                        </div>
                        <div class="login-field">
                            <label for="<%= txtStreetNumber.ClientID %>">Street number</label>
                            <asp:TextBox ID="txtStreetNumber" runat="server" CssClass="login-input" MaxLength="10" autocomplete="off" />
                        </div>
                        <div class="login-field">
                            <label for="<%= txtStreetName.ClientID %>">Street name</label>
                            <asp:TextBox ID="txtStreetName" runat="server" CssClass="login-input" MaxLength="100" autocomplete="address-line1" />
                        </div>
                        <div class="login-field">
                            <label for="<%= txtSuburb.ClientID %>">Suburb</label>
                            <asp:TextBox ID="txtSuburb" runat="server" CssClass="login-input" MaxLength="50" autocomplete="address-level3" />
                        </div>
                        <div class="login-field">
                            <label for="<%= txtCity.ClientID %>">City</label>
                            <asp:TextBox ID="txtCity" runat="server" CssClass="login-input" MaxLength="50" autocomplete="address-level2" />
                        </div>
                        <div class="login-field">
                            <label for="<%= txtPostalCode.ClientID %>">Postal code</label>
                            <asp:TextBox ID="txtPostalCode" runat="server" CssClass="login-input" MaxLength="4" autocomplete="postal-code" data-digits="1" />
                        </div>
                        <div class="login-field">
                            <label for="<%= ddlProvince.ClientID %>">Province</label>
                            <asp:DropDownList ID="ddlProvince" runat="server" CssClass="login-input">
                                <asp:ListItem Text="Select province" Value="" />
                                <asp:ListItem Text="Eastern Cape" Value="Eastern Cape" />
                                <asp:ListItem Text="Free State" Value="Free State" />
                                <asp:ListItem Text="Gauteng" Value="Gauteng" />
                                <asp:ListItem Text="KwaZulu-Natal" Value="KwaZulu-Natal" />
                                <asp:ListItem Text="Limpopo" Value="Limpopo" />
                                <asp:ListItem Text="Mpumalanga" Value="Mpumalanga" />
                                <asp:ListItem Text="Northern Cape" Value="Northern Cape" />
                                <asp:ListItem Text="North West" Value="North West" />
                                <asp:ListItem Text="Western Cape" Value="Western Cape" />
                            </asp:DropDownList>
                        </div>
                    </div>

                    <p class="register-static-field">Country: South Africa</p>

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
