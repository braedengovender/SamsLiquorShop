<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SalesBrowser.ascx.cs" Inherits="SamsLiquorShop.Controls.SalesBrowser" %>
<%-- Sales list + sale details, shared by Manager/Sales.aspx (Manage Sales) and Employee/Orders.aspx (cashier Orders).
     Set CashierMode="true" on the cashier page: it defaults to the logged-in cashier's own sales and hides CSV export. --%>

<div class="mgr-header-row emp-header-flex">
    <div>
        <h1><asp:Literal ID="litTitle" runat="server" /></h1>
        <p class="mgr-dash-subtext"><asp:Literal ID="litSubtitle" runat="server" /></p>
    </div>
    <div class="emp-header-actions">
        <asp:Button ID="btnExport" runat="server" Text="Export CSV" CssClass="btn-ghost" OnClick="btnExport_Click" />
    </div>
</div>

<asp:UpdatePanel ID="upSales" runat="server">
    <ContentTemplate>

        <!-- SUMMARY CARDS (for the sales matching the filters below) -->
        <div class="mgr-kpi-grid">
            <div class="mgr-kpi-card">
                <span class="dash-kpi-icon is-blue dash-icon-cash" aria-hidden="true"></span>
                <p class="mgr-kpi-label">Revenue</p>
                <p class="mgr-kpi-value"><asp:Literal ID="litKpiRevenue" runat="server" /></p>
            </div>
            <div class="mgr-kpi-card">
                <span class="dash-kpi-icon is-amber dash-icon-receipt" aria-hidden="true"></span>
                <p class="mgr-kpi-label">Sales</p>
                <p class="mgr-kpi-value"><asp:Literal ID="litKpiCount" runat="server" /></p>
            </div>
            <div class="mgr-kpi-card">
                <span class="dash-kpi-icon is-blue dash-icon-bag" aria-hidden="true"></span>
                <p class="mgr-kpi-label">Average sale</p>
                <p class="mgr-kpi-value"><asp:Literal ID="litKpiAverage" runat="server" /></p>
            </div>
            <div class="mgr-kpi-card">
                <span class="dash-kpi-icon is-amber dash-icon-alert" aria-hidden="true"></span>
                <p class="mgr-kpi-label">Discounts given</p>
                <p class="mgr-kpi-value"><asp:Literal ID="litKpiDiscounts" runat="server" /></p>
            </div>
        </div>

        <asp:Literal ID="litMessage" runat="server" />

        <!-- SALE DETAILS -->
        <asp:Panel ID="pnlDetail" runat="server" Visible="false">
            <div id="saleDetailCard" class="mgr-table-card emp-form-card">
                <div class="dash-card-head">
                    <h3>Sale <asp:Literal ID="litDetailInvoice" runat="server" /> <asp:Literal ID="litDetailStatus" runat="server" /></h3>
                    <asp:LinkButton ID="btnCloseDetail" runat="server" CssClass="emp-action" OnClick="btnCloseDetail_Click">Close</asp:LinkButton>
                </div>

                <div class="cust-history-stats">
                    <div><span class="mgr-kpi-label">Date</span><strong><asp:Literal ID="litDetailDate" runat="server" /></strong></div>
                    <div><span class="mgr-kpi-label">Customer</span><strong><asp:Literal ID="litDetailCustomer" runat="server" /></strong></div>
                    <div><span class="mgr-kpi-label">Cashier</span><strong><asp:Literal ID="litDetailCashier" runat="server" /></strong></div>
                    <div><span class="mgr-kpi-label">Sale type</span><strong><asp:Literal ID="litDetailType" runat="server" /></strong></div>
                    <div><span class="mgr-kpi-label">Points earned</span><strong><asp:Literal ID="litDetailPointsEarned" runat="server" /></strong></div>
                    <div><span class="mgr-kpi-label">Points used</span><strong><asp:Literal ID="litDetailPointsUsed" runat="server" /></strong></div>
                </div>

                <h4 class="sale-section-title">Items</h4>
                <asp:GridView ID="gvLines" runat="server" AutoGenerateColumns="false" CssClass="dash-table" GridLines="None"
                    EmptyDataText="No items were recorded for this sale." EmptyDataRowStyle-CssClass="dash-table-empty">
                    <Columns>
                        <asp:TemplateField HeaderText="Product" ItemStyle-CssClass="is-wrap">
                            <ItemTemplate>
                                <%#: Eval("Product_Name") %>
                                <%# LineDiscount(Eval("Discount_Name")) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="SaleLine_Quantity" HeaderText="Qty" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                        <asp:BoundField DataField="SaleLine_OriginalUnitPrice" HeaderText="Unit price" DataFormatString="{0:C}" ItemStyle-CssClass="is-num is-muted" HeaderStyle-CssClass="is-num" />
                        <asp:BoundField DataField="SaleLine_UnitPriceAfterDiscount" HeaderText="After discount" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                        <asp:BoundField DataField="SaleLine_Subtotal" HeaderText="Subtotal" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                    </Columns>
                </asp:GridView>

                <dl class="sale-totals">
                    <dt>Subtotal</dt><dd><asp:Literal ID="litDetailSubtotal" runat="server" /></dd>
                    <dt>Discount <asp:Literal ID="litDetailDiscountName" runat="server" /></dt><dd><asp:Literal ID="litDetailDiscount" runat="server" /></dd>
                    <dt class="is-total">Total</dt><dd class="is-total"><asp:Literal ID="litDetailTotal" runat="server" /></dd>
                </dl>

                <h4 class="sale-section-title">Payments</h4>
                <asp:GridView ID="gvPayments" runat="server" AutoGenerateColumns="false" CssClass="dash-table" GridLines="None"
                    EmptyDataText="No payments were recorded for this sale." EmptyDataRowStyle-CssClass="dash-table-empty">
                    <Columns>
                        <asp:BoundField DataField="PaymentMethod_Name" HeaderText="Method" NullDisplayText="Unknown" />
                        <asp:BoundField DataField="Payment_DateTime" HeaderText="Time" DataFormatString="{0:d MMM yyyy, HH:mm}" />
                        <asp:BoundField DataField="Payment_LoyaltyPointsUsed" HeaderText="Points used" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                        <asp:BoundField DataField="Payment_AmountTendered" HeaderText="Tendered" DataFormatString="{0:C}" NullDisplayText="&mdash;" ItemStyle-CssClass="is-num is-muted" HeaderStyle-CssClass="is-num" />
                        <asp:BoundField DataField="Payment_ChangeAmount" HeaderText="Change" DataFormatString="{0:C}" NullDisplayText="&mdash;" ItemStyle-CssClass="is-num is-muted" HeaderStyle-CssClass="is-num" />
                        <asp:BoundField DataField="Payment_AmountPaid" HeaderText="Paid" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='emp-badge emp-badge-<%# StatusClass(Eval("Payment_Status")) %>'><%#: Eval("Payment_Status") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </asp:Panel>

        <!-- SALES LIST -->
        <div class="mgr-table-card">
            <asp:Panel ID="pnlToolbar" runat="server" DefaultButton="btnSearch">
                <div class="emp-toolbar">
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="login-input" placeholder="Search by invoice number or customer name..." autocomplete="off" />
                    <asp:DropDownList ID="ddlRange" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                        <asp:ListItem Text="All time" Value="all" />
                        <asp:ListItem Text="Today" Value="today" />
                        <asp:ListItem Text="Last 7 days" Value="7d" />
                        <asp:ListItem Text="Last 30 days" Value="30d" />
                        <asp:ListItem Text="Last 3 months" Value="3m" />
                        <asp:ListItem Text="Last 12 months" Value="12m" />
                        <asp:ListItem Text="Custom dates..." Value="custom" />
                    </asp:DropDownList>
                    <asp:DropDownList ID="ddlCashier" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                    <asp:DropDownList ID="ddlPayment" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-primary" OnClick="btnSearch_Click" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-ghost" OnClick="btnClear_Click" />
                </div>
                <asp:Panel ID="pnlCustomDates" runat="server" Visible="false" CssClass="emp-toolbar sale-date-row">
                    <label for="<%= txtFrom.ClientID %>">From</label>
                    <asp:TextBox ID="txtFrom" runat="server" CssClass="login-input" TextMode="Date" />
                    <label for="<%= txtTo.ClientID %>">To</label>
                    <asp:TextBox ID="txtTo" runat="server" CssClass="login-input" TextMode="Date" />
                    <asp:Button ID="btnApplyDates" runat="server" Text="Apply" CssClass="btn-ghost" OnClick="btnSearch_Click" />
                </asp:Panel>
            </asp:Panel>

            <p class="emp-count"><asp:Literal ID="litCount" runat="server" /></p>

            <asp:GridView ID="gvSales" runat="server" AutoGenerateColumns="false" CssClass="dash-table emp-table"
                GridLines="None" DataKeyNames="Sale_ID" AllowPaging="true" PageSize="15" AllowSorting="true"
                EmptyDataText="No sales match these filters." EmptyDataRowStyle-CssClass="dash-table-empty"
                OnPageIndexChanging="gvSales_PageIndexChanging" OnRowCommand="gvSales_RowCommand"
                OnSorting="gvSales_Sorting" OnRowDataBound="gvSales_RowDataBound">
                <PagerStyle CssClass="emp-pager" />
                <Columns>
                    <asp:TemplateField HeaderText="Invoice #" SortExpression="Sale_ID">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" CommandName="ViewSale" CommandArgument='<%# Eval("Sale_ID") %>' CssClass="emp-action"><%# InvoiceNumber(Eval("Sale_ID")) %></asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Sale_DateTime" HeaderText="Date" SortExpression="Sale_DateTime" DataFormatString="{0:d MMM yyyy, HH:mm}" />
                    <asp:TemplateField HeaderText="Customer" SortExpression="CustomerName">
                        <ItemTemplate><%# CustomerCell(Eval("CustomerName")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="EmployeeName" HeaderText="Cashier" SortExpression="EmployeeName" />
                    <asp:BoundField DataField="ItemCount" HeaderText="Items" SortExpression="ItemCount" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                    <asp:BoundField DataField="PaymentMethods" HeaderText="Paid by" />
                    <asp:TemplateField HeaderText="Status" SortExpression="Sale_Status">
                        <ItemTemplate>
                            <span class='emp-badge emp-badge-<%# StatusClass(Eval("Sale_Status")) %>'><%#: Eval("Sale_Status") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Sale_TotalAmount" HeaderText="Total" SortExpression="Sale_TotalAmount" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                </Columns>
            </asp:GridView>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
