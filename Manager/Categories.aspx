<%@ Page Title="Manage Categories" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Categories.aspx.cs" Inherits="SamsLiquorShop.Manager.Categories" MaintainScrollPositionOnPostBack="true" %>
<%@ Register Src="~/Manager/ManagerNav.ascx" TagPrefix="uc" TagName="ManagerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:ManagerNav runat="server" />

        <div class="mgr-main">

            <div class="mgr-header-row emp-header-flex">
                <div>
                    <h1>Manage Categories</h1>
                    <p class="mgr-dash-subtext">View the shop's product categories, add new ones, and correct a category's name or description.</p>
                </div>
                <div class="emp-header-actions">
                    <asp:Button ID="btnAddNew" runat="server" Text="+ Add Category" CssClass="btn-primary" OnClick="btnAddNew_Click" CausesValidation="false" />
                </div>
            </div>

            <asp:UpdatePanel ID="upMain" runat="server">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnAddNew" EventName="Click" />
                </Triggers>
                <ContentTemplate>

                    <asp:Literal ID="litMessage" runat="server" />

                    <!-- ADD / EDIT FORM (name and description only) -->
                    <asp:Panel ID="pnlForm" runat="server" Visible="false" DefaultButton="btnSave">
                        <div id="catFormCard" class="mgr-table-card emp-form-card">
                            <div class="dash-card-head">
                                <h3><asp:Literal ID="litFormTitle" runat="server" /></h3>
                                <span class="dash-card-meta"><asp:Literal ID="litFormMeta" runat="server" /></span>
                            </div>
                            <asp:HiddenField ID="hfCategoryId" runat="server" />

                            <div class="emp-form-grid">
                                <div class="login-field emp-form-wide">
                                    <label for="<%= txtName.ClientID %>">Category name</label>
                                    <asp:TextBox ID="txtName" runat="server" CssClass="login-input" />
                                    <span class="field-hint">Shown in the shop menu. Each category needs its own name.</span>
                                </div>
                                <asp:PlaceHolder ID="phDescription" runat="server">
                                    <div class="login-field emp-form-wide">
                                        <label for="<%= txtDescription.ClientID %>">Description</label>
                                        <asp:TextBox ID="txtDescription" runat="server" CssClass="login-input" TextMode="MultiLine" Rows="4" />
                                        <span class="field-hint"><asp:Literal ID="litDescHint" runat="server" /></span>
                                    </div>
                                </asp:PlaceHolder>
                            </div>

                            <div class="emp-form-actions">
                                <asp:Button ID="btnSave" runat="server" Text="Save category" CssClass="btn-primary" OnClick="btnSave_Click" />
                                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-ghost" OnClick="btnCancel_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- CATEGORY PRODUCTS -->
                    <asp:Panel ID="pnlProducts" runat="server" Visible="false">
                        <div id="catProductsCard" class="mgr-table-card emp-form-card cat-products">
                            <div class="dash-card-head">
                                <h3><asp:Literal ID="litProdTitle" runat="server" /></h3>
                                <span class="dash-card-meta">Products in this category</span>
                            </div>
                            <asp:Literal ID="litProdDesc" runat="server" />

                            <div class="disc-readonly">
                                <div><span class="mgr-kpi-label">Products</span><strong><asp:Literal ID="litProdTotal" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Active</span><strong><asp:Literal ID="litProdActive" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Inactive</span><strong><asp:Literal ID="litProdInactive" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Low stock</span><strong><asp:Literal ID="litProdLow" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Out of stock</span><strong><asp:Literal ID="litProdOut" runat="server" /></strong></div>
                            </div>
                            <asp:Literal ID="litProdNote" runat="server" />

                            <asp:PlaceHolder ID="phProdEmpty" runat="server" Visible="false">
                                <p class="dash-empty">There are no products in this category yet.</p>
                            </asp:PlaceHolder>

                            <asp:Repeater ID="rptCatProducts" runat="server" OnItemCommand="rptCatProducts_ItemCommand">
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
                                            <%# SizeBadge(Eval("Product_SizeML")) %>
                                            <div class="shop-card-price-row">
                                                <span runat="server" class="shop-card-strike" visible='<%# (bool)Eval("IsOnSale") %>'><%# Money(Eval("Product_SellingPrice")) %></span>
                                                <span class='<%# (bool)Eval("IsOnSale") ? "shop-card-price is-sale" : "shop-card-price" %>'><%# Money(Eval("DiscountedPrice")) %></span>
                                            </div>
                                            <div class="cat-card-meta">
                                                <%# StockBadge(Eval("Product_QuantityInStock"), Eval("Product_ReorderQuantity")) %>
                                                <%# StatusBadge(Eval("Product_Status")) %>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                                <FooterTemplate></div></FooterTemplate>
                            </asp:Repeater>

                            <div class="emp-form-actions">
                                <asp:Button ID="btnCloseProducts" runat="server" Text="Close" CssClass="btn-ghost" OnClick="btnCloseProducts_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- CATEGORY LIST -->
                    <div class="mgr-table-card">
                        <asp:Panel ID="pnlToolbar" runat="server" DefaultButton="btnSearch">
                            <div class="emp-toolbar">
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="login-input" placeholder="Search by category name or description..." autocomplete="off" MaxLength="100" />
                                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-primary" OnClick="btnSearch_Click" />
                                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-ghost" OnClick="btnClear_Click" />
                            </div>
                        </asp:Panel>

                        <p class="emp-count"><asp:Literal ID="litCount" runat="server" /></p>

                        <asp:GridView ID="gvCategories" runat="server" AutoGenerateColumns="false" CssClass="dash-table emp-table"
                            GridLines="None" DataKeyNames="Category_ID" AllowPaging="true" PageSize="10" AllowSorting="true"
                            EmptyDataText="No categories match your search." EmptyDataRowStyle-CssClass="dash-table-empty"
                            OnPageIndexChanging="gvCategories_PageIndexChanging" OnRowCommand="gvCategories_RowCommand"
                            OnSorting="gvCategories_Sorting">
                            <PagerStyle CssClass="emp-pager" />
                            <Columns>
                                <asp:TemplateField HeaderText="Category" SortExpression="Category_Name">
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" CommandName="ViewProducts" CommandArgument='<%# Eval("Category_ID") %>' CssClass="disc-name-link" CausesValidation="false" ToolTip="View this category's products"><strong><%#: Eval("Category_Name") %></strong></asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Description" ItemStyle-CssClass="is-wrap">
                                    <ItemTemplate><%# DescriptionText(Eval("Category_Description")) %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="ProductCount" HeaderText="Products" SortExpression="ProductCount" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" CommandName="ViewProducts" CommandArgument='<%# Eval("Category_ID") %>' CssClass="emp-action" CausesValidation="false">View products</asp:LinkButton>
                                        <asp:LinkButton runat="server" CommandName="EditCategory" CommandArgument='<%# Eval("Category_ID") %>' CssClass="emp-action" CausesValidation="false">Edit</asp:LinkButton>
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
