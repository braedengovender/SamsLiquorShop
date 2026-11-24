<%@ Page Title="Checkout" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Checkout.aspx.cs" Inherits="SamsLiquorShop.Checkout" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .co-card { padding: 28px; margin-bottom: 22px; }
        .co-card h3 { margin: 0 0 18px; }
        .co-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px 18px; }
        .co-grid .co-full { grid-column: 1 / -1; }
        .co-field { position: relative; }
        .co-field label { display: block; font-size: 0.85rem; font-weight: 600; margin-bottom: 6px; color: var(--text-secondary); }
        .co-field .login-input, .co-select { width: 100%; box-sizing: border-box; }
        .co-select { padding: 13px 16px; border-radius: 14px; border: 1px solid var(--glass-border); background: var(--bg); color: inherit; font: inherit; }
        .co-note { font-size: 0.85rem; color: var(--text-secondary); margin: 8px 0 0; }
        .co-promo-row { display: flex; gap: 10px; align-items: center; }
        .co-promo-row .login-input { flex: 1; }

        /* delivery / collection mini cards */
        .co-methods { list-style: none; margin: 0; padding: 0; display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; }
        .co-methods li { position: relative; margin: 0; }
        .co-methods input { position: absolute; inset: 0; width: 100%; height: 100%; margin: 0; opacity: 0; cursor: pointer; z-index: 2; }
        .co-methods label { display: block; position: relative; padding: 20px 22px 20px 60px; border: 1.5px solid var(--glass-border); border-radius: var(--radius-md); background: var(--bg); font-size: 1rem; font-weight: 700; line-height: 1.3; transition: border-color .15s, background .15s, box-shadow .15s; }
        .co-methods label::before { content: ""; position: absolute; left: 22px; top: 22px; width: 20px; height: 20px; border-radius: 50%; border: 2px solid rgba(128,128,128,.55); }
        .co-methods label::after { display: block; margin-top: 6px; font-size: .84rem; font-weight: 400; line-height: 1.5; color: var(--text-secondary); }
        .co-methods li:nth-child(1) label::after { content: "Durban area only, from tomorrow. R70, free from R500. Minimum order R250."; }
        .co-methods li:nth-child(2) label::after { content: "Free for any order amount. Collect from our Tongaat store with your code and ID."; }
        .co-methods input:checked + label { border-color: var(--accent); background: var(--bg-tint-blue); box-shadow: 0 0 0 3px rgba(10,132,255,.14); }
        .co-methods input:checked + label::before { border-color: var(--accent); background: radial-gradient(circle, var(--accent) 0 5px, transparent 6px); }
        .co-methods input:focus-visible + label { outline: 2px solid var(--accent); outline-offset: 2px; }

        /* checkboxes */
        .co-check { display: flex; align-items: flex-start; gap: 14px; }
        .co-check input[type=checkbox] { -webkit-appearance: none; appearance: none; flex: 0 0 22px; width: 22px; height: 22px; margin: 2px 0 0; border: 1.5px solid rgba(128,128,128,.6); border-radius: 7px; background: transparent; cursor: pointer; position: relative; transition: background .15s, border-color .15s; }
        .co-check input[type=checkbox]:hover { border-color: var(--accent); }
        .co-check input[type=checkbox]:checked { background: var(--accent); border-color: var(--accent); }
        .co-check input[type=checkbox]:checked::after { content: ""; position: absolute; left: 6px; top: 2px; width: 6px; height: 11px; border: solid #fff; border-width: 0 2.5px 2.5px 0; transform: rotate(45deg); }
        .co-check input[type=checkbox]:focus-visible { outline: 2px solid var(--accent); outline-offset: 2px; }
        .co-check label { cursor: pointer; font-size: .92rem; line-height: 1.55; margin: 0; }

        /* live validation */
        .co-msg { display: block; margin-top: 5px; font-size: .78rem; color: var(--text-secondary); }
        .co-msg:empty { display: none; }
        .co-msg.is-bad { color: #d70015; }
        .co-msg.is-ok { color: #1e8e3e; }
        .login-input.co-bad, .co-select.co-bad { border-color: #d70015; box-shadow: 0 0 0 3px rgba(215,0,21,.10); }
        .login-input.co-ok, .co-select.co-ok { border-color: rgba(52,199,89,.75); }
        [data-theme="dark"] .co-msg.is-bad { color: #ff453a; }
        [data-theme="dark"] .co-msg.is-ok { color: #30d158; }

        /* address search */
        .co-addr-results { position: absolute; left: 0; right: 0; top: 100%; z-index: 60; margin-top: 4px; max-height: 240px; overflow-y: auto; background: var(--bg); border: 1px solid var(--glass-border); border-radius: 14px; box-shadow: 0 10px 28px rgba(20,40,80,.16); display: none; }
        .co-addr-item { padding: 11px 16px; font-size: .88rem; cursor: pointer; }
        .co-addr-item:hover { background: var(--bg-tint-blue); }
        .co-addr-empty { padding: 12px 16px; font-size: .85rem; color: var(--text-secondary); }

        /* place order button / switch */
        .cart-checkout-btn.aspNetDisabled { background: #c7c7cc; color: #fff; cursor: not-allowed; opacity: .85; pointer-events: none; }
        .co-switch { display: block; text-align: center; margin-top: 10px; border: 1px solid var(--glass-border); cursor: pointer; }

        /* payment portal */
        .co-portal { position: fixed; inset: 0; z-index: 2500; display: flex; align-items: center; justify-content: center; padding: 20px; background: rgba(10,12,20,.62); opacity: 0; visibility: hidden; transition: opacity .2s, visibility .2s; }
        .co-portal.is-open { opacity: 1; visibility: visible; }
        .co-portal-card { width: 100%; max-width: 440px; padding: 30px; background: #fff; color: #1d1d1f; border-radius: var(--radius-lg); box-shadow: 0 24px 60px rgba(0,0,0,.3); }
        [data-theme="dark"] .co-portal-card { background: #10131c; color: var(--text-primary); }
        .co-portal-brand { display: flex; align-items: center; gap: 8px; margin-bottom: 4px; font-weight: 700; color: #0057b8; }
        [data-theme="dark"] .co-portal-brand { color: #5ac8fa; }
        .co-portal-card h3 { margin: 0 0 4px; font-size: 1.15rem; }
        .co-portal-amount { margin: 0; font-size: .9rem; color: var(--text-secondary); }
        .co-portal-visual { display: flex; justify-content: space-between; align-items: center; gap: 12px; margin: 18px 0; padding: 16px 18px; border-radius: 14px; color: #fff; background: linear-gradient(135deg, #0a84ff, #003f87); font-variant-numeric: tabular-nums; letter-spacing: .08em; font-weight: 600; }
        .co-portal-visual small { letter-spacing: .04em; font-weight: 600; opacity: .85; }
        .co-portal-visual-main { display: flex; flex-direction: column; gap: 6px; min-width: 0; }
        .co-portal-name { text-transform: uppercase; font-size: .78rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .co-card h3 .field-help { vertical-align: 3px; }
        .co-check + .co-msg + .co-note, .co-check + .co-note { margin-top: 6px; }
        .co-portal-steps { list-style: none; margin: 0 0 16px; padding: 0; display: grid; gap: 10px; font-size: .9rem; }
        .co-portal-steps li { display: flex; align-items: center; gap: 10px; color: var(--text-secondary); }
        .co-portal-steps li::before { content: ""; flex: 0 0 18px; width: 18px; height: 18px; border-radius: 50%; border: 2px solid rgba(128,128,128,.45); display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; color: #fff; }
        .co-portal-steps li.is-active { color: inherit; font-weight: 600; }
        .co-portal-steps li.is-active::before { border-color: #0a84ff; border-top-color: transparent; animation: co-spin .8s linear infinite; }
        .co-portal-steps li.is-done::before { content: "\2713"; background: #34c759; border-color: #34c759; }
        .co-portal-steps li.is-failed { color: #d70015; font-weight: 600; }
        .co-portal-steps li.is-failed::before { content: "!"; background: #d70015; border-color: #d70015; }
        .co-portal-bar { height: 4px; border-radius: 99px; background: rgba(10,132,255,.15); overflow: hidden; }
        .co-portal-bar span { display: block; height: 100%; width: 0; background: #0a84ff; transition: width .6s ease; }
        .co-portal-status { margin: 12px 0 0; min-height: 1.3em; font-size: .85rem; color: var(--text-secondary); }
        .co-portal-status.is-bad { color: #d70015; font-weight: 600; }
        .co-portal-close { margin-top: 16px; width: 100%; border: none; cursor: pointer; padding: 12px; font: inherit; font-weight: 600; }
        @keyframes co-spin { to { transform: rotate(360deg); } }

        @media (max-width: 640px) { .co-grid, .co-methods { grid-template-columns: 1fr; } }
    </style>

    <section class="shell">
        <h1 class="login-heading" style="margin: 32px 0 8px;">Checkout</h1>
        <p class="login-subtext" style="margin: 0 0 16px;">Check your details and place your order. <a href="Cart.aspx">Back to cart</a></p>

        <asp:UpdatePanel ID="upnlCheckout" runat="server">
            <ContentTemplate>
                <asp:Literal ID="litMessage" runat="server" />

                <asp:Panel ID="pnlForm" runat="server">
                    <div class="cart-layout">
                        <div class="cart-items-col">

                            <div class="glass co-card">
                                <h3>1. Delivery or collection</h3>
                                <asp:RadioButtonList ID="rblMethod" runat="server" AutoPostBack="true" OnSelectedIndexChanged="rblMethod_Changed"
                                    RepeatLayout="UnorderedList" CssClass="co-methods">
                                    <asp:ListItem Value="Delivery" Selected="True">Delivery</asp:ListItem>
                                    <asp:ListItem Value="Collection">Click &amp; Collect</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <div class="glass co-card">
                                <h3>2. Your details</h3>
                                <div class="co-grid">
                                    <div class="co-field">
                                        <label for="<%= txtFirstName.ClientID %>">First name <button type="button" class="field-help" data-rules="Required.|Letters, spaces, hyphens and apostrophes only.|Up to 50 characters."></button></label>
                                        <asp:TextBox ID="txtFirstName" runat="server" CssClass="login-input" MaxLength="50" autocomplete="given-name" data-rule="name" data-letters="1" />
                                    </div>
                                    <div class="co-field">
                                        <label for="<%= txtLastName.ClientID %>">Last name <button type="button" class="field-help" data-rules="Required.|Letters, spaces, hyphens and apostrophes only.|Up to 50 characters."></button></label>
                                        <asp:TextBox ID="txtLastName" runat="server" CssClass="login-input" MaxLength="50" autocomplete="family-name" data-rule="name" data-letters="1" />
                                    </div>
                                    <div class="co-field">
                                        <label for="<%= txtEmail.ClientID %>">Email address <button type="button" class="field-help" data-rules="Required.|A valid address, e.g. name@example.com. Up to 100 characters.|We send your receipt and order updates here."></button></label>
                                        <asp:TextBox ID="txtEmail" runat="server" CssClass="login-input" MaxLength="100" TextMode="Email" autocomplete="email" data-rule="email" />
                                    </div>
                                    <div class="co-field">
                                        <label for="<%= txtPhone.ClientID %>">Phone number <button type="button" class="field-help" data-rules="Required.|9 to 15 digits, e.g. 0821234567. You can start with +27.|Spaces, dashes and brackets are ignored.|We only call if there is a problem with your order."></button></label>
                                        <asp:TextBox ID="txtPhone" runat="server" CssClass="login-input" MaxLength="20" data-phone="1" autocomplete="tel" data-rule="phone" />
                                    </div>
                                </div>
                            </div>

                            <div class="glass co-card">
                                <h3>3. <asp:Literal ID="litStepTitle" runat="server" Text="Delivery address and time" /></h3>

                                <asp:Panel ID="pnlAddress" runat="server">
                                    <div class="co-field" style="margin-bottom: 16px;">
                                        <label for="txtAddressSearch">Search your address <button type="button" class="field-help" data-rules="Optional shortcut.|Type at least 5 characters, then pick your address from the list.|Only Durban-area addresses are shown.|The fields below fill in for you. Check them and correct anything that's wrong."></button></label>
                                        <input type="text" id="txtAddressSearch" class="login-input" placeholder="Start typing your address..." autocomplete="off" />
                                        <span class="co-note" style="display:block;">We deliver in the Durban area only. Pick a result to fill in the fields below.</span>
                                    </div>
                                    <div class="co-grid">
                                        <div class="co-field">
                                            <label for="<%= txtUnitNumber.ClientID %>">Unit number (optional) <button type="button" class="field-help" data-rules="Optional. Only needed for flats, townhouses or complexes.|Up to 10 characters."></button></label>
                                            <asp:TextBox ID="txtUnitNumber" runat="server" CssClass="login-input" MaxLength="10" />
                                        </div>
                                        <div class="co-field">
                                            <label for="<%= txtUnitName.ClientID %>">Complex / building (optional) <button type="button" class="field-help" data-rules="Optional. The name of your building or complex.|Up to 50 characters."></button></label>
                                            <asp:TextBox ID="txtUnitName" runat="server" CssClass="login-input" MaxLength="50" />
                                        </div>
                                        <div class="co-field">
                                            <label for="<%= txtStreetNumber.ClientID %>">Street number <button type="button" class="field-help" data-rules="Required.|Your house or building number, e.g. 12 or 12A.|Up to 10 letters, numbers, / or -."></button></label>
                                            <asp:TextBox ID="txtStreetNumber" runat="server" CssClass="login-input" MaxLength="10" data-rule="streetno" />
                                        </div>
                                        <div class="co-field">
                                            <label for="<%= txtStreetName.ClientID %>">Street name <button type="button" class="field-help" data-rules="Required.|The full street name, e.g. Florida Road.|Up to 100 characters."></button></label>
                                            <asp:TextBox ID="txtStreetName" runat="server" CssClass="login-input" MaxLength="100" data-rule="text" />
                                        </div>
                                        <div class="co-field">
                                            <label for="<%= txtSuburb.ClientID %>">Suburb <button type="button" class="field-help" data-rules="Required.|e.g. Morningside or Umhlanga.|Up to 50 characters."></button></label>
                                            <asp:TextBox ID="txtSuburb" runat="server" CssClass="login-input" MaxLength="50" data-rule="text" />
                                        </div>
                                        <div class="co-field">
                                            <label for="<%= txtPostalCode.ClientID %>">Postal code <button type="button" class="field-help" data-rules="Required.|Exactly 4 digits, e.g. 4001."></button></label>
                                            <asp:TextBox ID="txtPostalCode" runat="server" CssClass="login-input" MaxLength="4" data-digits="1" inputmode="numeric" data-rule="postal" />
                                        </div>
                                        <div class="co-field">
                                            <label for="<%= txtCity.ClientID %>">City <button type="button" class="field-help" data-rules="Filled in for you.|We deliver within the Durban area only."></button></label>
                                            <asp:TextBox ID="txtCity" runat="server" CssClass="login-input" ReadOnly="true" />
                                        </div>
                                        <div class="co-field">
                                            <label for="<%= txtProvince.ClientID %>">Province <button type="button" class="field-help" data-rules="Filled in for you.|We deliver within the Durban area (KwaZulu-Natal) only."></button></label>
                                            <asp:TextBox ID="txtProvince" runat="server" CssClass="login-input" ReadOnly="true" />
                                        </div>
                                        <div class="co-field co-full">
                                            <label for="<%= txtInstructions.ClientID %>">Delivery instructions (optional) <button type="button" class="field-help" data-rules="Optional.|Anything the driver should know, e.g. a gate code or where to park.|Up to 250 characters."></button></label>
                                            <asp:TextBox ID="txtInstructions" runat="server" CssClass="login-input" TextMode="MultiLine" Rows="2" MaxLength="250" />
                                        </div>
                                    </div>
                                </asp:Panel>

                                <asp:Panel ID="pnlCollectNote" runat="server" Visible="false">
                                    <p style="margin: 0 0 12px;">Collect from <strong>21 Coronation Road, Mithangar, Tongaat, 4399</strong>. We'll give you a collection code after you order. Please bring it and your ID.</p>
                                </asp:Panel>

                                <div class="co-field" style="margin-top: 16px;">
                                    <label for="<%= ddlSlot.ClientID %>">Time slot <button type="button" class="field-help" data-rules="Required.|Pick a time that suits you from the list.|Slots that are full or too soon are not shown.|If your slot passes before you order, choose another one."></button></label>
                                    <asp:DropDownList ID="ddlSlot" runat="server" CssClass="co-select" data-rule="slot" />
                                    <asp:Literal ID="litSlotHint" runat="server" />
                                </div>
                            </div>

                            <div class="glass co-card">
                                <h3>4. Promo code and loyalty points <button type="button" class="field-help" data-title="Promo code and loyalty points" data-rules="Optional.|Promo codes are up to 20 letters, numbers, - or _. Press Apply to use one.|Student codes need a student email address.|Tick the loyalty box to use your points as a discount."></button></h3>
                                <div class="co-promo-row">
                                    <asp:TextBox ID="txtPromo" runat="server" CssClass="login-input" MaxLength="20" placeholder="Promo code" />
                                    <asp:Button ID="btnApplyPromo" runat="server" Text="Apply" CssClass="btn-ghost" OnClick="btnApplyPromo_Click" CausesValidation="false" />
                                    <asp:LinkButton ID="btnRemovePromo" runat="server" CssClass="danger-link" OnClick="btnRemovePromo_Click" CausesValidation="false">Remove</asp:LinkButton>
                                </div>
                                <asp:Literal ID="litPromoMsg" runat="server" />

                                <asp:Panel ID="pnlPoints" runat="server" Visible="false" style="margin-top: 20px;">
                                    <asp:CheckBox ID="chkUsePoints" runat="server" CssClass="co-check" AutoPostBack="true" OnCheckedChanged="chkUsePoints_Changed" />
                                    <asp:Literal ID="litPointsNote" runat="server" />
                                </asp:Panel>
                            </div>

                            <asp:Panel ID="pnlCard" runat="server" CssClass="glass co-card">
                                <h3>5. Payment</h3>
                                <div class="co-grid">
                                    <div class="co-field co-full">
                                        <label for="<%= txtCardName.ClientID %>">Name on card <button type="button" class="field-help" data-rules="Required.|Exactly as it is printed on the front of your card.|Letters, spaces, hyphens, apostrophes and full stops only.|2 to 50 characters."></button></label>
                                        <asp:TextBox ID="txtCardName" runat="server" CssClass="login-input" MaxLength="50" EnableViewState="false" autocomplete="cc-name" autocapitalize="characters" spellcheck="false" data-rule="cardname" />
                                    </div>
                                    <div class="co-field co-full">
                                        <label for="<%= txtCardNumber.ClientID %>">Card number <button type="button" class="field-help" data-rules="Required.|Visa, Mastercard, American Express or Diners Club.|Spaces are added for you as you type.|Only the last 4 digits are kept."></button></label>
                                        <asp:TextBox ID="txtCardNumber" runat="server" CssClass="login-input" MaxLength="23" EnableViewState="false" autocomplete="cc-number" inputmode="numeric" data-rule="card" />
                                    </div>
                                    <div class="co-field">
                                        <label for="<%= txtCardExpiry.ClientID %>">Expiry (MM/YY) <button type="button" class="field-help" data-rules="Required.|The month and year printed on your card, e.g. 08/28.|The card must not have expired."></button></label>
                                        <asp:TextBox ID="txtCardExpiry" runat="server" CssClass="login-input" MaxLength="5" EnableViewState="false" autocomplete="cc-exp" placeholder="MM/YY" inputmode="numeric" data-rule="expiry" />
                                    </div>
                                    <div class="co-field">
                                        <label for="<%= txtCardCvv.ClientID %>">Security code <button type="button" class="field-help" data-rules="Required.|The 3 digits on the back of your card.|American Express: the 4 digits on the front.|Never stored."></button></label>
                                        <asp:TextBox ID="txtCardCvv" runat="server" CssClass="login-input" MaxLength="4" EnableViewState="false" TextMode="Password" autocomplete="cc-csc" data-digits="1" inputmode="numeric" data-rule="cvv" />
                                    </div>
                                </div>
                            </asp:Panel>

                            <div class="glass co-card">
                                <asp:CheckBox ID="chkAge" runat="server" CssClass="co-check" Text="I confirm that I am 18 years or older and will show valid ID when my order is delivered or collected." />
                                <p class="co-note" style="margin-left: 36px;">Age confirmation <button type="button" class="field-help" data-title="Age confirmation" data-rules="Required. Tick the box to place your order.|You must be 18 or older to buy alcohol in South Africa.|Our driver or store staff will check your ID before handing over your order."></button></p>
                            </div>
                        </div>

                        <aside class="cart-summary-sidebar">
                            <div class="glass cart-summary-card">
                                <h3>Order Summary</h3>
                                <asp:Literal ID="litSummary" runat="server" />
                                <asp:LinkButton ID="btnPlaceOrder" runat="server" CssClass="btn-primary cart-checkout-btn" OnClick="btnPlaceOrder_Click"
                                    OnClientClick="return coPlaceOrder(this);">Place Order</asp:LinkButton>
                                <asp:LinkButton ID="btnSwitchCollect" runat="server" CssClass="btn-ghost co-switch" OnClick="btnSwitchCollect_Click" CausesValidation="false" Visible="false">Switch to click &amp; collect</asp:LinkButton>
                                <p class="co-note">By placing your order you agree to our <a href="/Legal.aspx#terms">terms &amp; conditions</a>. Not for persons under 18.</p>
                            </div>
                        </aside>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>

        <asp:UpdateProgress ID="updCheckoutProgress" runat="server" AssociatedUpdatePanelID="upnlCheckout">
            <ProgressTemplate>
                <div class="loading-overlay">
                    <div class="loading-spinner"></div>
                    <p>Working&hellip;</p>
                </div>
            </ProgressTemplate>
        </asp:UpdateProgress>

        <%-- Simulated payment portal --%>
        <div id="coPortal" class="co-portal" aria-hidden="true">
            <div class="co-portal-card" role="dialog" aria-modal="true" aria-live="polite">
                <div class="co-portal-brand">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="4" y="11" width="16" height="10" rx="2" /><path d="M8 11V7a4 4 0 0 1 8 0v4" /></svg>
                    SecurePay
                </div>
                <h3 id="coPortalTitle">Processing your payment</h3>
                <p class="co-portal-amount">Paying Sam's Liquor Shop <strong id="coPortalAmt"></strong></p>
                <div class="co-portal-visual">
                    <span class="co-portal-visual-main"><span id="coPortalCard">&bull;&bull;&bull;&bull; &bull;&bull;&bull;&bull; &bull;&bull;&bull;&bull; ----</span><small id="coPortalName" class="co-portal-name"></small></span>
                    <small id="coPortalBrand"></small>
                </div>
                <ul class="co-portal-steps">
                    <li>Connecting to the secure payment portal</li>
                    <li>Encrypting your card details</li>
                    <li>Contacting your bank</li>
                    <li>Authorising the payment</li>
                </ul>
                <div class="co-portal-bar"><span id="coPortalBar"></span></div>
                <p class="co-portal-status" id="coPortalStatus">Please don't close this window.</p>
                <button type="button" id="coPortalClose" class="btn-ghost co-portal-close" style="display:none;">Back to checkout</button>
            </div>
        </div>
    </section>

    <script type="text/javascript">
        (function () {
            var PANEL = '<%= upnlCheckout.ClientID %>';
            var PLACE = '<%= btnPlaceOrder.UniqueID %>';
            var AGE = '<%= chkAge.ClientID %>';
            var F = {
                streetno: '<%= txtStreetNumber.ClientID %>',
                street: '<%= txtStreetName.ClientID %>',
                suburb: '<%= txtSuburb.ClientID %>',
                postal: '<%= txtPostalCode.ClientID %>'
            };

            var NAME_RE;
            try { NAME_RE = new RegExp("^\\p{L}[\\p{L} '.\\-]*$", 'u'); } catch (e) { NAME_RE = /^[A-Za-z][A-Za-z '.\-]*$/; }

            function OK(t) { return { s: 'ok', t: t || '' }; }
            function BAD(t) { return { s: 'bad', t: t }; }
            function PEND(t) { return { s: 'pend', t: t || '' }; }
            function CLR() { return { s: '', t: '' }; }
            function digits(v) { return (v || '').replace(/\D/g, ''); }
            function brand(d) {
                if (/^4/.test(d)) return 'Visa';
                if (/^(5[1-5]|2[2-7])/.test(d)) return 'Mastercard';
                if (/^3[47]/.test(d)) return 'American Express';
                if (/^3(0[0-5]|[68])/.test(d)) return 'Diners Club';
                return '';
            }
            function expectedLen(d) { if (/^3[47]/.test(d)) return 15; if (/^3(0[0-5]|[68])/.test(d)) return 14; return 16; }
            function luhn(d) {
                var sum = 0, alt = false;
                for (var i = d.length - 1; i >= 0; i--) {
                    var n = d.charCodeAt(i) - 48;
                    if (alt) { n *= 2; if (n > 9) n -= 9; }
                    sum += n; alt = !alt;
                }
                return sum % 10 === 0;
            }

            var RULES = {
                name: function (v, f) {
                    if (!v) return f ? BAD('Please enter this name.') : CLR();
                    if (v.length > 50) return BAD('At most 50 characters.');
                    if (!/^[A-Za-z]+([ '-][A-Za-z]+)*$/.test(v)) return BAD('Letters, spaces, hyphens and apostrophes only.');
                    return OK();
                },
                email: function (v, f) {
                    if (!v) return f ? BAD('Please enter your email address.') : CLR();
                    if (v.length > 100) return BAD('At most 100 characters.');
                    if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(v)) return f ? BAD('Enter a valid email address, e.g. name@example.com.') : PEND('');
                    return OK();
                },
                phone: function (v, f) {
                    if (!v) return f ? BAD('Please enter your phone number.') : CLR();
                    var c = v.replace(/[\s\-()]/g, '');
                    if (!/^\+?[0-9]*$/.test(c)) return BAD('Digits only, e.g. 0821234567.');
                    if (!/^\+?[0-9]{9,15}$/.test(c)) return f ? BAD('Enter 9 to 15 digits, e.g. 0821234567.') : PEND('');
                    return OK();
                },
                streetno: function (v, f) {
                    if (!v) return f ? BAD('Please enter your street number.') : CLR();
                    if (!/^[0-9A-Za-z\/\- ]{1,10}$/.test(v)) return BAD('Up to 10 letters, numbers, / or -.');
                    return OK();
                },
                text: function (v, f, el) {
                    if (!v) return f ? BAD('This field is required.') : CLR();
                    var max = parseInt(el.getAttribute('maxlength'), 10) || 100;
                    if (v.length > max) return BAD('At most ' + max + ' characters.');
                    return OK();
                },
                postal: function (v, f) {
                    if (!v) return f ? BAD('Please enter your postal code.') : CLR();
                    if (!/^\d*$/.test(v)) return BAD('Digits only.');
                    if (v.length < 4) return f ? BAD('Postal code must be 4 digits.') : PEND('');
                    return OK();
                },
                slot: function (v, f) {
                    if (!v) return f ? BAD('Please choose a time slot.') : CLR();
                    return OK();
                },
                cardname: function (v, f) {
                    v = v.replace(/\s+/g, ' ');
                    if (!v) return f ? BAD('Please enter the name on the card.') : CLR();
                    if (v.length > 50) return BAD('At most 50 characters.');
                    if (!NAME_RE.test(v)) return BAD('Letters, spaces, hyphens, apostrophes and full stops only.');
                    if (v.length < 2) return f ? BAD('Enter the name as it appears on the card.') : PEND('');
                    return OK();
                },
                card: function (v, f) {
                    var d = digits(v);
                    if (!d) return f ? BAD('Please enter your card number.') : CLR();
                    var b = brand(d), n = expectedLen(d);
                    if (d.length < n) {
                        var left = n - d.length;
                        return f ? BAD('Card number is incomplete (' + left + ' more digit' + (left === 1 ? '' : 's') + ').') : PEND(b ? b : '');
                    }
                    if (!luhn(d)) return BAD("That card number doesn't look right. Please check it.");
                    return OK((b || 'Card') + ' accepted');
                },
                expiry: function (v, f) {
                    if (!v) return f ? BAD('Please enter the expiry date.') : CLR();
                    var m = /^(\d{2})\/(\d{2})$/.exec(v);
                    if (!m) return f ? BAD('Enter the expiry date as MM/YY.') : PEND('MM/YY');
                    var mo = parseInt(m[1], 10), yr = 2000 + parseInt(m[2], 10);
                    if (mo < 1 || mo > 12) return BAD('Month must be 01 to 12.');
                    var now = new Date();
                    if (new Date(yr, mo, 0, 23, 59, 59) < now) return BAD('That card has expired.');
                    if (yr > now.getFullYear() + 20) return BAD('Please check the expiry year.');
                    return OK('Valid until ' + v);
                },
                cvv: function (v, f) {
                    var d = digits(v);
                    var card = document.querySelector('#' + PANEL + ' [data-rule="card"]');
                    var need = card && /^3[47]/.test(digits(card.value)) ? 4 : 3;
                    if (!d) return f ? BAD('Please enter the security code.') : CLR();
                    if (d.length < need) return f ? BAD('Enter the ' + need + '-digit security code.') : PEND('');
                    if (d.length > need) return BAD('The security code is ' + need + ' digits.');
                    return OK();
                }
            };

            function msgFor(el) {
                var n = el.nextElementSibling;
                if (n && n.classList.contains('co-msg')) return n;
                n = document.createElement('span');
                n.className = 'co-msg';
                n.setAttribute('aria-live', 'polite');
                el.parentNode.insertBefore(n, el.nextSibling);
                return n;
            }

            function show(el, r) {
                el.classList.remove('co-bad', 'co-ok');
                if (r.s === 'bad') el.classList.add('co-bad'); else if (r.s === 'ok') el.classList.add('co-ok');
                el.setAttribute('aria-invalid', r.s === 'bad' ? 'true' : 'false');
                var m = msgFor(el);
                m.textContent = r.t;
                m.className = 'co-msg' + (r.s === 'bad' ? ' is-bad' : (r.s === 'ok' ? ' is-ok' : ''));
            }

            // true when the field is acceptable
            function run(el, final) {
                var rule = RULES[el.getAttribute('data-rule')];
                if (!rule) return true;
                var v = el.tagName === 'SELECT' ? el.value : (el.value || '').trim();
                var r = rule(v, final, el);
                show(el, r);
                return r.s !== 'bad';
            }

            // ----- formatting while typing -----
            function fmtCard(el) {
                var pos = el.selectionStart || 0;
                var before = el.value.slice(0, pos).replace(/\D/g, '').length;
                var d = el.value.replace(/\D/g, '').slice(0, 19);
                var out = /^3[47]/.test(d) ? (d.slice(0, 4) + ' ' + d.slice(4, 10) + ' ' + d.slice(10)).trim() : d.replace(/(.{4})/g, '$1 ').trim();
                el.value = out;
                var n = 0, i = 0;
                for (; i < out.length && n < before; i++) { if (/\d/.test(out.charAt(i))) n++; }
                try { el.setSelectionRange(i, i); } catch (e) { }
            }
            function fmtExpiry(el, e) {
                var d = digits(el.value).slice(0, 4);
                var del = e && e.inputType && e.inputType.indexOf('delete') === 0;
                if (d.length === 1 && parseInt(d, 10) > 1 && !del) d = '0' + d;
                el.value = d.length >= 3 ? d.slice(0, 2) + '/' + d.slice(2) : (d.length === 2 && !del ? d + '/' : d);
            }

            document.addEventListener('input', function (e) {
                var el = e.target;
                var rule = el && el.getAttribute && el.getAttribute('data-rule');
                if (!rule) return;
                if (rule === 'card') fmtCard(el);
                else if (rule === 'expiry') fmtExpiry(el, e);
                else if (rule === 'cvv' || rule === 'postal') el.value = digits(el.value);
                run(el, false);
                if (rule === 'card') {
                    var cv = document.querySelector('#' + PANEL + ' [data-rule="cvv"]');
                    if (cv && cv.value) run(cv, false);
                }
            });
            document.addEventListener('focusout', function (e) {
                var el = e.target;
                if (el && el.getAttribute && el.getAttribute('data-rule')) run(el, true);
            });
            document.addEventListener('change', function (e) {
                var el = e.target;
                if (el && el.tagName === 'SELECT' && el.getAttribute('data-rule')) run(el, true);
                if (el && el.id === AGE && el.checked) { var m = ageMsg(); m.textContent = ''; m.className = 'co-msg'; }
            });

            function ageMsg() {
                var box = document.getElementById(AGE).closest('.co-check');
                var n = box.nextElementSibling;
                if (n && n.classList.contains('co-msg')) return n;
                n = document.createElement('span');
                n.className = 'co-msg';
                box.parentNode.insertBefore(n, box.nextSibling);
                return n;
            }

            function validateAll() {
                var first = null, ok = true;
                var els = document.querySelectorAll('#' + PANEL + ' [data-rule]');
                for (var i = 0; i < els.length; i++) {
                    var el = els[i];
                    if (el.disabled || el.offsetParent === null) continue;
                    if (!run(el, true)) { ok = false; if (!first) first = el; }
                }
                var age = document.getElementById(AGE);
                if (age) {
                    var m = ageMsg();
                    if (!age.checked) {
                        m.textContent = 'Please confirm that you are 18 or older.';
                        m.className = 'co-msg is-bad';
                        ok = false;
                        if (!first) first = age;
                    } else { m.textContent = ''; m.className = 'co-msg'; }
                }
                return { ok: ok, first: first };
            }

            function revalidate() {
                var els = document.querySelectorAll('#' + PANEL + ' [data-rule]');
                for (var i = 0; i < els.length; i++) {
                    var v = els[i].tagName === 'SELECT' ? els[i].value : els[i].value;
                    if (v) run(els[i], false);
                }
            }

            // ----- simulated payment portal -----
            function $(id) { return document.getElementById(id); }
            function post() { __doPostBack(PLACE, ''); }

            function closePortal() {
                var box = $('coPortal');
                box.classList.remove('is-open');
                box.setAttribute('aria-hidden', 'true');
                var items = box.querySelectorAll('.co-portal-steps li');
                for (var i = 0; i < items.length; i++) items[i].className = '';
                $('coPortalBar').style.width = '0';
                $('coPortalClose').style.display = 'none';
                $('coPortalTitle').textContent = 'Processing your payment';
                var st = $('coPortalStatus'); st.className = 'co-portal-status'; st.textContent = "Please don't close this window.";
                window.__coBusy = false;
            }

            function runPortal(card, done) {
                var box = $('coPortal');
                var d = digits(card.value);
                var totalEl = document.querySelector('.cart-summary-total-line span:last-child');
                $('coPortalAmt').textContent = totalEl ? totalEl.textContent : '';
                $('coPortalCard').textContent = '\u2022\u2022\u2022\u2022 \u2022\u2022\u2022\u2022 \u2022\u2022\u2022\u2022 ' + d.slice(-4);
                $('coPortalBrand').textContent = brand(d);
                var nameEl = document.querySelector('#' + PANEL + ' [data-rule="cardname"]');
                $('coPortalName').textContent = nameEl ? nameEl.value.trim().replace(/\s+/g, ' ') : '';
                var items = box.querySelectorAll('.co-portal-steps li');
                var bar = $('coPortalBar');
                box.classList.add('is-open');
                box.removeAttribute('aria-hidden');

                var i = 0;
                function next() {
                    if (i > 0) items[i - 1].className = 'is-done';
                    if (i < items.length) {
                        items[i].className = 'is-active';
                        bar.style.width = (((i + 1) / (items.length + 1)) * 100) + '%';
                        i++;
                        setTimeout(next, 800 + Math.random() * 500);
                    } else {
                        bar.style.width = '100%';
                        $('coPortalStatus').textContent = 'Payment authorised. Finalising your order...';
                        setTimeout(done, 600);
                    }
                }
                next();
            }

            function onEnd() {
                window.__coBusy = false;
                var box = $('coPortal');
                if (!box || !box.classList.contains('is-open')) return;
                var alertEl = document.querySelector('#' + PANEL + ' div[role="alert"]');
                if (!alertEl) { closePortal(); return; }
                var items = box.querySelectorAll('.co-portal-steps li');
                for (var i = 0; i < items.length; i++) { if (items[i].className === 'is-active') items[i].className = 'is-failed'; }
                $('coPortalTitle').textContent = "We couldn't complete your order";
                var st = $('coPortalStatus'); st.className = 'co-portal-status is-bad'; st.textContent = alertEl.textContent;
                $('coPortalClose').style.display = 'block';
            }

            $('coPortalClose').addEventListener('click', function () { closePortal(); window.scrollTo({ top: 0, behavior: 'smooth' }); });

            window.coPlaceOrder = function (btn) {
                if (window.__coBusy) return false;
                var v = validateAll();
                if (!v.ok) {
                    try { v.first.scrollIntoView({ behavior: 'smooth', block: 'center' }); v.first.focus(); } catch (e) { }
                    return false;
                }
                window.__coBusy = true;
                var card = document.querySelector('#' + PANEL + ' [data-rule="card"]');
                if (card && card.offsetParent !== null) runPortal(card, post);
                else post();
                return false;
            };

            // ----- address search (same OpenStreetMap lookup as Register, limited to the Durban area) -----
            var addrTimer = null, addrSeq = 0;
            function setVal(id, value) {
                var el = document.getElementById(id);
                if (!el || !value) return;
                el.value = value;
                run(el, false);
            }
            function addrList(input) {
                var list = input.parentNode.querySelector('.co-addr-results');
                if (!list) { list = document.createElement('div'); list.className = 'co-addr-results'; input.parentNode.appendChild(list); }
                return list;
            }
            document.addEventListener('input', function (e) {
                var el = e.target;
                if (!el || el.id !== 'txtAddressSearch') return;
                clearTimeout(addrTimer);
                var list = addrList(el);
                var q = el.value.trim();
                if (q.length < 5) { list.style.display = 'none'; return; }
                addrTimer = setTimeout(function () {
                    var seq = ++addrSeq;
                    fetch('https://nominatim.openstreetmap.org/search?format=json&addressdetails=1&countrycodes=za&limit=5&bounded=1&viewbox=30.60,-29.50,31.30,-30.25&q=' + encodeURIComponent(q))
                        .then(function (r) { return r.json(); })
                        .then(function (results) {
                            if (seq !== addrSeq) return;
                            list.innerHTML = '';
                            if (!results.length) {
                                var none = document.createElement('div');
                                none.className = 'co-addr-empty';
                                none.textContent = 'No addresses found in the Durban area. Check the spelling or type your address below.';
                                list.appendChild(none);
                                list.style.display = 'block';
                                return;
                            }
                            results.forEach(function (place) {
                                var item = document.createElement('div');
                                item.className = 'co-addr-item';
                                item.textContent = place.display_name;
                                item.onclick = function () {
                                    var a = place.address || {};
                                    setVal(F.streetno, a.house_number);
                                    setVal(F.street, a.road);
                                    setVal(F.suburb, a.suburb || a.neighbourhood || a.city_district);
                                    setVal(F.postal, digits(a.postcode || '').slice(0, 4));
                                    var sn = document.getElementById(F.streetno);
                                    if (sn) run(sn, true);
                                    el.value = place.display_name;
                                    list.style.display = 'none';
                                };
                                list.appendChild(item);
                            });
                            list.style.display = 'block';
                        })
                        .catch(function () { list.style.display = 'none'; });
                }, 400);
            });
            document.addEventListener('click', function (e) {
                if (e.target && e.target.id === 'txtAddressSearch') return;
                var lists = document.querySelectorAll('.co-addr-results');
                for (var i = 0; i < lists.length; i++) lists[i].style.display = 'none';
            });

            // ----- "?" field help popovers (same look as Register) -----
            // One shared popover, driven by event delegation, so it keeps working after
            // the UpdatePanel re-renders the form.
            var helpTip = null, helpBtn = null, helpPinned = false;

            function helpName(btn) {
                var t = btn.getAttribute('data-title');
                if (t) return t;
                var holder = btn.closest('label, h3');
                var n = holder && holder.firstChild ? holder.firstChild.textContent.trim() : '';
                return n && n.length <= 40 ? n : 'This field';
            }
            function prepHelp() {
                var btns = document.querySelectorAll('.field-help[data-rules]');
                for (var i = 0; i < btns.length; i++) {
                    var b = btns[i];
                    if (b.getAttribute('data-ready')) continue;
                    b.setAttribute('data-ready', '1');
                    b.textContent = '?';
                    b.setAttribute('aria-label', 'Rules for ' + helpName(b));
                    b.setAttribute('aria-describedby', 'coHelpTip');
                    b.setAttribute('aria-expanded', 'false');
                }
            }
            function placeHelp() {
                if (!helpTip || !helpBtn) return;
                var r = helpBtn.getBoundingClientRect();
                helpTip.style.left = '0px'; helpTip.style.top = '0px';
                var w = helpTip.offsetWidth, h = helpTip.offsetHeight;
                var left = Math.min(Math.max(8, r.left + r.width / 2 - w / 2), window.innerWidth - w - 8);
                var top = r.bottom + 8;
                if (top + h > window.innerHeight - 8) top = Math.max(8, r.top - h - 8);
                helpTip.style.left = left + 'px';
                helpTip.style.top = top + 'px';
            }
            function openHelp(btn) {
                if (helpBtn && helpBtn !== btn) closeHelp();
                if (!helpTip) {
                    helpTip = document.createElement('div');
                    helpTip.className = 'field-help-tip';
                    helpTip.id = 'coHelpTip';
                    helpTip.setAttribute('role', 'tooltip');
                    document.body.appendChild(helpTip);
                }
                helpTip.innerHTML = '';
                var title = document.createElement('div');
                title.className = 'field-help-title';
                title.textContent = helpName(btn);
                helpTip.appendChild(title);
                var list = document.createElement('ul');
                btn.getAttribute('data-rules').split('|').forEach(function (rule) {
                    var li = document.createElement('li');
                    li.textContent = rule;
                    list.appendChild(li);
                });
                helpTip.appendChild(list);
                helpBtn = btn;
                helpTip.classList.add('open');
                placeHelp();
            }
            function closeHelp() {
                if (helpTip) helpTip.classList.remove('open');
                if (helpBtn) helpBtn.setAttribute('aria-expanded', 'false');
                helpBtn = null;
                helpPinned = false;
            }
            function helpTarget(e) {
                return e.target && e.target.closest ? e.target.closest('.field-help[data-rules]') : null;
            }
            document.addEventListener('mouseover', function (e) { var b = helpTarget(e); if (b && !helpPinned) openHelp(b); });
            document.addEventListener('mouseout', function (e) {
                var b = helpTarget(e);
                if (b && b === helpBtn && !helpPinned && !(e.relatedTarget && b.contains(e.relatedTarget))) closeHelp();
            });
            document.addEventListener('focusin', function (e) { var b = helpTarget(e); if (b) openHelp(b); });
            document.addEventListener('focusout', function (e) { if (helpTarget(e)) closeHelp(); });
            document.addEventListener('click', function (e) {
                var b = helpTarget(e);
                if (!b) { if (helpBtn) closeHelp(); return; }
                e.preventDefault();   // don't tick the checkbox / focus the field the label belongs to
                if (helpPinned && helpBtn === b) { closeHelp(); return; }
                openHelp(b);
                helpPinned = true;
                b.setAttribute('aria-expanded', 'true');
            });
            document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && helpBtn) closeHelp(); });
            window.addEventListener('scroll', function () { if (helpBtn) closeHelp(); }, { passive: true });
            window.addEventListener('resize', placeHelp);

            function hook() {
                if (window.__coHooked) return;
                if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(onEnd);
                    window.__coHooked = true;
                }
            }
            // Runs on first load and after every UpdatePanel refresh (the old help buttons are replaced)
            function onLoad() { closeHelp(); prepHelp(); hook(); revalidate(); }
            if (window.Sys && Sys.Application) Sys.Application.add_load(onLoad);
            else document.addEventListener('DOMContentLoaded', onLoad);
        })();
    </script>
</asp:Content>