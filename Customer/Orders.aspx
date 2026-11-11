<%@ Page Title="My Orders" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Orders.aspx.cs" Inherits="SamsLiquorShop.Customer.Orders" %>
<%@ Register Src="~/Customer/CustomerNav.ascx" TagPrefix="uc" TagName="CustomerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:CustomerNav runat="server" />

        <div class="mgr-main">
            <div class="mgr-header-row">
                <h1>My Orders</h1>
                <p class="mgr-dash-subtext">Everything you've bought from Sam's Liquor Shop. Open an order to see what was in it.</p>
            </div>

            <asp:Literal ID="litMessage" runat="server" />

            <div class="ord-toolbar">
                <p class="ord-summary"><asp:Literal ID="litSummary" runat="server" /></p>
                <asp:DropDownList ID="ddlRange" runat="server" CssClass="shop-sort-select" AutoPostBack="true" OnSelectedIndexChanged="ddlRange_Changed">
                    <asp:ListItem Text="All orders" Value="0" Selected="True" />
                    <asp:ListItem Text="Last 30 days" Value="30" />
                    <asp:ListItem Text="Last 6 months" Value="180" />
                    <asp:ListItem Text="Last 12 months" Value="365" />
                </asp:DropDownList>
            </div>

            <asp:Repeater ID="rptOrders" runat="server" OnItemDataBound="rptOrders_ItemDataBound" OnItemCommand="rptOrders_ItemCommand">
                <ItemTemplate>
                    <details class="mgr-table-card ord-card">
                        <summary class="ord-summary-row">
                            <span class="ord-inv"><%# Eval("InvoiceNumber") %></span>
                            <span class="ord-date"><%# Eval("Sale_DateTime", "{0:d MMM yyyy, HH:mm}") %></span>
                            <span class="ord-items"><%# Eval("ItemCount") %> item(s)</span>
                            <span class="ord-total"><%# Eval("Sale_TotalAmount", "{0:C}") %></span>
                        </summary>

                        <div class="ord-body">
                            <table class="dash-table">
                                <thead>
                                    <tr>
                                        <th>Product</th>
                                        <th class="is-num">Qty</th>
                                        <th class="is-num">Unit price</th>
                                        <th class="is-num">Subtotal</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptLines" runat="server">
                                        <ItemTemplate>
                                            <tr>
                                                <td><%# Eval("Product_Name") %></td>
                                                <td class="is-num"><%# Eval("Quantity") %></td>
                                                <td class="is-num"><%# Eval("UnitPrice", "{0:C}") %></td>
                                                <td class="is-num"><%# Eval("Subtotal", "{0:C}") %></td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>

                            <div class="ord-actions">
                                <span class="ord-total-line">Order total: <strong><%# Eval("Sale_TotalAmount", "{0:C}") %></strong></span>
                                <asp:LinkButton runat="server" CommandName="Reorder" CommandArgument='<%# Eval("Sale_ID") %>' CssClass="btn-primary ord-reorder-btn">Buy again</asp:LinkButton>
                            </div>
                        </div>
                    </details>
                </ItemTemplate>
            </asp:Repeater>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="mgr-table-card ord-empty">
                <p><asp:Literal ID="litEmpty" runat="server" /></p>
                <a href="~/Shop.aspx" runat="server" class="btn-primary ord-reorder-btn">Browse the shop</a>
            </asp:Panel>
        </div>
    </section>
</asp:Content>
