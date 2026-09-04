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

    <%-- ============ AT A GLANCE ============ --%>
    <section class="shell home-section">
      <div class="glass stats-bar">
        <div class="stat-item">
          <p class="stat-number">Up to 22%</p>
          <p class="stat-label">Off selected favourites</p>
        </div>
        <%-- [CONFIRM] Refresh cadence --%>
        <div class="stat-item">
          <p class="stat-number">Weekly</p>
          <p class="stat-label">New specials added regularly</p>
        </div>
      </div>
    </section>

    <%-- ============ FILTER + GRID ============ --%>
    <section class="shell home-section" id="specials-grid">
      <div class="section-header">
        <h2>This Week's Deals</h2>
      </div>
      <p class="section-subtext">Filter by category to find your next favourite.</p>

      <div class="filter-tabs">
        <button type="button" class="filter-tab is-active" onclick="filterSpecials('all', this)">All</button>
        <button type="button" class="filter-tab" onclick="filterSpecials('spirits', this)">Spirits</button>
        <button type="button" class="filter-tab" onclick="filterSpecials('wine', this)">Wine</button>
        <button type="button" class="filter-tab" onclick="filterSpecials('beer', this)">Beer</button>
        <button type="button" class="filter-tab" onclick="filterSpecials('cider-rtd', this)">Cider &amp; RTDs</button>
        <button type="button" class="filter-tab" onclick="filterSpecials('non-alcoholic', this)">Non-Alcoholic</button>
        <button type="button" class="filter-tab" onclick="filterSpecials('gifts', this)">Gift Sets</button>
      </div>

      <div class="products-grid">

        <a href="#" class="card-surface product-card special-card" data-cat="spirits">
          <span class="discount-badge">-13%</span>
          <img src="Images/products/promo1.jpg" alt="Jack Daniel's Whiskey 750ml" />
          <p class="product-name">Jack Daniel's Whiskey 750ml</p>
          <p class="product-price-old"><s>R379.99</s></p>
          <p class="product-price">R329.99</p>
        </a>

        <a href="#" class="card-surface product-card special-card" data-cat="spirits">
          <span class="discount-badge">-16%</span>
          <img src="Images/products/promo2.jpg" alt="Amarula Cream Liqueur 750ml" />
          <p class="product-name">Amarula Cream Liqueur 750ml</p>
          <p class="product-price-old"><s>R219.99</s></p>
          <p class="product-price">R184.99</p>
        </a>

        <a href="#" class="card-surface product-card special-card" data-cat="cider-rtd">
          <span class="discount-badge">-18%</span>
          <img src="Images/products/promo3.jpg" alt="Savanna Dry NRB 6 x 330ml" />
          <p class="product-name">Savanna Dry NRB 6 x 330ml</p>
          <p class="product-price-old"><s>R109.99</s></p>
          <p class="product-price">R89.99</p>
        </a>

        <a href="#" class="card-surface product-card special-card" data-cat="wine">
          <span class="discount-badge">-22%</span>
          <img src="Images/products/promo4.jpg" alt="Four Cousins Natural Sweet Rose 750ml" />
          <p class="product-name">Four Cousins Natural Sweet Rose 750ml</p>
          <p class="product-price-old"><s>R89.99</s></p>
          <p class="product-price">R69.99</p>
        </a>

        <a href="#" class="card-surface product-card special-card" data-cat="wine">
          <span class="discount-badge">-20%</span>
          <img src="Images/products/promo5.jpg" alt="Nederburg Baronne 750ml" />
          <p class="product-name">Nederburg Baronne 750ml</p>
          <p class="product-price-old"><s>R99.99</s></p>
          <p class="product-price">R79.99</p>
        </a>

        <a href="#" class="card-surface product-card special-card" data-cat="beer">
          <span class="discount-badge">-15%</span>
          <img src="Images/products/promo6.jpg" alt="Carling Black Label Local Beer 6 x 500ml" />
          <p class="product-name">Carling Black Label Local Beer 6 x 500ml</p>
          <p class="product-price-old"><s>R99.99</s></p>
          <p class="product-price">R84.99</p>
        </a>

        <a href="#" class="card-surface product-card special-card" data-cat="gifts">
          <span class="discount-badge">-20%</span>
          <img src="Images/products/gift1.jpg" alt="DAY Champagne Glass 250ml 4 Piece" />
          <p class="product-name">DAY Champagne Glass 250ml 4 Piece</p>
          <p class="product-price-old"><s>R149.99</s></p>
          <p class="product-price">R119.99</p>
        </a>

        <a href="#" class="card-surface product-card special-card" data-cat="non-alcoholic">
          <span class="discount-badge">-16%</span>
          <img src="Images/products/na2.jpg" alt="Brutal Fruit Ruby Apple 0% Can 6 x 300ml" />
          <p class="product-name">Brutal Fruit Ruby Apple 0% Can 6 x 300ml</p>
          <p class="product-price-old"><s>R94.99</s></p>
          <p class="product-price">R79.99</p>
        </a>

      </div>

      <p class="ship-table-note">Prices shown include VAT. While stocks last &mdash; specials may be withdrawn or changed without notice.</p>
    </section>

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