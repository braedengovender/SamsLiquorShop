<%@ Page Title="Specials" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Specials.aspx.cs" Inherits="SamsLiquorShop.Specials" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ============ HERO ============ --%>
    <section class="hero-wrap">
      <div class="glass hero-card">
        <div class="hero-text">
          <p class="hero-eyebrow">This Week's Specials</p>
          <h1 class="hero-heading">Great drinks.<br>Even better prices.</h1>
          <p class="hero-sub">Handpicked deals across spirits, wine, beer, and more &mdash; refreshed regularly and available while stocks last.</p>
          <div class="hero-buttons">
            <a href="#specials-grid" class="btn-primary">See This Week's Deals</a>
            <a href="Shop.aspx" class="btn-ghost">Browse Full Shop</a>
          </div>
        </div>
        <div class="hero-image-wrap">
          <img src="Images/hero-drink.jpg" alt="Featured drinks on special" class="hero-image" />
        </div>
      </div>
    </section>

    <asp:PlaceHolder ID="phSpecials" runat="server">

    <asp:PlaceHolder ID="phStats" runat="server">
    <%-- ============ AT A GLANCE ============ --%>
    <section class="shell home-section">
      <div class="glass stats-bar">
        <div class="stat-item">
          <p class="stat-number">Up to <asp:Literal ID="litMaxOff" runat="server" />%</p>
          <p class="stat-label">Off selected favourites</p>
        </div>
        <div class="stat-item">
          <p class="stat-number"><asp:Literal ID="litCount" runat="server" /></p>
          <p class="stat-label">Specials on right now</p>
        </div>
      </div>
    </section>
    </asp:PlaceHolder>

    <%-- ============ FILTER + GRID ============ --%>
    <section class="shell home-section" id="specials-grid">
      <div class="section-header">
        <h2>This Week's Deals</h2>
      </div>
      <p class="section-subtext">Filter by category to find your next favourite.</p>

      <asp:Literal ID="litCartMessage" runat="server" Visible="false" />

      <div class="filter-tabs">
        <button type="button" class="filter-tab is-active" onclick="filterSpecials('all', this)">All</button>
        <asp:Repeater ID="rptTabs" runat="server">
          <ItemTemplate>
            <button type="button" class="filter-tab" onclick="filterSpecials('<%# Eval("Category_ID") %>', this)"><%# H(Eval("Category_Name")) %></button>
          </ItemTemplate>
        </asp:Repeater>
      </div>

      <asp:Repeater ID="rptSpecials" runat="server" OnItemCommand="rptSpecials_ItemCommand">
        <HeaderTemplate><div class="products-grid"></HeaderTemplate>
        <ItemTemplate>
          <div class="card-surface product-card special-card" data-cat="<%# Eval("Category_ID") %>">
            <span class="discount-badge" style='<%# (bool)Eval("Running") ? "" : "background:#8a8f98;" %>'><%# (bool)Eval("Running") ? "-" + Eval("DiscountPercent") + "%" : H(Eval("Note")) %></span>
            <a href="Shop.aspx?search=<%# HttpUtility.UrlEncode(Convert.ToString(Eval("Product_Name"))) %>" style="display:block;">
              <img src="ProductImage.ashx?id=<%# Eval("Product_ID") %>" alt="<%# H(Eval("Product_Name")) %>" />
            </a>
            <p class="product-name"><%# H(Eval("Product_Name")) %></p>
            <p class="product-price-old" style='<%# (bool)Eval("Running") ? "" : "display:none;" %>'><s><%# Money(Eval("Product_SellingPrice")) %></s></p>
            <p class="product-price"><%# Money(Eval("DiscountedPrice")) %></p>
            <asp:LinkButton runat="server" CommandName="AddToCart" CommandArgument='<%# Eval("Product_ID") %>' CssClass="btn-primary shop-add-cart-btn" Enabled='<%# (bool)Eval("InStock") %>'><%# (bool)Eval("InStock") ? "Add to Cart" : "Out of Stock" %></asp:LinkButton>
          </div>
        </ItemTemplate>
        <FooterTemplate></div></FooterTemplate>
      </asp:Repeater>

      <p class="ship-table-note">Prices shown include VAT. While stocks last &mdash; specials may be withdrawn or changed without notice. Items marked in grey have a special that isn&rsquo;t running right now and are shown at their normal price.</p>
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
        function filterSpecials(cat, btn) {
            var tabs = document.querySelectorAll('.filter-tab');
            tabs.forEach(function (t) { t.classList.remove('is-active'); });
            btn.classList.add('is-active');

            var cards = document.querySelectorAll('.special-card');
            cards.forEach(function (c) {
                if (cat === 'all' || c.getAttribute('data-cat') === cat) {
                    c.style.display = '';
                } else {
                    c.style.display = 'none';
                }
            });
        }
    </script>

</asp:Content>