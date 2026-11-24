<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BlackFridayBanner.ascx.cs" Inherits="SamsLiquorShop.Controls.BlackFridayBanner" %>
<%-- Black Friday "10% off wines" banner. Clicking it opens the shop filtered to wines on sale.
     Dates and link live in BlackFridayBanner.ascx.cs.
     Layout="Card" (default): a card in the page.  Layout="Bar": a thin strip stuck to the bottom of the window, with an x to close it. --%>
<asp:PlaceHolder ID="phBanner" runat="server">

<asp:PlaceHolder ID="phCard" runat="server">
<style>
    .bf-banner { position: relative; margin: 20px auto; border-radius: var(--radius-lg, 22px); overflow: hidden; color: #fff;
        background: radial-gradient(120% 140% at 100% 0%, #7b1530 0%, rgba(123,21,48,0) 55%), linear-gradient(120deg, #0b0b0f 0%, #1b1218 60%, #3a0d1c 100%);
        box-shadow: 0 14px 34px rgba(20, 8, 14, .28); }
    .bf-link { display: flex; align-items: center; gap: 22px; padding: 22px 28px; color: inherit; text-decoration: none; }
    .bf-link:hover, .bf-link:focus { color: inherit; text-decoration: none; }
    .bf-link:focus-visible { outline: 3px solid #ffd60a; outline-offset: -3px; border-radius: inherit; }
    .bf-badge { flex: 0 0 auto; display: flex; flex-direction: column; align-items: center; justify-content: center; width: 92px; height: 92px; border-radius: 50%;
        background: #ffd60a; color: #1d1d1f; font-weight: 800; line-height: 1; box-shadow: 0 0 0 6px rgba(255,214,10,.18); }
    .bf-badge strong { font-size: 2rem; letter-spacing: -.02em; }
    .bf-badge span { font-size: .72rem; letter-spacing: .14em; margin-top: 3px; }
    .bf-text { flex: 1; min-width: 0; }
    .bf-eyebrow { display: block; margin: 0 0 4px; font-size: .74rem; font-weight: 700; letter-spacing: .22em; text-transform: uppercase; color: #ffd60a; }
    .bf-heading { display: block; margin: 0; font-size: clamp(1.25rem, 2.6vw, 1.75rem); font-weight: 700; line-height: 1.2; color: #fff; }
    .bf-sub { display: block; margin: 6px 0 0; font-size: .9rem; color: rgba(255,255,255,.78); }
    .bf-cta { flex: 0 0 auto; padding: 12px 22px; border-radius: 999px; background: #fff; color: #1d1d1f; font-weight: 700; font-size: .92rem; white-space: nowrap; transition: transform .15s, background .15s; }
    .bf-link:hover .bf-cta { background: #ffd60a; transform: translateX(2px); }
    @media (max-width: 640px) {
        .bf-link { flex-wrap: wrap; gap: 14px; padding: 20px; }
        .bf-badge { width: 70px; height: 70px; }
        .bf-badge strong { font-size: 1.5rem; }
        .bf-cta { width: 100%; text-align: center; }
    }
</style>
<div class="shell">
    <div class="bf-banner" role="region" aria-label="Black Friday wine special">
        <a class="bf-link" href="<%= ResolveUrl(ShopUrl) %>">
            <span class="bf-badge" aria-hidden="true"><strong>10%</strong><span>OFF</span></span>
            <span class="bf-text">
                <span class="bf-eyebrow">Black Friday</span>
                <span class="bf-heading">10% off wines this Black Friday</span>
                <span class="bf-sub">Red, white, ros&eacute; and bubbly. No code needed. Ends <%= EndText %>.</span>
            </span>
            <span class="bf-cta">Shop wine specials &rarr;</span>
        </a>
    </div>
</div>
</asp:PlaceHolder>

<asp:PlaceHolder ID="phBar" runat="server" Visible="false">
<style>
    :root { --bf-bar-h: 38px; }
    .bf-bar { position: fixed; left: 0; right: 0; bottom: 0; z-index: 980; height: var(--bf-bar-h);
        padding-bottom: env(safe-area-inset-bottom, 0px); box-sizing: content-box;
        display: flex; align-items: center; color: #fff; font-size: .84rem; line-height: 1;
        background: linear-gradient(90deg, #0b0b0f 0%, #2a0c17 55%, #5c1026 100%);
        box-shadow: 0 -4px 16px rgba(0, 0, 0, .18); }
    .bf-bar[hidden] { display: none !important; }
    .bf-bar-link { flex: 1; min-width: 0; height: 100%; display: flex; align-items: center; justify-content: center; gap: 10px;
        padding: 0 8px 0 16px; color: inherit; text-decoration: none; white-space: nowrap; overflow: hidden; }
    .bf-bar-link:hover, .bf-bar-link:focus { color: inherit; text-decoration: none; }
    .bf-bar-link:focus-visible { outline: 2px solid #ffd60a; outline-offset: -3px; }
    .bf-bar-tag { flex: 0 0 auto; padding: 4px 8px; border-radius: 4px; background: #ffd60a; color: #1d1d1f; font-size: .68rem; font-weight: 800; letter-spacing: .14em; }
    .bf-bar-text { overflow: hidden; text-overflow: ellipsis; font-weight: 600; }
    .bf-bar-cta { flex: 0 0 auto; color: #ffd60a; font-weight: 700; }
    .bf-bar-link:hover .bf-bar-cta { text-decoration: underline; }
    .bf-bar-close { flex: 0 0 auto; width: var(--bf-bar-h); height: 100%; border: 0; padding: 0; cursor: pointer;
        background: transparent; color: rgba(255,255,255,.8); font-size: 1.3rem; line-height: 1; }
    .bf-bar-close:hover, .bf-bar-close:focus-visible { color: #fff; background: rgba(255,255,255,.12); outline: none; }

    /* While the bar shows, keep the page's last bit and the floating buttons clear of it */
    body.has-bf-bar { padding-bottom: var(--bf-bar-h); }
    body.has-bf-bar .chatbot-widget,
    body.has-bf-bar .cookie-banner { bottom: calc(24px + var(--bf-bar-h)); }
    body.has-bf-bar .back-to-top { bottom: calc(32px + var(--bf-bar-h)); }

    @media (max-width: 640px) {
        .bf-bar { font-size: .78rem; }
        .bf-bar-sub { display: none; }
    }
</style>
<div class="bf-bar" id="bfBar" role="region" aria-label="Black Friday wine special" hidden>
    <a class="bf-bar-link" href="<%= ResolveUrl(ShopUrl) %>">
        <span class="bf-bar-tag">BLACK FRIDAY</span>
        <span class="bf-bar-text">10% off wines<span class="bf-bar-sub"> this Black Friday &middot; ends <%= EndText %></span></span>
        <span class="bf-bar-cta">Shop now &rarr;</span>
    </a>
    <button type="button" class="bf-bar-close" aria-label="Close Black Friday banner" onclick="bfCloseBar()">&times;</button>
</div>
<script>
    (function () {
        var KEY = '<%= StorageKey %>';
        var bar = document.getElementById('bfBar');
        var closed = false;
        // Closing is remembered in this browser only, so the bar stays away on later visits
        try { closed = localStorage.getItem(KEY) === '1'; } catch (e) { }
        if (!closed) {
            bar.hidden = false;
            document.body.classList.add('has-bf-bar');
        }
        window.bfCloseBar = function () {
            bar.hidden = true;
            document.body.classList.remove('has-bf-bar');
            try { localStorage.setItem(KEY, '1'); } catch (e) { }
        };
    })();
</script>
</asp:PlaceHolder>

</asp:PlaceHolder>
