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

            <!-- CHANGE PASSWORD (step 1: current + new password; step 2: code emailed to the account) -->
            <asp:Panel ID="pnlPassword" runat="server" DefaultButton="btnChangePassword">
                <div class="mgr-table-card emp-form-card" id="change-password">
                    <div class="dash-card-head"><h3>Change Password</h3></div>
                    <p class="mgr-dash-subtext">For your security, we'll email a 6-digit code to the address on your account before the new password is saved.</p>

                    <div class="emp-form-grid">
                        <div class="login-field login-password-wrap">
                            <label for="<%= txtCurrentPassword.ClientID %>">Current password <button type="button" class="field-help" data-rules="Required.|The password you use to log in now.|Forgotten it? Log out and use &quot;Forgot password&quot; on the login page."></button></label>
                            <div class="password-input-wrap">
                                <asp:TextBox ID="txtCurrentPassword" runat="server" CssClass="login-input" TextMode="Password" MaxLength="50" autocomplete="current-password" />
                                <button type="button" class="login-eye-btn" onclick="toggleFieldVisibility('<%= txtCurrentPassword.ClientID %>', this)" aria-label="Show password">
                                    <svg class="eye-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6z"></path><circle cx="12" cy="12" r="2.5"></circle></svg>
                                </button>
                            </div>
                        </div>
                        <div></div>
                        <div class="login-field login-password-wrap">
                            <div class="password-label-row">
                                <label for="<%= txtNewPassword.ClientID %>">New password <button type="button" class="field-help" data-rules="Required.|8 to 50 characters.|At least 1 lowercase letter, 1 uppercase letter, 1 number and 1 special character (e.g. ! @ # ?).|Must be different from your current password.|Tip: press &quot;Suggest password&quot; to generate a strong one."></button></label>
                                <button type="button" class="suggest-password-btn" onclick="suggestPassword('<%= txtNewPassword.ClientID %>', '<%= txtConfirmPassword.ClientID %>')">Suggest password</button>
                            </div>
                            <div class="password-input-wrap">
                                <asp:TextBox ID="txtNewPassword" runat="server" CssClass="login-input" TextMode="Password" MaxLength="50" autocomplete="new-password" oninput="checkPasswordStrength(this.value)" />
                                <button type="button" class="login-eye-btn" onclick="toggleFieldVisibility('<%= txtNewPassword.ClientID %>', this)" aria-label="Show password">
                                    <svg class="eye-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6z"></path><circle cx="12" cy="12" r="2.5"></circle></svg>
                                </button>
                            </div>

                            <div id="suggestedPasswordNote" class="suggested-password-note" hidden>
                                <span>Suggested password filled in both fields. Save it somewhere safe.</span>
                                <button type="button" class="suggest-password-copy" onclick="copySuggestedPassword(this)">Copy</button>
                            </div>

                            <div class="password-strength">
                                <div class="password-strength-bar"><div id="passwordStrengthFill"></div></div>
                                <div id="passwordStrengthText" class="password-strength-text">Password strength</div>
                                <div class="password-requirements">
                                    <div id="reqLength" class="password-requirement"><span class="requirement-icon">○</span><span>At least 8 characters</span></div>
                                    <div id="reqLower" class="password-requirement"><span class="requirement-icon">○</span><span>1 lowercase letter</span></div>
                                    <div id="reqUpper" class="password-requirement"><span class="requirement-icon">○</span><span>1 uppercase letter</span></div>
                                    <div id="reqNumber" class="password-requirement"><span class="requirement-icon">○</span><span>1 number</span></div>
                                    <div id="reqSpecial" class="password-requirement"><span class="requirement-icon">○</span><span>1 special character</span></div>
                                </div>
                            </div>
                        </div>
                        <div class="login-field login-password-wrap">
                            <label for="<%= txtConfirmPassword.ClientID %>">Confirm new password <button type="button" class="field-help" data-rules="Required.|Type the same password again. It must match exactly (capitals count)."></button></label>
                            <div class="password-input-wrap">
                                <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="login-input" TextMode="Password" MaxLength="50" autocomplete="new-password" />
                                <button type="button" class="login-eye-btn" onclick="toggleFieldVisibility('<%= txtConfirmPassword.ClientID %>', this)" aria-label="Show password">
                                    <svg class="eye-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6z"></path><circle cx="12" cy="12" r="2.5"></circle></svg>
                                </button>
                            </div>
                        </div>
                    </div>

                    <div class="emp-form-actions">
                        <asp:Button ID="btnChangePassword" runat="server" Text="Email me a code" CssClass="btn-primary" OnClick="btnChangePassword_Click" />
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlPasswordVerify" runat="server" Visible="false" DefaultButton="btnVerifyPassword">
                <div class="mgr-table-card emp-form-card" id="change-password-code">
                    <div class="dash-card-head"><h3>Confirm your password change</h3></div>
                    <p class="mgr-dash-subtext">
                        We sent a 6-digit code to <strong><asp:Label ID="lblPasswordEmail" runat="server" /></strong>.
                        Enter it below to save your new password. The code expires in 10 minutes.
                        Until then, your current password keeps working.
                    </p>

                    <asp:Label ID="lblPasswordDemo" runat="server" CssClass="field-hint invalid" Visible="false" />

                    <div class="login-field" style="max-width: 320px;">
                        <label for="<%= txtPasswordCode.ClientID %>">Verification code <button type="button" class="field-help" data-rules="Required.|The 6-digit code from the email we just sent. Check your spam folder if you can't see it.|The code expires after 10 minutes. You have 5 tries."></button></label>
                        <asp:TextBox ID="txtPasswordCode" runat="server" CssClass="login-input" MaxLength="6"
                            autocomplete="one-time-code" inputmode="numeric" data-digits="1" placeholder="000000"
                            Style="text-align:center;font-size:1.4rem;letter-spacing:8px;" />
                    </div>

                    <asp:Label ID="lblPasswordCodeError" runat="server" CssClass="field-error login-server-error" Visible="false" />

                    <div class="emp-form-actions">
                        <asp:Button ID="btnVerifyPassword" runat="server" Text="Verify and change password" CssClass="btn-primary login-submit" CausesValidation="false" OnClick="btnVerifyPassword_Click"
                            OnClientClick="var c = this.closest('.emp-form-card').querySelector('input[autocomplete=one-time-code]'); if (!/^\d{6}$/.test(c.value.trim())) { c.classList.add('invalid'); c.focus(); return false; } if (this.getAttribute('aria-busy')) { return false; } this.setAttribute('aria-busy', 'true'); this.value = 'Verifying...'; this.classList.add('is-busy'); document.getElementById('pwOtpVerifying').hidden = false; return true;" />
                        <asp:Button ID="btnResendPasswordCode" runat="server" Text="Send a new code" CssClass="btn-ghost" CausesValidation="false" OnClick="btnResendPasswordCode_Click" />
                        <asp:Button ID="btnCancelPasswordChange" runat="server" Text="Keep my current password" CssClass="btn-ghost" CausesValidation="false" OnClick="btnCancelPasswordChange_Click" />
                    </div>
                    <div id="pwOtpVerifying" class="otp-verifying" role="status" aria-live="polite" style="justify-content:flex-start;" hidden>
                        <span class="otp-spinner" aria-hidden="true"></span>
                        <span>Verifying your code&hellip;</span>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </section>

    <script src="<%= ResolveUrl("~/Scripts/form-tools.js") %>"></script>
</asp:Content>
