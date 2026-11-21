<%@ Page Title="Manage Categories" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Categories.aspx.cs" Inherits="SamsLiquorShop.Manager.Categories" %>
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

                    <!-- CATEGORY LIST -->
                    <div class="mgr-table-card">
                        <asp:Panel ID="pnlToolbar" runat="server" DefaultButton="btnSearch">
                            <div class="emp-toolbar">
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="login-input" placeholder="Search by category name or description..." autocomplete="off" />
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
                                    <ItemTemplate><strong><%#: Eval("Category_Name") %></strong></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Description" ItemStyle-CssClass="is-wrap">
                                    <ItemTemplate><%# DescriptionText(Eval("Category_Description")) %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="ProductCount" HeaderText="Products" SortExpression="ProductCount" ItemStyle-CssClass="is-num" HeaderStyle-CssClass="is-num" />
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" CommandName="EditCategory" CommandArgument='<%# Eval("Category_ID") %>' CssClass="emp-action" CausesValidation="false">Edit</asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                </ContentTemplate>
            </asp:UpdatePanel>

        </div>
    </section>
</asp:Content>
