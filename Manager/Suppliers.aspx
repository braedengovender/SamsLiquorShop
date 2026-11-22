<%@ Page Title="Manage Suppliers" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Suppliers.aspx.cs" Inherits="SamsLiquorShop.Manager.Suppliers" MaintainScrollPositionOnPostBack="true" %>
<%@ Register Src="~/Manager/ManagerNav.ascx" TagPrefix="uc" TagName="ManagerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:ManagerNav runat="server" />

        <div class="mgr-main">

            <div class="mgr-header-row emp-header-flex">
                <div>
                    <h1>Manage Suppliers</h1>
                    <p class="mgr-dash-subtext">Manage supplier details and see the products each one supplies.</p>
                </div>
                <div class="emp-header-actions" style="flex-shrink:0; white-space:nowrap;">
                    <asp:Button ID="btnAddNew" runat="server" Text="+ Add Supplier" CssClass="btn-primary" OnClick="btnAddNew_Click" CausesValidation="false" />
                </div>
            </div>

            <asp:UpdatePanel ID="upMain" runat="server">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnAddNew" EventName="Click" />
                </Triggers>
                <ContentTemplate>

                    <!-- SUMMARY CARDS -->
                    <div class="mgr-kpi-grid">
                        <div class="mgr-kpi-card">
                            <span class="dash-kpi-icon is-blue dash-icon-receipt" aria-hidden="true"></span>
                            <p class="mgr-kpi-label">Total suppliers</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiTotal" runat="server" /></p>
                        </div>
                        <div class="mgr-kpi-card">
                            <span class="dash-kpi-icon is-blue dash-icon-bag" aria-hidden="true"></span>
                            <p class="mgr-kpi-label">Active</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiActive" runat="server" /></p>
                        </div>
                        <div class="mgr-kpi-card">
                            <span class="dash-kpi-icon is-red dash-icon-alert" aria-hidden="true"></span>
                            <p class="mgr-kpi-label">Inactive</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiInactive" runat="server" /></p>
                        </div>
                        <div class="mgr-kpi-card">
                            <span class="dash-kpi-icon is-amber dash-icon-cash" aria-hidden="true"></span>
                            <p class="mgr-kpi-label">Products supplied</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiProducts" runat="server" /></p>
                        </div>
                    </div>

                    <asp:Literal ID="litSchemaNote" runat="server" />
                    <asp:Literal ID="litMessage" runat="server" />

                    <!-- ADD / EDIT FORM -->
                    <asp:Panel ID="pnlForm" runat="server" Visible="false" DefaultButton="btnSave">
                        <div id="supFormCard" class="mgr-table-card emp-form-card">
                            <div class="dash-card-head">
                                <h3><asp:Literal ID="litFormTitle" runat="server" /></h3>
                                <span class="dash-card-meta"><asp:Literal ID="litFormMeta" runat="server" /></span>
                            </div>
                            <asp:HiddenField ID="hfSupplierId" runat="server" />

                            <h4 class="sup-section-title">Supplier details</h4>
                            <div class="emp-form-grid">
                                <div class="login-field emp-form-wide">
                                    <label for="<%= txtName.ClientID %>">Supplier name</label>
                                    <asp:TextBox ID="txtName" runat="server" CssClass="login-input" placeholder="e.g. Karoo Biltong Co." />
                                </div>
                                <asp:PlaceHolder ID="phContact" runat="server">
                                    <div class="login-field">
                                        <label for="<%= txtContact.ClientID %>">Contact person <span class="register-optional">(optional)</span></label>
                                        <asp:TextBox ID="txtContact" runat="server" CssClass="login-input" autocomplete="off" />
                                    </div>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="phPhone" runat="server">
                                    <div class="login-field">
                                        <label for="<%= txtPhone.ClientID %>">Phone number</label>
                                        <asp:TextBox ID="txtPhone" runat="server" CssClass="login-input" MaxLength="12" data-phone="1" placeholder="e.g. 0311234567" autocomplete="off" />
                                        <span class="field-hint">For example 0311234567 or +27311234567.</span>
                                    </div>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="phEmail" runat="server">
                                    <div class="login-field">
                                        <label for="<%= txtEmail.ClientID %>">Email address</label>
                                        <asp:TextBox ID="txtEmail" runat="server" CssClass="login-input" autocomplete="off" />
                                        <span class="field-hint">Used for purchase orders.</span>
                                    </div>
                                </asp:PlaceHolder>
                            </div>

                            <asp:PlaceHolder ID="phAddressSection" runat="server">
                                <h4 class="sup-section-title">Address</h4>
                                <div class="login-field">
                                    <label for="supAddressSearch">Search the address</label>
                                    <input type="text" id="supAddressSearch" class="login-input" placeholder="Start typing the supplier's address..." autocomplete="off" />
                                    <span class="field-hint">Pick a result to fill in the fields below, then check them.</span>
                                </div>

                                <div class="emp-form-grid">
                                    <asp:PlaceHolder ID="phAddressLine" runat="server" Visible="false">
                                        <div class="login-field emp-form-wide">
                                            <label for="supAddressLine">Address</label>
                                            <asp:TextBox ID="supAddressLine" runat="server" ClientIDMode="Static" CssClass="login-input" autocomplete="off" />
                                        </div>
                                    </asp:PlaceHolder>
                                    <asp:PlaceHolder ID="phUnitNumber" runat="server">
                                        <div class="login-field">
                                            <label for="supUnitNumber">Unit number <span class="register-optional">(optional)</span></label>
                                            <asp:TextBox ID="supUnitNumber" runat="server" ClientIDMode="Static" CssClass="login-input" autocomplete="off" />
                                        </div>
                                    </asp:PlaceHolder>
                                    <asp:PlaceHolder ID="phUnitName" runat="server">
                                        <div class="login-field">
                                            <label for="supUnitName">Unit / complex / building <span class="register-optional">(optional)</span></label>
                                            <asp:TextBox ID="supUnitName" runat="server" ClientIDMode="Static" CssClass="login-input" autocomplete="off" />
                                        </div>
                                    </asp:PlaceHolder>
                                    <asp:PlaceHolder ID="phStreetNumber" runat="server">
                                        <div class="login-field">
                                            <label for="supStreetNumber">Street number</label>
                                            <asp:TextBox ID="supStreetNumber" runat="server" ClientIDMode="Static" CssClass="login-input" autocomplete="off" />
                                        </div>
                                    </asp:PlaceHolder>
                                    <asp:PlaceHolder ID="phStreetName" runat="server">
                                        <div class="login-field">
                                            <label for="supStreetName">Street name</label>
                                            <asp:TextBox ID="supStreetName" runat="server" ClientIDMode="Static" CssClass="login-input" autocomplete="off" />
                                        </div>
                                    </asp:PlaceHolder>
                                    <asp:PlaceHolder ID="phSuburb" runat="server">
                                        <div class="login-field">
                                            <label for="supSuburb">Suburb</label>
                                            <asp:TextBox ID="supSuburb" runat="server" ClientIDMode="Static" CssClass="login-input" autocomplete="off" />
                                        </div>
                                    </asp:PlaceHolder>
                                    <asp:PlaceHolder ID="phCity" runat="server">
                                        <div class="login-field">
                                            <label for="supCity">City</label>
                                            <asp:TextBox ID="supCity" runat="server" ClientIDMode="Static" CssClass="login-input" autocomplete="off" />
                                        </div>
                                    </asp:PlaceHolder>
                                    <asp:PlaceHolder ID="phPostalCode" runat="server">
                                        <div class="login-field">
                                            <label for="supPostalCode">Postal code</label>
                                            <asp:TextBox ID="supPostalCode" runat="server" ClientIDMode="Static" CssClass="login-input" MaxLength="4" data-digits="1" autocomplete="off" />
                                        </div>
                                    </asp:PlaceHolder>
                                    <asp:PlaceHolder ID="phProvince" runat="server">
                                        <div class="login-field">
                                            <label for="supProvince">Province</label>
                                            <asp:DropDownList ID="supProvince" runat="server" ClientIDMode="Static" CssClass="login-input">
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
                                    </asp:PlaceHolder>
                                </div>
                                <p class="register-static-field">Country: South Africa</p>
                            </asp:PlaceHolder>

                            <div class="emp-form-actions">
                                <asp:Button ID="btnSave" runat="server" Text="Save supplier" CssClass="btn-primary" OnClick="btnSave_Click" />
                                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-ghost" OnClick="btnCancel_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- PRODUCTS FROM THIS SUPPLIER -->
                    <asp:Panel ID="pnlProducts" runat="server" Visible="false">
                        <div id="supProductsCard" class="mgr-table-card emp-form-card cat-products">
                            <div class="dash-card-head">
                                <h3>Products from <asp:Literal ID="litProdTitle" runat="server" /></h3>
                                <asp:LinkButton ID="btnCloseProducts" runat="server" CssClass="emp-action" OnClick="btnCloseProducts_Click" CausesValidation="false">Close</asp:LinkButton>
                            </div>

                            <asp:Literal ID="Literal1" runat="server" />

                            <asp:Literal ID="litProdContactCards" runat="server" />

                            <div class="cust-history-stats">
                                <div><span class="mgr-kpi-label">Products</span><strong><asp:Literal ID="litProdCount" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Low or out of stock</span><strong><asp:Literal ID="litProdLow" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Stock value (cost)</span><strong><asp:Literal ID="litProdValue" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Status</span><strong><asp:Literal ID="litProdStatus" runat="server" /></strong></div>
                            </div>

                            <asp:Literal ID="litProdAddress" runat="server" />

                            <asp:PlaceHolder ID="phProdEmpty" runat="server" Visible="false">
                                <p class="dash-empty">No products are linked to this supplier yet. Choose this supplier on a product in Manage Products.</p>
                            </asp:PlaceHolder>

                            <asp:Repeater ID="rptSupProducts" runat="server" OnItemCommand="rptSupProducts_ItemCommand">
                                <HeaderTemplate><div class="shop-grid"></HeaderTemplate>
                                <ItemTemplate>
                                    <div class='<%# IsActiveProduct(Eval("Product_Status")) ? "shop-card" : "shop-card is-inactive" %>'>
                                        <div class="shop-card-image-wrap">
                                            <asp:LinkButton runat="server" CommandName="ViewProduct" CommandArgument='<%# Eval("Product_ID") %>' CssClass="shop-card-image-link" ToolTip="View details" CausesValidation="false">
                                                <img src='<%# "/ProductImage.ashx?id=" + Eval("Product_ID") %>' alt='<%#: Eval("Product_Name") %>' class="shop-card-image" loading="lazy" onerror="this.style.visibility='hidden'" />
                                            </asp:LinkButton>
                                            <span runat="server" class="shop-sale-badge" visible='<%# (bool)Eval("IsOnSale") %>'>-<%# Eval("DiscountPercent") %>%</span>
                                        </div>
                                        <div class="shop-card-body">
                                            <p class="shop-card-brand"><%#: Eval("Product_Brand") %></p>
                                            <asp:LinkButton runat="server" CommandName="ViewProduct" CommandArgument='<%# Eval("Product_ID") %>' CssClass="shop-card-name-link" CausesValidation="false"><%#: Eval("Product_Name") %></asp:LinkButton>
                                            <%# CardSub(Eval("Product_SizeML"), Eval("Category_Name")) %>
                                            <div class="shop-card-price-row">
                                                <span runat="server" class="shop-card-strike" visible='<%# (bool)Eval("IsOnSale") %>'><%# Money(Eval("Product_SellingPrice")) %></span>
                                                <span class='<%# (bool)Eval("IsOnSale") ? "shop-card-price is-sale" : "shop-card-price" %>'><%# Money(Eval("DiscountedPrice")) %></span>
                                            </div>
                                            <span class="emp-sub">Cost price <%# Money(Eval("Product_CostPrice")) %></span>
                                            <div class="cat-card-meta">
                                                <%# StockBadge(Eval("Product_QuantityInStock"), Eval("Product_ReorderQuantity")) %>
                                                <%# StatusBadge(Eval("Product_Status")) %>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                                <FooterTemplate></div></FooterTemplate>
                            </asp:Repeater>
                        </div>
                    </asp:Panel>

                    <!-- SUPPLIER LIST -->
                    <div class="mgr-table-card">
                        <asp:Panel ID="pnlToolbar" runat="server" DefaultButton="btnSearch">
                            <div class="emp-toolbar">
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="login-input" placeholder="Search by name, contact person, phone, email or city..." autocomplete="off" />
                                <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                    <asp:ListItem Text="All statuses" Value="" />
                                    <asp:ListItem Text="Active" Value="active" />
                                    <asp:ListItem Text="Inactive" Value="inactive" />
                                </asp:DropDownList>
                                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-primary" OnClick="btnSearch_Click" />
                                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-ghost" OnClick="btnClear_Click" />
                            </div>
                        </asp:Panel>

                        <p class="emp-count"><asp:Literal ID="litCount" runat="server" /></p>

                        <asp:GridView ID="gvSuppliers" runat="server" AutoGenerateColumns="false" CssClass="dash-table emp-table"
                            GridLines="None" DataKeyNames="Supplier_ID" AllowPaging="true" PageSize="10" AllowSorting="true"
                            EmptyDataText="No suppliers match your search." EmptyDataRowStyle-CssClass="dash-table-empty"
                            OnPageIndexChanging="gvSuppliers_PageIndexChanging" OnRowCommand="gvSuppliers_RowCommand"
                            OnSorting="gvSuppliers_Sorting" OnRowDataBound="gvSuppliers_RowDataBound">
                            <PagerStyle CssClass="emp-pager" />
                            <Columns>
                                <asp:TemplateField HeaderText="Supplier" SortExpression="Supplier_Name" ItemStyle-CssClass="is-wrap">
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" CommandName="ViewProducts" CommandArgument='<%# Eval("Supplier_ID") %>' CssClass="disc-name-link" CausesValidation="false" ToolTip="See the products from this supplier">
                                            <strong><%#: Eval("Supplier_Name") %></strong>
                                        </asp:LinkButton>
                                        <%# SubText(Eval("F_ContactPerson")) %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Contact" ItemStyle-CssClass="is-wrap">
                                    <ItemTemplate>
                                        <%# ContactLinks(Eval("F_Phone"), Eval("F_Email")) %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Location" ItemStyle-CssClass="is-wrap">
                                    <ItemTemplate><%#: LocationText(Eval("F_City"), Eval("F_Province"), Eval("F_Address")) %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="ProductCount" HeaderText="Products" SortExpression="ProductCount" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                <asp:TemplateField HeaderText="Status" SortExpression="StatusSort">
                                    <ItemTemplate>
                                        <span class='emp-badge emp-badge-<%# StatusClass(Eval("F_Status")) %>'><%# StatusClass(Eval("F_Status")) == "active" ? "Active" : "Inactive" %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" CommandName="ViewProducts" CommandArgument='<%# Eval("Supplier_ID") %>' CssClass="emp-action" CausesValidation="false">Products</asp:LinkButton>
                                        <asp:LinkButton runat="server" CommandName="EditSupplier" CommandArgument='<%# Eval("Supplier_ID") %>' CssClass="emp-action" CausesValidation="false">Edit</asp:LinkButton>
                                        <asp:LinkButton runat="server" CommandName="ToggleStatus" CommandArgument='<%# Eval("Supplier_ID") %>' CssClass="emp-action" CausesValidation="false"
                                            Visible='<%# HasStatusColumn %>'
                                            Text='<%# StatusClass(Eval("F_Status")) == "active" ? "Deactivate" : "Reactivate" %>'
                                            OnClientClick='<%# DeactivateJs(Eval("F_Status"), Eval("Supplier_Name"), Eval("ActiveProductCount")) %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                <!-- PRODUCT DETAIL OVERLAY -->
                    <asp:Panel ID="pnlDetail" runat="server" Visible="false" CssClass="pd-overlay">
                        <div class="pd-card" role="dialog" aria-modal="true" aria-label="Product details">
                            <asp:LinkButton ID="btnCloseDetail" runat="server" CssClass="pd-close" OnClick="btnCloseDetail_Click" aria-label="Close" Text="&times;" CausesValidation="false" />
                            <div class="pd-image-wrap">
                                <img id="imgDetail" runat="server" class="pd-image" alt="" />
                                <span id="spnDetailBadge" runat="server" class="shop-sale-badge" visible="false"></span>
                            </div>
                            <div class="pd-body">
                                <p class="pd-brand"><asp:Literal ID="litDetailBrand" runat="server" Mode="Encode" /></p>
                                <h2 class="pd-name"><asp:Literal ID="litDetailName" runat="server" Mode="Encode" /></h2>
                                <div><span class="pd-category"><asp:Literal ID="litDetailCategory" runat="server" Mode="Encode" /></span></div>
                                <div class="shop-card-price-row pd-price-row">
                                    <span id="spnDetailOld" runat="server" class="shop-card-strike" visible="false"></span>
                                    <span id="spnDetailPrice" runat="server" class="shop-card-price"></span>
                                </div>
                                <p class="pd-desc"><asp:Literal ID="litDetailDesc" runat="server" Mode="Encode" /></p>
                                <asp:Literal ID="litDetailFacts" runat="server" />
                                <asp:Literal ID="litDetailIngredients" runat="server" />
                            </div>
                        </div>
                    </asp:Panel>

                </ContentTemplate>
            </asp:UpdatePanel>

        </div>
    </section>

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

    <script>
        // Address autofill - the same OpenStreetMap (Nominatim) lookup used on the Register page.
        // The form sits in an UpdatePanel, so this re-attaches after every partial postback.
        (function () {
            function setFieldValue(id, value) {
                var el = document.getElementById(id);
                if (el && value) el.value = value;
            }

            function setProvince(provinceName) {
                var select = document.getElementById('supProvince');
                if (!select || !provinceName) return;
                for (var i = 0; i < select.options.length; i++) {
                    if (select.options[i].value === provinceName || select.options[i].text === provinceName) {
                        select.selectedIndex = i;
                        break;
                    }
                }
            }

            function initSupplierAddressAutocomplete() {
                var input = document.getElementById('supAddressSearch');
                if (!input || input.getAttribute('data-ac-ready')) return;
                input.setAttribute('data-ac-ready', '1');

                var list = document.createElement('div');
                list.id = 'supAddressResults';
                list.style.cssText = 'position:relative;background:#fff;border:1px solid var(--glass-border);border-radius:14px;margin-top:4px;max-height:220px;overflow-y:auto;display:none;z-index:50;';
                input.parentNode.appendChild(list);

                var debounceTimer;
                input.addEventListener('keydown', function (e) {
                    // Enter in the search box shouldn't submit the form
                    if (e.key === 'Enter') e.preventDefault();
                });
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
                                    item.style.cssText = 'padding:10px 14px;cursor:pointer;font-size:0.88rem;color:#1d1d1f;';
                                    item.onmouseover = function () { item.style.background = 'var(--bg-tint-blue)'; };
                                    item.onmouseout = function () { item.style.background = ''; };
                                    item.onclick = function () {
                                        var a = place.address || {};
                                        setFieldValue('supStreetNumber', a.house_number);
                                        setFieldValue('supStreetName', a.road);
                                        setFieldValue('supSuburb', a.suburb || a.neighbourhood);
                                        setFieldValue('supCity', a.city || a.town || a.village);
                                        setFieldValue('supPostalCode', a.postcode);
                                        setProvince(a.state);
                                        setFieldValue('supAddressLine', place.display_name);
                                        input.value = place.display_name;
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
                    if (e.target !== input) list.style.display = 'none';
                });
            }

            function hook() {
                if (window.Sys && Sys.Application) Sys.Application.add_load(initSupplierAddressAutocomplete);
                else initSupplierAddressAutocomplete();
            }
            if (window.Sys && Sys.Application) hook();
            else document.addEventListener('DOMContentLoaded', hook);
        })();
    </script>
    <script type="text/javascript">
        (function () {
            function lockScroll() {
                document.body.style.overflow = document.querySelector('.pd-overlay') ? 'hidden' : '';
            }
            function closeOverlay() {
                var c = document.querySelector('.pd-overlay .pd-close');
                if (c) c.click();
            }
            document.addEventListener('keydown', function (e) { if (e.key === 'Escape') closeOverlay(); });
            document.addEventListener('click', function (e) {
                if (e.target && e.target.classList && e.target.classList.contains('pd-overlay')) closeOverlay();
            });
            // ----- Scroll control (called from the server after a partial postback) -----
            var saved = null;

            function scrollers() {
                var list = [document.scrollingElement || document.documentElement, document.documentElement, document.body];
                var el = document.querySelector('.mgr-main');
                while (el && el !== document.body && el !== document.documentElement) { list.push(el); el = el.parentElement; }
                return list;
            }
            function snapshot() {
                var s = { y: window.pageYOffset || 0, nodes: [] };
                scrollers().forEach(function (n) { if (n) s.nodes.push({ n: n, t: n.scrollTop }); });
                return s;
            }
            function moveTo(snap) {
                try {
                    window.scrollTo(0, snap ? snap.y : 0);
                    (snap ? snap.nodes : scrollers().map(function (n) { return { n: n, t: 0 }; }))
                        .forEach(function (i) { if (i.n) i.n.scrollTop = i.t; });
                } catch (ex) { }
            }
            function repeat(fn) {
                fn();
                try { window.requestAnimationFrame(fn); } catch (ex) { }
                setTimeout(fn, 120); // beats any scroll restore done after the partial update
            }
            window.dashScroll = function (mode) {
                try {
                    if (mode === 'top') repeat(function () { moveTo(null); });
                    else if (mode === 'keep' && saved) { var s = saved; repeat(function () { moveTo(s); }); }
                } catch (ex) { }
            };

            document.addEventListener('DOMContentLoaded', function () {
                lockScroll();
                if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                    var prm = Sys.WebForms.PageRequestManager.getInstance();
                    prm.add_beginRequest(function () { try { saved = snapshot(); } catch (ex) { saved = null; } });
                    prm.add_endRequest(lockScroll);
                }
            });
        })();
    </script>
</asp:Content>
