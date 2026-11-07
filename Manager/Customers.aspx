<%@ Page Title="Manage Customers" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Customers.aspx.cs" Inherits="SamsLiquorShop.Manager.Customers" %>
<%@ Register Src="~/Manager/ManagerNav.ascx" TagPrefix="uc" TagName="ManagerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:ManagerNav runat="server" />

        <div class="mgr-main">

            <div class="mgr-header-row emp-header-flex">
                <div>
                    <h1>Manage Customers</h1>
                    <p class="mgr-dash-subtext">Look up customers, update their details, and see what they've bought.</p>
                </div>
                <div class="emp-header-actions">
                    <asp:Button ID="btnExport" runat="server" Text="Export CSV" CssClass="btn-ghost" OnClick="btnExport_Click" />
                </div>
            </div>

            <asp:UpdatePanel ID="upMain" runat="server">
                <ContentTemplate>

                    <!-- SUMMARY CARDS -->
                    <div class="mgr-kpi-grid">
                        <div class="mgr-kpi-card">
                            <span class="dash-kpi-icon is-blue dash-icon-receipt" aria-hidden="true"></span>
                            <p class="mgr-kpi-label">Total customers</p>
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
                            <p class="mgr-kpi-label">Joined this month</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiNew" runat="server" /></p>
                        </div>
                    </div>

                    <asp:Literal ID="litMessage" runat="server" />

                    <!-- EDIT FORM -->
                    <asp:Panel ID="pnlForm" runat="server" Visible="false" DefaultButton="btnSave">
                        <div id="custFormCard" class="mgr-table-card emp-form-card">
                            <div class="dash-card-head">
                                <h3><asp:Literal ID="litFormTitle" runat="server" /></h3>
                                <span class="dash-card-meta"><asp:Literal ID="litFormPoints" runat="server" /></span>
                            </div>
                            <asp:HiddenField ID="hfCustomerId" runat="server" />

                            <div class="emp-form-grid">
                                <div class="login-field">
                                    <label for="<%= txtFirstName.ClientID %>">First name</label>
                                    <asp:TextBox ID="txtFirstName" runat="server" CssClass="login-input" MaxLength="50" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtLastName.ClientID %>">Last name</label>
                                    <asp:TextBox ID="txtLastName" runat="server" CssClass="login-input" MaxLength="50" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtUsername.ClientID %>">Username</label>
                                    <asp:TextBox ID="txtUsername" runat="server" CssClass="login-input" MaxLength="30" />
                                    <span class="field-hint">Leave blank for in-store customers without a website account.</span>
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtEmail.ClientID %>">Email address</label>
                                    <asp:TextBox ID="txtEmail" runat="server" CssClass="login-input" MaxLength="100" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtIdNumber.ClientID %>">ID number</label>
                                    <asp:TextBox ID="txtIdNumber" runat="server" CssClass="login-input" MaxLength="13" data-digits="1" />
                                    <span class="field-hint">13-digit South African ID number.</span>
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtPhone.ClientID %>">Phone number</label>
                                    <asp:TextBox ID="txtPhone" runat="server" CssClass="login-input" MaxLength="12" data-phone="1" />
                                    <span class="field-hint">For example 0821234567 or +27821234567.</span>
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtUnitNumber.ClientID %>">Unit number</label>
                                    <asp:TextBox ID="txtUnitNumber" runat="server" CssClass="login-input" MaxLength="20" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtUnitName.ClientID %>">Complex / building</label>
                                    <asp:TextBox ID="txtUnitName" runat="server" CssClass="login-input" MaxLength="100" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtStreetNumber.ClientID %>">Street number</label>
                                    <asp:TextBox ID="txtStreetNumber" runat="server" CssClass="login-input" MaxLength="20" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtStreetName.ClientID %>">Street name</label>
                                    <asp:TextBox ID="txtStreetName" runat="server" CssClass="login-input" MaxLength="100" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtSuburb.ClientID %>">Suburb</label>
                                    <asp:TextBox ID="txtSuburb" runat="server" CssClass="login-input" MaxLength="100" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtCity.ClientID %>">City</label>
                                    <asp:TextBox ID="txtCity" runat="server" CssClass="login-input" MaxLength="100" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtPostalCode.ClientID %>">Postal code</label>
                                    <asp:TextBox ID="txtPostalCode" runat="server" CssClass="login-input" MaxLength="4" data-digits="1" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= ddlProvince.ClientID %>">Province</label>
                                    <asp:DropDownList ID="ddlProvince" runat="server" CssClass="login-input">
                                        <asp:ListItem Text="Not specified" Value="" />
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
                                <div class="login-field">
                                    <label for="<%= ddlStatus.ClientID %>">Status</label>
                                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="login-input">
                                        <asp:ListItem Text="Active" Value="Active" />
                                        <asp:ListItem Text="Inactive" Value="Inactive" />
                                    </asp:DropDownList>
                                    <span class="field-hint">Inactive customers can't log in and don't earn loyalty points.</span>
                                </div>
                            </div>

                            <div class="emp-form-actions">
                                <asp:Button ID="btnSave" runat="server" Text="Save Customer" CssClass="btn-primary" OnClick="btnSave_Click" />
                                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-ghost" OnClick="btnCancel_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- PURCHASE HISTORY -->
                    <asp:Panel ID="pnlHistory" runat="server" Visible="false">
                        <div id="custHistoryCard" class="mgr-table-card emp-form-card">
                            <div class="dash-card-head">
                                <h3>Purchase history: <asp:Literal ID="litHistoryName" runat="server" /></h3>
                                <asp:LinkButton ID="btnCloseHistory" runat="server" CssClass="emp-action" OnClick="btnCloseHistory_Click">Close</asp:LinkButton>
                            </div>

                            <div class="cust-history-stats">
                                <div><span class="mgr-kpi-label">Total spent</span><strong><asp:Literal ID="litHistorySpent" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Purchases</span><strong><asp:Literal ID="litHistoryOrders" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Average purchase</span><strong><asp:Literal ID="litHistoryAvg" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Loyalty points</span><strong><asp:Literal ID="litHistoryPoints" runat="server" /></strong></div>
                                <div><span class="mgr-kpi-label">Last purchase</span><strong><asp:Literal ID="litHistoryLast" runat="server" /></strong></div>
                            </div>

                            <asp:GridView ID="gvHistory" runat="server" AutoGenerateColumns="false" CssClass="dash-table" GridLines="None"
                                EmptyDataText="This customer hasn't bought anything yet." EmptyDataRowStyle-CssClass="dash-table-empty">
                                <Columns>
                                    <asp:TemplateField HeaderText="Invoice #">
                                        <ItemTemplate>
                                            <a class="emp-action" href='<%# "Sales.aspx?sale=" + Eval("Sale_ID") %>'><%# InvoiceNumber(Eval("Sale_ID")) %></a>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Sale_DateTime" HeaderText="Date" DataFormatString="{0:d MMM yyyy, HH:mm}" />
                                    <asp:BoundField DataField="ItemCount" HeaderText="Items" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                    <asp:BoundField DataField="PaymentMethods" HeaderText="Paid by" />
                                    <asp:BoundField DataField="Sale_LoyaltyPointsEarned" HeaderText="Points earned" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                    <asp:BoundField DataField="Sale_TotalAmount" HeaderText="Total" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                </Columns>
                            </asp:GridView>
                        </div>
                    </asp:Panel>

                    <!-- CUSTOMER LIST -->
                    <div class="mgr-table-card">
                        <asp:Panel ID="pnlToolbar" runat="server" DefaultButton="btnSearch">
                            <div class="emp-toolbar">
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="login-input" placeholder="Search by name, username, email, phone or ID number..." autocomplete="off" />
                                <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                    <asp:ListItem Text="All statuses" Value="" />
                                    <asp:ListItem Text="Active" Value="Active" />
                                    <asp:ListItem Text="Inactive" Value="Inactive" />
                                </asp:DropDownList>
                                <asp:DropDownList ID="ddlTypeFilter" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                    <asp:ListItem Text="All customers" Value="" />
                                    <asp:ListItem Text="Website account" Value="online" />
                                    <asp:ListItem Text="In-store only" Value="instore" />
                                </asp:DropDownList>
                                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-primary" OnClick="btnSearch_Click" />
                                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-ghost" OnClick="btnClear_Click" />
                            </div>
                        </asp:Panel>

                        <p class="emp-count"><asp:Literal ID="litCount" runat="server" /></p>

                        <asp:GridView ID="gvCustomers" runat="server" AutoGenerateColumns="false" CssClass="dash-table emp-table"
                            GridLines="None" DataKeyNames="Customer_ID" AllowPaging="true" PageSize="10" AllowSorting="true"
                            EmptyDataText="No customers match your search." EmptyDataRowStyle-CssClass="dash-table-empty"
                            OnPageIndexChanging="gvCustomers_PageIndexChanging" OnRowCommand="gvCustomers_RowCommand"
                            OnSorting="gvCustomers_Sorting" OnRowDataBound="gvCustomers_RowDataBound">
                            <PagerStyle CssClass="emp-pager" />
                            <Columns>
                                <asp:TemplateField HeaderText="Customer" SortExpression="Customer_LastName">
                                    <ItemTemplate>
                                        <div class="emp-person">
                                            <span class="emp-avatar"><%# Initials(Eval("Customer_FirstName"), Eval("Customer_LastName")) %></span>
                                            <div>
                                                <strong><%#: Eval("Customer_FirstName") %> <%#: Eval("Customer_LastName") %></strong>
                                                <%# AccountLabel(Eval("Customer_Username")) %>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Contact" SortExpression="Customer_EmailAddress" ItemStyle-CssClass="is-wrap">
                                    <ItemTemplate>
                                        <%#: Eval("Customer_EmailAddress") %>
                                        <span class="emp-sub"><%#: Eval("Customer_PhoneNumber") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="OrderCount" HeaderText="Purchases" SortExpression="OrderCount" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                <asp:BoundField DataField="TotalSpent" HeaderText="Spent" SortExpression="TotalSpent" DataFormatString="{0:C}" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                <asp:BoundField DataField="Customer_LoyaltyPointsBalance" HeaderText="Points" SortExpression="Customer_LoyaltyPointsBalance" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                <asp:TemplateField HeaderText="Status" SortExpression="Customer_Status">
                                    <ItemTemplate>
                                        <span class='emp-badge emp-badge-<%# StatusClass(Eval("Customer_Status")) %>'><%#: Eval("Customer_Status") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" CommandName="ViewCust" CommandArgument='<%# Eval("Customer_ID") %>' CssClass="emp-action">History</asp:LinkButton>
                                        <asp:LinkButton runat="server" CommandName="EditCust" CommandArgument='<%# Eval("Customer_ID") %>' CssClass="emp-action">Edit</asp:LinkButton>
                                        <asp:LinkButton runat="server" CommandName="ToggleCust" CommandArgument='<%# Eval("Customer_ID") %>' CssClass="emp-action"
                                            Text='<%# StatusClass(Eval("Customer_Status")) == "active" ? "Deactivate" : "Reactivate" %>'
                                            OnClientClick='<%# DeactivateJs(Eval("Customer_Status"), Eval("Customer_FirstName"), Eval("Customer_LastName")) %>' />
                                        <asp:LinkButton runat="server" CommandName="DeleteCust" CommandArgument='<%# Eval("Customer_ID") %>' CssClass="emp-action emp-action-danger"
                                            Visible='<%# Convert.ToInt32(Eval("OrderCount")) == 0 %>'
                                            OnClientClick='<%# DeleteJs(Eval("Customer_FirstName"), Eval("Customer_LastName")) %>'>Delete</asp:LinkButton>
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
