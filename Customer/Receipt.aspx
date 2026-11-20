<%@ Page Title="Receipt" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Receipt.aspx.cs" Inherits="SamsLiquorShop.Customer.Receipt" %>
<%@ Register Src="~/Customer/CustomerNav.ascx" TagPrefix="uc" TagName="CustomerNav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <section class="shell mgr-layout dash-glass">
        <uc:CustomerNav runat="server" />

        <div class="mgr-main">
            <div class="mgr-header-row emp-header-flex receipt-no-print">
                <div>
                    <h1>Receipt</h1>
                    <p class="mgr-dash-subtext"><a href="Orders.aspx" class="emp-action">&larr; Back to My Orders</a></p>
                </div>
                <asp:PlaceHolder ID="phActions" runat="server">
                    <div class="emp-header-actions">
                        <button type="button" class="btn-ghost" onclick="window.print()">Print</button>
                        <button type="button" class="btn-ghost" onclick="window.print()" title="Choose &quot;Save as PDF&quot; in the print window">Download PDF</button>
                        <asp:Button ID="btnEmail" runat="server" Text="Email receipt" CssClass="btn-primary" OnClick="btnEmail_Click" />
                    </div>
                </asp:PlaceHolder>
            </div>

            <div class="receipt-no-print">
                <asp:Literal ID="litMessage" runat="server" />
                <asp:PlaceHolder ID="phPdfTip" runat="server">
                    <p class="receipt-tip">To download a PDF, click <strong>Download PDF</strong> and choose <strong>Save as PDF</strong> as the printer.</p>
                </asp:PlaceHolder>
            </div>

            <%-- Receipt HTML comes from Helpers/ReceiptHelper.cs (the same HTML is used for the emailed copy).
                 data-autoprint opens the print window straight away for links ending in &print=1 (handled in Site.Master). --%>
            <div class="receipt-print" id="receiptPrint" runat="server">
                <asp:Literal ID="litReceipt" runat="server" />
            </div>
        </div>
    </section>
</asp:Content>
