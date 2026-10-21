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
                <div class="emp-header-actions">
                    <asp:Button ID="btnExport" runat="server" Text="Export CSV" CssClass="btn-ghost" OnClick="btnExport_Click" />
                    <asp:Button ID="btnAddNew" runat="server" Text="+ Add Employee" CssClass="btn-primary" OnClick="btnAddNew_Click" />
                </div>
            </div>

            <asp:UpdatePanel ID="upSummary" runat="server" UpdateMode="Always">
                <ContentTemplate>

                    <!-- SUMMARY CARDS -->
                    <div class="mgr-kpi-grid">
                        <div class="mgr-kpi-card">
                            <p class="mgr-kpi-label">Total employees</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiTotal" runat="server" /></p>
                        </div>
                        <div class="mgr-kpi-card">
                            <p class="mgr-kpi-label">Active</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiActive" runat="server" /></p>
                        </div>
                        <div class="mgr-kpi-card">
                            <p class="mgr-kpi-label">Inactive</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiInactive" runat="server" /></p>
                        </div>
                        <div class="mgr-kpi-card">
                            <p class="mgr-kpi-label">Managers</p>
                            <p class="mgr-kpi-value"><asp:Literal ID="litKpiManagers" runat="server" /></p>
                        </div>
                    </div>

                    <asp:Literal ID="litMessage" runat="server" />

                    <!-- ADD / EDIT FORM -->
                    <asp:Panel ID="pnlForm" runat="server" Visible="false" DefaultButton="btnSave">
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
                                    <asp:TextBox ID="txtIdNumber" runat="server" CssClass="login-input" MaxLength="13" data-digits="1" />
                                    <span class="field-hint">13-digit South African ID number.</span>
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtPhone.ClientID %>">Phone number</label>
                                    <asp:TextBox ID="txtPhone" runat="server" CssClass="login-input" MaxLength="12" data-phone="1" />
                                    <span class="field-hint">For example 0824055932 or +27824055932.</span>
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtHireDate.ClientID %>">Hire date</label>
                                    <asp:TextBox ID="txtHireDate" runat="server" CssClass="login-input" TextMode="Date" />
                                </div>
                                <div class="login-field">
                                    <label for="<%= txtPassword.ClientID %>">Password</label>
                                    <asp:TextBox ID="txtPassword" runat="server" CssClass="login-input" TextMode="Password" MaxLength="50" autocomplete="new-password" />
                                    <div class="emp-pw-tools">
                                        <button type="button" class="emp-linkbtn" onclick="empTogglePw()">Show / hide</button>
                                        <button type="button" class="emp-linkbtn" onclick="empGeneratePw()">Generate strong password</button>
                                    </div>
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

                </ContentTemplate>
            </asp:UpdatePanel>

            <!-- EMPLOYEE LIST -->
            <div class="mgr-table-card">

                <asp:Panel ID="pnlToolbar" runat="server" DefaultButton="btnSearch">
                    <div class="emp-toolbar">
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="login-input" placeholder="Search by name, username, email or phone..." autocomplete="off" />
                        <asp:DropDownList ID="ddlRoleFilter" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                            <asp:ListItem Text="All roles" Value="" />
                            <asp:ListItem Text="Cashier" Value="Cashier" />
                            <asp:ListItem Text="Manager" Value="Manager" />
                        </asp:DropDownList>
                        <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="login-input emp-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                            <asp:ListItem Text="All statuses" Value="" />
                            <asp:ListItem Text="Active" Value="Active" />
                            <asp:ListItem Text="Inactive" Value="Inactive" />
                        </asp:DropDownList>
                        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-primary" OnClick="btnSearch_Click" />
                        <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-ghost" OnClick="btnClear_Click" OnClientClick="empResetFilters(); return true;" />
                    </div>
                </asp:Panel>

                <asp:UpdateProgress ID="uprogress" runat="server" DisplayAfter="300">
                    <ProgressTemplate>
                        <p class="emp-loading">Updating...</p>
                    </ProgressTemplate>
                </asp:UpdateProgress>

                <asp:UpdatePanel ID="upList" runat="server" UpdateMode="Always">
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnAddNew" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnSearch" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnClear" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="ddlRoleFilter" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="ddlStatusFilter" EventName="SelectedIndexChanged" />
                    </Triggers>
                    <ContentTemplate>

                        <p class="emp-count"><asp:Literal ID="litCount" runat="server" /></p>

                        <asp:GridView ID="gvEmployees" runat="server" AutoGenerateColumns="false" CssClass="mgr-table emp-table"
                            GridLines="None" DataKeyNames="Employee_ID" AllowPaging="true" PageSize="10" AllowSorting="true"
                            EmptyDataText="No employees match your search."
                            OnPageIndexChanging="gvEmployees_PageIndexChanging" OnRowCommand="gvEmployees_RowCommand"
                            OnSorting="gvEmployees_Sorting" OnRowDataBound="gvEmployees_RowDataBound">
                            <PagerStyle CssClass="emp-pager" />
                            <Columns>
                                <asp:TemplateField HeaderText="Employee" SortExpression="Employee_LastName">
                                    <ItemTemplate>
                                        <div class="emp-person">
                                            <span class="emp-avatar"><%# Initials(Eval("Employee_FirstName"), Eval("Employee_LastName")) %></span>
                                            <div>
                                                <strong><%#: Eval("Employee_FirstName") %> <%#: Eval("Employee_LastName") %></strong>
                                                <span class="emp-sub">@<%#: Eval("Employee_Username") %></span>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Contact" SortExpression="Employee_EmailAddress">
                                    <ItemTemplate>
                                        <%#: Eval("Employee_EmailAddress") %>
                                        <span class="emp-sub"><%#: Eval("Employee_PhoneNumber") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Role" SortExpression="Employee_Role">
                                    <ItemTemplate>
                                        <span class="emp-badge emp-badge-role"><%#: Eval("Employee_Role") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Status" SortExpression="Employee_Status">
                                    <ItemTemplate>
                                        <span class='emp-badge emp-badge-<%# Eval("Employee_Status").ToString().Trim().ToLower() %>'><%#: Eval("Employee_Status") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Hired" SortExpression="Employee_HireDate">
                                    <ItemTemplate>
                                        <%# Convert.ToDateTime(Eval("Employee_HireDate")).ToString("dd MMM yyyy") %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" CommandName="EditEmp" CommandArgument='<%# Eval("Employee_ID") %>' CssClass="emp-action">Edit</asp:LinkButton>
                                        <asp:LinkButton runat="server" CommandName="ToggleEmp" CommandArgument='<%# Eval("Employee_ID") %>' CssClass="emp-action"
                                            Text='<%# Eval("Employee_Status").ToString().Trim() == "Active" ? "Deactivate" : "Reactivate" %>'
                                            OnClientClick='<%# DeactivateJs(Eval("Employee_Status"), Eval("Employee_FirstName"), Eval("Employee_LastName")) %>' />
                                        <asp:LinkButton runat="server" CommandName="DeleteEmp" CommandArgument='<%# Eval("Employee_ID") %>' CssClass="emp-action emp-action-danger"
                                            OnClientClick='<%# DeleteJs(Eval("Employee_FirstName"), Eval("Employee_LastName")) %>'>Delete</asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>

                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>

        </div>
    </section>

    <!-- CONFIRMATION DIALOG -->
    <div class="emp-modal" id="empModal" aria-hidden="true">
        <div class="emp-modal-card" role="dialog" aria-modal="true" aria-labelledby="empModalTitle">
            <h3 id="empModalTitle">Confirm</h3>
            <p id="empModalText"></p>
            <div class="emp-modal-actions">
                <button type="button" class="btn-ghost" onclick="empModalClose()">Cancel</button>
                <button type="button" class="btn-primary" id="empModalOk">Confirm</button>
            </div>
        </div>
    </div>

    <script>
        // ----- Live search (waits 350ms after typing stops) -----
        var empSearchTimer = null;
        document.addEventListener('input', function (e) {
            var t = e.target;
            if (!t) return;

            if (t.id === '<%= txtSearch.ClientID %>') {
                clearTimeout(empSearchTimer);
                empSearchTimer = setTimeout(function () {
                    document.getElementById('<%= btnSearch.ClientID %>').click();
                }, 350);
            }

            if (t.getAttribute && t.getAttribute('data-digits')) {
                t.value = t.value.replace(/[^0-9]/g, '');
            }
            if (t.getAttribute && t.getAttribute('data-phone')) {
                t.value = t.value.replace(/(?!^\+)[^0-9]/g, '');
            }
        });

        function empResetFilters() {
            document.getElementById('<%= txtSearch.ClientID %>').value = '';
            document.getElementById('<%= ddlRoleFilter.ClientID %>').selectedIndex = 0;
            document.getElementById('<%= ddlStatusFilter.ClientID %>').selectedIndex = 0;
        }

        // ----- Password helpers -----
        function empTogglePw() {
            var p = document.getElementById('<%= txtPassword.ClientID %>');
            if (p) p.type = (p.type === 'password') ? 'text' : 'password';
        }

        function empGeneratePw() {
            var chars = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%';
            var arr = new Uint32Array(12);
            window.crypto.getRandomValues(arr);
            var pw = '';
            for (var i = 0; i < arr.length; i++) pw += chars.charAt(arr[i] % chars.length);
            var p = document.getElementById('<%= txtPassword.ClientID %>');
            if (p) { p.value = pw; p.type = 'text'; }
        }

        // ----- Confirmation dialog -----
        var empPendingAction = null;

        function empConfirm(link, title, text, okLabel, danger) {
            empPendingAction = link.getAttribute('href');
            document.getElementById('empModalTitle').textContent = title;
            document.getElementById('empModalText').textContent = text;
            var ok = document.getElementById('empModalOk');
            ok.textContent = okLabel;
            ok.className = 'btn-primary' + (danger ? ' emp-btn-danger' : '');
            document.getElementById('empModal').classList.add('is-open');
            return false;
        }

        function empModalClose() {
            document.getElementById('empModal').classList.remove('is-open');
            empPendingAction = null;
        }

        document.getElementById('empModalOk').addEventListener('click', function () {
            var js = empPendingAction;
            empModalClose();
            if (js) { new Function(js.replace(/^javascript:/i, ''))(); }
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') empModalClose();
        });

        // ----- Success messages fade away -----
        function empAutoDismiss() {
            var a = document.querySelector('.emp-alert-success');
            if (!a || a.getAttribute('data-timer')) return;
            a.setAttribute('data-timer', '1');
            setTimeout(function () {
                a.classList.add('is-hiding');
                setTimeout(function () { if (a.parentNode) a.parentNode.removeChild(a); }, 450);
            }, 5000);
        }

        if (window.Sys && Sys.Application) {
            Sys.Application.add_load(empAutoDismiss);
        } else {
            document.addEventListener('DOMContentLoaded', empAutoDismiss);
        }
    </script>
</asp:Content>