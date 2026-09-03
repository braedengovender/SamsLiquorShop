<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="SamsLiquorShop.Register" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="login-wrap">
        <div class="glass login-card wide-card">
            <div class="register-inner">

                <asp:Panel ID="pnlForm" runat="server">

                    <p class="login-eyebrow">Join Sam's Liquor Shop</p>
                    <h1 class="login-heading">Create your account</h1>
                    <p class="login-subtext">You must be 18 or older to register. We deliver within South Africa only.</p>

                    <div class="register-section">
                        <h3 class="register-section-title">Personal Details</h3>

                        <div class="register-grid-2">
                            <div class="login-field">
                                <label for="txtFirstName">First name</label>
                                <asp:TextBox ID="txtFirstName" runat="server" CssClass="login-input" MaxLength="50" autocomplete="given-name" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFirstName" CssClass="field-error" Display="Dynamic" ErrorMessage="First name is required." ValidationGroup="RegisterForm" />
                            </div>
                            <div class="login-field">
                                <label for="txtLastName">Last name</label>
                                <asp:TextBox ID="txtLastName" runat="server" CssClass="login-input" MaxLength="50" autocomplete="family-name" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtLastName" CssClass="field-error" Display="Dynamic" ErrorMessage="Last name is required." ValidationGroup="RegisterForm" />
                            </div>
                        </div>

                        <div class="register-grid-2">
                            <div class="login-field">
                                <label for="txtIdNumber">SA ID number</label>
                                <asp:TextBox ID="txtIdNumber" runat="server" ClientIDMode="Static" CssClass="login-input" MaxLength="13" autocomplete="off" placeholder="13 digits" oninput="onIdNumberInput()" />
                                <span class="field-hint" id="idNumberHint"></span>
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtIdNumber" CssClass="field-error" Display="Dynamic" ErrorMessage="ID number is required." ValidationGroup="RegisterForm" />
                                <asp:CustomValidator ID="cvIdNumber" runat="server" ControlToValidate="txtIdNumber" CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid South African ID number." OnServerValidate="cvIdNumber_ServerValidate" ValidationGroup="RegisterForm" />
                            </div>
                            <div class="login-field">
                                <label for="txtPhone">Phone number</label>
                                <asp:TextBox ID="txtPhone" runat="server" CssClass="login-input" MaxLength="15" autocomplete="tel" placeholder="e.g. 0821234567" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPhone" CssClass="field-error" Display="Dynamic" ErrorMessage="Phone number is required." ValidationGroup="RegisterForm" />
                                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPhone" CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid SA phone number." ValidationExpression="^(\+27|0)[1-9][0-9]{8}$" ValidationGroup="RegisterForm" />
                            </div>
                        </div>

                        <div class="login-field">
                            <label for="txtEmail">Email address</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="login-input" TextMode="Email" MaxLength="256" autocomplete="email" />
                            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" CssClass="field-error" Display="Dynamic" ErrorMessage="Email is required." ValidationGroup="RegisterForm" />
                            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail" CssClass="field-error" Display="Dynamic" ErrorMessage="Enter a valid email address." ValidationExpression="^[^\s@]+@[^\s@]+\.[^\s@]+$" ValidationGroup="RegisterForm" />
                        </div>
                    </div>

                    <div class="register-section">
                        <h3 class="register-section-title">Residential Address</h3>

                        <div class="login-field">
                            <label for="txtAddressSearch">Search your address</label>
                            <input type="text" id="txtAddressSearch" class="login-input" placeholder="Start typing your address..." autocomplete="off" />
                        </div>

                        <div class="register-grid-2">
                            <div class="login-field">
                                <label for="txtUnitNumber">Unit number <span class="register-optional">(optional)</span></label>
                                <asp:TextBox ID="txtUnitNumber" runat="server" CssClass="login-input" MaxLength="20" autocomplete="off" />
                            </div>
                            <div class="login-field">
                                <label for="txtUnitName">Unit / complex name <span class="register-optional">(optional)</span></label>
                                <asp:TextBox ID="txtUnitName" runat="server" CssClass="login-input" MaxLength="100" autocomplete="off" />
                            </div>
                        </div>

                        <div class="register-grid-2">
                            <div class="login-field">
                                <label for="txtStreetNumber">Street number</label>
                                <asp:TextBox ID="txtStreetNumber" runat="server" ClientIDMode="Static" CssClass="login-input" MaxLength="20" autocomplete="off" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtStreetNumber" CssClass="field-error" Display="Dynamic" ErrorMessage="Street number is required." ValidationGroup="RegisterForm" />
                            </div>
                            <div class="login-field">
                                <label for="txtStreetName">Street name</label>
                                <asp:TextBox ID="txtStreetName" runat="server" ClientIDMode="Static" CssClass="login-input" MaxLength="100" autocomplete="address-line1" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtStreetName" CssClass="field-error" Display="Dynamic" ErrorMessage="Street name is required." ValidationGroup="RegisterForm" />
                            </div>
                        </div>

                        <div class="register-grid-2">
                            <div class="login-field">
                                <label for="txtSuburb">Suburb</label>
                                <asp:TextBox ID="txtSuburb" runat="server" ClientIDMode="Static" CssClass="login-input" MaxLength="100" autocomplete="address-level3" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtSuburb" CssClass="field-error" Display="Dynamic" ErrorMessage="Suburb is required." ValidationGroup="RegisterForm" />
                            </div>
                            <div class="login-field">
                                <label for="txtCity">City</label>
                                <asp:TextBox ID="txtCity" runat="server" ClientIDMode="Static" CssClass="login-input" MaxLength="100" autocomplete="address-level2" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtCity" CssClass="field-error" Display="Dynamic" ErrorMessage="City is required." ValidationGroup="RegisterForm" />
                            </div>
                        </div>

                        <div class="register-grid-2">
                            <div class="login-field">
                                <label for="txtPostalCode">Postal code</label>
                                <asp:TextBox ID="txtPostalCode" runat="server" ClientIDMode="Static" CssClass="login-input" MaxLength="4" autocomplete="postal-code" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPostalCode" CssClass="field-error" Display="Dynamic" ErrorMessage="Postal code is required." ValidationGroup="RegisterForm" />
                                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtPostalCode" CssClass="field-error" Display="Dynamic" ErrorMessage="Postal code must be 4 digits." ValidationExpression="^\d{4}$" ValidationGroup="RegisterForm" />
                            </div>
                            <div class="login-field">
                                <label for="ddlProvince">Province</label>
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
                            <label for="txtUsername">Username <span class="register-optional">(optional - we'll use your email if left blank)</span></label>
                            <asp:TextBox ID="txtUsername" runat="server" CssClass="login-input" MaxLength="30" autocomplete="username" />
                            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtUsername" CssClass="field-error" Display="Dynamic" ErrorMessage="Username must be 4-30 letters, numbers or underscores." ValidationExpression="^[a-zA-Z0-9_]{4,30}$" ValidationGroup="RegisterForm" />
                        </div>

                        <div class="register-grid-2">

                            <div class="login-field login-password-wrap">
                                <label for="<%= txtPassword.ClientID %>">Password</label>

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
                                <label for="<%= txtConfirmPassword.ClientID %>">Confirm password</label>

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
                                    Display="Dynamic" />

                                <asp:CompareValidator
                                    ID="cvConfirmPassword"
                                    runat="server"
                                    ControlToValidate="txtConfirmPassword"
                                    ControlToCompare="txtPassword"
                                    ErrorMessage="Passwords do not match."
                                    CssClass="field-validation"
                                    Display="Dynamic" />
                            </div>

                        </div>
                    </div>

                    <div class="register-section">
                        <div class="login-field register-terms-field">
                            <label class="register-terms-label">
                                <asp:CheckBox ID="chkTerms" runat="server" ClientIDMode="Static" />
                                <span>I confirm I am 18 or older and agree to the Terms &amp; Conditions and Privacy Policy.</span>
                            </label>
                            <asp:CustomValidator ID="cvTerms" runat="server" CssClass="field-error" Display="Dynamic" ErrorMessage="You must accept the Terms &amp; Conditions to continue." OnServerValidate="cvTerms_ServerValidate" ValidationGroup="RegisterForm" />
                        </div>

                        <asp:Button ID="btnRegister" runat="server" Text="Create account" CssClass="btn-primary login-submit" ValidationGroup="RegisterForm" OnClick="btnRegister_Click" />
                        <asp:Label ID="lblError" runat="server" CssClass="field-error"></asp:Label>
                        <div class="login-divider"><span>or</span></div>
                        <button type="button" class="login-google-btn" onclick="alert('Google sign-up isn\'t connected yet — this is a placeholder for now.');">
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

                    window.addEventListener('load', initAddressAutocomplete);
                </script>

                <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="register-success">

                    <p class="login-eyebrow">Email Verification</p>

                    <h1 class="login-heading">Check your email</h1>

                    <p class="login-subtext">
                        We've sent a 5-digit verification code to your email address.
                        Enter the code below to verify your account.
                    </p>

                    <div class="login-field" style="margin-top: 25px;">

                        <label for="txtOTP">Verification code</label>

                        <asp:TextBox
                            ID="txtOTP"
                            runat="server"
                            ClientIDMode="Static"
                            CssClass="login-input"
                            MaxLength="5"
                            placeholder="Enter 5-digit code"
                            autocomplete="one-time-code"
                            Style="text-align: center; font-size: 24px; letter-spacing: 8px;" />

                        <asp:RequiredFieldValidator
                            ID="rfvOTP"
                            runat="server"
                            ControlToValidate="txtOTP"
                            CssClass="field-error"
                            Display="Dynamic"
                            ErrorMessage="Please enter the verification code." />

                    </div>

                    <asp:Label
                        ID="lblOtpError"
                        runat="server"
                        CssClass="field-error"
                        Style="display: block; text-align: center; margin-top: 10px;">
                    </asp:Label>

                    <asp:Button
                        ID="btnVerifyOTP"
                        runat="server"
                        Text="Verify email"
                        CssClass="btn-primary login-submit"
                        OnClick="btnVerifyOTP_Click" />

                    <p style="text-align: center; margin-top: 18px; color: #718093; font-size: 12px;">
                        Didn't receive the email? Check your spam/junk folder.
                    </p>

                </asp:Panel>

            </div>
        </div>
    </div>
</asp:Content>