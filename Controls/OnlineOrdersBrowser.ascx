<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OnlineOrdersBrowser.ascx.cs" Inherits="SamsLiquorShop.Controls.OnlineOrdersBrowser" %>

<div class="mgr-header-row">
    <h1>Manage Online Orders</h1>
    <p class="mgr-dash-subtext">Orders placed on the website. Move them along step by step and the customer is emailed.</p>
</div>

<asp:UpdatePanel ID="upOrders" runat="server">
    <ContentTemplate>
        <asp:Literal ID="litMessage" runat="server" />

        <div class="mgr-kpi-grid">
            <div class="mgr-kpi-card">
                <div class="dash-kpi-icon is-blue dash-icon-receipt"></div>
                <p class="mgr-kpi-label">New orders</p>
                <p class="mgr-kpi-value"><asp:Literal ID="litKpiNew" runat="server" /></p>
            </div>
            <div class="mgr-kpi-card">
                <div class="dash-kpi-icon is-amber dash-icon-bag"></div>
                <p class="mgr-kpi-label">In progress</p>
                <p class="mgr-kpi-value"><asp:Literal ID="litKpiLive" runat="server" /></p>
            </div>
            <div class="mgr-kpi-card">
                <div class="dash-kpi-icon is-red dash-icon-alert"></div>
                <p class="mgr-kpi-label">Due today</p>
                <p class="mgr-kpi-value"><asp:Literal ID="litKpiToday" runat="server" /></p>
            </div>
            <div class="mgr-kpi-card">
                <div class="dash-kpi-icon is-blue dash-icon-cash"></div>
                <p class="mgr-kpi-label">Ready for collection</p>
                <p class="mgr-kpi-value"><asp:Literal ID="litKpiReady" runat="server" /></p>
            </div>
        </div>

        <%-- ORDER DETAIL --%>
        <asp:Panel ID="pnlDetail" runat="server" Visible="false">
            <div id="obDetailCard" class="mgr-table-card emp-form-card">
                <div class="dash-card-head ob-head">
                    <div class="ob-title">
                        <h3><asp:Literal ID="litTitle" runat="server" /></h3>
                        <asp:Literal ID="litHeadBadge" runat="server" />
                    </div>
                    <asp:Button ID="btnCloseDetail" runat="server" Text="Close" CssClass="btn-ghost" OnClick="btnCloseDetail_Click" CausesValidation="false" />
                </div>

                <asp:HiddenField ID="hfOrderId" runat="server" />
                <asp:HiddenField ID="hfNext" runat="server" />

                <asp:Literal ID="litProgress" runat="server" />

                <asp:Panel ID="pnlActions" runat="server" Visible="false" CssClass="ob-box ob-actions">
                    <h4>Update this order</h4>
                    <div class="ob-fields">
                        <asp:Panel ID="pnlCode" runat="server" Visible="false" CssClass="login-field">
                            <label for="<%= txtCode.ClientID %>">Customer's collection code</label>
                            <asp:TextBox ID="txtCode" runat="server" CssClass="login-input" MaxLength="10" autocomplete="off" />
                            <span class="field-hint">Ask for the code in their email and check it matches.</span>
                        </asp:Panel>
                        <div class="login-field ob-note">
                            <label for="<%= txtNote.ClientID %>">Note <span class="register-optional">(optional, required when cancelling)</span></label>
                            <asp:TextBox ID="txtNote" runat="server" CssClass="login-input" MaxLength="250" placeholder="e.g. Packed by Sam, left with security guard" />
                        </div>
                    </div>
                    <asp:Panel ID="pnlIdCheck" runat="server" Visible="false">
                        <asp:CheckBox ID="chkIdChecked" runat="server" CssClass="ob-check" Text="I checked the customer's ID and they are 18 or older." />
                    </asp:Panel>
                    <div class="ob-buttons">
                        <asp:Button ID="btnAdvance" runat="server" CssClass="btn-primary" OnClick="btnAdvance_Click" CausesValidation="false" Visible="false" />
                        <asp:Button ID="btnCancelOrder" runat="server" Text="Cancel order" CssClass="btn-ghost emp-btn-danger" OnClick="btnCancelOrder_Click" CausesValidation="false" Visible="false"
                            OnClientClick="return confirm('Cancel this order? Stock is put back, loyalty points are returned and the customer is emailed. This cannot be undone.');" />
                    </div>
                </asp:Panel>

                <asp:Literal ID="litDetail" runat="server" />
                <asp:Literal ID="litHistory" runat="server" />
            </div>
        </asp:Panel>

        <%-- ORDER LIST --%>
        <div class="mgr-table-card">
            <asp:Panel ID="pnlToolbar" runat="server" DefaultButton="btnSearch">
                <div class="emp-toolbar">
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="login-input" placeholder="Search invoice, name, phone, email or collection code..." autocomplete="off" MaxLength="100" />
                    <asp:DropDownList ID="ddlView" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                        <asp:ListItem Value="live" Text="In progress" Selected="True" />
                        <asp:ListItem Value="today" Text="Due today" />
                        <asp:ListItem Value="ready" Text="Ready for collection" />
                        <asp:ListItem Value="done" Text="Delivered / collected" />
                        <asp:ListItem Value="ended" Text="Cancelled / expired" />
                        <asp:ListItem Value="all" Text="All orders" />
                    </asp:DropDownList>
                    <asp:DropDownList ID="ddlMethod" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                        <asp:ListItem Value="" Text="Delivery + collection" Selected="True" />
                        <asp:ListItem Value="Delivery" Text="Delivery only" />
                        <asp:ListItem Value="Collection" Text="Collection only" />
                    </asp:DropDownList>
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-primary" OnClick="btnSearch_Click" CausesValidation="false" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-ghost" OnClick="btnClear_Click" CausesValidation="false" />
                </div>
            </asp:Panel>

            <p class="emp-count"><asp:Literal ID="litCount" runat="server" /></p>

            <asp:GridView ID="gvOrders" runat="server" AutoGenerateColumns="false" CssClass="dash-table emp-table" GridLines="None"
                DataKeyNames="OnlineOrder_ID" AllowPaging="true" PageSize="10"
                EmptyDataText="No online orders match this view." EmptyDataRowStyle-CssClass="dash-table-empty"
                OnPageIndexChanging="gvOrders_PageIndexChanging" OnRowCommand="gvOrders_RowCommand">
                <PagerStyle CssClass="emp-pager" />
                <Columns>
                    <asp:TemplateField HeaderText="Order">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" CommandName="ManageOrder" CommandArgument='<%# Eval("OnlineOrder_ID") %>' CssClass="disc-name-link" CausesValidation="false" ToolTip="Open this order"><strong><%# Eval("InvoiceNumber") %></strong></asp:LinkButton>
                            <span class="emp-sub"><%# Eval("Sale_DateTime", "{0:d MMM yyyy, HH:mm}") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Customer">
                        <ItemTemplate>
                            <%#: Eval("CustomerName") %>
                            <span class="emp-sub"><%#: Eval("Phone") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Delivery / collection">
                        <ItemTemplate>
                            <%# TypeText(Eval("Method")) %>
                            <span class="emp-sub"><%# SlotShort(Eval("Method"), Eval("Slot")) %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Total" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num">
                        <ItemTemplate><%# Money(Eval("Sale_TotalAmount")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><%# StatusHtml(Eval("CurrentStatus"), Eval("Method"), Eval("Slot")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" CommandName="ManageOrder" CommandArgument='<%# Eval("OnlineOrder_ID") %>' CssClass="emp-action" CausesValidation="false">Manage</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>