<%@ Page Title="Reset password" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="SamsLiquorShop.ForgotPassword" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell login-wrap">
        <div class="glass login-card wide-card">

            <asp:Panel ID="pnlEmail" runat="server">
                <p class="login-eyebrow">Sam's Liquor Shop</p>
                <h1 class="login-heading">Reset your password</h1>
                <p class="login-subtext">Enter your email address or username and we'll email you a 6-digit code.</p>

                <div class="login-field">
                    <label for="<%= txtEmail.ClientID %>">Email or username <button type="button" class="field-help" data-rules="Required.|The email address or username you log in with.|If we find your account, we email a 6-digit code to the address on it."></button></label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="login-input" MaxLength="100" autocomplete="username" autocapitalize="none" spellcheck="false" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" CssClass="field-error" Display="Dynamic" ErrorMessage="Enter your email address or username." ValidationGroup="EmailForm" />
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
                <p class="login-subtext">If we found your account, we've sent a 6-digit code to the email address on it. It expires in 10 minutes.</p>

                <span id="spanDemo" runat="server" class="field-error" visible="false"></span>

                <div class="login-field">
                    <label for="<%= txtCode.ClientID %>">Reset code <button type="button" class="field-help" data-rules="Required.|The 6-digit code from the email we just sent. Check your spam folder if you can't see it.|The code expires after 10 minutes. You have 5 tries."></button></label>
                    <asp:TextBox ID="txtCode" runat="server" CssClass="login-input" MaxLength="6" autocomplete="one-time-code" />
                </div>
                <asp:Label ID="lblCodeError" runat="server" CssClass="field-error login-server-error" Visible="false"></asp:Label>
                <asp:Button ID="btnVerify" runat="server" Text="Verify code" CssClass="btn-primary login-submit" OnClick="btnVerify_Click"
                    OnClientClick="if (this.getAttribute('aria-busy')) { return false; } this.setAttribute('aria-busy', 'true'); this.value = 'Verifying...'; this.classList.add('is-busy'); document.getElementById('otpVerifying').hidden = false; return true;" />
                <div id="otpVerifying" class="otp-verifying" role="status" aria-live="polite" hidden>
                    <span class="otp-spinner" aria-hidden="true"></span>
                    <span>Verifying your code&hellip;</span>
                </div>
                <p class="login-register-text"><a href="ForgotPassword.aspx">Start again</a></p>
            </asp:Panel>

            <asp:Panel ID="pnlNew" runat="server" Visible="false">
                <p class="login-eyebrow">Sam's Liquor Shop</p>
                <h1 class="login-heading">Choose a new password</h1>
                <p class="login-subtext">Code verified. Set your new password below.</p>

                <div class="login-field login-password-wrap">
                    <div class="password-label-row">
                        <label for="<%= txtNewPassword.ClientID %>">New password <button type="button" class="field-help" data-rules="Required.|8 to 50 characters.|At least 1 lowercase letter, 1 uppercase letter, 1 number and 1 special character (e.g. ! @ # ?).|Tip: press &quot;Suggest password&quot; to generate a strong one."></button></label>
                        <button type="button" class="suggest-password-btn" onclick="suggestPassword('<%= txtNewPassword.ClientID %>', '<%= txtConfirmPassword.ClientID %>')">Suggest password</button>
                    </div>
                    <div class="password-input-wrap">
                        <asp:TextBox ID="txtNewPassword" runat="server" CssClass="login-input" TextMode="Password" MaxLength="50" autocomplete="new-password" oninput="checkPasswordStrength(this.value)" />
                        <button type="button" class="login-eye-btn" onclick="toggleFieldVisibility('<%= txtNewPassword.ClientID %>', this)" aria-label="Show password">
                            <svg class="eye-icon" viewBox="0 0 24 24" aria-hidden="true">
                                <path d="M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6z"></path>
                                <circle cx="12" cy="12" r="2.5"></circle>
                            </svg>
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

                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNewPassword" ErrorMessage="Password is required." CssClass="field-validation" Display="Dynamic" ValidationGroup="ResetForm" />
                    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtNewPassword" ErrorMessage="Password must meet all the requirements above." CssClass="field-validation" Display="Dynamic" ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,50}$" ValidationGroup="ResetForm" />
                </div>

                <div class="login-field login-password-wrap">
                    <label for="<%= txtConfirmPassword.ClientID %>">Confirm new password <button type="button" class="field-help" data-rules="Required.|Type the same password again. It must match exactly (capitals count)."></button></label>
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

    <script src="<%= ResolveUrl("~/Scripts/form-tools.js") %>"></script>
</asp:Content>