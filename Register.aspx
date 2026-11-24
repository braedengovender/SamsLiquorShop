<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="SamsLiquorShop.Register" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="login-wrap">
        <div class="glass login-card wide-card">
            <div class="register-inner">

                <asp:Panel ID="pnlForm" runat="server">

                    <p class="login-eyebrow">Join Sam's Liquor Shop</p>
                    <h1 class="login-heading">Create your account</h1>
                    <p class="login-subtext">You must be 18 or older to register. We deliver within the Durban area only.</p>

                    <div class="register-section">
                        <h3 class="register-section-title">Personal Details</h3>

                        <div class="register-grid-2">
                            <div class="login-field">
                                <label for="txtFirstName">First name <button type="button" class="field-help" data-rules="Required.|Use your first name exactly as it appears on your ID.|Letters, spaces, hyphens and apostrophes only.|Up to 50 characters."></button></label>
                                <asp:TextBox ID="txtFirstName" runat="server" CssClass="login-input" MaxLength="50" autocomplete="given-name" data-letters="1" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFirstName" CssClass="field-error" Display="Dynamic" ErrorMessage="First name is required." ValidationGroup="RegisterForm" />
                                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtFirstName" CssClass="field-error" Display="Dynamic" ErrorMessage="First name can only contain letters, spaces, hyphens and apostrophes." ValidationExpression="^\s*[A-Za-z]+([ '-][A-Za-z]+)*\s*$" ValidationGroup="RegisterForm" />
                            </div>
                            <div class="login-field">
                                <label for="txtLastName">Last name <button type="button" class="field-help" data-rules="Required.|Use your surname exactly as it appears on your ID.|Letters, spaces, hyphens and apostrophes only.|Up to 50 characters."></button></label>
                                <asp:TextBox ID="txtLastName" runat="server" CssClass="login-input" MaxLength="50" autocomplete="family-name" data-letters="1" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtLastName" CssClass="field-error" Display="Dynamic" ErrorMessage="Last name is required." ValidationGroup="RegisterForm" />
                                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtLastName" CssClass="field-error" Display="Dynamic" ErrorMessage="Last name can only contain letters, spaces, hyphens and apostrophes." ValidationExpression="^\s*[A-Za-z]+([ '-][A-Za-z]+)*\s*$" ValidationGroup="RegisterForm" />
                            </div>
                        </div>

                        <div class="register-grid-2">
                            <div class="login-field">
                                <label for="txtIdNumber">SA ID number <button type="button" class="field-help" data-rules="Required.|Exactly 13 digits, numbers only (no spaces).|Must be a valid South African ID number.|Your date of birth is read from the first 6 digits. You must be 18 or older.|Only one account per ID number."></button></label>
                                <asp:TextBox ID="txtIdNumber" runat="server" ClientIDMode="Static" CssClass="login-input" MaxLength="13" autocomplete="off" placeholder="13 digits" oninput="onIdNumberInput()" />
                                <span class="field-hint" id="idNumberHint"></span>
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtIdNumber" CssClass="field-error" Display="Dynamic" ErrorMessage="ID number is required." ValidationGroup="RegisterForm" />
                                <asp:CustomValidator ID="cvIdNumber" runat="server" ControlToValidate="txtIdNumber" CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid South African ID number." OnServerValidate="cvIdNumber_ServerValidate" ValidationGroup="RegisterForm" />
                            </div>
                            <div class="login-field">
                                <label for="txtPhone">Phone number <button type="button" class="field-help" data-rules="Required.|A South African number: 10 digits starting with 0 (e.g. 0821234567), or +27 followed by 9 digits.|No spaces or dashes."></button></label>
                                <asp:TextBox ID="txtPhone" runat="server" CssClass="login-input" MaxLength="15" autocomplete="tel" placeholder="e.g. 0821234567" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPhone" CssClass="field-error" Display="Dynamic" ErrorMessage="Phone number is required." ValidationGroup="RegisterForm" />
                                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPhone" CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid SA phone number." ValidationExpression="^(\+27|0)[1-9][0-9]{8}$" ValidationGroup="RegisterForm" />
                            </div>
                        </div>

                        <div class="login-field">
                            <label for="fuIdPhoto">Photo of your ID <span class="register-optional">(any image)</span> <button type="button" class="field-help" data-rules="Required.|Fill in your first name, last name and ID number first.|Any image file works (JPG, PNG, HEIC, WebP and so on).|Wait until it shows &quot;ID verified&quot; before you continue."></button></label>
                            <asp:FileUpload ID="fuIdPhoto" runat="server" ClientIDMode="Static" CssClass="login-input" accept="image/*" onchange="onIdPhotoChange(this)" />
                            <asp:HiddenField ID="hdnIdPhotoOk" runat="server" ClientIDMode="Static" Value="0" />

                            <div id="idScan" class="id-scan" style="display:none;">
                                <img id="idScanThumb" class="id-scan-thumb" alt="ID preview" />
                                <div class="id-scan-info">
                                    <div id="idScanStatus" class="id-scan-status"></div>
                                    <div id="idScanBarWrap" class="id-scan-bar"><div id="idScanBarFill"></div></div>
                                    <div id="idScanResult" class="id-scan-result"></div>
                                    <div class="id-scan-demo">Demo mode: no real image analysis is performed.</div>
                                </div>
                            </div>

                            <asp:CustomValidator ID="cvIdPhoto" runat="server" CssClass="field-error" Display="Dynamic"
                                ErrorMessage="Please upload a clear photo of your ID and wait for it to be verified."
                                ClientValidationFunction="validateIdPhoto" OnServerValidate="cvIdPhoto_ServerValidate"
                                ValidationGroup="RegisterForm" />
                        </div>

                        <style>
                            .id-scan { display:flex; gap:14px; align-items:flex-start; margin-top:10px; padding:12px; border:1px solid var(--glass-border); border-radius:14px; background:#fff; }
                            .id-scan-thumb { width:110px; height:72px; object-fit:cover; border-radius:8px; border:1px solid var(--glass-border); }
                            .id-scan-info { flex:1; min-width:0; }
                            .id-scan-status { font-weight:600; font-size:0.92rem; margin-bottom:6px; }
                            .id-scan-status.ok { color:#1a7f37; }
                            .id-scan-status.fail { color:#c0392b; }
                            .id-scan-bar { height:6px; background:#e8ecf2; border-radius:3px; overflow:hidden; }
                            #idScanBarFill { height:100%; width:0; background:#4285F4; transition:width .6s ease; }
                            .id-scan-result { font-size:0.85rem; margin-top:8px; }
                            .id-scan-result div { display:flex; gap:8px; padding:2px 0; }
                            .id-scan-result span:first-child { color:#6b7280; min-width:110px; }
                            .id-scan-demo { font-size:0.72rem; color:#9ca3af; margin-top:8px; }
                        </style>

                        <div class="login-field">
                            <label for="txtEmail">Email address <button type="button" class="field-help" data-rules="Required.|A valid address, e.g. name@example.com. Up to 100 characters.|We send a 6-digit code here to verify your account, so use one you can open now.|Only one account per email address."></button></label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="login-input" TextMode="Email" MaxLength="256" autocomplete="email" />
                            <span id="emailAvailHint" class="field-hint avail-hint" aria-live="polite"></span>
                            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" CssClass="field-error" Display="Dynamic" ErrorMessage="Email is required." ValidationGroup="RegisterForm" />
                            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail" CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid email address." ValidationExpression="^[^\s@]+@[^\s@]+\.[^\s@]+$" ValidationGroup="RegisterForm" />
                        </div>
                    </div>

                    <div class="register-section">
                        <h3 class="register-section-title">Residential Address</h3>

                        <div class="login-field">
                            <label for="txtAddressSearch">Search your address <button type="button" class="field-help" data-rules="Optional shortcut.|Type at least 5 characters, then pick your address from the list.|The fields below fill in for you. Check them and correct anything that's wrong."></button></label>
                            <input type="text" id="txtAddressSearch" class="login-input" placeholder="Start typing your address..." autocomplete="off" />
                        </div>

                        <div class="register-grid-2">
                            <div class="login-field">
                                <label for="txtUnitNumber">Unit number <span class="register-optional">(optional)</span> <button type="button" class="field-help" data-rules="Optional. Only needed for flats, townhouses or complexes.|Up to 20 characters."></button></label>
                                <asp:TextBox ID="txtUnitNumber" runat="server" CssClass="login-input" MaxLength="20" autocomplete="off" />
                            </div>
                            <div class="login-field">
                                <label for="txtUnitName">Unit / complex name <span class="register-optional">(optional)</span> <button type="button" class="field-help" data-rules="Optional. The name of your building or complex.|Up to 50 characters."></button></label>
                                <asp:TextBox ID="txtUnitName" runat="server" CssClass="login-input" MaxLength="100" autocomplete="off" />
                            </div>
                        </div>

                        <div class="register-grid-2">
                            <div class="login-field">
                                <label for="txtStreetNumber">Street number <button type="button" class="field-help" data-rules="Required.|Your house or building number, e.g. 12 or 12A.|Up to 10 characters."></button></label>
                                <asp:TextBox ID="txtStreetNumber" runat="server" ClientIDMode="Static" CssClass="login-input" MaxLength="20" autocomplete="off" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtStreetNumber" CssClass="field-error" Display="Dynamic" ErrorMessage="Street number is required." ValidationGroup="RegisterForm" />
                            </div>
                            <div class="login-field">
                                <label for="txtStreetName">Street name <button type="button" class="field-help" data-rules="Required.|The full street name, e.g. Florida Road.|Up to 100 characters."></button></label>
                                <asp:TextBox ID="txtStreetName" runat="server" ClientIDMode="Static" CssClass="login-input" MaxLength="100" autocomplete="address-line1" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtStreetName" CssClass="field-error" Display="Dynamic" ErrorMessage="Street name is required." ValidationGroup="RegisterForm" />
                            </div>
                        </div>

                        <div class="register-grid-2">
                            <div class="login-field">
                                <label for="txtSuburb">Suburb <button type="button" class="field-help" data-rules="Required.|e.g. Morningside or Umhlanga.|Up to 50 characters."></button></label>
                                <asp:TextBox ID="txtSuburb" runat="server" ClientIDMode="Static" CssClass="login-input" MaxLength="100" autocomplete="address-level3" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtSuburb" CssClass="field-error" Display="Dynamic" ErrorMessage="Suburb is required." ValidationGroup="RegisterForm" />
                            </div>
                            <div class="login-field">
                                <label for="txtCity">City <button type="button" class="field-help" data-rules="Required.|Up to 50 characters.|We deliver within the Durban area only."></button></label>
                                <asp:TextBox ID="txtCity" runat="server" ClientIDMode="Static" CssClass="login-input" MaxLength="100" autocomplete="address-level2" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtCity" CssClass="field-error" Display="Dynamic" ErrorMessage="City is required." ValidationGroup="RegisterForm" />
                            </div>
                        </div>

                        <div class="register-grid-2">
                            <div class="login-field">
                                <label for="txtPostalCode">Postal code <button type="button" class="field-help" data-rules="Required.|Exactly 4 digits, e.g. 4001."></button></label>
                                <asp:TextBox ID="txtPostalCode" runat="server" ClientIDMode="Static" CssClass="login-input" MaxLength="4" autocomplete="postal-code" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPostalCode" CssClass="field-error" Display="Dynamic" ErrorMessage="Postal code is required." ValidationGroup="RegisterForm" />
                                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPostalCode" CssClass="field-error" Display="Dynamic" ErrorMessage="Postal code must be 4 digits." ValidationExpression="^\d{4}$" ValidationGroup="RegisterForm" />
                            </div>
                            <div class="login-field">
                                <label for="ddlProvince">Province <button type="button" class="field-help" data-rules="Required.|Choose your province from the list."></button></label>
                                <asp:DropDownList ID="ddlProvince" runat="server" ClientIDMode="Static" CssClass="login-input">
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
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlProvince" InitialValue="" CssClass="field-error" Display="Dynamic" ErrorMessage="Please select a province." ValidationGroup="RegisterForm" />
                            </div>
                        </div>

                        <p class="register-static-field">Country: South Africa</p>
                    </div>

                    <div class="register-section">
                        <h3 class="register-section-title">Account Details</h3>

                        <div class="login-field">
                            <label for="txtUsername">Username <span class="register-optional">(optional - we'll use your email if left blank)</span> <button type="button" class="field-help" data-rules="Optional. If left blank, your email address becomes your username.|4 to 30 characters: letters, numbers and underscores (_) only. No spaces.|Must not already be taken.|If your email is longer than 50 characters, you must choose a username."></button></label>
                            <asp:TextBox ID="txtUsername" runat="server" CssClass="login-input" MaxLength="30" autocomplete="username" />
                            <span id="usernameAvailHint" class="field-hint avail-hint" aria-live="polite"></span>
                            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtUsername" CssClass="field-error" Display="Dynamic" ErrorMessage="Username must be 4-30 letters, numbers or underscores." ValidationExpression="^[a-zA-Z0-9_]{4,30}$" ValidationGroup="RegisterForm" />
                        </div>

                        <div class="register-grid-2">

                            <div class="login-field login-password-wrap">
                                <div class="password-label-row">
                                    <label for="<%= txtPassword.ClientID %>">Password <button type="button" class="field-help" data-rules="Required.|8 to 50 characters.|At least 1 lowercase letter, 1 uppercase letter, 1 number and 1 special character (e.g. ! @ # ?).|Tip: press &quot;Suggest password&quot; to generate a strong one."></button></label>
                                    <button type="button" class="suggest-password-btn" onclick="suggestPassword()">Suggest password</button>
                                </div>

                                <div class="password-input-wrap">
                                    <asp:TextBox
                                        ID="txtPassword"
                                        runat="server"
                                        CssClass="login-input"
                                        TextMode="Password"
                                        MaxLength="100"
                                        autocomplete="new-password"
                                        oninput="checkPasswordStrength(this.value)" />

                                    <button
                                        type="button"
                                        class="login-eye-btn"
                                        onclick="toggleFieldVisibility('<%= txtPassword.ClientID %>', this)"
                                        aria-label="Show password">

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

                                <!-- Password strength -->
                                <div class="password-strength">
                                    <div class="password-strength-bar">
                                        <div id="passwordStrengthFill"></div>
                                    </div>

                                    <div id="passwordStrengthText" class="password-strength-text">
                                        Password strength
                                    </div>

                                    <div class="password-requirements">

                                        <div id="reqLength" class="password-requirement">
                                            <span class="requirement-icon">○</span>
                                            <span>At least 8 characters</span>
                                        </div>

                                        <div id="reqLower" class="password-requirement">
                                            <span class="requirement-icon">○</span>
                                            <span>1 lowercase letter</span>
                                        </div>

                                        <div id="reqUpper" class="password-requirement">
                                            <span class="requirement-icon">○</span>
                                            <span>1 uppercase letter</span>
                                        </div>

                                        <div id="reqNumber" class="password-requirement">
                                            <span class="requirement-icon">○</span>
                                            <span>1 number</span>
                                        </div>

                                        <div id="reqSpecial" class="password-requirement">
                                            <span class="requirement-icon">○</span>
                                            <span>1 special character</span>
                                        </div>

                                    </div>
                                </div>

                                <asp:RequiredFieldValidator
                                    ID="rfvPassword"
                                    runat="server"
                                    ControlToValidate="txtPassword"
                                    ErrorMessage="Password is required."
                                    CssClass="field-validation"
                                    Display="Dynamic"
                                    ValidationGroup="RegisterForm" />

                                <asp:RegularExpressionValidator
                                    ID="revPassword"
                                    runat="server"
                                    ControlToValidate="txtPassword"
                                    ErrorMessage="Password must meet all the requirements above."
                                    CssClass="field-validation"
                                    Display="Dynamic"
                                    ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,100}$"
                                    ValidationGroup="RegisterForm" />
                            </div>


                            <div class="login-field login-password-wrap">
                                <label for="<%= txtConfirmPassword.ClientID %>">Confirm password <button type="button" class="field-help" data-rules="Required.|Type the same password again. It must match exactly (capitals count)."></button></label>

                                <div class="password-input-wrap">
                                    <asp:TextBox
                                        ID="txtConfirmPassword"
                                        runat="server"
                                        CssClass="login-input"
                                        TextMode="Password"
                                        MaxLength="100"
                                        autocomplete="new-password" />

                                    <button
                                        type="button"
                                        class="login-eye-btn"
                                        onclick="toggleFieldVisibility('<%= txtConfirmPassword.ClientID %>', this)"
                                        aria-label="Show password">

                                        <svg class="eye-icon" viewBox="0 0 24 24" aria-hidden="true">
                                            <path d="M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6z"></path>
                                            <circle cx="12" cy="12" r="2.5"></circle>
                                        </svg>

                                    </button>
                                </div>

                                <asp:RequiredFieldValidator
                                    ID="rfvConfirmPassword"
                                    runat="server"
                                    ControlToValidate="txtConfirmPassword"
                                    ErrorMessage="Please confirm your password."
                                    CssClass="field-validation"
                                    Display="Dynamic"
                                    ValidationGroup="RegisterForm" />

                                <asp:CompareValidator
                                    ID="cvConfirmPassword"
                                    runat="server"
                                    ControlToValidate="txtConfirmPassword"
                                    ControlToCompare="txtPassword"
                                    ErrorMessage="Passwords do not match."
                                    CssClass="field-validation"
                                    Display="Dynamic"
                                    ValidationGroup="RegisterForm" />
                            </div>

                        </div>
                    </div>

                    <div class="register-section">
                        <div class="login-field register-terms-field">
                            <div class="register-terms-box">
                                <asp:CheckBox ID="chkTerms" runat="server" ClientIDMode="Static" />
                                <label for="chkTerms" class="register-terms-label">
                                    I confirm I am 18 or older and agree to the
                                    <a href="Legal.aspx#terms" target="_blank" rel="noopener">Terms &amp; Conditions</a>
                                    and
                                    <a href="Legal.aspx#privacy" target="_blank" rel="noopener">Privacy Policy</a>.
                                </label>
                            </div>
                            <asp:CustomValidator ID="cvTerms" runat="server" CssClass="field-error" Display="Dynamic" ErrorMessage="You must accept the Terms &amp; Conditions to continue." OnServerValidate="cvTerms_ServerValidate" ValidationGroup="RegisterForm" />
                        </div>

                        <%-- reCAPTCHA "I'm not a robot" box (drawn by the script in Site.Master; checked in Helpers/RecaptchaHelper.cs) --%>
                        <div class="g-recaptcha login-recaptcha" data-sitekey="<%= SamsLiquorShop.Helpers.RecaptchaHelper.SiteKey %>"></div>

                        <asp:Button ID="btnRegister" runat="server" Text="Create account" CssClass="btn-primary login-submit" ValidationGroup="RegisterForm" OnClick="btnRegister_Click" />
                        <asp:Label ID="lblError" runat="server" CssClass="field-error"></asp:Label>
                        <div class="login-divider"><span>or</span></div>
                        <button type="button" class="login-google-btn" onclick="googleDemoSignIn(); return false;">
                            <svg width="18" height="18" viewBox="0 0 18 18" xmlns="http://www.w3.org/2000/svg">
                                <path fill="#4285F4" d="M17.64 9.2c0-.64-.06-1.25-.16-1.84H9v3.48h4.84a4.14 4.14 0 0 1-1.8 2.72v2.26h2.92c1.7-1.57 2.68-3.88 2.68-6.62z" />
                                <path fill="#34A853" d="M9 18c2.43 0 4.47-.8 5.96-2.18l-2.92-2.26c-.81.54-1.84.86-3.04.86-2.34 0-4.32-1.58-5.03-3.7H.96v2.33A9 9 0 0 0 9 18z" />
                                <path fill="#FBBC05" d="M3.97 10.72A5.4 5.4 0 0 1 3.68 9c0-.6.1-1.18.29-1.72V4.95H.96A9 9 0 0 0 0 9c0 1.45.35 2.83.96 4.05l3.01-2.33z" />
                                <path fill="#EA4335" d="M9 3.58c1.32 0 2.51.45 3.44 1.35l2.58-2.58C13.46.89 11.43 0 9 0A9 9 0 0 0 .96 4.95l3.01 2.33C4.68 5.16 6.66 3.58 9 3.58z" />
                            </svg>
                            Sign up with Google
                        </button>
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlVerify" runat="server" Visible="false" DefaultButton="btnVerify">
                    <div style="max-width:440px;">
                        <p class="login-eyebrow">One last step</p>
                        <h1 class="login-heading">Verify your email</h1>
                        <p class="login-subtext">We sent a 6-digit code to <strong><asp:Label ID="lblVerifyEmail" runat="server" /></strong>. Enter it below to create your account. The code expires in 10 minutes.</p>

                        <asp:Label ID="lblDemo" runat="server" CssClass="field-hint invalid" Visible="false" />

                        <div class="login-field">
                            <label for="<%= txtOtp.ClientID %>">Verification code</label>
                            <asp:TextBox ID="txtOtp" runat="server" CssClass="login-input" MaxLength="6"
                                autocomplete="one-time-code" inputmode="numeric" data-digits="1" placeholder="000000"
                                Style="text-align:center;font-size:1.4rem;letter-spacing:8px;" />
                        </div>

                        <asp:Label ID="lblVerifyError" runat="server" CssClass="field-error login-server-error" />
                        <asp:Label ID="lblVerifyInfo" runat="server" CssClass="field-hint valid login-server-error" />

                        <asp:Button ID="btnVerify" runat="server" Text="Verify and create account" CssClass="btn-primary login-submit" CausesValidation="false" OnClick="btnVerify_Click"
                            OnClientClick="if (this.getAttribute('aria-busy')) { return false; } this.setAttribute('aria-busy', 'true'); this.value = 'Verifying...'; this.classList.add('is-busy'); document.getElementById('otpVerifying').hidden = false; return true;" />
                        <div id="otpVerifying" class="otp-verifying" role="status" aria-live="polite" hidden>
                            <span class="otp-spinner" aria-hidden="true"></span>
                            <span>Verifying your code&hellip;</span>
                        </div>

                        <p class="login-register-text">
                            Didn't get it? Check your spam folder or
                            <asp:LinkButton ID="btnResend" runat="server" CausesValidation="false" OnClick="btnResend_Click">send a new code</asp:LinkButton>.
                            <br />
                            <asp:LinkButton ID="btnChangeEmail" runat="server" CausesValidation="false" OnClick="btnChangeEmail_Click">Use a different email address</asp:LinkButton>
                        </p>
                    </div>
                </asp:Panel>

                <script>
                    function onIdNumberInput() {
                        var input = document.getElementById('txtIdNumber');
                        var hint = document.getElementById('idNumberHint');
                        var id = input.value.trim();

                        hint.className = 'field-hint';
                        hint.textContent = '';

                        if (id.length === 0) return;

                        if (!/^\d+$/.test(id)) {
                            hint.textContent = 'ID number must contain digits only.';
                            hint.className = 'field-hint invalid';
                            return;
                        }
                        if (id.length < 13) {
                            hint.textContent = (13 - id.length) + ' more digit(s) needed.';
                            return;
                        }

                        var sum = 0, alt = false;
                        for (var i = id.length - 1; i >= 0; i--) {
                            var n = parseInt(id.charAt(i), 10);
                            if (alt) { n *= 2; if (n > 9) n -= 9; }
                            sum += n;
                            alt = !alt;
                        }
                        if (sum % 10 !== 0) {
                            hint.textContent = "That doesn't look like a valid SA ID number.";
                            hint.className = 'field-hint invalid';
                            return;
                        }

                        var yy = parseInt(id.substring(0, 2), 10);
                        var mm = parseInt(id.substring(2, 4), 10);
                        var dd = parseInt(id.substring(4, 6), 10);
                        var currentYY = new Date().getFullYear() % 100;
                        var fullYear = (yy > currentYY ? 1900 : 2000) + yy;

                        var dob = new Date(fullYear, mm - 1, dd);
                        var validDate = dob.getFullYear() === fullYear && (dob.getMonth() + 1) === mm && dob.getDate() === dd;
                        if (!validDate) {
                            hint.textContent = "That doesn't look like a valid SA ID number.";
                            hint.className = 'field-hint invalid';
                            return;
                        }

                        var today = new Date();
                        var age = today.getFullYear() - dob.getFullYear();
                        var hadBirthday = (today.getMonth() > dob.getMonth()) ||
                            (today.getMonth() === dob.getMonth() && today.getDate() >= dob.getDate());
                        if (!hadBirthday) age--;

                        var months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
                        var dobText = dd + ' ' + months[mm - 1] + ' ' + fullYear;

                        if (age < 18) {
                            hint.textContent = 'Date of birth: ' + dobText + ' (must be 18 or older to register).';
                            hint.className = 'field-hint invalid';
                        } else {
                            hint.textContent = 'Date of birth: ' + dobText + ' \u2713';
                            hint.className = 'field-hint valid';
                        }
                    }

                    function setFieldValue(id, value) {
                        var el = document.getElementById(id);
                        if (el && value) el.value = value;
                    }

                    function setProvince(provinceName) {
                        var select = document.getElementById('ddlProvince');
                        if (!select || !provinceName) return;
                        for (var i = 0; i < select.options.length; i++) {
                            if (select.options[i].value === provinceName || select.options[i].text === provinceName) {
                                select.selectedIndex = i;
                                break;
                            }
                        }
                    }

                    function initAddressAutocomplete() {
                        var input = document.getElementById('txtAddressSearch');
                        if (!input) return;

                        var list = document.createElement('div');
                        list.id = 'addressResults';
                        list.style.cssText = 'position:relative;background:#fff;border:1px solid var(--glass-border);border-radius:14px;margin-top:4px;max-height:220px;overflow-y:auto;display:none;z-index:50;';
                        input.parentNode.appendChild(list);

                        var debounceTimer;
                        input.addEventListener('input', function () {
                            clearTimeout(debounceTimer);
                            var q = input.value.trim();
                            if (q.length < 5) { list.style.display = 'none'; return; }

                            debounceTimer = setTimeout(function () {
                                fetch('https://nominatim.openstreetmap.org/search?format=json&addressdetails=1&countrycodes=za&limit=5&q=' + encodeURIComponent(q))
                                    .then(function (r) { return r.json(); })
                                    .then(function (results) {
                                        list.innerHTML = '';
                                        if (!results.length) { list.style.display = 'none'; return; }

                                        results.forEach(function (place) {
                                            var item = document.createElement('div');
                                            item.textContent = place.display_name;
                                            item.style.cssText = 'padding:10px 14px;cursor:pointer;font-size:0.88rem;';
                                            item.onmouseover = function () { item.style.background = 'var(--bg-tint-blue)'; };
                                            item.onmouseout = function () { item.style.background = ''; };
                                            item.onclick = function () {
                                                var a = place.address || {};
                                                setFieldValue('txtStreetNumber', a.house_number);
                                                setFieldValue('txtStreetName', a.road);
                                                setFieldValue('txtSuburb', a.suburb || a.neighbourhood);
                                                setFieldValue('txtCity', a.city || a.town || a.village);
                                                setFieldValue('txtPostalCode', a.postcode);
                                                setProvince(a.state);
                                                input.value = place.display_name;
                                                list.style.display = 'none';
                                            };
                                            list.appendChild(item);
                                        });
                                        list.style.display = 'block';
                                    });
                            }, 400);
                        });

                        document.addEventListener('click', function (e) {
                            if (e.target !== input) list.style.display = 'none';
                        });
                    }

                    function toggleFieldVisibility(fieldId, button) {
                        const field = document.getElementById(fieldId);

                        if (!field) {
                            console.error("Password field not found:", fieldId);
                            return;
                        }

                        const icon = button.querySelector(".eye-icon");

                        if (field.type === "password") {
                            field.type = "text";

                            button.setAttribute("aria-label", "Hide password");

                            icon.innerHTML = `
                            <path d="M3 3l18 18"></path>
                            <path d="M10.6 10.6a2 2 0 0 0 2.8 2.8"></path>
                            <path d="M9.9 5.1A10.8 10.8 0 0 1 12 5c6 0 9.5 7 9.5 7a16.5 16.5 0 0 1-3.2 3.8"></path>
                            <path d="M6.2 6.2C3.9 7.8 2.5 12 2.5 12a16.5 16.5 0 0 0 6.4 5.5A10.8 10.8 0 0 0 12 19c1.7 0 3.2-.5 4.5-1.2"></path>
                        `;
                        } else {
                            field.type = "password";

                            button.setAttribute("aria-label", "Show password");

                            icon.innerHTML = `
                            <path d="M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6z"></path>
                            <circle cx="12" cy="12" r="2.5"></circle>
                        `;
                        }
                    }

                    function checkPasswordStrength(password) {
                        var lengthValid = password.length >= 8;
                        var lowerValid = /[a-z]/.test(password);
                        var upperValid = /[A-Z]/.test(password);
                        var numberValid = /[0-9]/.test(password);
                        var specialValid = /[^A-Za-z0-9]/.test(password);

                        updatePasswordRequirement('reqLength', lengthValid);
                        updatePasswordRequirement('reqLower', lowerValid);
                        updatePasswordRequirement('reqUpper', upperValid);
                        updatePasswordRequirement('reqNumber', numberValid);
                        updatePasswordRequirement('reqSpecial', specialValid);

                        var score = 0;

                        if (lengthValid) score++;
                        if (lowerValid) score++;
                        if (upperValid) score++;
                        if (numberValid) score++;
                        if (specialValid) score++;

                        var fill = document.getElementById('passwordStrengthFill');
                        var text = document.getElementById('passwordStrengthText');

                        if (!fill || !text) return;

                        // Reset
                        fill.classList.remove('strength-weak', 'strength-medium', 'strength-strong');
                        text.classList.remove('strength-weak', 'strength-medium', 'strength-strong');

                        if (password.length === 0) {
                            fill.style.width = '0%';
                            text.textContent = 'Password strength';
                            return;
                        }

                        if (score <= 2) {
                            fill.style.width = '25%';
                            fill.classList.add('strength-weak');

                            text.textContent = 'Weak password';
                            text.classList.add('strength-weak');
                        }
                        else if (score <= 4) {
                            fill.style.width = '60%';
                            fill.classList.add('strength-medium');

                            text.textContent = 'Medium password';
                            text.classList.add('strength-medium');
                        }
                        else {
                            fill.style.width = '100%';
                            fill.classList.add('strength-strong');

                            text.textContent = 'Strong password';
                            text.classList.add('strength-strong');
                        }
                    }

                    function updatePasswordRequirement(id, valid) {
                        var requirement = document.getElementById(id);

                        if (!requirement) return;

                        if (valid) {
                            requirement.classList.add('valid');
                        } else {
                            requirement.classList.remove('valid');
                        }
                    }

                    // ---------- Demo ID photo "AI" check ----------
                    var ID_MAX_BYTES = 25 * 1024 * 1024; // matches maxRequestLength in Web.config
                    var idScanTimer = null;

                    function idEl(id) { return document.getElementById(id); }

                    function validateIdPhoto(src, args) {
                        args.IsValid = idEl('hdnIdPhotoOk').value === '1';
                    }

                    function getIdFormValues() {
                        return {
                            first: idEl('<%= txtFirstName.ClientID %>').value.trim(),
                            last: idEl('<%= txtLastName.ClientID %>').value.trim(),
                            id: idEl('txtIdNumber').value.trim()
                        };
                    }

                    function resetIdScan(clearFile) {
                        clearTimeout(idScanTimer);
                        idEl('hdnIdPhotoOk').value = '0';
                        idEl('idScan').style.display = 'none';
                        idEl('idScanResult').innerHTML = '';
                        idEl('idScanBarFill').style.width = '0';
                        if (clearFile) idEl('fuIdPhoto').value = '';
                    }

                    function showIdStatus(text, cls, showBar) {
                        var s = idEl('idScanStatus');
                        s.textContent = text;
                        s.className = 'id-scan-status' + (cls ? ' ' + cls : '');
                        idEl('idScanBarWrap').style.display = showBar ? 'block' : 'none';
                    }

                    function failIdScan(message) {
                        resetIdScan(true);
                        idEl('idScanThumb').style.display = 'none';
                        idEl('idScan').style.display = 'flex';
                        showIdStatus(message, 'fail', false);
                    }

                    function idDobText(id) {
                        var yy = parseInt(id.substring(0, 2), 10);
                        var mm = parseInt(id.substring(2, 4), 10);
                        var dd = parseInt(id.substring(4, 6), 10);
                        var fullYear = (yy > new Date().getFullYear() % 100 ? 1900 : 2000) + yy;
                        var months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
                        return dd + ' ' + months[mm - 1] + ' ' + fullYear;
                    }

                    function onIdPhotoChange(input) {
                        var file = input.files && input.files[0];
                        resetIdScan(false);
                        if (!file) return;

                        var v = getIdFormValues();
                        var hint = idEl('idNumberHint');
                        if (!v.first || !v.last || v.id.length !== 13 || hint.className.indexOf('invalid') !== -1) {
                            failIdScan('Enter your first name, last name and a valid ID number above, then choose your photo again.');
                            return;
                        }
                        if (!isImageFile(file)) {
                            failIdScan('Please choose an image file.');
                            return;
                        }
                        if (file.size > ID_MAX_BYTES) {
                            failIdScan('That image is too large. Maximum size is 25 MB.');
                            return;
                        }

                        // Any image is accepted. If the browser can't preview it (e.g. HEIC), just skip the thumbnail.
                        var thumb = idEl('idScanThumb');
                        var url = URL.createObjectURL(file);
                        thumb.onerror = function () { thumb.style.display = 'none'; URL.revokeObjectURL(url); };
                        thumb.src = url;
                        thumb.style.display = 'block';
                        runIdScan(file, v);
                    }

                    function isImageFile(file) {
                        if (/^image\//i.test(file.type)) return true;
                        return /\.(jpe?g|jfif|png|gif|bmp|webp|heic|heif|tiff?|avif)$/i.test(file.name);
                    }

                    function runIdScan(file, v) {
                        var steps = [
                            'Uploading image...',
                            'Analysing your ID...',
                            'Reading name and ID number...',
                            'Matching against your details...'
                        ];
                        var i = 0;
                        idEl('idScan').style.display = 'flex';
                        idEl('idScanResult').innerHTML = '';
                        idEl('idScanBarFill').style.width = '0';

                        function next() {
                            if (i < steps.length) {
                                showIdStatus(steps[i], '', true);
                                idEl('idScanBarFill').style.width = (((i + 1) / steps.length) * 100) + '%';
                                i++;
                                idScanTimer = setTimeout(next, 750);
                            } else {
                                finishIdScan(file, v);
                            }
                        }
                        next();
                    }

                    function addIdRow(container, label, value) {
                        var row = document.createElement('div');
                        var a = document.createElement('span');
                        var b = document.createElement('span');
                        a.textContent = label;
                        b.textContent = value;
                        row.appendChild(a);
                        row.appendChild(b);
                        container.appendChild(row);
                    }

                    function finishIdScan(file, v) {
                        // Demo: "extracted" values are the ones the user typed.
                        var confidence = (94 + (file.size % 50) / 10).toFixed(1);
                        var out = idEl('idScanResult');
                        out.innerHTML = '';
                        addIdRow(out, 'Name on ID', v.first + ' ' + v.last);
                        addIdRow(out, 'ID number', v.id);
                        addIdRow(out, 'Date of birth', idDobText(v.id));
                        addIdRow(out, 'Match confidence', confidence + '%');

                        showIdStatus('ID verified \u2713', 'ok', false);
                        idEl('hdnIdPhotoOk').value = '1';
                    }

                    // If the user edits the typed details after verifying, the check no longer applies.
                    function watchIdDetails() {
                        var ids = ['<%= txtFirstName.ClientID %>', '<%= txtLastName.ClientID %>', 'txtIdNumber'];
                        ids.forEach(function (id) {
                            var el = idEl(id);
                            if (!el) return;
                            el.addEventListener('input', function () {
                                if (idEl('idScan').style.display !== 'none' || idEl('hdnIdPhotoOk').value === '1') {
                                    resetIdScan(true);
                                }
                            });
                        });
                    }

                    // ---------- Live username / email availability ----------
                    // Asks CheckAvailability.ashx while the customer types (after a short pause).
                    // Only a hint: the Create account button still re-checks on the server.
                    function initAvailabilityCheck(inputId, hintId, type, pattern, idleText) {
                        var input = idEl(inputId), hint = idEl(hintId);
                        if (!input || !hint) return;
                        var timer = null, latest = 0;

                        function setHint(text, cls) {
                            hint.textContent = text || '';
                            hint.className = 'field-hint avail-hint' + (cls ? ' ' + cls : '');
                        }

                        function check() {
                            var value = input.value.trim();
                            clearTimeout(timer);
                            if (!value) { setHint(idleText, ''); return; }
                            if (!pattern.test(value)) { setHint('', ''); return; } // the format validator handles this
                            setHint('Checking availability…', 'checking');

                            timer = setTimeout(function () {
                                var req = ++latest;
                                fetch('CheckAvailability.ashx?type=' + type + '&value=' + encodeURIComponent(value), { credentials: 'same-origin' })
                                    .then(function (r) { return r.json(); })
                                    .then(function (d) {
                                        if (req !== latest || input.value.trim() !== value) return; // a newer check is on its way
                                        if (d.available === true) setHint('✓ ' + d.message, 'valid');
                                        else if (d.available === false) setHint('✗ ' + d.message, 'invalid');
                                        else setHint(d.message, '');
                                    })
                                    .catch(function () { if (req === latest) setHint('', ''); });
                            }, 450);
                        }

                        input.addEventListener('input', check);
                        if (input.value.trim()) check(); // e.g. after a postback
                    }

                    window.addEventListener('load', function () {
                        initAvailabilityCheck('<%= txtEmail.ClientID %>', 'emailAvailHint', 'email', /^[^\s@]+@[^\s@]+\.[^\s@]+$/, '');
                        initAvailabilityCheck('<%= txtUsername.ClientID %>', 'usernameAvailHint', 'username', /^[a-zA-Z0-9_]{4,30}$/, '');
                    });

                    // ---------- Suggest password ----------
                    // No < > & characters: ASP.NET request validation rejects some combinations of them.
                    var PW_SETS = {
                        lower: 'abcdefghijkmnopqrstuvwxyz',
                        upper: 'ABCDEFGHJKLMNPQRSTUVWXYZ',
                        digit: '23456789',
                        special: '!@#$%*?-_=+'
                    };

                    function randomIndex(max) {
                        var buf = new Uint32Array(1);
                        var limit = Math.floor(0x100000000 / max) * max;
                        do { window.crypto.getRandomValues(buf); } while (buf[0] >= limit);
                        return buf[0] % max;
                    }

                    function pick(chars) { return chars.charAt(randomIndex(chars.length)); }

                    function generatePassword(length) {
                        var all = PW_SETS.lower + PW_SETS.upper + PW_SETS.digit + PW_SETS.special;
                        // One of each required type, the rest from everything.
                        var chars = [pick(PW_SETS.lower), pick(PW_SETS.upper), pick(PW_SETS.digit), pick(PW_SETS.special)];
                        while (chars.length < length) chars.push(pick(all));
                        for (var i = chars.length - 1; i > 0; i--) {
                            var j = randomIndex(i + 1);
                            var t = chars[i]; chars[i] = chars[j]; chars[j] = t;
                        }
                        return chars.join('');
                    }

                    function suggestPassword() {
                        var pw = idEl('<%= txtPassword.ClientID %>');
                        var confirm = idEl('<%= txtConfirmPassword.ClientID %>');
                        var value = generatePassword(16);

                        [pw, confirm].forEach(function (el) {
                            el.value = value;
                            el.dispatchEvent(new Event('input', { bubbles: true }));
                            el.dispatchEvent(new Event('change', { bubbles: true })); // re-runs the ASP.NET validators
                        });

                        // Show the password so the customer can see (and note) what was chosen.
                        if (pw.type === 'password') {
                            toggleFieldVisibility(pw.id, pw.parentNode.querySelector('.login-eye-btn'));
                        }
                        idEl('suggestedPasswordNote').hidden = false;
                    }

                    function copySuggestedPassword(btn) {
                        var value = idEl('<%= txtPassword.ClientID %>').value;
                        function done(ok) {
                            btn.textContent = ok ? 'Copied' : 'Copy failed';
                            setTimeout(function () { btn.textContent = 'Copy'; }, 1800);
                        }
                        if (navigator.clipboard && navigator.clipboard.writeText) {
                            navigator.clipboard.writeText(value).then(function () { done(true); }, function () { done(false); });
                        } else {
                            done(false);
                        }
                    }

                    // ---------- Field help "?" popovers ----------
                    function initFieldHelp() {
                        var buttons = document.querySelectorAll('.field-help[data-rules]');
                        var openBtn = null;

                        function position(btn) {
                            var tip = btn._tip;
                            var r = btn.getBoundingClientRect();
                            tip.style.left = '0px';
                            tip.style.top = '0px';
                            var w = tip.offsetWidth, h = tip.offsetHeight;
                            var left = Math.min(Math.max(8, r.left + r.width / 2 - w / 2), window.innerWidth - w - 8);
                            var top = r.bottom + 8;
                            if (top + h > window.innerHeight - 8) top = Math.max(8, r.top - h - 8);
                            tip.style.left = left + 'px';
                            tip.style.top = top + 'px';
                        }

                        function show(btn) {
                            if (openBtn && openBtn !== btn) hide(openBtn, true);
                            btn._tip.classList.add('open');
                            position(btn);
                        }

                        function hide(btn, force) {
                            if (!force && btn._pinned) return;
                            btn._pinned = false;
                            btn._tip.classList.remove('open');
                            btn.setAttribute('aria-expanded', 'false');
                            if (openBtn === btn) openBtn = null;
                        }

                        Array.prototype.forEach.call(buttons, function (btn, i) {
                            var label = btn.closest('label');
                            var name = label && label.firstChild ? label.firstChild.textContent.trim() : 'this field';
                            if (name.length > 40) name = 'this field';

                            var tip = document.createElement('div');
                            tip.className = 'field-help-tip';
                            tip.id = 'fieldHelpTip' + i;
                            tip.setAttribute('role', 'tooltip');
                            var title = document.createElement('div');
                            title.className = 'field-help-title';
                            title.textContent = name;
                            tip.appendChild(title);
                            var list = document.createElement('ul');
                            btn.getAttribute('data-rules').split('|').forEach(function (rule) {
                                var li = document.createElement('li');
                                li.textContent = rule;
                                list.appendChild(li);
                            });
                            tip.appendChild(list);
                            document.body.appendChild(tip);

                            btn._tip = tip;
                            btn.textContent = '?';
                            btn.setAttribute('aria-label', 'Rules for ' + name);
                            btn.setAttribute('aria-describedby', tip.id);
                            btn.setAttribute('aria-expanded', 'false');

                            btn.addEventListener('mouseenter', function () { show(btn); });
                            btn.addEventListener('mouseleave', function () { hide(btn); });
                            btn.addEventListener('focus', function () { show(btn); });
                            btn.addEventListener('blur', function () { hide(btn, true); });
                            btn.addEventListener('click', function (e) {
                                e.preventDefault();
                                e.stopPropagation();
                                if (btn._pinned) { hide(btn, true); return; }
                                show(btn);
                                btn._pinned = true;
                                btn.setAttribute('aria-expanded', 'true');
                                openBtn = btn;
                            });
                        });

                        document.addEventListener('click', function () { if (openBtn) hide(openBtn, true); });
                        document.addEventListener('keydown', function (e) {
                            if (e.key === 'Escape' && openBtn) hide(openBtn, true);
                        });
                        window.addEventListener('scroll', function () {
                            document.querySelectorAll('.field-help-tip.open').forEach(function (t) { t.classList.remove('open'); });
                            if (openBtn) hide(openBtn, true);
                        }, { passive: true });
                        window.addEventListener('resize', function () { if (openBtn) position(openBtn); });
                    }

                    window.addEventListener('load', initAddressAutocomplete);
                    window.addEventListener('load', watchIdDetails);
                    window.addEventListener('load', initFieldHelp);
                    window.addEventListener('load', function () {
                        var pw = idEl('<%= txtPassword.ClientID %>');
                        if (!pw) return;
                        pw.addEventListener('input', function (e) {
                            if (e.isTrusted) idEl('suggestedPasswordNote').hidden = true; // typed by the customer
                        });
                    });
                </script>
            </div>
        </div>
    </div>
</asp:Content>