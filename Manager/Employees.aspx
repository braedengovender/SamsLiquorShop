<%@ Page Title="Manage Employees" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Employees.aspx.cs" Inherits="SamsLiquorShop.Manager.Employees" %>
<%@ Register Src="~/Manager/ManagerNav.ascx" TagPrefix="uc" TagName="ManagerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout">
        <uc:ManagerNav runat="server" />

        <div class="mgr-main">

            <div class="mgr-header-row emp-header-flex">
                <div>
                    <h1>Manage Employees</h1>
                    <p class="mgr-dash-subtext">Add, update, deactivate or remove staff accounts.</p>
                </div>
                <asp:Button ID="btnAddNew" runat="server" Text="+ Add Employee" CssClass="btn-primary" OnClick="btnAddNew_Click" />
            </div>

            <asp:Literal ID="litMessage" runat="server" />

            <!-- ADD / EDIT FORM -->
            <asp:Panel ID="pnlForm" runat="server" Visible="false">
                <div id="empFormCard" class="mgr-table-card emp-form-card">
                    <h3><asp:Literal ID="litFormTitle" runat="server" /></h3>
                    <asp:HiddenField ID="hfEmployeeId" runat="server" />

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
                        </div>
                        <div class="login-field">
                            <label for="<%= txtEmail.ClientID %>">Email address</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="login-input" MaxLength="100" />
                        </div>
                        <div class="login-field">
                            <label for="<%= txtIdNumber.ClientID %>">ID number</label>
                            <asp:TextBox ID="txtIdNumber" runat="server" CssClass="login-input" MaxLength="13" />
                            <span class="field-hint">13-digit South African ID number.</span>
                        </div>
                        <div class="login-field">
                            <label for="<%= txtPhone.ClientID %>">Phone number</label>
                            <asp:TextBox ID="txtPhone" runat="server" CssClass="login-input" MaxLength="12" />
                            <span class="field-hint">For example 0824055932 or +27824055932.</span>
                        </div>
                        <div class="login-field">
                            <label for="<%= txtHireDate.ClientID %>">Hire date</label>
                            <asp:TextBox ID="txtHireDate" runat="server" CssClass="login-input" TextMode="Date" />
                        </div>
                        <div class="login-field">
                            <label for="<%= txtPassword.ClientID %>">Password</label>
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="login-input" TextMode="Password" MaxLength="50" autocomplete="new-password" />
                            <span class="field-hint"><asp:Literal ID="litPasswordHint" runat="server" /></span>
                        </div>
                        <div class="login-field">
                            <label for="<%= ddlRole.ClientID %>">Role</label>
                            <asp:DropDownList ID="ddlRole" runat="server" CssClass="login-input">
                                <asp:ListItem Text="Cashier" Value="Cashier" />
                                <asp:ListItem Text="Manager" Value="Manager" />
                            </asp:DropDownList>
                        </div>
                        <div class="login-field">
                            <label for="<%= ddlStatus.ClientID %>">Status</label>
                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="login-input">
                                <asp:ListItem Text="Active" Value="Active" />
                                <asp:ListItem Text="Inactive" Value="Inactive" />
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div class="emp-form-actions">
                        <asp:Button ID="btnSave" runat="server" Text="Save Employee" CssClass="btn-primary" OnClick="btnSave_Click" />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-ghost" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </asp:Panel>

            <!-- EMPLOYEE LIST -->
            <div class="mgr-table-card">
                <div class="emp-toolbar">
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="login-input" placeholder="Search name, username or email..." />
                    <asp:DropDownList ID="ddlRoleFilter" runat="server" CssClass="login-input emp-select">
                        <asp:ListItem Text="All roles" Value="" />
                        <asp:ListItem Text="Cashier" Value="Cashier" />
                        <asp:ListItem Text="Manager" Value="Manager" />
                    </asp:DropDownList>
                    <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="login-input emp-select">
                        <asp:ListItem Text="All statuses" Value="" />
                        <asp:ListItem Text="Active" Value="Active" />
                        <asp:ListItem Text="Inactive" Value="Inactive" />
                    </asp:DropDownList>
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-primary" OnClick="btnSearch_Click" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-ghost" OnClick="btnClear_Click" />
                </div>

                <p class="emp-count"><asp:Literal ID="litCount" runat="server" /></p>

                <asp:GridView ID="gvEmployees" runat="server" AutoGenerateColumns="false" CssClass="mgr-table emp-table"
                    GridLines="None" DataKeyNames="Employee_ID" AllowPaging="true" PageSize="10"
                    EmptyDataText="No employees match your search."
                    OnPageIndexChanging="gvEmployees_PageIndexChanging" OnRowCommand="gvEmployees_RowCommand">
                    <PagerStyle CssClass="emp-pager" />
                    <Columns>
                        <asp:TemplateField HeaderText="Employee">
                            <ItemTemplate>
                                <strong><%#: Eval("Employee_FirstName") %> <%#: Eval("Employee_LastName") %></strong>
                                <span class="emp-sub">@<%#: Eval("Employee_Username") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Employee_EmailAddress" HeaderText="Email" />
                        <asp:TemplateField HeaderText="Role">
                            <ItemTemplate>
                                <span class="emp-badge emp-badge-role"><%#: Eval("Employee_Role") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='emp-badge emp-badge-<%# Eval("Employee_Status").ToString().Trim().ToLower() %>'><%#: Eval("Employee_Status") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <asp:LinkButton runat="server" CommandName="EditEmp" CommandArgument='<%# Eval("Employee_ID") %>' CssClass="emp-action">Edit</asp:LinkButton>
                                <asp:LinkButton runat="server" CommandName="ToggleEmp" CommandArgument='<%# Eval("Employee_ID") %>' CssClass="emp-action"
                                    Text='<%# Eval("Employee_Status").ToString().Trim() == "Active" ? "Deactivate" : "Reactivate" %>' />
                                <asp:LinkButton runat="server" CommandName="DeleteEmp" CommandArgument='<%# Eval("Employee_ID") %>' CssClass="emp-action emp-action-danger"
                                    OnClientClick="return confirm('Permanently delete this employee? This cannot be undone.');">Delete</asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

        </div>
    </section>
</asp:Content>