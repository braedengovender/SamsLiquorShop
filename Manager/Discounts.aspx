<%@ Page Title="Manage Discounts" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Discounts.aspx.cs" Inherits="SamsLiquorShop.Manager.Discounts" %>
<%@ Register Src="~/Manager/ManagerNav.ascx" TagPrefix="uc" TagName="ManagerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:ManagerNav runat="server" />

        <div class="mgr-main">

            <div class="mgr-header-row emp-header-flex">
                <div>
                    <h1>Manage Discounts</h1>
                    <p class="mgr-dash-subtext">See every promotion and promo code, add new ones, extend or shorten an end date, and switch discounts off. Click a discount to see the products it applies to.</p>
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

                    <!-- SUMMARY CARDS -->
                    <div class="mgr-kpi-grid">
                        <div class="mgr-kpi-card">
                            <span class="dash-kpi-icon is-blue dash-icon-receipt" aria-hidden="true"></span>
                            <p class="mgr-kpi-label">Total discounts</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiTotal" runat="server" /></p>
                        </div>
                        <div class="mgr-kpi-card">
                            <span class="dash-kpi-icon is-blue dash-icon-bag" aria-hidden="true"></span>
                            <p class="mgr-kpi-label">Running now</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiRunning" runat="server" /></p>
                        </div>
                        <div class="mgr-kpi-card">
                            <span class="dash-kpi-icon is-amber dash-icon-cash" aria-hidden="true"></span>
                            <p class="mgr-kpi-label">Scheduled</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiScheduled" runat="server" /></p>
                        </div>
                        <div class="mgr-kpi-card">
                            <span class="dash-kpi-icon is-red dash-icon-alert" aria-hidden="true"></span>
                            <p class="mgr-kpi-label">Ended or inactive</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiOff" runat="server" /></p>
                        </div>
                    </div>

                    <asp:Literal ID="litMessage" runat="server" />

                    <!-- ADD FORM / CHANGE END DATE FORM -->
                    <asp:Panel ID="pnlForm" runat="server" Visible="false" DefaultButton="btnSave">
                        <div id="discFormCard" class="mgr-table-card emp-form-card">
                            <div class="dash-card-head">
                                <h3><asp:Literal ID="litFormTitle" runat="server" /></h3>
                                <span class="dash-card-meta"><asp:Literal ID="litFormMeta" runat="server" /></span>
                            </div>
                            <asp:HiddenField ID="hfDiscountId" runat="server" />

                            <!-- Shown when editing: everything except the end date is read-only -->
                            <asp:PlaceHolder ID="phReadOnly" runat="server" Visible="false">
                                <div class="disc-readonly">
                                    <div><span class="mgr-kpi-label">Discount</span><strong><asp:Literal ID="litRoName" runat="server" /></strong></div>
                                    <div><span class="mgr-kpi-label">Amount</span><strong><asp:Literal ID="litRoAmount" runat="server" /></strong></div>
                                    <div><span class="mgr-kpi-label">Starts</span><strong><asp:Literal ID="litRoStart" runat="server" /></strong></div>
                                    <div><span class="mgr-kpi-label">Status</span><strong><asp:Literal ID="litRoStatus" runat="server" /></strong></div>
                                    <asp:PlaceHolder ID="phRoPromo" runat="server">
                                        <div><span class="mgr-kpi-label">Promo code</span><strong><asp:Literal ID="litRoCode" runat="server" /></strong></div>
                                        <div><span class="mgr-kpi-label">Minimum spend</span><strong><asp:Literal ID="litRoMinSpend" runat="server" /></strong></div>
                                        <div><span class="mgr-kpi-label">Who can use it</span><strong><asp:Literal ID="litRoRule" runat="server" /></strong></div>
                                    </asp:PlaceHolder>
                                </div>
                                <p class="field-hint">Only the end date of an existing discount can be changed. To stop a discount, use Deactivate in the list.</p>
                            </asp:PlaceHolder>

                            <div class="emp-form-grid">
                                <!-- Shown when adding a new discount -->
                                <asp:PlaceHolder ID="phAddFields" runat="server">
                                    <div class="login-field emp-form-wide">
                                        <label for="<%= txtName.ClientID %>">Discount name</label>
                                        <asp:TextBox ID="txtName" runat="server" CssClass="login-input" placeholder="e.g. Johnnie Walker Special" />
                                        <span class="field-hint">Shown to customers on the Specials page. Names containing "Wednesday" only run on Wednesdays.</span>
                                    </div>
                                    <asp:PlaceHolder ID="phDescription" runat="server" Visible="false">
                                        <div class="login-field emp-form-wide">
                                            <label for="<%= txtDescription.ClientID %>">Description <span class="register-optional">(optional)</span></label>
                                            <asp:TextBox ID="txtDescription" runat="server" CssClass="login-input" TextMode="MultiLine" Rows="3" />
                                        </div>
                                    </asp:PlaceHolder>
                                    <div class="login-field">
                                        <label for="<%= ddlType.ClientID %>">Discount type</label>
                                        <asp:DropDownList ID="ddlType" runat="server" CssClass="login-input">
                                            <asp:ListItem Text="Percentage off (%)" Value="Percentage" />
                                            <asp:ListItem Text="Rand amount off (R)" Value="Fixed" />
                                        </asp:DropDownList>
                                    </div>
                                    <div class="login-field">
                                        <label for="<%= txtValue.ClientID %>">Discount value</label>
                                        <asp:TextBox ID="txtValue" runat="server" CssClass="login-input" placeholder="e.g. 15" inputmode="decimal" />
                                        <span class="field-hint">Percentage: 1 to 99. Rand amount: taken off each item's price.</span>
                                    </div>
                                    <div class="login-field">
                                        <label for="<%= txtStartDate.ClientID %>">Start date</label>
                                        <asp:TextBox ID="txtStartDate" runat="server" CssClass="login-input" TextMode="Date" />
                                    </div>

                                    <!-- Promo code fields (Discount_Code, Discount_MinSpend, Discount_Rule) -->
                                    <asp:PlaceHolder ID="phPromoFields" runat="server">
                                        <div class="login-field">
                                            <label for="<%= txtCode.ClientID %>">Promo code <span class="register-optional">(optional)</span></label>
                                            <asp:TextBox ID="txtCode" runat="server" CssClass="login-input" placeholder="e.g. SPEND500" autocomplete="off" style="text-transform:uppercase" />
                                            <span class="field-hint">Leave empty for a normal product discount. With a code, customers type it in the cart and it comes off the whole order.</span>
                                        </div>
                                        <div class="login-field">
                                            <label for="<%= txtMinSpend.ClientID %>">Minimum spend (R) <span class="register-optional">(optional)</span></label>
                                            <asp:TextBox ID="txtMinSpend" runat="server" CssClass="login-input" placeholder="e.g. 500" inputmode="decimal" />
                                            <span class="field-hint">Cart total needed before the code works. Only used with a promo code.</span>
                                        </div>
                                        <div class="login-field">
                                            <label for="<%= ddlRule.ClientID %>">Who can use it</label>
                                            <asp:DropDownList ID="ddlRule" runat="server" CssClass="login-input">
                                                <asp:ListItem Text="Anyone" Value="" />
                                                <asp:ListItem Text="Students only (.ac.za email)" Value="Student" />
                                                <asp:ListItem Text="First order only" Value="FirstOrder" />
                                            </asp:DropDownList>
                                            <span class="field-hint">Student and first-order rules need a promo code.</span>
                                        </div>
                                    </asp:PlaceHolder>
                                </asp:PlaceHolder>

                                <div class="login-field">
                                    <label for="<%= txtEndDate.ClientID %>">End date</label>
                                    <asp:TextBox ID="txtEndDate" runat="server" CssClass="login-input" TextMode="Date" />
                                    <span class="field-hint">The discount runs until the end of this day.</span>
                                </div>

                                <asp:PlaceHolder ID="phAddStatus" runat="server">
                                    <div class="login-field emp-form-wide">
                                        <span class="field-hint">New discounts are saved as Active. Without a promo code, link products to it in Manage Products (Discount drop-down). With a promo code, don't link products: the code already takes money off the whole order.</span>
                                    </div>
                                </asp:PlaceHolder>
                            </div>

                            <div class="emp-form-actions">
                                <asp:Button ID="btnSave" runat="server" Text="Save discount" CssClass="btn-primary" OnClick="btnSave_Click" />
                                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-ghost" OnClick="btnCancel_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- PRODUCTS THE DISCOUNT APPLIES TO -->
                    <asp:Panel ID="pnlProducts" runat="server" Visible="false">
                        <div id="discProductsCard" class="mgr-table-card emp-form-card">
                            <div class="dash-card-head">
                                <h3>Products on <asp:Literal ID="litProdTitle" runat="server" /></h3>
                                <asp:LinkButton ID="btnCloseProducts" runat="server" CssClass="emp-action" OnClick="btnCloseProducts_Click" CausesValidation="false">Close</asp:LinkButton>
                            </div>

                            <div class="cust-history-stats">
                                <div><span class="mgr-kpi-label">Amount</span><strong><asp:Literal ID="litProdAmount" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Period</span><strong><asp:Literal ID="litProdPeriod" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Status</span><strong><asp:Literal ID="litProdStatus" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Products</span><strong><asp:Literal ID="litProdCount" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Whole categories</span><strong><asp:Literal ID="litProdCategories" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Promo code</span><strong><asp:Literal ID="litProdCode" runat="server" /></strong></div>
                            </div>

                            <asp:Literal ID="litProdNote" runat="server" />

                            <asp:GridView ID="gvProducts" runat="server" AutoGenerateColumns="false" CssClass="dash-table emp-table" GridLines="None"
                                AllowPaging="true" PageSize="10" OnPageIndexChanging="gvProducts_PageIndexChanging"
                                EmptyDataText="No products are linked to this discount yet. Link products in Manage Products, or link a whole category to it."
                                EmptyDataRowStyle-CssClass="dash-table-empty">
                                <PagerStyle CssClass="emp-pager" />
                                <Columns>
                                    <asp:TemplateField HeaderText="Product" ItemStyle-CssClass="is-wrap">
                                        <ItemTemplate>
                                            <strong><%#: Eval("Product_Name") %></strong>
                                            <span class="emp-sub"><%#: ProductSub(Eval("Product_Brand"), Eval("Product_SizeML")) %></span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Category_Name" HeaderText="Category" />
                                    <asp:BoundField DataField="AppliedVia" HeaderText="Linked through" />
                                    <asp:BoundField DataField="Product_SellingPrice" HeaderText="Shelf price" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                    <asp:BoundField DataField="DiscountedPrice" HeaderText="With discount" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                    <asp:BoundField DataField="Product_QuantityInStock" HeaderText="Stock" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                    <asp:TemplateField HeaderText="Status">
                                        <ItemTemplate>
                                            <span class='emp-badge emp-badge-<%# ActiveClass(Eval("Product_Status")) %>'><%#: Eval("Product_Status") %></span>
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
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="login-input" placeholder="Search by discount name or code..." autocomplete="off" />
                                <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                    <asp:ListItem Text="All discounts" Value="" />
                                    <asp:ListItem Text="Running now" Value="running" />
                                    <asp:ListItem Text="Scheduled" Value="scheduled" />
                                    <asp:ListItem Text="Ended" Value="ended" />
                                    <asp:ListItem Text="Inactive" Value="inactive" />
                                    <asp:ListItem Text="Promo codes only" Value="code" />
                                    <asp:ListItem Text="Product discounts only" Value="nocode" />
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
                            OnSorting="gvDiscounts_Sorting" OnRowDataBound="gvDiscounts_RowDataBound">
                            <PagerStyle CssClass="emp-pager" />
                            <Columns>
                                <asp:TemplateField HeaderText="Discount" SortExpression="Discount_Name" ItemStyle-CssClass="is-wrap">
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" CommandName="ViewProducts" CommandArgument='<%# Eval("Discount_ID") %>' CssClass="disc-name-link" CausesValidation="false" ToolTip="See the products this discount applies to">
                                            <strong><%#: Eval("Discount_Name") %></strong>
                                        </asp:LinkButton>
                                        <%# SubText(Eval("Discount_Description")) %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Amount" SortExpression="Discount_Value">
                                    <ItemTemplate><%#: AmountText(Eval("Discount_Type"), Eval("Discount_Value")) %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Promo code" SortExpression="Discount_Code">
                                    <ItemTemplate>
                                        <%# CodeText(Eval("Discount_Code")) %>
                                        <%# PromoSub(Eval("Discount_MinSpend"), Eval("Discount_Rule")) %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Starts" SortExpression="Discount_StartDate">
                                    <ItemTemplate><%#: DateText(Eval("Discount_StartDate")) %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Ends" SortExpression="Discount_EndDate">
                                    <ItemTemplate><%#: DateText(Eval("Discount_EndDate")) %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Applies to" SortExpression="ProductCount">
                                    <ItemTemplate>
                                        <%# AppliesText(Eval("Discount_Code"), Eval("ProductCount"), Eval("CategoryCount")) %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <span class='emp-badge emp-badge-<%# Eval("StatusClass") %>'><%#: Eval("StatusLabel") %></span>
                                        <%# SubText(Eval("StatusNote")) %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" CommandName="ViewProducts" CommandArgument='<%# Eval("Discount_ID") %>' CssClass="emp-action" CausesValidation="false">Products</asp:LinkButton>
                                        <asp:LinkButton runat="server" CommandName="EditEnd" CommandArgument='<%# Eval("Discount_ID") %>' CssClass="emp-action" CausesValidation="false">Change end date</asp:LinkButton>
                                        <asp:LinkButton runat="server" CommandName="ToggleStatus" CommandArgument='<%# Eval("Discount_ID") %>' CssClass="emp-action" CausesValidation="false"
                                            Text='<%# (bool)Eval("IsActiveFlag") ? "Deactivate" : "Reactivate" %>'
                                            OnClientClick='<%# DeactivateJs(Eval("IsActiveFlag"), Eval("Discount_Name")) %>' />
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
</asp:Content>
