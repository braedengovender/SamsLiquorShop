<%@ Page Title="Order Confirmed" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="OrderConfirmation.aspx.cs" Inherits="SamsLiquorShop.OrderConfirmation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .co-conf-head { display: flex; align-items: flex-end; justify-content: space-between; gap: 16px 28px; flex-wrap: wrap; margin: 32px 0 18px; }
        .co-conf-head h1 { margin: 0 0 8px; }
        .co-conf-head p { margin: 0; }
        .co-actions { display: flex; gap: 12px; flex-wrap: wrap; }
        .co-actions .btn-primary, .co-actions .btn-ghost { display: inline-block; border: none; outline: none; font: inherit; font-size: 0.88rem; font-weight: 600; text-align: center; text-decoration: none; cursor: pointer; padding: 11px 22px; }
        .co-actions .btn-ghost { background: #f2f2f2; color: var(--text-primary); }
        [data-theme="dark"] .co-actions .btn-ghost { background: rgba(255,255,255,0.08); }
        .co-card-flush { padding: 0; overflow: hidden; margin-bottom: 40px; }
        @media print {
            /* take the page chrome out of the layout completely, so no blank pages are printed */
            .site-header, .site-footer-main, .footer-newsletter-section, footer.footer-bottom, .chatbot-widget,
            .cookie-banner, .back-to-top, #newsletterModal, #pageLoader, #ageGate, .co-noprint { display: none !important; }
            .body-content { margin: 0 !important; padding: 0 !important; }
            .shell { max-width: none !important; padding: 0 !important; }
            /* the card must not clip the receipt, and the receipt flows normally instead of being absolutely positioned */
            .co-card-flush { overflow: visible !important; height: auto !important; margin: 0 !important; background: #fff !important; border: 0 !important; box-shadow: none !important; backdrop-filter: none !important; -webkit-backdrop-filter: none !important; }
            .receipt-print { position: static !important; width: 100% !important; margin: 0 !important; }
            .co-receipt-body { grid-template-columns: 1fr 1.4fr !important; }
            * { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
        }
        @media (max-width: 800px) { .co-receipt-body { grid-template-columns: 1fr !important; } }
    </style>

    <section class="shell">
        <asp:Panel ID="pnlOk" runat="server" Visible="false">
            <div class="co-conf-head co-noprint">
                <div>
                    <h1 class="login-heading">Thank you for your order!</h1>
                    <p class="login-subtext"><asp:Literal ID="litLead" runat="server" /></p>
                </div>
                <div class="co-actions">
                    <button type="button" class="btn-primary" onclick="window.print();">Print receipt</button>
                    <a href="Shop.aspx" class="btn-ghost">Continue shopping</a>
                    <a runat="server" href="~/Customer/Orders.aspx" class="btn-ghost">My orders</a>
                </div>
            </div>

            <div class="glass co-card-flush">
                <div class="receipt-print"><asp:Literal ID="litReceipt" runat="server" /></div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlMissing" runat="server" Visible="false">
            <h1 class="login-heading" style="margin: 32px 0 8px;">Order not found</h1>
            <p class="login-subtext"><asp:Literal ID="litMissing" runat="server" /></p>
            <p><a href="Shop.aspx" class="btn-primary">Continue shopping</a></p>
        </asp:Panel>

        <div id="autoPrint" runat="server" visible="false" data-autoprint="1"></div>
    </section>
</asp:Content>