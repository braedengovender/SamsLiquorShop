<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SamsLiquorShop._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <section class="hero-wrap">
      <div class="glass hero-card">
        <div class="hero-text">
          <p class="hero-eyebrow">Sam's Liquor Shop</p>
          <h1 class="hero-heading">Good times<br>start here.</h1>
          <p class="hero-sub">Discover your favourites, explore something new, and enjoy great drinks at great prices.</p>
          <div class="hero-buttons">
            <a href="Shop.aspx" class="btn-primary">Shop Now</a>
            <a href="Specials.aspx" class="btn-ghost">View Specials</a>
          </div>
        </div>
        <div class="hero-image-wrap">
          <img src="Images/hero-drink.jpg" alt="Featured drinks" class="hero-image" />
        </div>
      </div>
    </section>

    <section class="shell home-section">
      <div class="section-header">
        <h2>Featured at Sam's</h2>
        <a href="Shop.aspx" class="view-all-link">View all &rarr;</a>
      </div>
      <div class="ribbon">
        <a href="#" class="card-surface product-card">
          <img src="Images/products/product1.jpg" alt="Johnnie Walker Black Label 12yo Whisky 750ml" />
          <p class="product-name">Johnnie Walker Black Label 12yo Whisky 750ml</p>
          <p class="product-price">R419.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/product2.jpg" alt="Ciroc Vodka 750ml" />
          <p class="product-name">Ciroc Vodka 750m</p>
          <p class="product-price">R399.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/product3.jpg" alt="Grey Goose Vodka 750ml" />
          <p class="product-name">Grey Goose Vodka 750ml</p>
          <p class="product-price">R699.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/product4.jpg" alt="Moet & Chandon Brut Imperial Champagne 750ml" />
          <p class="product-name">Moet & Chandon Brut Imperial Champagne 750ml</p>
          <p class="product-price">R749.95</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/product5.jpg" alt="Hendricks Gin 750ml" />
          <p class="product-name">Hendricks Gin 750ml</p>
          <p class="product-price">R499.99</p>
        </a>
      </div>
    </section>

    <section class="shell home-section">
      <div class="section-header">
        <h2>Shop Promotions</h2>
        <a href="Specials.aspx" class="view-all-link">View all &rarr;</a>
      </div>
      <p class="section-subtext">Priced to leave our stores. Here while stocks last.</p>
      <div class="ribbon">
        <a href="#" class="card-surface product-card">
          <img src="Images/products/promo1.jpg" alt="Jack Daniel's Whiskey 750ml" />
          <p class="product-name">Jack Daniel's Whiskey 750ml</p>
          <p class="product-price">R329.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/promo2.jpg" alt="Amarula Cream Liqueur 750ml" />
          <p class="product-name">Amarula Cream Liqueur 750ml</p>
          <p class="product-price">R184.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/promo3.jpg" alt="Savanna Dry NRB 6 x 330ml" />
          <p class="product-name">Savanna Dry NRB 6 x 330ml</p>
          <p class="product-price">R89.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/promo4.jpg" alt="Four Cousins Natural Sweet Rose 750ml" />
          <p class="product-name">Four Cousins Natural Sweet Rose 750ml</p>
          <p class="product-price">R69.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/promo5.jpg" alt="Nederburg Baronne 750ml" />
          <p class="product-name">Nederburg Baronne 750ml</p>
          <p class="product-price">R79.99</p>
        </a>
      </div>
    </section>

    <section class="shell home-section">
      <div class="section-header">
        <h2>Non-Alcoholic Favourites</h2>
        <a href="Shop.aspx?cat=non-alcoholic" class="view-all-link">View all &rarr;</a>
      </div>
      <div class="ribbon">
        <a href="#" class="card-surface product-card">
          <img src="Images/products/na1.jpg" alt="Heineken Lager 0.0% Non-Alcoholic 6 x 330ml" />
          <p class="product-name">Heineken Lager 0.0% Non-Alcoholic 6 x 330ml</p>
          <p class="product-price">R109.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/na2.jpg" alt="Brutal Fruit Ruby Apple 0% Can 6 x 300ml" />
          <p class="product-name">Brutal Fruit Ruby Apple 0% Can 6 x 300ml</p>
          <p class="product-price">R94.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/na3.jpg" alt="Fruit Lagoon Pina Colada Cocktail Base 750ml" />
          <p class="product-name">Fruit Lagoon Pina Colada Cocktail Base 750ml</p>
          <p class="product-price">R109.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/na4.jpg" alt="JC Le Roux Le Domaine Non-Alcoholic 750ml" />
          <p class="product-name">JC Le Roux Le Domaine Non-Alcoholic 750ml</p>
          <p class="product-price">R109.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/na5.jpg" alt="Ice Tropez 0% Grape Cooler Nrb 275ml" />
          <p class="product-name">Ice Tropez 0% Grape Cooler Nrb 275ml</p>
          <p class="product-price">R84.99</p>
        </a>
      </div>
    </section>

    <section class="shell home-section">
      <div class="section-header">
        <h2>Gift Ideas & Accessories</h2>
        <a href="Shop.aspx?cat=gifts" class="view-all-link">View all &rarr;</a>
      </div>
      <div class="ribbon">
        <a href="#" class="card-surface product-card">
          <img src="Images/products/gift1.jpg" alt="DAY Champagne Glass 250ml 4 Piece" />
          <p class="product-name">DAY Champagne Glass 250ml 4 Piece</p>
          <p class="product-price">R149.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/gift2.jpg" alt="Prestige Wing Type Corkscrew" />
          <p class="product-name">Prestige Wing Type Corkscrew</p>
          <p class="product-price">R89.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/gift4.jpg" alt="Kitchen Inspire Stainless Steel Ice Tong" />
          <p class="product-name">Kitchen Inspire Stainless Steel Ice Tong</p>
          <p class="product-price">R149.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/gift5.jpg" alt="Ethnix Ice Cube Trays 3 Pack" />
          <p class="product-name">Ethnix Ice Cube Trays 3 Pack</p>
          <p class="product-price">R69.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/gift3.jpg" alt="Outfit Freezer Pack 1kg" />
          <p class="product-name">Outfit Freezer Pack 1kg</p>
          <p class="product-price">R39.99</p>
        </a>
      </div>
    </section>

    <section class="shell home-section">
      <div class="section-header">
        <h2>Shop by Category</h2>
        <a href="Shop.aspx" class="view-all-link">View all &rarr;</a>
      </div>
      <div class="category-grid">
        <a href="#" class="card-surface category-card">
          <img src="Images/categories/beer.jpg" alt="Beer & Ciders" />
          <p>Beer & Ciders</p>
        </a>
        <a href="#" class="card-surface category-card">
          <img src="Images/categories/wine.jpg" alt="Wine" />
          <p>Wine</p>
        </a>
        <a href="#" class="card-surface category-card">
          <img src="Images/categories/whiskey.jpg" alt="Whiskey" />
          <p>Whiskey</p>
        </a>
        <a href="#" class="card-surface category-card">
          <img src="Images/categories/spirits.jpg" alt="Spirits" />
          <p>Spirits</p>
        </a>
        <%--<a href="#" class="card-surface category-card">
          <img src="Images/categories/rtdandpremix.jpg" alt="Ready-to-Drink (RTD) & Premix" />
          <p>Ready-to-Drink (RTD) & Premix</p>
        </a>--%>
        <a href="#" class="card-surface category-card">
          <img src="Images/categories/nonalcoholic.png" alt="Non-Alcoholic Beverages" />
          <p>Non-Alcoholic Beverages</p>
        </a>
        <a href="#" class="card-surface category-card">
          <img src="Images/categories/accessories.jpg" alt="'Accessories & Barware'" />
          <p>Accessories & Barware</p>
        </a>
        <a href="#" class="card-surface category-card">
          <img src="Images/categories/snacks.jpg" alt="Snacks & Food" />
          <p>Snacks & Food</p>
        </a>
        <%--<a href="#" class="card-surface category-card">
          <img src="Images/categories/tobaccoandvape.jpg" alt="Tobacco & Vaping" />
          <p>Tobacco & Vaping</p>
        </a>--%>
        
      </div>
    </section>

    <section class="shell home-section">
      <div class="section-header">
        <h2>Expert Guides & Resources</h2>
      </div>
      <div class="guides-grid">
        <a href="https://winefolly.com/tips/host-wine-tasting-party/" target="_blank" rel="noopener" class="card-surface guide-card">
          <p>How to Host a Wine Tasting Party</p>
          <span class="arrow">&rarr;</span>
        </a>
        <a href="https://www.jamesonwhiskey.com/en/whiskey-guide/how-is-whiskey-made/" target="_blank" rel="noopener" class="card-surface guide-card">
          <p>How Whiskey Is Made</p>
          <span class="arrow">&rarr;</span>
        </a>
        <a href="https://www.theguardian.com/food/2026/aug/23/quench-your-thirst-20-perfect-spritzes-and-shandies-to-try-now-from-kumquat-to-portergaff/" target="_blank" rel="noopener" class="card-surface guide-card">
          <p>20 Perfect Spritzes & Shandies to Try Now</p>
          <span class="arrow">&rarr;</span>
        </a>
        <a href="https://www.acouplecooks.com/summer-cocktails/" target="_blank" rel="noopener" class="card-surface guide-card">
          <p>The Ultimate Summer Cocktails Guide</p>
          <span class="arrow">&rarr;</span>
        </a>
        <a href="https://www.pnpwineandfoodfestival.co.za/events/durban/" target="_blank" rel="noopener" class="card-surface guide-card">
          <p>PnP Wine & Food Festival — Durban</p>
          <span class="arrow">&rarr;</span>
        </a>
        <a href="https://www.worlddrinksawards.com/" target="_blank" rel="noopener" class="card-surface guide-card">
          <p>World Drinks Awards</p>
          <span class="arrow">&rarr;</span>
        </a>
        <a href="https://www.internationalspiritsandwinefestival.com/" target="_blank" rel="noopener" class="card-surface guide-card">
          <p>International Spirits & Wine Festival</p>
          <span class="arrow">&rarr;</span>
        </a>
        <a href="https://www.slightlypretentious.co/great-cocktails/" target="_blank" rel="noopener" class="card-surface guide-card">
          <p>A Guide to Great Cocktails</p>
          <span class="arrow">&rarr;</span>
        </a>
        <%--<a href="https://www.midnightblueelephant.com/wine-farms-in-constantia-cape-town/" target="_blank" rel="noopener" class="card-surface guide-card">
          <p>Best Wine Farms in Constantia, Cape Town</p>
          <span class="arrow">&rarr;</span>
        </a>--%>
      </div>
    </section>

    <section class="shell home-section">
      <div class="section-header">
        <h2>From Our Blog</h2>
        <a href="Blog.aspx" class="view-all-link">View all &rarr;</a>
      </div>
      <div class="ribbon">
        <a href="#" class="card-surface blog-card">
          <img src="Images/blog/post1.jpg" alt="Blog post" />
          <div class="blog-card-body">
            <p class="blog-title">5 Wines to Try This Summer</p>
            <p class="blog-excerpt">Our top picks for warm Durban evenings, from crisp whites to light rosés.</p>
          </div>
        </a>
        <a href="#" class="card-surface blog-card">
          <img src="Images/blog/post2.jpg" alt="Blog post" />
          <div class="blog-card-body">
            <p class="blog-title">How to Host the Perfect Braai</p>
            <p class="blog-excerpt">Drinks pairings, timing tips, and a playlist to keep the evening going.</p>
          </div>
        </a>
        <a href="#" class="card-surface blog-card">
          <img src="Images/blog/post3.jpg" alt="Blog post" />
          <div class="blog-card-body">
            <p class="blog-title">Behind the Bar: Meet Our Spirits Buyer</p>
            <p class="blog-excerpt">We sat down with our head buyer to talk rare finds and what's coming next.</p>
          </div>
        </a>
        <a href="#" class="card-surface blog-card">
          <img src="Images/blog/post4.jpg" alt="Blog post" />
          <div class="blog-card-body">
            <p class="blog-title">Gin 101: A Beginner's Guide</p>
            <p class="blog-excerpt">Botanicals, styles, and how to build your first proper gin and tonic.</p>
          </div>
        </a>
      </div>
    </section>

    <section class="shell home-section">
      <div class="section-header">
        <h2>Featured Reviews</h2>
      </div>
      <div class="reviews-grid">
        <div class="card-surface review-card">
          <div class="review-stars">&#9733;&#9733;&#9733;&#9733;&#9733;</div>
          <p class="review-quote">Best selection in Durban, hands down. Fast delivery too.</p>
          <p class="review-name">— Thando M.</p>
        </div>
        <div class="card-surface review-card">
          <div class="review-stars">&#9733;&#9733;&#9733;&#9733;&#9733;</div>
          <p class="review-quote">Love the rewards program, and the staff always know their stuff.</p>
          <p class="review-name">— Priya N.</p>
        </div>
        <div class="card-surface review-card">
          <div class="review-stars">&#9733;&#9733;&#9733;&#9733;&#9733;</div>
          <p class="review-quote">My go-to for gifts. The gift sets are always a hit.</p>
          <p class="review-name">— James K.</p>
        </div>
      </div>
    </section>

    <section class="shell home-section">
      <div class="glass stats-bar">
        <div class="stat-item">
          <p class="stat-number">100+</p>
          <p class="stat-label">Unique products listed on our website</p>
        </div>
        <div class="stat-item">
          <p class="stat-number">10,000+</p>
          <p class="stat-label">Sales made</p>
        </div>
        <div class="stat-item">
          <p class="stat-number">200+</p>
          <p class="stat-label">Subscribed newsletter customers</p>
        </div>
      </div>
    </section>


</asp:Content>
