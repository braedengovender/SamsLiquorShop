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

            <!-- CONFIRM A NEW EMAIL ADDRESS (shown after the email is changed and saved) -->
            <asp:Panel ID="pnlEmailVerify" runat="server" Visible="false" DefaultButton="btnVerifyEmail">
                <div class="mgr-table-card emp-form-card">
                    <div class="dash-card-head"><h3>Confirm your new email address</h3></div>
                    <p class="mgr-dash-subtext">
                        We sent a 6-digit code to <strong><asp:Label ID="lblPendingEmail" runat="server" /></strong>.
                        Enter it below to finish changing your email. The code expires in 10 minutes.
                        Until then, your account keeps using your current email address.
                    </p>

                    <asp:Label ID="lblEmailDemo" runat="server" CssClass="field-hint invalid" Visible="false" />

                    <div class="login-field" style="max-width: 320px;">
                        <label for="<%= txtEmailCode.ClientID %>">Verification code</label>
                        <asp:TextBox ID="txtEmailCode" runat="server" CssClass="login-input" MaxLength="6"
                            autocomplete="one-time-code" inputmode="numeric" data-digits="1" placeholder="000000"
                            Style="text-align:center;font-size:1.4rem;letter-spacing:8px;" />
                    </div>

                    <asp:Label ID="lblEmailCodeError" runat="server" CssClass="field-error login-server-error" Visible="false" />

                    <div class="emp-form-actions">
                        <asp:Button ID="btnVerifyEmail" runat="server" Text="Verify and change email" CssClass="btn-primary login-submit" CausesValidation="false" OnClick="btnVerifyEmail_Click"
                            OnClientClick="var c = this.closest('.emp-form-card').querySelector('input[autocomplete=one-time-code]'); if (!/^\d{6}$/.test(c.value.trim())) { c.classList.add('invalid'); c.focus(); return false; } if (this.getAttribute('aria-busy')) { return false; } this.setAttribute('aria-busy', 'true'); this.value = 'Verifying...'; this.classList.add('is-busy'); document.getElementById('otpVerifying').hidden = false; return true;" />
                        <asp:Button ID="btnResendEmailCode" runat="server" Text="Send a new code" CssClass="btn-ghost" CausesValidation="false" OnClick="btnResendEmailCode_Click" />
                        <asp:Button ID="btnCancelEmailChange" runat="server" Text="Keep my current email" CssClass="btn-ghost" CausesValidation="false" OnClick="btnCancelEmailChange_Click" />
                    </div>
                    <div id="otpVerifying" class="otp-verifying" role="status" aria-live="polite" style="justify-content:flex-start;" hidden>
                        <span class="otp-spinner" aria-hidden="true"></span>
                        <span>Verifying your code&hellip;</span>
                    </div>
                </div>
            </asp:Panel>

            <!-- PERSONAL + ADDRESS DETAILS -->
            <asp:Panel ID="pnlDetails" runat="server" DefaultButton="btnSaveDetails">

                <div class="mgr-table-card emp-form-card">
                    <div class="dash-card-head"><h3>Personal Details</h3></div>

                    <div class="emp-form-grid">
                        <div class="login-field">
                            <label for="<%= txtFirstName.ClientID %>">First name</label>
                            <asp:TextBox ID="txtFirstName" runat="server" CssClass="login-input" MaxLength="50" autocomplete="given-name" data-letters="1" />
                            <span class="field-hint">Letters, spaces, hyphens and apostrophes only.</span>
                        </div>
                        <div class="login-field">
                            <label for="<%= txtLastName.ClientID %>">Last name</label>
                            <asp:TextBox ID="txtLastName" runat="server" CssClass="login-input" MaxLength="50" autocomplete="family-name" data-letters="1" />
                            <span class="field-hint">Letters, spaces, hyphens and apostrophes only.</span>
                        </div>
                        <div class="login-field">
                            <label for="<%= txtIdNumber.ClientID %>">SA ID number</label>
                            <asp:TextBox ID="txtIdNumber" runat="server" CssClass="login-input is-locked" MaxLength="13" ReadOnly="true" />
                            <span class="field-hint">Your ID number can't be changed. Contact the store if it is wrong.</span>
                        </div>
                        <div class="login-field">
                            <label for="<%= txtPhone.ClientID %>">Phone number</label>
                            <asp:TextBox ID="txtPhone" runat="server" CssClass="login-input" MaxLength="12" autocomplete="tel" placeholder="e.g. 0821234567" data-phone="1" />
                            <span class="field-hint">For example 0821234567 or +27821234567.</span>
                        </div>
                        <div class="login-field">
                            <label for="<%= txtUsername.ClientID %>">Username</label>
                            <asp:TextBox ID="txtUsername" runat="server" CssClass="login-input" MaxLength="30" autocomplete="username" data-avail="username" />
                            <span class="field-hint">4&ndash;30 letters, numbers or underscores.</span>
                            <span class="field-hint avail-hint" aria-live="polite"></span>
                        </div>
                        <div class="login-field">
                            <label for="<%= txtEmail.ClientID %>">Email address</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="login-input" MaxLength="100" autocomplete="email" data-avail="email" />
                            <span class="field-hint">If you change this, we'll email a code to the new address to confirm it.</span>
                            <span class="field-hint avail-hint" aria-live="polite"></span>
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
                        <asp:Button ID="btnSaveDetails" runat="server" Text="Save Changes" CssClass="btn-primary" OnClick="btnSaveDetails_Click" OnClientClick="return availabilityOk();" />
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

            <!-- DEACTIVATE PROFILE -->
            <asp:Panel ID="pnlDeactivate" runat="server" DefaultButton="btnDeactivate">
                <div class="mgr-table-card emp-form-card acct-danger-card">
                    <div class="dash-card-head"><h3>Deactivate My Profile</h3></div>

                    <p class="mgr-dash-subtext">If you deactivate your profile:</p>
                    <ul class="field-hint">
                        <li>You'll be logged out straight away and won't be able to log in again.</li>
                        <li>Orders you've already placed will still be processed and delivered.</li>
                        <li>Your order history is kept for our records. Contact us if you want your profile switched back on.</li>
                    </ul>

                    <div class="emp-form-grid">
                        <div class="login-field">
                            <label for="<%= txtDeactivatePassword.ClientID %>">Enter your password to confirm</label>
                            <asp:TextBox ID="txtDeactivatePassword" runat="server" CssClass="login-input" TextMode="Password" MaxLength="50" autocomplete="current-password" />
                        </div>
                    </div>

                    <div class="emp-form-actions">
                        <asp:LinkButton ID="btnDeactivate" runat="server" CssClass="btn-primary emp-btn-danger" OnClick="btnDeactivate_Click" CausesValidation="false"
                            OnClientClick="return dashConfirm(this, 'Deactivate your profile?', 'You will be logged out and will not be able to log in to this profile again. This cannot be undone online.', 'Deactivate', true);">Deactivate my profile</asp:LinkButton>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </section>

    <script src="<%= ResolveUrl("~/Scripts/form-tools.js") %>"></script>

    <!-- CONFIRMATION DIALOG (opened by dashConfirm() in Site.Master) -->
    <div class="emp-modal" id="dashModal" aria-hidden="true">
        <div class="emp-modal-card" role="dialog" aria-modal="true" aria-labelledby="dashModalTitle">
            <h3 id="dashModalTitle">Confirm</h3>
            <p id="dashModalText"></p>
            <div class="emp-modal-actions">
                <button type="button" class="btn-ghost" onclick="dashModalClose()">Cancel</button>
                <button type="button" class="btn-primary" id="dashModalOk">Confirm</button>
            </div>
        </div>
    </div>
</asp:Content>
