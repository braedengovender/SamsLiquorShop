<%@ Page Title="Specials" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Specials.aspx.cs" Inherits="SamsLiquorShop.Specials" MaintainScrollPositionOnPostBack="true" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="upnlSpecials" runat="server">
    <ContentTemplate>

        <div class="shell specials-toast-wrap">
            <asp:Literal ID="litCartMessage" runat="server" Visible="false" />
        </div>

    <asp:PlaceHolder ID="phSpecials" runat="server">

    <%-- ============ FILTER + GRID ============ --%>
    <section class="shell home-section" id="specials-grid">
      <div class="section-header">
        <h2>This Week's Deals</h2>
      </div>
      <p class="section-subtext">Filter by category to find your next favourite.</p>

      <div class="filter-tabs">
        <button type="button" class="filter-tab is-active" data-cat="all" onclick="filterSpecials('all', this)">All</button>
        <asp:Repeater ID="rptTabs" runat="server">
          <ItemTemplate>
            <button type="button" class="filter-tab" data-cat="<%# Eval("Category_ID") %>" onclick="filterSpecials('<%# Eval("Category_ID") %>', this)"><%# H(Eval("Category_Name")) %></button>
          </ItemTemplate>
        </asp:Repeater>
      </div>

      <asp:Repeater ID="rptSpecials" runat="server" OnItemCommand="rptSpecials_ItemCommand">
        <HeaderTemplate><div class="products-grid specials-grid"></HeaderTemplate>
        <ItemTemplate>
          <div class="card-surface product-card special-card" data-cat="<%# Eval("Category_ID") %>">
            <span class="discount-badge" style='<%# (bool)Eval("Running") ? "" : "background:#8a8f98;" %>'><%# (bool)Eval("Running") ? "-" + Eval("DiscountPercent") + "%" : H(Eval("Note")) %></span>
            <asp:LinkButton runat="server" CommandName="ViewProduct" CommandArgument='<%# Eval("Product_ID") %>' style="display:block;" ToolTip="View details">
              <img src="ProductImage.ashx?id=<%# Eval("Product_ID") %>" alt="<%# H(Eval("Product_Name")) %>" />
            </asp:LinkButton>
            <p class="product-name"><%# H(Eval("Product_Name")) %></p>
            <p class="product-price-old" style='<%# (bool)Eval("Running") ? "" : "display:none;" %>'><s><%# Money(Eval("Product_SellingPrice")) %></s></p>
            <p class="product-price"><%# Money(Eval("DiscountedPrice")) %></p>
            <div class="special-buy-row">
              <div class='special-qty<%# (int)Eval("MaxQty") > 0 ? "" : " is-off" %>' data-max='<%# Eval("MaxQty") %>' title='<%# (int)Eval("MaxQty") > 0 ? "Up to " + Eval("MaxQty") + " available" : "" %>'>
                <button type="button" class="special-qty-btn" data-step="-1" aria-label="Decrease quantity">&minus;</button>
                <asp:TextBox runat="server" ID="txtQty" CssClass="special-qty-input" Text='<%# (int)Eval("MaxQty") > 0 ? "1" : "0" %>' MaxLength="2" Enabled='<%# (int)Eval("MaxQty") > 0 %>' autocomplete="off" inputmode="numeric" aria-label="Quantity" />
                <button type="button" class="special-qty-btn" data-step="1" aria-label="Increase quantity">+</button>
              </div>
              <asp:LinkButton runat="server" CommandName="AddToCart" CommandArgument='<%# Eval("Product_ID") %>' CssClass="btn-primary shop-add-cart-btn" OnClientClick="return specialsCheckQty(this);" Enabled='<%# (int)Eval("MaxQty") > 0 %>'><%# Eval("BtnText") %></asp:LinkButton>
            </div>
          </div>
        </ItemTemplate>
        <FooterTemplate></div></FooterTemplate>
      </asp:Repeater>

      <p class="ship-table-note" style="margin-top:36px;">Prices shown include VAT. While stocks last &mdash; specials may be withdrawn or changed without notice. Items marked in grey have a special that isn&rsquo;t running right now and are shown at their normal price.</p>
    </section>

    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phNone" runat="server" Visible="false">
    <section class="shell home-section">
      <div class="glass ship-cta">
        <h3>No specials on right now</h3>
        <p>Our next round of deals will be up soon. In the meantime, have a browse of the full range.</p>
        <div class="hero-buttons">
          <a href="Shop.aspx" class="btn-primary">Browse Full Shop</a>
        </div>
      </div>
    </section>
    </asp:PlaceHolder>

    <%-- ============ PRODUCT DETAIL POPUP (same one the Shop uses) ============ --%>
    <asp:Panel ID="pnlDetail" runat="server" Visible="false" CssClass="pd-overlay">
      <div class="pd-card" role="dialog" aria-modal="true">
        <asp:LinkButton ID="btnCloseDetail" runat="server" CssClass="pd-close" OnClick="btnCloseDetail_Click" aria-label="Close" Text="&times;" />
        <div class="pd-image-wrap">
          <img id="imgDetail" runat="server" class="pd-image" alt="" />
          <span id="spnDetailBadge" runat="server" class="shop-sale-badge" visible="false"></span>
        </div>
        <div class="pd-body">
          <p class="pd-brand"><asp:Literal ID="litDetailBrand" runat="server" Mode="Encode" /></p>
          <h2 class="pd-name"><asp:Literal ID="litDetailName" runat="server" Mode="Encode" /></h2>
          <asp:Literal ID="litDetailRating" runat="server" />
          <div><asp:HyperLink ID="lnkDetailCategory" runat="server" CssClass="pd-category" /></div>
          <div class="shop-card-price-row pd-price-row">
            <span id="spnDetailOld" runat="server" class="shop-card-strike" visible="false"></span>
            <span id="spnDetailPrice" runat="server" class="shop-card-price"></span>
          </div>
          <p class="pd-desc"><asp:Literal ID="litDetailDesc" runat="server" Mode="Encode" /></p>
          <asp:Literal ID="litDetailFacts" runat="server" />
          <asp:Literal ID="litDetailIngredients" runat="server" />
          <asp:Literal ID="litDetailMsg" runat="server" />
          <div class="pd-actions">
            <div class="special-buy-row pd-buy-row">
              <div id="divDetailQty" runat="server" class="special-qty">
                <button type="button" class="special-qty-btn" data-step="-1" aria-label="Decrease quantity">&minus;</button>
                <asp:TextBox ID="txtDetailQty" runat="server" CssClass="special-qty-input" MaxLength="2" autocomplete="off" inputmode="numeric" aria-label="Quantity" />
                <button type="button" class="special-qty-btn" data-step="1" aria-label="Increase quantity">+</button>
              </div>
              <asp:LinkButton ID="btnDetailAdd" runat="server" CssClass="btn-primary shop-add-cart-btn pd-add" OnClick="btnDetailAdd_Click" OnClientClick="return specialsCheckQty(this);" />
            </div>
            <asp:LinkButton ID="btnDetailWish" runat="server" CssClass="btn-ghost pd-wish" OnClick="btnDetailWish_Click" />
          </div>
        </div>
      </div>
    </asp:Panel>

    </ContentTemplate>
    </asp:UpdatePanel>

    <%-- ============ CLOSING CTA ============ --%>
    <section class="shell home-section">
      <div class="glass ship-cta">
        <h3>Never miss a deal</h3>
        <p>Subscribe and we'll let you know as soon as new specials go live.</p>
        <div class="hero-buttons">
          <button type="button" class="btn-primary" onclick="showNewsletter()">Get Specials by Email</button>
          <a href="Shop.aspx" class="btn-ghost">Browse Full Shop</a>
        </div>
      </div>
    </section>

    <%-- ============ FILTER SCRIPT ============ --%>
    <script>
        (function () {
            var currentCat = 'all';

            /* ---------- category filter (survives the UpdatePanel refresh) ---------- */
            function applyFilter() {
                var tabs = document.querySelectorAll('.filter-tab');
                var found = false;
                tabs.forEach(function (t) {
                    var on = t.getAttribute('data-cat') === currentCat;
                    if (on) found = true;
                    t.classList.toggle('is-active', on);
                });
                if (!found) {                       // that category no longer has specials
                    currentCat = 'all';
                    if (tabs.length) tabs[0].classList.add('is-active');
                }
                document.querySelectorAll('.special-card').forEach(function (c) {
                    c.style.display = (currentCat === 'all' || c.getAttribute('data-cat') === currentCat) ? '' : 'none';
                });
            }

            window.filterSpecials = function (cat, btn) {
                currentCat = String(cat);
                applyFilter();
            };

            /* ---------- quantity stepper ---------- */
            function maxOf(wrap) {
                var m = parseInt(wrap.getAttribute('data-max'), 10);
                return (isNaN(m) || m < 0) ? 0 : m;
            }

            document.addEventListener('click', function (e) {
                var t = e.target;
                var btn = (t && t.closest) ? t.closest('.special-qty-btn') : null;
                if (!btn) return;
                e.preventDefault();
                var wrap = btn.parentNode;
                var input = wrap.querySelector('.special-qty-input');
                if (!input || input.disabled) return;
                var max = maxOf(wrap);
                if (max < 1) return;
                var v = parseInt(input.value, 10);
                if (isNaN(v) || v < 1) v = 1;
                v += parseInt(btn.getAttribute('data-step'), 10) || 0;
                input.value = Math.min(max, Math.max(1, v));
            });

            // typing: digits only, and never above what's available
            document.addEventListener('input', function (e) {
                var input = e.target;
                if (!input.classList || !input.classList.contains('special-qty-input')) return;
                var max = maxOf(input.parentNode);
                var digits = input.value.replace(/[^0-9]/g, '');
                if (digits !== '') {
                    var n = parseInt(digits, 10);
                    digits = String(n > max ? max : n);
                }
                if (input.value !== digits) input.value = digits;
            });

            // leaving the box: empty or 0 goes back to 1
            document.addEventListener('change', function (e) {
                var input = e.target;
                if (!input.classList || !input.classList.contains('special-qty-input')) return;
                var v = parseInt(input.value, 10);
                if (isNaN(v) || v < 1) input.value = 1;
            });

            // Enter in the box must not submit the whole page
            document.addEventListener('keydown', function (e) {
                var input = e.target;
                if (e.key === 'Enter' && input.classList && input.classList.contains('special-qty-input')) {
                    e.preventDefault();
                    input.blur();
                }
            });

            // Runs before the Add to Cart postback. The server re-checks everything anyway.
            window.specialsCheckQty = function (link) {
                try {
                    if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                        if (Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack()) return false;   // stops double-clicks
                    }
                    var row = link.parentNode;
                    var input = row.querySelector('.special-qty-input');
                    var wrap = row.querySelector('.special-qty');
                    if (!input || !wrap) return true;
                    var max = maxOf(wrap);
                    var v = parseInt(input.value, 10);
                    if (isNaN(v) || v < 1) { input.value = 1; input.focus(); return false; }
                    if (v > max) { input.value = max; input.focus(); return false; }
                    return true;
                } catch (err) {
                    return true;
                }
            };

            /* ---------- popup: lock page scroll while open, Esc / click outside closes ---------- */
            function lockScroll() {
                document.body.style.overflow = document.querySelector('.pd-overlay') ? 'hidden' : '';
            }
            function closeOverlay() {
                var c = document.querySelector('.pd-close');
                if (c) c.click();
            }

            document.addEventListener('keydown', function (e) {
                if (e.key === 'Escape') closeOverlay();
            });
            document.addEventListener('click', function (e) {
                if (e.target && e.target.classList && e.target.classList.contains('pd-overlay')) closeOverlay();
            });

            var savedScrollY = 0;

            function rememberScroll() {
                savedScrollY = window.pageYOffset || document.documentElement.scrollTop || 0;
            }

            // Remember the position the instant a card/popup button is clicked (before anything else can move the page)
            document.addEventListener('click', function (e) {
                if (e.target && e.target.closest && e.target.closest('.special-card, .pd-card')) rememberScroll();
            }, true);

            function restoreScroll() {
                window.scrollTo(0, savedScrollY);
            }

            function afterRefresh() {
                applyFilter();
                lockScroll();
                restoreScroll();                          // straight away
                requestAnimationFrame(restoreScroll);     // after the browser lays out the new HTML
                setTimeout(restoreScroll, 50);            // after any other scripts (e.g. the page loader) have run
            }

            document.addEventListener('DOMContentLoaded', function () {
                lockScroll();
                try {
                    if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                        var prm = Sys.WebForms.PageRequestManager.getInstance();
                        prm.add_beginRequest(rememberScroll);
                        prm.add_endRequest(afterRefresh);
                    }
                } catch (err) { /* filter and popup still work, they just won't re-sync after a refresh */ }
            });
        })();
    </script>

</asp:Content>