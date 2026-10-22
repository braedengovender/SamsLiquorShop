<%@ Page Title="Shop" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Shop.aspx.cs" Inherits="SamsLiquorShop.Shop" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <!-- SEARCH: deliberately OUTSIDE the UpdatePanel so the box is never re-rendered (keeps focus + caret while typing) -->
    <section class="shell shop-search-wrap">
        <div class="shop-search">
            <img src="/Images/icons/search.png" alt="" class="nav-icon search-bar-icon-img shop-search-icon" width="18" height="18" />
            <asp:TextBox ID="txtSearch" runat="server" CssClass="shop-search-input" placeholder="Search by name, brand, type, flavour, ingredients, category..." autocomplete="off" aria-label="Search products" />
            <button type="button" id="shopSearchClear" class="shop-search-clear" aria-label="Clear search" style="display:none">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" aria-hidden="true"><path d="M6 6l12 12M18 6L6 18"></path></svg>
            </button>
        </div>
        <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" UseSubmitBehavior="false" CausesValidation="false" style="display:none" />
    </section>

    <asp:UpdatePanel ID="upnlShop" runat="server">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnSearch" EventName="Click" />
        </Triggers>
        <ContentTemplate>

            <!-- Slider values + histogram data (written by JS / server) -->
            <asp:HiddenField ID="hfPriceMin" runat="server" />
            <asp:HiddenField ID="hfPriceMax" runat="server" />
            <asp:HiddenField ID="hfAbvMin" runat="server" />
            <asp:HiddenField ID="hfAbvMax" runat="server" />
            <asp:HiddenField ID="hfRangeData" runat="server" />
            <asp:Button ID="btnApplyRanges" runat="server" Text="Apply" OnClick="btnApplyRanges_Click" UseSubmitBehavior="false" CausesValidation="false" style="display:none" />

            <section class="shell shop-layout">

                <!-- FILTER SIDEBAR -->
                <aside class="shop-filters">

                    <div class="shop-filter-block">
                        <h3>Specials</h3>
                        <asp:CheckBox ID="chkOnSale" runat="server" Text="On sale only" CssClass="shop-sale-toggle" AutoPostBack="true" OnCheckedChanged="Filter_Changed" />
                        <asp:CheckBox ID="chkWishlistOnly" runat="server" Text="Wishlist only" CssClass="shop-sale-toggle" AutoPostBack="true" OnCheckedChanged="Filter_Changed" />
                    </div>

                    <div class="shop-filter-block">
                        <h3>Category</h3>
                        <asp:CheckBoxList ID="cblCategories" runat="server" CssClass="shop-category-list" RepeatLayout="UnorderedList" RepeatDirection="Vertical" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                    </div>

                    <div class="shop-filter-block" id="blkPrice" runat="server">
                        <h3>Price</h3>
                        <div class="range-block" data-range="price" data-lo="priceMin" data-hi="priceMax">
                            <div class="range-hist" aria-hidden="true"></div>
                            <div class="range-track">
                                <div class="range-fill"></div>
                                <input type="range" class="range-input range-lo" aria-label="Minimum price" />
                                <input type="range" class="range-input range-hi" aria-label="Maximum price" />
                            </div>
                            <div class="range-readout"><span class="range-lo-text"></span><span class="range-hi-text"></span></div>
                        </div>
                    </div>

                    <div class="shop-filter-block" id="blkAbv" runat="server">
                        <h3>Alcohol %</h3>
                        <div class="range-block" data-range="abv" data-lo="abvMin" data-hi="abvMax">
                            <div class="range-hist" aria-hidden="true"></div>
                            <div class="range-track">
                                <div class="range-fill"></div>
                                <input type="range" class="range-input range-lo" aria-label="Minimum alcohol percentage" />
                                <input type="range" class="range-input range-hi" aria-label="Maximum alcohol percentage" />
                            </div>
                            <div class="range-readout"><span class="range-lo-text"></span><span class="range-hi-text"></span></div>
                        </div>
                    </div>

                    <div class="shop-filter-block" id="blkBrand" runat="server">
                        <h3>Brand</h3>
                        <asp:CheckBoxList ID="cblBrands" runat="server" CssClass="shop-category-list shop-check-scroll" RepeatLayout="UnorderedList" RepeatDirection="Vertical" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                    </div>

                    <div class="shop-filter-block" id="blkType" runat="server">
                        <h3>Type</h3>
                        <asp:CheckBoxList ID="cblTypes" runat="server" CssClass="shop-category-list shop-check-scroll" RepeatLayout="UnorderedList" RepeatDirection="Vertical" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                    </div>

                    <div class="shop-filter-block" id="blkOrigin" runat="server">
                        <h3>Origin</h3>
                        <asp:CheckBoxList ID="cblOrigins" runat="server" CssClass="shop-category-list shop-check-scroll" RepeatLayout="UnorderedList" RepeatDirection="Vertical" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
                    </div>

                    <asp:LinkButton ID="btnClearFilters" runat="server" CssClass="shop-clear-link" OnClick="btnClearFilters_Click">Clear all filters</asp:LinkButton>
                </aside>

                <!-- PRODUCT GRID -->
                <div class="shop-main">
                    <div class="shop-toolbar">
                        <p class="shop-result-count"><asp:Literal runat="server" ID="litResultCount" /></p>
                        <asp:DropDownList ID="ddlSort" runat="server" CssClass="shop-sort-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                            <asp:ListItem Text="Sort: Featured" Value="featured" />
                            <asp:ListItem Text="Price: Low to High" Value="price_asc" />
                            <asp:ListItem Text="Price: High to Low" Value="price_desc" />
                            <asp:ListItem Text="Name: A-Z" Value="name_asc" />
                        </asp:DropDownList>
                    </div>

                    <asp:Literal runat="server" ID="litCartMessage" Visible="false" />

                    <asp:Repeater ID="rptProducts" runat="server" OnItemCommand="rptProducts_ItemCommand">
                        <HeaderTemplate><div class="shop-grid"></HeaderTemplate>
                        <ItemTemplate>
                            <div class="shop-card">
                                <div class="shop-card-image-wrap">
                                    <asp:LinkButton runat="server" CommandName="ViewProduct" CommandArgument='<%# Eval("Product_ID") %>' CssClass="shop-card-image-link" ToolTip="View details">
                                        <img src='<%# "/ProductImage.ashx?id=" + Eval("Product_ID") %>' alt='<%# Eval("Product_Name") %>' class="shop-card-image" />
                                    </asp:LinkButton>
                                    <span runat="server" class="shop-sale-badge" visible='<%# (bool)Eval("IsOnSale") %>'>-<%# Eval("DiscountPercent") %>%</span>
                                    <asp:LinkButton runat="server" CommandName="ToggleWishlist" CommandArgument='<%# Eval("Product_ID") %>' CssClass='<%# "shop-heart-btn" + (IsWishlisted((int)Eval("Product_ID")) ? " is-active" : "") %>' ToolTip="Wishlist"><%# IsWishlisted((int)Eval("Product_ID")) ? "&#9829;" : "&#9825;" %></asp:LinkButton>
                                </div>
                                <div class="shop-card-body">
                                    <p class="shop-card-brand"><%# Eval("Product_Brand") %></p>
                                    <asp:LinkButton runat="server" CommandName="ViewProduct" CommandArgument='<%# Eval("Product_ID") %>' CssClass="shop-card-name-link"><%# Eval("Product_Name") %></asp:LinkButton>
                                    <div class="rating" title='<%# Eval("RatingText") %> out of 5'>
                                        <span class="rating-stars"><span class="rating-stars-fill" style='width:<%# Eval("RatingPct") %>%'></span></span>
                                        <span class="rating-count"><%# Eval("RatingText") %> (<%# Eval("RatingCount") %>)</span>
                                    </div>
                                    <div class="shop-card-price-row">
                                        <span runat="server" class="shop-card-strike" visible='<%# (bool)Eval("IsOnSale") %>'><%# Money(Eval("Product_SellingPrice")) %></span>
                                        <span class='<%# (bool)Eval("IsOnSale") ? "shop-card-price is-sale" : "shop-card-price" %>'><%# Money(Eval("DiscountedPrice")) %></span>
                                    </div>
                                    <asp:LinkButton runat="server" CommandName="AddToCart" CommandArgument='<%# Eval("Product_ID") %>' CssClass="btn-primary shop-add-cart-btn" Enabled='<%# Convert.ToInt32(Eval("Product_QuantityInStock")) > 0 %>'>
                                        <%# Convert.ToInt32(Eval("Product_QuantityInStock")) > 0 ? "Add to Cart" : "Out of Stock" %>
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                        <FooterTemplate></div></FooterTemplate>
                    </asp:Repeater>

                    <p runat="server" id="litNoResults" visible="false" class="shop-no-results">No products match your filters. Try adjusting your search.</p>
                </div>

            </section>

            <!-- PRODUCT DETAIL OVERLAY -->
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
                        <div><asp:LinkButton ID="lnkDetailCategory" runat="server" CssClass="pd-category" OnClick="lnkDetailCategory_Click" /></div>
                        <div class="shop-card-price-row pd-price-row">
                            <span id="spnDetailOld" runat="server" class="shop-card-strike" visible="false"></span>
                            <span id="spnDetailPrice" runat="server" class="shop-card-price"></span>
                        </div>
                        <p class="pd-desc"><asp:Literal ID="litDetailDesc" runat="server" Mode="Encode" /></p>
                        <asp:Literal ID="litDetailFacts" runat="server" />
                        <asp:Literal ID="litDetailIngredients" runat="server" />
                        <asp:Literal ID="litDetailMsg" runat="server" />
                        <div class="pd-actions">
                            <asp:LinkButton ID="btnDetailAdd" runat="server" CssClass="btn-primary shop-add-cart-btn pd-add" OnClick="btnDetailAdd_Click" />
                            <asp:LinkButton ID="btnDetailWish" runat="server" CssClass="btn-ghost pd-wish" OnClick="btnDetailWish_Click" />
                        </div>
                    </div>
                </div>
            </asp:Panel>

        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdateProgress ID="updShopProgress" runat="server" AssociatedUpdatePanelID="upnlShop" DisplayAfter="700">
        <ProgressTemplate>
            <div class="loading-overlay">
                <div class="loading-spinner"></div>
                <p>Updating&hellip;</p>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script type="text/javascript">
        (function () {
            var IDS = {
                search: '<%= txtSearch.ClientID %>',
                searchBtn: '<%= btnSearch.ClientID %>',
                apply: '<%= btnApplyRanges.ClientID %>',
                data: '<%= hfRangeData.ClientID %>',
                priceMin: '<%= hfPriceMin.ClientID %>',
                priceMax: '<%= hfPriceMax.ClientID %>',
                abvMin: '<%= hfAbvMin.ClientID %>',
                abvMax: '<%= hfAbvMax.ClientID %>'
            };
            var searchTimer = null, rangeTimer = null;
            function el(id) { return document.getElementById(id); }

            /* ---------- dynamic search ---------- */
            function initSearch() {
                var input = el(IDS.search), clear = el('shopSearchClear'), submit = el(IDS.searchBtn);
                if (!input || !clear || !submit) return;
                function sync() { clear.style.display = input.value.length ? 'flex' : 'none'; }
                function fire() { submit.click(); }
                input.addEventListener('input', function () {
                    sync();
                    clearTimeout(searchTimer);
                    searchTimer = setTimeout(fire, 350);
                });
                input.addEventListener('keydown', function (e) {
                    if (e.key === 'Enter') { e.preventDefault(); clearTimeout(searchTimer); fire(); }
                });
                clear.addEventListener('click', function () {
                    input.value = ''; sync();
                    clearTimeout(searchTimer); fire(); input.focus();
                });
                window.shopSyncSearch = sync;   // server calls this after "Clear all filters"
                sync();
            }

            /* ---------- histogram range sliders ---------- */
            function initRanges() {
                var dataField = el(IDS.data);
                if (!dataField || !dataField.value) return;
                var data;
                try { data = JSON.parse(dataField.value); } catch (e) { return; }

                document.querySelectorAll('.range-block').forEach(function (block) {
                    if (block.getAttribute('data-bound')) return;
                    var cfg = data[block.getAttribute('data-range')];
                    var hiddenLo = el(IDS[block.getAttribute('data-lo')]);
                    var hiddenHi = el(IDS[block.getAttribute('data-hi')]);
                    if (!cfg || !hiddenLo || !hiddenHi) return;
                    block.setAttribute('data-bound', '1');

                    var isPrice = block.getAttribute('data-range') === 'price';
                    var min = cfg.min, max = cfg.max, span = (max - min) || 1, n = cfg.buckets.length;
                    var lo = block.querySelector('.range-lo'), hi = block.querySelector('.range-hi');
                    var fill = block.querySelector('.range-fill');
                    var txtLo = block.querySelector('.range-lo-text'), txtHi = block.querySelector('.range-hi-text');
                    var hist = block.querySelector('.range-hist');

                    [lo, hi].forEach(function (r) { r.min = min; r.max = max; r.step = cfg.step; });

                    function fmt(v) { return isPrice ? 'R' + Math.round(v) : Number(v).toFixed(1) + '%'; }

                    var peak = Math.max.apply(null, cfg.buckets.concat([1]));
                    var bars = cfg.buckets.map(function (count) {
                        var b = document.createElement('span');
                        b.className = 'range-bar';
                        b.style.height = (count ? Math.max(10, Math.round(count / peak * 100)) : 3) + '%';
                        b.title = count + (count === 1 ? ' product' : ' products');
                        hist.appendChild(b);
                        return b;
                    });

                    function paint() {
                        var a = parseFloat(lo.value), b = parseFloat(hi.value);
                        bars.forEach(function (bar, i) {
                            var bLo = min + span * i / n, bHi = min + span * (i + 1) / n;
                            bar.classList.toggle('is-out', bHi <= a || bLo >= b);
                        });
                        fill.style.left = ((a - min) / span * 100) + '%';
                        fill.style.right = (100 - (b - min) / span * 100) + '%';
                        txtLo.textContent = fmt(a);
                        txtHi.textContent = fmt(b);
                        lo.style.zIndex = a > (min + max) / 2 ? 5 : 3;   // keeps the low thumb grabbable when both sit at the max
                    }

                    lo.value = hiddenLo.value !== '' ? hiddenLo.value : min;
                    hi.value = hiddenHi.value !== '' ? hiddenHi.value : max;
                    paint();

                    lo.addEventListener('input', function () { if (parseFloat(lo.value) > parseFloat(hi.value)) lo.value = hi.value; paint(); });
                    hi.addEventListener('input', function () { if (parseFloat(hi.value) < parseFloat(lo.value)) hi.value = lo.value; paint(); });

                    function commit() {
                        hiddenLo.value = parseFloat(lo.value) <= min ? '' : lo.value;
                        hiddenHi.value = parseFloat(hi.value) >= max ? '' : hi.value;
                        clearTimeout(rangeTimer);
                        rangeTimer = setTimeout(function () { var b = el(IDS.apply); if (b) b.click(); }, 150);
                    }
                    lo.addEventListener('change', commit);   // fires on release, so dragging never re-renders the panel
                    hi.addEventListener('change', commit);
                });
            }

            /* ---------- product overlay ---------- */
            function lockScroll() {
                document.body.style.overflow = document.querySelector('.pd-overlay') ? 'hidden' : '';
            }
            function closeOverlay() { var c = document.querySelector('.pd-close'); if (c) c.click(); }

            document.addEventListener('DOMContentLoaded', function () {
                initSearch();
                initRanges();
                lockScroll();
                if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () { initRanges(); lockScroll(); });
                }
                document.addEventListener('keydown', function (e) { if (e.key === 'Escape') closeOverlay(); });
                document.addEventListener('click', function (e) {
                    if (e.target.classList && e.target.classList.contains('pd-overlay')) closeOverlay();
                });
            });
        })();
    </script>
</asp:Content>
