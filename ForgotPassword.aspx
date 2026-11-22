<%@ Page Title="Reset password" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="SamsLiquorShop.ForgotPassword" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell login-wrap">
        <div class="glass login-card wide-card">

            <asp:Panel ID="pnlEmail" runat="server">
                <p class="login-eyebrow">Sam's Liquor Shop</p>
                <h1 class="login-heading">Reset your password</h1>
                <p class="login-subtext">Enter your account email and we'll send you a 6-digit code.</p>

                <div class="login-field">
                    <label for="<%= txtEmail.ClientID %>">Email address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="login-input" TextMode="Email" MaxLength="100" autocomplete="email" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" CssClass="field-error" Display="Dynamic" ErrorMessage="Email is required." ValidationGroup="EmailForm" />
                </div>
                <asp:Label ID="lblEmailError" runat="server" CssClass="field-error login-server-error" Visible="false"></asp:Label>
                <%-- reCAPTCHA "I'm not a robot" box (drawn by the script in Site.Master; checked in Helpers/RecaptchaHelper.cs) --%>
                <div class="g-recaptcha login-recaptcha" data-sitekey="<%= SamsLiquorShop.Helpers.RecaptchaHelper.SiteKey %>"></div>
                <asp:Button ID="btnSend" runat="server" Text="Send code" CssClass="btn-primary login-submit" ValidationGroup="EmailForm" OnClick="btnSend_Click" />
                <p class="login-register-text"><a href="Login.aspx">Back to login</a></p>
            </asp:Panel>

            <asp:Panel ID="pnlCode" runat="server" Visible="false">
                <p class="login-eyebrow">Sam's Liquor Shop</p>
                <h1 class="login-heading">Check your email</h1>
                <p class="login-subtext">If that email is registered, we've sent a 6-digit code. It expires in 10 minutes.</p>

                <span id="spanDemo" runat="server" class="field-error" visible="false"></span>

                <div class="login-field">
                    <label for="<%= txtCode.ClientID %>">Reset code</label>
                    <asp:TextBox ID="txtCode" runat="server" CssClass="login-input" MaxLength="6" autocomplete="one-time-code" />
                </div>
                <asp:Label ID="lblCodeError" runat="server" CssClass="field-error login-server-error" Visible="false"></asp:Label>
                <asp:Button ID="btnVerify" runat="server" Text="Verify code" CssClass="btn-primary login-submit" OnClick="btnVerify_Click" />
                <p class="login-register-text"><a href="ForgotPassword.aspx">Start again</a></p>
            </asp:Panel>

            <asp:Panel ID="pnlNew" runat="server" Visible="false">
                <p class="login-eyebrow">Sam's Liquor Shop</p>
                <h1 class="login-heading">Choose a new password</h1>
                <p class="login-subtext">Code verified. Set your new password below.</p>

                <div class="login-field login-password-wrap">
                    <label for="<%= txtNewPassword.ClientID %>">New password</label>
                    <div class="password-input-wrap">
                        <asp:TextBox ID="txtNewPassword" runat="server" CssClass="login-input" TextMode="Password" MaxLength="50" autocomplete="new-password" oninput="checkPasswordStrength(this.value)" />
                        <button type="button" class="login-eye-btn" onclick="toggleFieldVisibility('<%= txtNewPassword.ClientID %>', this)" aria-label="Show password">
                            <svg class="eye-icon" viewBox="0 0 24 24" aria-hidden="true">
                                <path d="M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6z"></path>
                                <circle cx="12" cy="12" r="2.5"></circle>
                            </svg>
                        </button>
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

                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNewPassword" ErrorMessage="Password is required." CssClass="field-validation" Display="Dynamic" ValidationGroup="ResetForm" />
                    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtNewPassword" ErrorMessage="Password must meet all the requirements above." CssClass="field-validation" Display="Dynamic" ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,50}$" ValidationGroup="ResetForm" />
                </div>

                <div class="login-field login-password-wrap">
                    <label for="<%= txtConfirmPassword.ClientID %>">Confirm new password</label>
                    <div class="password-input-wrap">
                        <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="login-input" TextMode="Password" MaxLength="50" autocomplete="new-password" />
                        <button type="button" class="login-eye-btn" onclick="toggleFieldVisibility('<%= txtConfirmPassword.ClientID %>', this)" aria-label="Show password">
                            <svg class="eye-icon" viewBox="0 0 24 24" aria-hidden="true">
                                <path d="M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6z"></path>
                                <circle cx="12" cy="12" r="2.5"></circle>
                            </svg>
                        </button>
                    </div>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtConfirmPassword" ErrorMessage="Please confirm your password." CssClass="field-validation" Display="Dynamic" ValidationGroup="ResetForm" />
                    <asp:CompareValidator runat="server" ControlToValidate="txtConfirmPassword" ControlToCompare="txtNewPassword" ErrorMessage="Passwords do not match." CssClass="field-validation" Display="Dynamic" ValidationGroup="ResetForm" />
                </div>

                <asp:Label ID="lblResetError" runat="server" CssClass="field-error login-server-error" Visible="false"></asp:Label>
                <asp:Button ID="btnReset" runat="server" Text="Reset password" CssClass="btn-primary login-submit" ValidationGroup="ResetForm" OnClick="btnReset_Click" />
            </asp:Panel>

            <asp:Panel ID="pnlDone" runat="server" Visible="false">
                <p class="login-eyebrow">Sam's Liquor Shop</p>
                <h1 class="login-heading">Password updated</h1>
                <p class="login-subtext">You can now log in with your new password.</p>
                <a href="Login.aspx" class="btn-primary login-submit">Go to login</a>
            </asp:Panel>

        </div>
    </section>

    <script>
        function toggleFieldVisibility(fieldId, button) {
            var field = document.getElementById(fieldId);
            if (!field) return;
            var icon = button.querySelector(".eye-icon");
            if (field.type === "password") {
                field.type = "text";
                button.setAttribute("aria-label", "Hide password");
                icon.innerHTML = '<path d="M3 3l18 18"></path><path d="M2.5 12s3.5-6 9.5-6c1.2 0 2.3.2 3.3.6"></path><path d="M20.5 12s-3.5 6-9.5 6c-1.2 0-2.3-.2-3.3-.6"></path>';
            } else {
                field.type = "password";
                button.setAttribute("aria-label", "Show password");
                icon.innerHTML = '<path d="M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6z"></path><circle cx="12" cy="12" r="2.5"></circle>';
            }
        }

        function updatePasswordRequirement(id, valid) {
            var el = document.getElementById(id);
            if (!el) return;
            if (valid) el.classList.add('valid'); else el.classList.remove('valid');
        }

        function checkPasswordStrength(password) {
            var checks = {
                reqLength: password.length >= 8,
                reqLower: /[a-z]/.test(password),
                reqUpper: /[A-Z]/.test(password),
                reqNumber: /[0-9]/.test(password),
                reqSpecial: /[^A-Za-z0-9]/.test(password)
            };
            var score = 0;
            for (var id in checks) { updatePasswordRequirement(id, checks[id]); if (checks[id]) score++; }

            var fill = document.getElementById('passwordStrengthFill');
            var text = document.getElementById('passwordStrengthText');
            if (!fill || !text) return;

            var cls = ['strength-weak', 'strength-medium', 'strength-strong'];
            cls.forEach(function (c) { fill.classList.remove(c); text.classList.remove(c); });

            if (password.length === 0) { fill.style.width = '0%'; text.textContent = 'Password strength'; return; }

            var level = score <= 2 ? 0 : (score <= 4 ? 1 : 2);
            fill.style.width = ['25%', '60%', '100%'][level];
            text.textContent = ['Weak password', 'Medium password', 'Strong password'][level];
            fill.classList.add(cls[level]);
            text.classList.add(cls[level]);
        }
    </script>
</asp:Content>