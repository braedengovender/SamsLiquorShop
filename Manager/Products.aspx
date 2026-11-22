<%@ Page Title="Manage Products" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Products.aspx.cs" Inherits="SamsLiquorShop.Manager.Products" %>
<%@ Register Src="~/Manager/ManagerNav.ascx" TagPrefix="uc" TagName="ManagerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:ManagerNav runat="server" />

        <div class="mgr-main">

            <div class="mgr-header-row emp-header-flex">
                <div>
                    <h1>Manage Products</h1>
                    <p class="mgr-dash-subtext">Add and update products, record stock changes, and control what appears in the shop.</p>
                </div>
                <div class="emp-header-actions">
                    <asp:Button ID="btnDownloadLog" runat="server" Text="Download stock log" CssClass="btn-ghost" OnClick="btnDownloadLog_Click" />
                    <asp:Button ID="btnExport" runat="server" Text="Export CSV" CssClass="btn-ghost" OnClick="btnExport_Click" />
                    <asp:Button ID="btnMarginReport" runat="server" Text="Margin report" CssClass="btn-ghost" OnClick="btnMarginReport_Click" CausesValidation="false" />
                    <asp:Button ID="btnAddNew" runat="server" Text="+ Add Product" CssClass="btn-primary" OnClick="btnAddNew_Click" />
                </div>
            </div>

            <asp:UpdatePanel ID="upMain" runat="server">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="btnAddNew" EventName="Click" />
                    <asp:AsyncPostBackTrigger ControlID="btnMarginReport" EventName="Click" />
                    <%-- The CSV download must be a full postback too --%>
                    <asp:PostBackTrigger ControlID="btnMrExport" />
                    <%-- File uploads only work with a full postback --%>
                    <asp:PostBackTrigger ControlID="btnSave" />
                </Triggers>
                <ContentTemplate>

                    <!-- SUMMARY CARDS -->
                    <div class="mgr-kpi-grid">
                        <div class="mgr-kpi-card">
                            <span class="dash-kpi-icon is-blue dash-icon-receipt" aria-hidden="true"></span>
                            <p class="mgr-kpi-label">Total products</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiTotal" runat="server" /></p>
                        </div>
                        <div class="mgr-kpi-card">
                            <span class="dash-kpi-icon is-blue dash-icon-bag" aria-hidden="true"></span>
                            <p class="mgr-kpi-label">Active in shop</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiActive" runat="server" /></p>
                        </div>
                        <div class="mgr-kpi-card">
                            <span class="dash-kpi-icon is-amber dash-icon-alert" aria-hidden="true"></span>
                            <p class="mgr-kpi-label">Low stock</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiLow" runat="server" /></p>
                        </div>
                        <div class="mgr-kpi-card">
                            <span class="dash-kpi-icon is-red dash-icon-alert" aria-hidden="true"></span>
                            <p class="mgr-kpi-label">Out of stock</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiOut" runat="server" /></p>
                        </div>
                        <div class="mgr-kpi-card">
                            <span class="dash-kpi-icon is-amber dash-icon-cash" aria-hidden="true"></span>
                            <p class="mgr-kpi-label">Stock value (at cost)</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiValue" runat="server" /></p>
                        </div>
                    </div>

                    <asp:Literal ID="litMessage" runat="server" />

                    <!-- ADD / EDIT FORM -->
                    <asp:Panel ID="pnlForm" runat="server" Visible="false" DefaultButton="btnSave">
                        <div id="prodFormCard" class="mgr-table-card emp-form-card">
                            <div class="dash-card-head">
                                <h3><asp:Literal ID="litFormTitle" runat="server" /></h3>
                                <span class="dash-card-meta"><asp:Literal ID="litFormMeta" runat="server" /></span>
                            </div>
                            <asp:HiddenField ID="hfProductId" runat="server" />

                            <div class="emp-form-grid">
                                <div class="login-field emp-form-wide">
                                    <label for="<%= txtName.ClientID %>">Product name</label>
                                    <asp:TextBox ID="txtName" runat="server" CssClass="login-input" MaxLength="100" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtBrand.ClientID %>">Brand</label>
                                    <asp:TextBox ID="txtBrand" runat="server" CssClass="login-input" MaxLength="50" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtType.ClientID %>">Type</label>
                                    <asp:TextBox ID="txtType" runat="server" CssClass="login-input" MaxLength="50" />
                                    <span class="field-hint">For example Lager, Merlot or Single malt.</span>
                                </div>
                                <div class="login-field">
                                    <label for="<%= ddlCategory.ClientID %>">Category</label>
                                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="login-input" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= ddlSupplier.ClientID %>">Supplier</label>
                                    <asp:DropDownList ID="ddlSupplier" runat="server" CssClass="login-input" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtSize.ClientID %>">Size (ml)</label>
                                    <asp:TextBox ID="txtSize" runat="server" CssClass="login-input" MaxLength="5" data-digits="1" />
                                    <span class="field-hint">Use 0 for items without a volume, such as accessories.</span>
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtAlcohol.ClientID %>">Alcohol %</label>
                                    <asp:TextBox ID="txtAlcohol" runat="server" CssClass="login-input" MaxLength="6" data-decimal="1" />
                                    <span class="field-hint">Optional. Enter 0 for non-alcoholic drinks.</span>
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtFlavour.ClientID %>">Flavour</label>
                                    <asp:TextBox ID="txtFlavour" runat="server" CssClass="login-input" MaxLength="100" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtOrigin.ClientID %>">Origin region</label>
                                    <asp:TextBox ID="txtOrigin" runat="server" CssClass="login-input" MaxLength="100" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtBarcode.ClientID %>">Barcode number</label>
                                    <asp:TextBox ID="txtBarcode" runat="server" CssClass="login-input" MaxLength="13" data-digits="1" />
                                    <span class="field-hint">13 digits. Each product needs its own barcode.</span>
                                </div>
                                <div class="login-field">
                                    <label for="<%= ddlDiscount.ClientID %>">Discount</label>
                                    <asp:DropDownList ID="ddlDiscount" runat="server" CssClass="login-input" />
                                    <span class="field-hint">Optional. A product discount overrides its category discount.</span>
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtCost.ClientID %>">Cost price (R)</label>
                                    <asp:TextBox ID="txtCost" runat="server" CssClass="login-input" MaxLength="10" data-decimal="1" oninput="prodMarginPreview()" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtPrice.ClientID %>">Selling price (R)</label>
                                    <asp:TextBox ID="txtPrice" runat="server" CssClass="login-input" MaxLength="10" data-decimal="1" oninput="prodMarginPreview()" />
                                </div>
                                <div class="emp-form-wide">
                                    <p id="prodMarginPreview" class="emp-stock-preview"></p>
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtReorder.ClientID %>">Reorder level</label>
                                    <asp:TextBox ID="txtReorder" runat="server" CssClass="login-input" MaxLength="6" data-digits="1" />
                                    <span class="field-hint">The product shows as "Low" when stock falls to this number.</span>
                                </div>
                                <asp:PlaceHolder ID="phInitialStock" runat="server" Visible="false">
                                    <div class="login-field">
                                        <label for="<%= txtInitialStock.ClientID %>">Starting stock</label>
                                        <asp:TextBox ID="txtInitialStock" runat="server" CssClass="login-input" MaxLength="6" data-digits="1" />
                                        <span class="field-hint">Recorded in the stock log. Later changes use the Stock button.</span>
                                    </div>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="phStockNote" runat="server" Visible="false">
                                    <div class="login-field">
                                        <label>Current stock</label>
                                        <div class="emp-static-field"><asp:Literal ID="litStockNote" runat="server" /></div>
                                    </div>
                                </asp:PlaceHolder>
                                <div class="login-field">
                                    <label for="<%= ddlStatus.ClientID %>">Status</label>
                                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="login-input">
                                        <asp:ListItem Text="Active" Value="Active" />
                                        <asp:ListItem Text="Inactive" Value="Inactive" />
                                    </asp:DropDownList>
                                    <span class="field-hint">Inactive products are hidden from the shop.</span>
                                </div>
                                <div class="login-field emp-form-wide">
                                    <label for="<%= txtIngredients.ClientID %>">Ingredients</label>
                                    <asp:TextBox ID="txtIngredients" runat="server" CssClass="login-input" MaxLength="255" />
                                </div>
                                <div class="login-field emp-form-wide">
                                    <label for="<%= txtDescription.ClientID %>">Description</label>
                                    <asp:TextBox ID="txtDescription" runat="server" CssClass="login-input" TextMode="MultiLine" Rows="4" />
                                    <span class="field-hint">Up to 1000 characters.</span>
                                </div>
                                <asp:PlaceHolder ID="phImage" runat="server" Visible="false">
                                    <div class="login-field emp-form-wide">
                                        <label for="<%= fuImage.ClientID %>">Product picture</label>
                                        <div class="prod-image-row">
                                            <asp:Literal ID="litImagePreview" runat="server" />
                                            <div class="prod-image-controls">
                                                <asp:FileUpload ID="fuImage" runat="server" CssClass="login-input" accept="image/jpeg,image/png,image/gif,image/webp" />
                                                <span class="field-hint">JPG, PNG, GIF or WEBP, up to 2 MB. When editing, choosing a new picture replaces the current one.</span>
                                                <label class="emp-check">
                                                    <asp:CheckBox ID="chkRemoveImage" runat="server" />
                                                    <span>Remove the current picture</span>
                                                </label>
                                            </div>
                                        </div>
                                    </div>
                                </asp:PlaceHolder>
                            </div>

                            <div class="emp-form-actions">
                                <asp:Button ID="btnSave" runat="server" Text="Save product" CssClass="btn-primary" OnClick="btnSave_Click" />
                                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-ghost" OnClick="btnCancel_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- ADJUST STOCK -->
                    <asp:Panel ID="pnlStock" runat="server" Visible="false" DefaultButton="btnSaveStock">
                        <div id="prodStockCard" class="mgr-table-card emp-form-card">
                            <div class="dash-card-head">
                                <h3><asp:Literal ID="litStockTitle" runat="server" /></h3>
                                <span class="dash-card-meta"><asp:Literal ID="litStockCurrent" runat="server" /></span>
                            </div>
                            <asp:HiddenField ID="hfCurrentStock" runat="server" />

                            <div class="emp-form-grid">
                                <div class="login-field">
                                    <label for="<%= ddlChangeType.ClientID %>">Change type</label>
                                    <asp:DropDownList ID="ddlChangeType" runat="server" CssClass="login-input" onchange="prodStockPreview()">
                                        <asp:ListItem Text="Add stock" Value="add" />
                                        <asp:ListItem Text="Remove stock" Value="remove" />
                                        <asp:ListItem Text="Set exact quantity" Value="set" />
                                    </asp:DropDownList>
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtQuantity.ClientID %>">Quantity</label>
                                    <asp:TextBox ID="txtQuantity" runat="server" CssClass="login-input" MaxLength="6" data-digits="1" oninput="prodStockPreview()" />
                                    <span class="field-hint">For "Set exact quantity", enter the new total.</span>
                                </div>
                                <div class="login-field">
                                    <label for="<%= ddlReason.ClientID %>">Reason</label>
                                    <asp:DropDownList ID="ddlReason" runat="server" CssClass="login-input">
                                        <asp:ListItem Text="Select a reason" Value="" />
                                        <asp:ListItem Text="Delivery received" Value="Delivery received" />
                                        <asp:ListItem Text="Returned by customer" Value="Returned by customer" />
                                        <asp:ListItem Text="Stock count correction" Value="Stock count correction" />
                                        <asp:ListItem Text="Damaged / broken" Value="Damaged / broken" />
                                        <asp:ListItem Text="Expired / spoiled" Value="Expired / spoiled" />
                                        <asp:ListItem Text="Theft / loss" Value="Theft / loss" />
                                        <asp:ListItem Text="Returned to supplier" Value="Returned to supplier" />
                                        <asp:ListItem Text="Other" Value="Other" />
                                    </asp:DropDownList>
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtNotes.ClientID %>">Notes</label>
                                    <asp:TextBox ID="txtNotes" runat="server" CssClass="login-input" MaxLength="150" />
                                    <span class="field-hint">Optional (for example an invoice number). Required when the reason is Other.</span>
                                </div>
                            </div>

                            <p id="prodStockPreview" class="emp-stock-preview"></p>

                            <div class="emp-form-actions">
                                <asp:Button ID="btnSaveStock" runat="server" Text="Save stock change" CssClass="btn-primary" OnClick="btnSaveStock_Click" />
                                <asp:Button ID="btnCancelStock" runat="server" Text="Cancel" CssClass="btn-ghost" OnClick="btnCancelStock_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- STOCK HISTORY -->
                    <asp:Panel ID="pnlHistory" runat="server" Visible="false">
                        <div id="prodHistoryCard" class="mgr-table-card emp-form-card">
                            <div class="dash-card-head">
                                <h3>Stock history: <asp:Literal ID="litHistoryName" runat="server" /></h3>
                                <asp:LinkButton ID="btnCloseHistory" runat="server" CssClass="emp-action" OnClick="btnCloseHistory_Click">Close</asp:LinkButton>
                            </div>
                            <p class="field-hint">The last 50 manual changes made on this page. Sales and deliveries recorded elsewhere are not included.</p>

                            <asp:GridView ID="gvStockLog" runat="server" AutoGenerateColumns="false" CssClass="dash-table" GridLines="None"
                                EmptyDataText="No stock changes have been logged for this product yet." EmptyDataRowStyle-CssClass="dash-table-empty">
                                <Columns>
                                    <asp:BoundField DataField="When" HeaderText="When" DataFormatString="{0:d MMM yyyy, HH:mm}" />
                                    <asp:BoundField DataField="OldQuantity" HeaderText="Old" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                    <asp:BoundField DataField="NewQuantity" HeaderText="New" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                    <asp:TemplateField HeaderText="Change" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num">
                                        <ItemTemplate><%# ChangeText(Eval("Change")) %></ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Reason" HeaderText="Reason" ItemStyle-CssClass="is-wrap" />
                                    <asp:BoundField DataField="ChangedBy" HeaderText="By" />
                                </Columns>
                            </asp:GridView>
                        </div>
                    </asp:Panel>

                    <!-- MARGIN REPORT -->
                    <asp:Panel ID="pnlMargin" runat="server" Visible="false" DefaultButton="btnMrRun">
                        <div id="prodMarginCard" class="mgr-table-card emp-form-card">
                            <div class="dash-card-head">
                                <h3>Margin report</h3>
                                <asp:LinkButton ID="btnMrClose" runat="server" CssClass="emp-action" OnClick="btnMrClose_Click" CausesValidation="false">Close</asp:LinkButton>
                            </div>
                            <p class="field-hint">Profit is what customers actually paid (after discounts) minus each product's current cost price. Cancelled, refunded and voided sales are left out. List margin is (price &minus; cost) &divide; price.</p>

                            <div class="emp-form-grid">
                                <div class="login-field">
                                    <label for="<%= txtMrFrom.ClientID %>">Sales from</label>
                                    <asp:TextBox ID="txtMrFrom" runat="server" CssClass="login-input" TextMode="Date" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtMrTo.ClientID %>">Sales to</label>
                                    <asp:TextBox ID="txtMrTo" runat="server" CssClass="login-input" TextMode="Date" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= ddlMrCategory.ClientID %>">Category</label>
                                    <asp:DropDownList ID="ddlMrCategory" runat="server" CssClass="login-input" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= ddlMrSort.ClientID %>">Order by</label>
                                    <asp:DropDownList ID="ddlMrSort" runat="server" CssClass="login-input">
                                        <asp:ListItem Text="Most profit earned" Value="profit" />
                                        <asp:ListItem Text="Highest margin" Value="margin_hi" />
                                        <asp:ListItem Text="Lowest margin" Value="margin_lo" />
                                        <asp:ListItem Text="Most units sold" Value="units" />
                                    </asp:DropDownList>
                                </div>
                                <div class="login-field">
                                    <label class="emp-check">
                                        <asp:CheckBox ID="chkMrActiveOnly" runat="server" Checked="true" />
                                        <span>Active products only</span>
                                    </label>
                                </div>
                            </div>

                            <div class="emp-form-actions">
                                <asp:Button ID="btnMrRun" runat="server" Text="Run report" CssClass="btn-primary" OnClick="btnMrRun_Click" CausesValidation="false" />
                                <asp:Button ID="btnMrExport" runat="server" Text="Export CSV" CssClass="btn-ghost" OnClick="btnMrExport_Click" CausesValidation="false" />
                            </div>

                            <asp:Literal ID="litMrSummary" runat="server" />

                            <asp:GridView ID="gvMargin" runat="server" AutoGenerateColumns="false" CssClass="dash-table emp-table"
                                GridLines="None" AllowPaging="true" PageSize="15"
                                EmptyDataText="No products match these filters." EmptyDataRowStyle-CssClass="dash-table-empty"
                                OnPageIndexChanging="gvMargin_PageIndexChanging">
                                <PagerStyle CssClass="emp-pager" />
                                <Columns>
                                    <asp:TemplateField HeaderText="Product" ItemStyle-CssClass="is-wrap">
                                        <ItemTemplate>
                                            <strong><%#: Eval("Product_Name") %></strong>
                                            <span class="emp-sub"><%# SubLine(Eval("Product_Brand"), Eval("Product_SizeML")) %></span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Category_Name" HeaderText="Category" />
                                    <asp:BoundField DataField="Product_CostPrice" HeaderText="Cost" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                    <asp:BoundField DataField="Product_SellingPrice" HeaderText="Price" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                    <asp:TemplateField HeaderText="List margin" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num">
                                        <ItemTemplate>
                                            <strong><%# MarginText(Eval("MarginPct")) %></strong>
                                            <span class='emp-badge emp-badge-<%# MarginClass(Eval("MarginPct")) %>'><%# MarginLabel(Eval("MarginPct")) %></span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="UnitsSold" HeaderText="Units sold" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                    <asp:BoundField DataField="Revenue" HeaderText="Revenue" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                    <asp:BoundField DataField="ProfitEarned" HeaderText="Profit earned" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                    <asp:TemplateField HeaderText="Realised margin" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num">
                                        <ItemTemplate><%# RealisedText(Eval("RealisedPct")) %></ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </asp:Panel>

                    <!-- PRODUCT LIST -->
                    <div class="mgr-table-card">
                        <asp:Panel ID="pnlToolbar" runat="server" DefaultButton="btnSearch">
                            <div class="emp-toolbar">
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="login-input" placeholder="Search by product, brand, category or barcode..." autocomplete="off" />
                                <asp:DropDownList ID="ddlCategoryFilter" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                                <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                    <asp:ListItem Text="All statuses" Value="" />
                                    <asp:ListItem Text="Active" Value="Active" />
                                    <asp:ListItem Text="Inactive" Value="Inactive" />
                                </asp:DropDownList>
                                <asp:DropDownList ID="ddlStockFilter" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                    <asp:ListItem Text="All stock levels" Value="" />
                                    <asp:ListItem Text="In stock" Value="in" />
                                    <asp:ListItem Text="Low stock" Value="low" />
                                    <asp:ListItem Text="Out of stock" Value="out" />
                                </asp:DropDownList>
                                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-primary" OnClick="btnSearch_Click" />
                                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-ghost" OnClick="btnClear_Click" />
                            </div>
                        </asp:Panel>

                        <p class="emp-count"><asp:Literal ID="litCount" runat="server" /></p>

                        <asp:GridView ID="gvProducts" runat="server" AutoGenerateColumns="false" CssClass="dash-table emp-table"
                            GridLines="None" DataKeyNames="Product_ID" AllowPaging="true" PageSize="10" AllowSorting="true"
                            EmptyDataText="No products match your search." EmptyDataRowStyle-CssClass="dash-table-empty"
                            OnPageIndexChanging="gvProducts_PageIndexChanging" OnRowCommand="gvProducts_RowCommand"
                            OnSorting="gvProducts_Sorting" OnRowDataBound="gvProducts_RowDataBound">
                            <PagerStyle CssClass="emp-pager" />
                            <Columns>
                                <asp:TemplateField HeaderText="Product" SortExpression="Product_Name" ItemStyle-CssClass="is-wrap">
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
                                <asp:BoundField DataField="Category_Name" HeaderText="Category" SortExpression="Category_Name" />
                                <asp:BoundField DataField="Product_SellingPrice" HeaderText="Price" SortExpression="Product_SellingPrice" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                <asp:TemplateField HeaderText="Stock" SortExpression="Product_QuantityInStock" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num">
                                    <ItemTemplate>
                                        <strong><%# Eval("Product_QuantityInStock") %></strong>
                                        <span class='emp-badge emp-badge-<%# StockClass(Eval("Product_QuantityInStock"), Eval("Product_ReorderQuantity")) %>'><%# StockLabel(Eval("Product_QuantityInStock"), Eval("Product_ReorderQuantity")) %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Product_ReorderQuantity" HeaderText="Reorder at" SortExpression="Product_ReorderQuantity" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                <asp:TemplateField HeaderText="Status" SortExpression="Product_Status">
                                    <ItemTemplate>
                                        <span class='emp-badge emp-badge-<%# StatusClass(Eval("Product_Status")) %>'><%#: Eval("Product_Status") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" CommandName="EditProduct" CommandArgument='<%# Eval("Product_ID") %>' CssClass="emp-action">Edit</asp:LinkButton>
                                        <asp:LinkButton runat="server" CommandName="AdjustStock" CommandArgument='<%# Eval("Product_ID") %>' CssClass="emp-action">Stock</asp:LinkButton>
                                        <asp:LinkButton runat="server" CommandName="StockHistory" CommandArgument='<%# Eval("Product_ID") %>' CssClass="emp-action">History</asp:LinkButton>
                                        <asp:LinkButton runat="server" CommandName="ToggleProduct" CommandArgument='<%# Eval("Product_ID") %>' CssClass="emp-action"
                                            Text='<%# StatusClass(Eval("Product_Status")) == "active" ? "Deactivate" : "Reactivate" %>'
                                            OnClientClick='<%# DeactivateJs(Eval("Product_Status"), Eval("Product_Name")) %>' />
                                       
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
        // Only digits and one decimal separator in money / percentage boxes
        document.addEventListener('input', function (e) {
            var t = e.target;
            if (t && t.getAttribute && t.getAttribute('data-decimal')) {
                t.value = t.value.replace(/[^0-9.,]/g, '');
            }
        });

        // Shows the profit and margin while the manager types the prices
        function prodMarginPreview() {
            var out = document.getElementById('prodMarginPreview');
            var cost = document.getElementById('<%= txtCost.ClientID %>');
            var price = document.getElementById('<%= txtPrice.ClientID %>');
            if (!out || !cost || !price) return;

            var c = parseFloat(cost.value.replace(',', '.'));
            var p = parseFloat(price.value.replace(',', '.'));
            if (isNaN(c) || isNaN(p) || p <= 0) {
                out.textContent = '';
                out.className = 'emp-stock-preview';
                return;
            }

            var profit = p - c;
            var pct = (profit / p) * 100;
            out.textContent = 'Profit per unit: R ' + profit.toFixed(2) + ' (' + pct.toFixed(1) + '% margin)';
            out.className = 'emp-stock-preview' + (profit < 0 ? ' is-bad' : '');
        }

        // Shows what the stock level will be before the manager saves
        function prodStockPreview() {
            var out = document.getElementById('prodStockPreview');
            var cur = document.getElementById('<%= hfCurrentStock.ClientID %>');
            var type = document.getElementById('<%= ddlChangeType.ClientID %>');
            var qty = document.getElementById('<%= txtQuantity.ClientID %>');
            if (!out || !cur || !type || !qty) return;

            var current = parseInt(cur.value, 10);
            var q = parseInt(qty.value, 10);
            if (isNaN(current) || isNaN(q)) {
                out.textContent = '';
                out.className = 'emp-stock-preview';
                return;
            }

            var result = type.value === 'add' ? current + q : (type.value === 'remove' ? current - q : q);
            var diff = result - current;

            if (result < 0) {
                out.textContent = 'You only have ' + current + ' in stock.';
                out.className = 'emp-stock-preview is-bad';
                return;
            }

            out.textContent = 'New stock level: ' + result + ' (' + (diff > 0 ? '+' : '') + diff + ')';
            out.className = 'emp-stock-preview';
        }

        function prodRefreshPreviews() {
            prodMarginPreview();
            prodStockPreview();
        }

        if (window.Sys && Sys.Application) {
            Sys.Application.add_load(prodRefreshPreviews);
        } else {
            document.addEventListener('DOMContentLoaded', prodRefreshPreviews);
        }
    </script>
</asp:Content>