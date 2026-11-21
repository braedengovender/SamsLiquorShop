<%@ Page Title="Page Not Found" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NotFound.aspx.cs" Inherits="SamsLiquorShop.NotFound" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .nf-code {
            font-size: clamp(5rem, 18vw, 9rem);
            font-weight: 800;
            line-height: 1;
            margin: 0 0 8px;
            color: var(--accent);
            letter-spacing: -2px;
        }

        .nf-actions {
            display: flex;
            flex-wrap: wrap;
            gap: 12px;
            justify-content: center;
            margin-top: 24px;
        }

        .nf-actions a {
            text-decoration: none;
        }
    </style>

    <section class="shell login-wrap">
        <div class="glass login-card wide-card" style="text-align:center;">
            <p class="login-eyebrow">Sam's Liquor Shop</p>
            <p class="nf-code">404</p>
            <h1 class="login-heading">This bottle's gone missing</h1>
            <p class="login-subtext">The page you're looking for doesn't exist, has moved, or the link may be out of date. Let's get you back to something good.</p>

            <div class="nf-actions">
                <a href="Default.aspx" class="btn-primary">Back to Home</a>
                <a href="Shop.aspx" class="btn-ghost">Browse the Shop</a>
                <a href="Contact.aspx" class="btn-ghost">Contact Us</a>
            </div>
        </div>
    </section>
</asp:Content>