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
          <img src="Images/products/product1.jpg" alt="Johnnie Walker Black Label" />
          <p class="product-name">Johnnie Walker Black Label</p>
          <p class="product-price">R449.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/product2.jpg" alt="Ciroc Vodka" />
          <p class="product-name">Ciroc Vodka</p>
          <p class="product-price">R589.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/product3.jpg" alt="Grey Goose Vodka" />
          <p class="product-name">Grey Goose Vodka</p>
          <p class="product-price">R699.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/product4.jpg" alt="Moet & Chandon" />
          <p class="product-name">Moët & Chandon</p>
          <p class="product-price">R799.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/product5.jpg" alt="Hendrick's Gin" />
          <p class="product-name">Hendrick's Gin</p>
          <p class="product-price">R459.99</p>
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
          <img src="Images/products/promo1.jpg" alt="Jack Daniel's" />
          <p class="product-name">Jack Daniel's Old No. 7</p>
          <p class="product-price">R329.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/promo2.jpg" alt="Amarula Cream" />
          <p class="product-name">Amarula Cream</p>
          <p class="product-price">R199.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/promo3.jpg" alt="Savanna Dry 6-Pack" />
          <p class="product-name">Savanna Dry 6-Pack</p>
          <p class="product-price">R89.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/promo4.jpg" alt="Four Cousins" />
          <p class="product-name">Four Cousins Sweet Red</p>
          <p class="product-price">R69.99</p>
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
          <img src="Images/products/gift1.jpg" alt="Whiskey Gift Set" />
          <p class="product-name">Whiskey Glass Gift Set</p>
          <p class="product-price">R349.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/gift2.jpg" alt="Hip Flask" />
          <p class="product-name">Leather Hip Flask</p>
          <p class="product-price">R249.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/gift3.jpg" alt="Cocktail Shaker Set" />
          <p class="product-name">Cocktail Shaker Set</p>
          <p class="product-price">R429.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/gift4.jpg" alt="Whiskey Stones" />
          <p class="product-name">Whiskey Stones Set</p>
          <p class="product-price">R179.99</p>
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
          <img src="Images/categories/whiskey.jpg" alt="Whiskey" />
          <p>Whiskey</p>
        </a>
        <a href="#" class="card-surface category-card">
          <img src="Images/categories/wine.jpg" alt="Wine" />
          <p>Wine</p>
        </a>
        <a href="#" class="card-surface category-card">
          <img src="Images/categories/beer.jpg" alt="Beer" />
          <p>Beer</p>
        </a>
        <a href="#" class="card-surface category-card">
          <img src="Images/categories/vodka.jpg" alt="Vodka" />
          <p>Vodka</p>
        </a>
        <a href="#" class="card-surface category-card">
          <img src="Images/categories/gin.jpg" alt="Gin" />
          <p>Gin</p>
        </a>
        <a href="#" class="card-surface category-card">
          <img src="Images/categories/rtd.jpg" alt="Ready to Drink" />
          <p>RTDs</p>
        </a>
      </div>
    </section>

    <section class="shell home-section">
      <div class="section-header">
        <h2>Expert Guides & Resources</h2>
      </div>
      <div class="guides-grid">
        <a href="#" class="card-surface guide-card"><p>Best Whiskeys Under R500</p><span class="arrow">&rarr;</span></a>
        <a href="#" class="card-surface guide-card"><p>Ultimate Braai Guide</p><span class="arrow">&rarr;</span></a>
        <a href="#" class="card-surface guide-card"><p>Complete Cocktails Guide</p><span class="arrow">&rarr;</span></a>
        <a href="#" class="card-surface guide-card"><p>Find Your Dream Alcohol Quiz</p><span class="arrow">&rarr;</span></a>
        <a href="#" class="card-surface guide-card"><p>Alcohol Calculator</p><span class="arrow">&rarr;</span></a>
      </div>
    </section>

    <section class="shell home-section">
      <div class="section-header">
        <h2>Non-Alcoholic Favourites</h2>
        <a href="Shop.aspx?cat=non-alcoholic" class="view-all-link">View all &rarr;</a>
      </div>
      <div class="ribbon">
        <a href="#" class="card-surface product-card">
          <img src="Images/products/na1.jpg" alt="Seedlip Spice" />
          <p class="product-name">Seedlip Spice 94</p>
          <p class="product-price">R429.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/na2.jpg" alt="Heineken 0.0" />
          <p class="product-name">Heineken 0.0</p>
          <p class="product-price">R99.99</p>
        </a>
        <a href="#" class="card-surface product-card">
          <img src="Images/products/na3.jpg" alt="Sparkling Grape Juice" />
          <p class="product-name">Sparkling Grape Juice</p>
          <p class="product-price">R59.99</p>
        </a>
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
            <p class="blog-excerpt">Our top picks for warm Durban evenings.</p>
          </div>
        </a>
        <a href="#" class="card-surface blog-card">
          <img src="Images/blog/post2.jpg" alt="Blog post" />
          <div class="blog-card-body">
            <p class="blog-title">How to Host the Perfect Braai</p>
            <p class="blog-excerpt">Drinks pairings, timing, and playlist tips.</p>
          </div>
        </a>
        <a href="#" class="card-surface blog-card">
          <img src="Images/blog/post3.jpg" alt="Blog post" />
          <div class="blog-card-body">
            <p class="blog-title">Newsletter: August Picks</p>
            <p class="blog-excerpt">What our team is drinking this month.</p>
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
