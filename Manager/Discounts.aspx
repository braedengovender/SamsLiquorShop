<%@ Page Title="Manage Discounts" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Discounts.aspx.cs" Inherits="SamsLiquorShop.Manager.Discounts" %>
<%@ Register Src="~/Manager/ManagerNav.ascx" TagPrefix="uc" TagName="ManagerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .disc-picker { max-height: 280px; overflow-y: auto; border: 1px solid rgba(127,127,127,.25); border-radius: 10px; padding: 8px 12px; }
        .disc-picker table { width: 100%; }
        .disc-picker td { padding: 4px 0; }
        .disc-picker label { margin-left: 6px; cursor: pointer; }
        .disc-apply-list label { margin: 0 18px 0 6px; cursor: pointer; }
        .disc-name-link { font-weight: 600; text-decoration: underline; text-underline-offset: 3px; }
        .disc-strike { text-decoration: line-through; opacity: .6; margin-right: 6px; }
    </style>

    <section class="shell mgr-layout dash-glass">
        <uc:ManagerNav runat="server" />

        <div class="mgr-main">

            <div class="mgr-header-row emp-header-flex">
                <div>
                    <h1>Manage Discounts</h1>
                    <p class="mgr-dash-subtext">View discounts and the products they apply to, add new discounts, change end dates, and switch discounts on or off.</p>
                </div>
                <div class="emp-header-actions">
                    <asp:Button ID="btnAddNew" runat="server" Text="+ Add Discount" CssClass="btn-primary" OnClick="btnAddNew_Click" CausesValidation="false" />
                </div>
            </div>

            <asp:UpdatePanel ID="upMain" runat="server">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnAddNew" EventName="Click" />
                </Triggers>
                <ContentTemplate>

                    <asp:Literal ID="litMessage" runat="server" />

                    <!-- ADD FORM -->
                    <asp:Panel ID="pnlAdd" runat="server" Visible="false" DefaultButton="btnSaveNew">
                        <div id="discAddCard" class="mgr-table-card emp-form-card">
                            <div class="dash-card-head">
                                <h3>Add discount</h3>
                                <span class="dash-card-meta">After saving, only the end date and the status can be changed.</span>
                            </div>

                            <div class="emp-form-grid">
                                <div class="login-field emp-form-wide">
                                    <label for="<%= txtName.ClientID %>">Discount name</label>
                                    <asp:TextBox ID="txtName" runat="server" CssClass="login-input" />
                                    <span class="field-hint">For example "Johnnie Walker Special" or "Wine Wednesday". Names containing "Wednesday" only run on Wednesdays.</span>
                                </div>
                                <div class="login-field">
                                    <label for="<%= ddlType.ClientID %>">Discount type</label>
                                    <asp:DropDownList ID="ddlType" runat="server" CssClass="login-input" onchange="discValueHint()">
                                        <asp:ListItem Text="Percentage off (%)" Value="Percentage" />
                                        <asp:ListItem Text="Fixed amount off (R)" Value="Fixed" />
                                    </asp:DropDownList>
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtValue.ClientID %>">Value</label>
                                    <asp:TextBox ID="txtValue" runat="server" CssClass="login-input" MaxLength="10" data-decimal="1" />
                                    <span class="field-hint" id="discValueHint">Between 1 and 100 percent.</span>
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtStart.ClientID %>">Start date</label>
                                    <asp:TextBox ID="txtStart" runat="server" CssClass="login-input" TextMode="Date" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtEnd.ClientID %>">End date</label>
                                    <asp:TextBox ID="txtEnd" runat="server" CssClass="login-input" TextMode="Date" />
                                    <span class="field-hint">The discount runs until the end of this day.</span>
                                </div>
                                <asp:PlaceHolder ID="phDescription" runat="server">
                                    <div class="login-field emp-form-wide">
                                        <label for="<%= txtDescription.ClientID %>">Description</label>
                                        <asp:TextBox ID="txtDescription" runat="server" CssClass="login-input" TextMode="MultiLine" Rows="3" />
                                        <span class="field-hint">Optional.</span>
                                    </div>
                                </asp:PlaceHolder>

                                <div class="login-field emp-form-wide">
                                    <label>Apply this discount to</label>
                                    <asp:RadioButtonList ID="rblApplyTo" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow"
                                        CssClass="disc-apply-list" AutoPostBack="true" OnSelectedIndexChanged="rblApplyTo_Changed">
                                        <asp:ListItem Text="Specific products" Value="products" Selected="True" />
                                        <asp:ListItem Text="A whole category" Value="category" />
                                        <asp:ListItem Text="Nothing for now" Value="none" />
                                    </asp:RadioButtonList>
                                </div>

                                <asp:PlaceHolder ID="phApplyCategory" runat="server" Visible="false">
                                    <div class="login-field emp-form-wide">
                                        <label for="<%= ddlApplyCategory.ClientID %>">Category</label>
                                        <asp:DropDownList ID="ddlApplyCategory" runat="server" CssClass="login-input" />
                                        <span class="field-hint">Every product in this category gets the discount, unless a product has its own discount running. If the category already has a discount, this one replaces it.</span>
                                    </div>
                                </asp:PlaceHolder>

                                <asp:PlaceHolder ID="phApplyProducts" runat="server">
                                    <div class="login-field emp-form-wide">
                                        <label for="discProductFilter">Products</label>
                                        <input type="text" id="discProductFilter" class="login-input" placeholder="Type to filter, e.g. Johnnie Walker..." autocomplete="off" oninput="discFilterProducts()" />
                                        <div class="disc-picker" id="discPicker">
                                            <asp:CheckBoxList ID="cblProducts" runat="server" RepeatLayout="Table" />
                                        </div>
                                        <span class="field-hint">Tick each product the discount applies to. A product that already has a discount switches to this one.</span>
                                    </div>
                                </asp:PlaceHolder>
                            </div>

                            <div class="emp-form-actions">
                                <asp:Button ID="btnSaveNew" runat="server" Text="Add discount" CssClass="btn-primary" OnClick="btnSaveNew_Click" />
                                <asp:Button ID="btnCancelNew" runat="server" Text="Cancel" CssClass="btn-ghost" OnClick="btnCancel_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- EDIT FORM: END DATE ONLY -->
                    <asp:Panel ID="pnlEdit" runat="server" Visible="false" DefaultButton="btnSaveEnd">
                        <div id="discEditCard" class="mgr-table-card emp-form-card">
                            <div class="dash-card-head">
                                <h3>Change end date: <asp:Literal ID="litEditName" runat="server" /></h3>
                                <span class="dash-card-meta">Only the end date can be changed.</span>
                            </div>
                            <asp:HiddenField ID="hfEditId" runat="server" />

                            <div class="emp-form-grid">
                                <div class="login-field">
                                    <label>Discount</label>
                                    <div class="emp-static-field"><asp:Literal ID="litEditValue" runat="server" /></div>
                                </div>
                                <div class="login-field">
                                    <label>Start date</label>
                                    <div class="emp-static-field"><asp:Literal ID="litEditStart" runat="server" /></div>
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtEditEnd.ClientID %>">End date</label>
                                    <asp:TextBox ID="txtEditEnd" runat="server" CssClass="login-input" TextMode="Date" />
                                    <span class="field-hint">The discount runs until the end of this day.</span>
                                </div>
                            </div>

                            <div class="emp-form-actions">
                                <asp:Button ID="btnSaveEnd" runat="server" Text="Save end date" CssClass="btn-primary" OnClick="btnSaveEnd_Click" />
                                <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" CssClass="btn-ghost" OnClick="btnCancel_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- PRODUCTS IN A DISCOUNT -->
                    <asp:Panel ID="pnlProducts" runat="server" Visible="false">
                        <div id="discProductsCard" class="mgr-table-card emp-form-card">
                            <div class="dash-card-head">
                                <h3>Products in: <asp:Literal ID="litProductsName" runat="server" /></h3>
                                <asp:LinkButton ID="btnCloseProducts" runat="server" CssClass="emp-action" OnClick="btnCloseProducts_Click" CausesValidation="false">Close</asp:LinkButton>
                            </div>
                            <p class="field-hint"><asp:Literal ID="litProductsSummary" runat="server" /></p>

                            <asp:GridView ID="gvDiscountProducts" runat="server" AutoGenerateColumns="false" CssClass="dash-table emp-table" GridLines="None"
                                EmptyDataText="No products use this discount yet. To add one, open the product in Manage Products and choose this discount."
                                EmptyDataRowStyle-CssClass="dash-table-empty">
                                <Columns>
                                    <asp:TemplateField HeaderText="Product" ItemStyle-CssClass="is-wrap">
                                        <ItemTemplate>
                                            <div class="prod-cell">
                                                <img class="prod-thumb" src='<%# ImageUrl(Eval("Product_ID")) %>' alt="" loading="lazy" />
                                                <div>
                                                    <strong><%#: Eval("Product_Name") %></strong>
                                                    <span class="emp-sub"><%# SubLine(Eval("Product_Brand"), Eval("Product_SizeML")) %></span>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Category_Name" HeaderText="Category" />
                                    <asp:TemplateField HeaderText="Applied through">
                                        <ItemTemplate><%# AppliedText(Eval("AppliedVia"), Eval("OverrideName")) %></ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Price" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num">
                                        <ItemTemplate><%# PriceText(Eval("Product_SellingPrice"), Eval("OverrideName")) %></ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Product status">
                                        <ItemTemplate>
                                            <span class='emp-badge emp-badge-<%# ProductStatusClass(Eval("Product_Status")) %>'><%#: Eval("Product_Status") %></span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </asp:Panel>

                    <!-- DISCOUNT LIST -->
                    <div class="mgr-table-card">
                        <asp:Panel ID="pnlToolbar" runat="server" DefaultButton="btnSearch">
                            <div class="emp-toolbar">
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="login-input" placeholder="Search by discount name..." autocomplete="off" />
                                <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                    <asp:ListItem Text="All discounts" Value="" />
                                    <asp:ListItem Text="Active" Value="active" />
                                    <asp:ListItem Text="Inactive" Value="inactive" />
                                    <asp:ListItem Text="Running now" Value="running" />
                                    <asp:ListItem Text="Starting later" Value="scheduled" />
                                    <asp:ListItem Text="Ended" Value="ended" />
                                </asp:DropDownList>
                                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-primary" OnClick="btnSearch_Click" />
                                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-ghost" OnClick="btnClear_Click" />
                            </div>
                        </asp:Panel>

                        <p class="emp-count"><asp:Literal ID="litCount" runat="server" /></p>

                        <asp:GridView ID="gvDiscounts" runat="server" AutoGenerateColumns="false" CssClass="dash-table emp-table"
                            GridLines="None" DataKeyNames="Discount_ID" AllowPaging="true" PageSize="10" AllowSorting="true"
                            EmptyDataText="No discounts match your search." EmptyDataRowStyle-CssClass="dash-table-empty"
                            OnPageIndexChanging="gvDiscounts_PageIndexChanging" OnRowCommand="gvDiscounts_RowCommand"
                            OnSorting="gvDiscounts_Sorting">
                            <PagerStyle CssClass="emp-pager" />
                            <Columns>
                                <asp:TemplateField HeaderText="Discount" SortExpression="Discount_Name" ItemStyle-CssClass="is-wrap">
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" CommandName="ViewProducts" CommandArgument='<%# Eval("Discount_ID") %>'
                                            CssClass="disc-name-link" CausesValidation="false" ToolTip="Show the products in this discount"
                                            Text='<%#: Eval("Discount_Name") %>' />
                                        <span class="emp-sub"><%# AppliesToText(Eval("CategoryNames"), Eval("ProductCount")) %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Value" SortExpression="Discount_Value">
                                    <ItemTemplate><%# ValueText(Eval("Discount_Type"), Eval("Discount_Value")) %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Discount_StartDate" HeaderText="Starts" SortExpression="Discount_StartDate" DataFormatString="{0:d MMM yyyy}" />
                                <asp:BoundField DataField="Discount_EndDate" HeaderText="Ends" SortExpression="Discount_EndDate" DataFormatString="{0:d MMM yyyy}" />
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <span class='emp-badge emp-badge-<%# IsActive(Eval("Discount_Status")) ? "active" : "inactive" %>'><%# IsActive(Eval("Discount_Status")) ? "Active" : "Inactive" %></span>
                                        <span class="emp-sub"><%# RunState(Eval("Discount_Status"), Eval("Discount_Name"), Eval("Discount_StartDate"), Eval("Discount_EndDate")) %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" CommandName="ViewProducts" CommandArgument='<%# Eval("Discount_ID") %>' CssClass="emp-action" CausesValidation="false">Products</asp:LinkButton>
                                        <asp:LinkButton runat="server" CommandName="EditEnd" CommandArgument='<%# Eval("Discount_ID") %>' CssClass="emp-action" CausesValidation="false">Edit end date</asp:LinkButton>
                                        <asp:LinkButton runat="server" CommandName="ToggleStatus" CommandArgument='<%# Eval("Discount_ID") %>' CssClass="emp-action" CausesValidation="false"
                                            Text='<%# IsActive(Eval("Discount_Status")) ? "Deactivate" : "Reactivate" %>'
                                            OnClientClick='<%# DeactivateJs(Eval("Discount_Status"), Eval("Discount_Name")) %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

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
        // Only digits and one decimal separator in the value box
        document.addEventListener('input', function (e) {
            var t = e.target;
            if (t && t.getAttribute && t.getAttribute('data-decimal')) {
                t.value = t.value.replace(/[^0-9.,]/g, '');
            }
        });

        // Changes the hint under Value to match the discount type
        function discValueHint() {
            var type = document.getElementById('<%= ddlType.ClientID %>');
            var hint = document.getElementById('discValueHint');
            if (!type || !hint) return;
            hint.textContent = type.value === 'Percentage'
                ? 'Between 1 and 100 percent.'
                : 'Rand amount taken off each item, for example 20.';
        }

        // Hides products that don't match what the manager types (ticked products stay visible)
        function discFilterProducts() {
            var box = document.getElementById('discProductFilter');
            var picker = document.getElementById('discPicker');
            if (!box || !picker) return;
            var term = box.value.trim().toLowerCase();
            var rows = picker.getElementsByTagName('tr');
            for (var i = 0; i < rows.length; i++) {
                var cb = rows[i].querySelector('input[type=checkbox]');
                var text = rows[i].textContent.toLowerCase();
                rows[i].style.display = (term === '' || text.indexOf(term) >= 0 || (cb && cb.checked)) ? '' : 'none';
            }
        }

        function discRefresh() {
            discValueHint();
            discFilterProducts();
        }

        if (window.Sys && Sys.Application) {
            Sys.Application.add_load(discRefresh);
        } else {
            document.addEventListener('DOMContentLoaded', discRefresh);
        }
    </script>
</asp:Content>
