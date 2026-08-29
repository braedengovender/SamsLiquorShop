<%@ Page Title="Gallery" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Gallery.aspx.cs" Inherits="SamsLiquorShop.Gallery" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ============ PAGE HEADER ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Gallery</h2>
      </div>
      <p class="section-subtext">A look inside Sam's &mdash; our store, our shelves, and our community.</p>
    </section>

    <%-- ============ FILTERS ============ --%>
    <section class="shell home-section">
      <div class="gallery-filters">
        <button type="button" class="gallery-filter-btn active" data-filter="all" onclick="filterGallery('all', this)">All</button>
        <button type="button" class="gallery-filter-btn" data-filter="store" onclick="filterGallery('store', this)">In Store</button>
        <button type="button" class="gallery-filter-btn" data-filter="products" onclick="filterGallery('products', this)">Products</button>
        <button type="button" class="gallery-filter-btn" data-filter="community" onclick="filterGallery('community', this)">Community</button>
      </div>

      <div class="gallery-grid" id="galleryGrid">
        <div class="card-surface gallery-item" data-category="store">
          <img src="Images/about/store-history.jpg" alt="Sam's Liquor Shop storefront" />
          <div class="gallery-caption">
            <span class="gallery-cat-tag">In Store</span>
            <p class="gallery-title">Our Tongaat storefront</p>
          </div>
        </div>
        <div class="card-surface gallery-item" data-category="store">
          <img src="Images/about/store-location.jpg" alt="Sam's Liquor Shop location" />
          <div class="gallery-caption">
            <span class="gallery-cat-tag">In Store</span>
            <p class="gallery-title">21 Coronation Road</p>
          </div>
        </div>
        <div class="card-surface gallery-item" data-category="store">
          <img src="Images/about/loyalty-card.jpg" alt="Sam's Liquor Shop loyalty card" />
          <div class="gallery-caption">
            <span class="gallery-cat-tag">In Store</span>
            <p class="gallery-title">The Sam's loyalty card</p>
          </div>
        </div>
        <div class="card-surface gallery-item" data-category="products">
          <img src="Images/hero-drink.jpg" alt="Featured drinks display" />
          <div class="gallery-caption">
            <span class="gallery-cat-tag">Products</span>
            <p class="gallery-title">Featured drinks display</p>
          </div>
        </div>
        <div class="card-surface gallery-item" data-category="products">
          <img src="Images/categories/wine.jpg" alt="Wine selection" />
          <div class="gallery-caption">
            <span class="gallery-cat-tag">Products</span>
            <p class="gallery-title">Our wine selection</p>
          </div>
        </div>
        <div class="card-surface gallery-item" data-category="products">
          <img src="Images/categories/whiskey.jpg" alt="Whiskey shelf" />
          <div class="gallery-caption">
            <span class="gallery-cat-tag">Products</span>
            <p class="gallery-title">Whiskey &amp; spirits shelf</p>
          </div>
        </div>
        <div class="card-surface gallery-item" data-category="products">
          <img src="Images/categories/beer.jpg" alt="Beer & ciders" />
          <div class="gallery-caption">
            <span class="gallery-cat-tag">Products</span>
            <p class="gallery-title">Beer &amp; ciders</p>
          </div>
        </div>
        <div class="card-surface gallery-item" data-category="products">
          <img src="Images/categories/accessories.jpg" alt="Barware and accessories" />
          <div class="gallery-caption">
            <span class="gallery-cat-tag">Products</span>
            <p class="gallery-title">Barware &amp; accessories</p>
          </div>
        </div>
        <div class="card-surface gallery-item" data-category="community">
          <img src="Images/blog/post1.jpg" alt="Summer tasting event" />
          <div class="gallery-caption">
            <span class="gallery-cat-tag">Community</span>
            <p class="gallery-title">Summer wine tasting</p>
          </div>
        </div>
        <div class="card-surface gallery-item" data-category="community">
          <img src="Images/blog/post2.jpg" alt="Braai season pairings" />
          <div class="gallery-caption">
            <span class="gallery-cat-tag">Community</span>
            <p class="gallery-title">Braai season pairings</p>
          </div>
        </div>
        <div class="card-surface gallery-item" data-category="community">
          <img src="Images/blog/post3.jpg" alt="Meet the spirits buyer" />
          <div class="gallery-caption">
            <span class="gallery-cat-tag">Community</span>
            <p class="gallery-title">Meet our spirits buyer</p>
          </div>
        </div>
        <div class="card-surface gallery-item" data-category="community">
          <img src="Images/blog/post4.jpg" alt="Gin masterclass" />
          <div class="gallery-caption">
            <span class="gallery-cat-tag">Community</span>
            <p class="gallery-title">Gin masterclass evening</p>
          </div>
        </div>
        <div class="card-surface gallery-item" data-category="community">
          <img src="Images/blog/post5.jpg" alt="Zero-proof pop-up" />
          <div class="gallery-caption">
            <span class="gallery-cat-tag">Community</span>
            <p class="gallery-title">Zero-proof tasting pop-up</p>
          </div>
        </div>
        <div class="card-surface gallery-item" data-category="community">
          <img src="Images/responsible-drinking.jpg" alt="Responsible drinking campaign" />
          <div class="gallery-caption">
            <span class="gallery-cat-tag">Community</span>
            <p class="gallery-title">Promoting responsible drinking</p>
          </div>
        </div>
      </div>
    </section>

    <%-- ============ CTA ============ --%>
    <section class="shell home-section">
      <div class="glass cta-panel">
        <h3>Tag us in your photos!</h3>
        <p>Share your Sam's moments on social and tag us to be featured in our next gallery update.</p>
        <div class="footer-social" style="justify-content:center; display:flex; gap:14px;">
          <a href="#" aria-label="Facebook"><img src="Images/social/facebook.png" alt="Facebook" /></a>
          <a href="#" aria-label="Instagram"><img src="Images/social/instagram.png" alt="Instagram" /></a>
          <a href="#" aria-label="TikTok"><img src="Images/social/tiktok.png" alt="TikTok" /></a>
        </div>
      </div>
    </section>

    <%-- ============ RELATED ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>More About Us</h2>
      </div>
      <div class="guides-grid">
        <a href="About.aspx" class="card-surface guide-card">
          <p>Our Story</p><span class="arrow">&rarr;</span>
        </a>
        <a href="Blog.aspx" class="card-surface guide-card">
          <p>Blog &amp; Newsletter</p><span class="arrow">&rarr;</span>
        </a>
        <a href="Sustainability.aspx" class="card-surface guide-card">
          <p>Sustainability</p><span class="arrow">&rarr;</span>
        </a>
        <a href="Contact.aspx" class="card-surface guide-card">
          <p>Contact Us</p><span class="arrow">&rarr;</span>
        </a>
      </div>
    </section>

    <script>
        function filterGallery(category, btn) {
            var buttons = document.querySelectorAll('.gallery-filter-btn');
            for (var i = 0; i < buttons.length; i++) {
                buttons[i].classList.remove('active');
            }
            btn.classList.add('active');

            var items = document.querySelectorAll('#galleryGrid .gallery-item');
            for (var j = 0; j < items.length; j++) {
                var item = items[j];
                if (category === 'all' || item.getAttribute('data-category') === category) {
                    item.classList.remove('is-hidden');
                } else {
                    item.classList.add('is-hidden');
                }
            }
        }
    </script>

</asp:Content>
