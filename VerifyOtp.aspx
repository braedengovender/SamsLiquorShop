<%@ Page Title="Verify" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="VerifyOtp.aspx.cs" Inherits="SamsLiquorShop.VerifyOtp" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell login-wrap">
        <div class="glass login-card wide-card">
            <p class="login-eyebrow">Sam's Liquor Shop</p>
            <h1 class="login-heading">Check your email</h1>
            <p class="login-subtext">Enter the 6-digit code we sent you.</p>

            <span id="spanDemo" runat="server" class="field-error" visible="false"></span>

            <div class="login-field">
                <label for="otp1">Login code</label>

                <div class="otp-boxes" id="otpBoxes">
                    <input type="text" id="otp1" class="otp-box" maxlength="6" inputmode="numeric" pattern="[0-9]*" autocomplete="one-time-code" aria-label="Digit 1" />
                    <input type="text" class="otp-box" maxlength="1" inputmode="numeric" pattern="[0-9]*" autocomplete="off" aria-label="Digit 2" />
                    <input type="text" class="otp-box" maxlength="1" inputmode="numeric" pattern="[0-9]*" autocomplete="off" aria-label="Digit 3" />
                    <input type="text" class="otp-box" maxlength="1" inputmode="numeric" pattern="[0-9]*" autocomplete="off" aria-label="Digit 4" />
                    <input type="text" class="otp-box" maxlength="1" inputmode="numeric" pattern="[0-9]*" autocomplete="off" aria-label="Digit 5" />
                    <input type="text" class="otp-box" maxlength="1" inputmode="numeric" pattern="[0-9]*" autocomplete="off" aria-label="Digit 6" />
                </div>

                <%-- The server still reads this one; the boxes above fill it in --%>
                <asp:TextBox ID="txtCode" runat="server" ClientIDMode="Static" MaxLength="6" Style="display:none;"></asp:TextBox>
                <span class="field-error" id="otpClientError"></span>
            </div>

            <asp:Label ID="lblInfo" runat="server" CssClass="login-subtext" Visible="false"></asp:Label>
            <asp:Label ID="lblError" runat="server" CssClass="field-error login-server-error" Visible="false"></asp:Label>

            <asp:Button ID="btnVerify" runat="server" Text="Verify" CssClass="btn-primary login-submit" OnClick="btnVerify_Click" OnClientClick="if (window.__otpBusy) { return false; } if (!otpReady()) { return false; } window.__otpBusy = true; return true;" />
            <asp:Button ID="btnResend" runat="server" Text="Resend code" CssClass="btn-ghost login-submit otp-resend" OnClick="btnResend_Click" CausesValidation="false" />

            <p class="login-register-text"><a href="Login.aspx">Back to login</a></p>
        </div>
    </section>
    <style>
        .otp-boxes {
            display: flex;
            justify-content: center;
            gap: 10px;
            margin: 8px 0 4px;
        }

        .otp-box {
            width: 52px;
            height: 64px;
            text-align: center;
            font-size: 1.9rem;
            font-weight: 700;
            font-family: inherit;
            color: var(--text-primary);
            background: rgba(128, 128, 128, 0.10);
            border: 1.5px solid rgba(128, 128, 128, 0.35);
            border-radius: 12px;
            outline: none;
            padding: 0;
            caret-color: var(--accent);
            transition: border-color 0.15s ease, box-shadow 0.15s ease;
        }

        .otp-box:focus {
            border-color: var(--accent);
            box-shadow: 0 0 0 3px rgba(10, 132, 255, 0.25);
        }

        .otp-box.invalid {
            border-color: #d93025;
        }

        /* breathing room between Verify and Resend */
        .otp-resend {
            margin-top: 16px;
        }

        @media (max-width: 420px) {
            .otp-boxes { gap: 6px; }
            .otp-box { width: 44px; height: 56px; font-size: 1.6rem; }
        }
    </style>

    <script>
        (function () {
            var boxes = Array.prototype.slice.call(document.querySelectorAll('.otp-box'));
            var hidden = document.getElementById('txtCode');
            var errorEl = document.getElementById('otpClientError');

            function sync() {
                hidden.value = boxes.map(function (b) { return b.value; }).join('');
                errorEl.textContent = '';
                boxes.forEach(function (b) { b.classList.remove('invalid'); });
            }

            // Spread a string of digits over the boxes starting at index i
            function fill(i, digits) {
                for (var k = 0; k < digits.length && i + k < boxes.length; k++) {
                    boxes[i + k].value = digits.charAt(k);
                }
                var next = Math.min(i + digits.length, boxes.length - 1);
                boxes[next].focus();
                boxes[next].select();
                sync();
            }

            boxes.forEach(function (box, i) {
                box.addEventListener('input', function () {
                    var digits = box.value.replace(/\D/g, '');
                    box.value = '';
                    if (!digits) { sync(); return; }
                    fill(i, digits);   // handles typing one digit, autofill and paste alike
                });

                box.addEventListener('keydown', function (e) {
                    if (e.key === 'Backspace') {
                        if (box.value) {
                            box.value = '';
                        } else if (i > 0) {
                            boxes[i - 1].value = '';
                            boxes[i - 1].focus();
                        }
                        sync();
                        e.preventDefault();
                    } else if (e.key === 'ArrowLeft' && i > 0) {
                        boxes[i - 1].focus();
                        e.preventDefault();
                    } else if (e.key === 'ArrowRight' && i < boxes.length - 1) {
                        boxes[i + 1].focus();
                        e.preventDefault();
                    } else if (e.key === 'Enter') {
                        e.preventDefault();
                        document.getElementById('<%= btnVerify.ClientID %>').click();
                    }
                });

                box.addEventListener('paste', function (e) {
                    var text = (e.clipboardData || window.clipboardData).getData('text') || '';
                    var digits = text.replace(/\D/g, '');
                    if (!digits) { e.preventDefault(); return; }
                    e.preventDefault();
                    fill(i, digits);
                });

                box.addEventListener('focus', function () { box.select(); });
            });

            window.otpReady = function () {
                sync();
                if (hidden.value.length === 6) return true;
                errorEl.textContent = 'Please enter all 6 digits.';
                boxes.forEach(function (b) { if (!b.value) b.classList.add('invalid'); });
                var firstEmpty = boxes.filter(function (b) { return !b.value; })[0];
                if (firstEmpty) firstEmpty.focus();
                return false;
            };

            // Fresh start on every page load (including after a wrong code or a resend)
            hidden.value = '';
            boxes[0].focus();
        })();
    </script>
</asp:Content>