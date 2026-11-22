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

    <section class="shell home-section about-section">
      <div class="section-header">
        <h2>Get to Know Us</h2>
      </div>
      <p class="section-subtext">Discover Sam's World</p>

      <div class="about-grid">
          <div class="card-surface about-card">
            <img src="Images/about/store-history.jpg" alt="Sam's Liquor Shop storefront" />
            <div class="about-card-body">
              <h3>21 Years in Tongaat</h3>
              <p>From a corner shop to Durban's trusted name in spirits, wine and more.</p>
            </div>
          </div>
          <div class="card-surface about-card">
            <img src="Images/about/loyalty-card.jpg" alt="Sam's Liquor Shop loyalty card" />
            <div class="about-card-body">
              <h3>Loyalty That Pays</h3>
              <p>Earn points on every order and unlock extra discounts along the way.</p>
            </div>
          </div>
          <div class="card-surface about-card">
            <img src="Images/about/store-location.jpg" alt="Sam's Liquor Shop location" />
            <div class="about-card-body">
              <h3>21 Coronation Road</h3>
              <p>Drop by for expert recommendations and same-day collection.</p>
            </div>
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
          <img src="Images/products/product6.jpg" alt="Amarula Cream Liqueur 750ml" />
          <p class="product-name">Amarula Cream Liqueur 750ml</p>
          <p class="product-price">R184.99</p>
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
        <a href="#" class="card-surface product-card">
          <img src="Images/products/promo6.jpg" alt="Carling Black Label Local Beer 6 x 500ml" />
          <p class="product-name">Carling Black Label Local Beer 6 x 500ml</p>
          <p class="product-price">R84.99</p>
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
        <a href="#" class="card-surface product-card">
          <img src="Images/products/na6.jpg" alt="Savanna Alcoholic Free Lemon NRB 330ml" />
          <p class="product-name">Savanna Alcoholic Free Lemon NRB 330ml</p>
          <p class="product-price">R24.99</p>
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
        <a href="#" class="card-surface product-card">
          <img src="Images/products/gift6.jpg" alt="Kitchen Inspire Stainless Steel Ice Tong" />
          <p class="product-name">Kitchen Inspire Stainless Steel Ice Tong</p>
          <p class="product-price">R149.99</p>
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
        <a href="#" class="card-surface category-card">
          <img src="Images/categories/tobaccoandvape.jpg" alt="Tobacco & Vaping" />
          <p>Tobacco & Vaping</p>
        </a>
        
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
        <a href="https://www.midnightblueelephant.com/wine-farms-in-constantia-cape-town/" target="_blank" rel="noopener" class="card-surface guide-card">
          <p>Best Wine Farms in Constantia, Cape Town</p>
          <span class="arrow">&rarr;</span>
        </a>
        <a href="https://www.wineenthusiast.com/culture/industry-news/drinks-trends-2026/" target="_blank" rel="noopener" class="card-surface guide-card">
          <p>What's on the Drinks Menu for 2026</p>
          <span class="arrow">&rarr;</span>
        </a>
        <a href="https://www.tastingtable.com/2082831/cocktail-trends-expected-2026/" target="_blank" rel="noopener" class="card-surface guide-card">
          <p>8 Cocktail Trends to Look Forward to in 2026</p>
          <span class="arrow">&rarr;</span>
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
        <a href="#" class="card-surface blog-card">
          <img src="Images/blog/post5.jpg" alt="Blog post" />
          <div class="blog-card-body">
            <p class="blog-title">Low & No: Our Favourite Alcohol-Free Picks</p>
            <p class="blog-excerpt">Great taste doesn't need proof — our top zero-proof serves for any occasion.</p>
          </div>
        </a>
      </div>
    </section>

    <section class="shell home-section" id="drinkQuiz">
      <div class="section-header">
        <h2>Find My Drink</h2>
      </div>
      <p class="section-subtext">Five quick questions to match you with your perfect pour.</p>

      <div class="glass quiz-card">
        <div class="quiz-side">
          <p class="quiz-side-tag">Sam's Liquor Shop</p>
          <h3 class="quiz-side-heading">Not sure what to pour?</h3>
          <p class="quiz-side-sub">Answer five quick questions and we'll match you with a drink you'll love.</p>
          <span class="quiz-side-glow" aria-hidden="true"></span>
        </div>
        <div class="quiz-main">
        <div id="quizQuestionView">
          <div class="quiz-progress-row">
            <span id="quizStepLabel" class="quiz-step-label">Question 1 of 5</span>
            <div class="quiz-progress"><span id="quizProgressFill"></span></div>
          </div>
          <h3 id="quizQuestionText" class="quiz-question"></h3>
          <div id="quizOptions" class="quiz-options"></div>
          <button type="button" id="quizBackBtn" class="quiz-back" onclick="quizBack()">&larr; Back</button>
        </div>

        <div id="quizResultView" class="quiz-result" style="display:none;">
          <p class="quiz-result-eyebrow">Your match</p>
          <div id="quizResultIcon" class="quiz-result-icon"></div>
          <h3 id="quizResultName" class="quiz-result-name"></h3>
          <p id="quizResultBlurb" class="quiz-result-blurb"></p>
          <p id="quizResultPick" class="quiz-result-pick"></p>
          <div class="quiz-result-actions">
            <a id="quizShopLink" href="Shop.aspx" class="btn-primary">Shop now</a>
            <button type="button" class="btn-ghost quiz-retake" onclick="quizRestart()">Retake quiz</button>
          </div>
          <p id="quizRunnerUp" class="quiz-runner-up"></p>
          <p class="quiz-note">Please enjoy responsibly. You must be 18 or older to purchase alcohol.</p>
        </div>
        </div>
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
          <p class="stat-number" data-count="100" data-suffix="+">100+</p>
          <p class="stat-label">Unique products listed on our website</p>
        </div>
        <div class="stat-item">
          <p class="stat-number" data-count="10000" data-suffix="+">10,000+</p>
          <p class="stat-label">Sales made</p>
        </div>
        <div class="stat-item">
          <p class="stat-number" data-count="2000" data-suffix="+">2,000+</p>
          <p class="stat-label">Subscribed newsletter customers</p>
        </div>
      </div>
    </section>

        <script>
            (function () {
                var results = {
                    whisky: {
                        icon: '🥃', name: 'Whisky', zName: 'Iwiski', search: 'whisky',
                        blurb: 'You like smooth, rich and a little bit classic. Sip it neat, on ice, or with a splash of soda.',
                        zBlurb: 'Uthanda okushelelayo, okunothile nokuncane okwakudala. Yiphuze ngokungaxutshiwe, ngeqhwa, noma ngokufaka i-soda encane.',
                        pick: 'Try: Johnnie Walker Black Label 12yo or Jack Daniel\'s',
                        zPick: 'Linga: Johnnie Walker Black Label 12yo noma Jack Daniel\'s'
                    },
                    gin: {
                        icon: '🍸', name: 'Gin & Tonic', zName: 'I-Gin ne-Tonic', search: 'gin',
                        blurb: 'Crisp, botanical and refreshing. You enjoy something with character that\'s still easy-drinking.',
                        zBlurb: 'Kuyakhahlela, kunamakha ezitshalo futhi kuyaqabula. Uthanda okunesimilo okuphuzeka kalula.',
                        pick: 'Try: Hendrick\'s Gin with tonic and cucumber',
                        zPick: 'Linga: Hendrick\'s Gin ne-tonic nekhukhamba'
                    },
                    vodka: {
                        icon: '🍹', name: 'Vodka & Mixers', zName: 'I-Vodka Nezixubanisi', search: 'vodka',
                        blurb: 'Clean, versatile and social. You like to mix it up, from a simple vodka soda to a full cocktail night.',
                        zBlurb: 'Ihlanzekile, iyaguquguquka futhi ihambisana nabantu. Uthanda ukuxuba, kusukela ku-vodka ne-soda kuya ebusuku obuphelele bama-cocktail.',
                        pick: 'Try: Ciroc or Grey Goose',
                        zPick: 'Linga: Ciroc noma Grey Goose'
                    },
                    red: {
                        icon: '🍷', name: 'Red Wine', zName: 'Iwayini Elibomvu', search: 'red wine',
                        blurb: 'Rich, warm and made for good food and long conversations.',
                        zBlurb: 'Elinothile, elifudumele futhi lenzelwe ukudla okuhle nezingxoxo ezinde.',
                        pick: 'Try: a bold red with your next braai or dinner',
                        zPick: 'Linga: iwayini elibomvu elinamandla ne-braai noma isidlo sakusihlwa esilandelayo'
                    },
                    white: {
                        icon: '🥂', name: 'White & Rosé Wine', zName: 'Iwayini Elimhlophe Ne-Rosé', search: 'rose',
                        blurb: 'Light, fruity and easy-going. Perfect for warm Durban afternoons.',
                        zBlurb: 'Elilula, elinezithelo futhi elikhululekile. Linembile ntambama efudumele yaseThekwini.',
                        pick: 'Try: Four Cousins Natural Sweet Rosé or Nederburg Baronne',
                        zPick: 'Linga: Four Cousins Natural Sweet Rosé noma Nederburg Baronne'
                    },
                    bubbly: {
                        icon: '🍾', name: 'Champagne & Bubbly', zName: 'I-Champagne Nesiphuzo Esinamabhamuza', search: 'champagne',
                        blurb: 'You\'re here for the celebration. Elegant, festive and always a good time.',
                        zBlurb: 'Ulapha ngenxa yomkhosi. Okuhle, okunemicimbi futhi njalo kuyajabulisa.',
                        pick: 'Try: Moët & Chandon Brut Impérial',
                        zPick: 'Linga: Moët & Chandon Brut Impérial'
                    },
                    beer: {
                        icon: '🍺', name: 'Beer & Cider', zName: 'Ibhiya Nesayida', search: 'beer',
                        blurb: 'Cold, crisp and sociable. You keep it relaxed and easy-going.',
                        zBlurb: 'Ibandayo, ibukhali futhi ihambisana nabantu. Uyakhululeka futhi awunazo izinkinga.',
                        pick: 'Try: Carling Black Label or Savanna Dry',
                        zPick: 'Linga: Carling Black Label noma Savanna Dry'
                    },
                    cream: {
                        icon: '🍨', name: 'Cream Liqueur', zName: 'I-Cream Liqueur', search: 'amarula',
                        blurb: 'Sweet, smooth and indulgent. Great over ice or as a dessert in a glass.',
                        zBlurb: 'Imnandi, ishelelayo futhi iyajabulisa. Ilunge ngeqhwa noma njengophudingi engilazini.',
                        pick: 'Try: Amarula Cream Liqueur',
                        zPick: 'Linga: Amarula Cream Liqueur'
                    },
                    zero: {
                        icon: '🍋', name: 'Non-Alcoholic', zName: 'Okungenatshwala', search: 'non-alcoholic',
                        blurb: 'Great taste doesn\'t need proof. There are some brilliant zero-alcohol options these days.',
                        zBlurb: 'Ukunambitheka okuhle akudingi utshwala. Kunezinketho ezinhle kakhulu ezingenatshwala namuhla.',
                        pick: 'Try: Heineken 0.0 or JC Le Roux Le Domaine Non-Alcoholic',
                        zPick: 'Linga: Heineken 0.0 noma JC Le Roux Le Domaine Non-Alcoholic'
                    }
                };

                var questions = [
                    {
                        q: 'What\'s the occasion?', z: 'Uyini umcimbi?', options: [
                            { t: 'Braai with friends', z: 'I-braai nabangani', s: { beer: 2, red: 2, whisky: 1, vodka: 1 } },
                            { t: 'Quiet night in', z: 'Ubusuku obuthule ekhaya', s: { whisky: 2, red: 2, cream: 2 } },
                            { t: 'A celebration', z: 'Umkhosi', s: { bubbly: 3, vodka: 1, white: 1 } },
                            { t: 'Dinner party', z: 'Isidlo sakusihlwa', s: { red: 2, white: 2, gin: 1 } },
                            { t: 'Sundowners outside', z: 'Ukuphuza ekushoneni kwelanga ngaphandle', s: { gin: 2, white: 2, beer: 1, vodka: 1 } }
                        ]
                    },
                    {
                        q: 'Which flavour sounds best?', z: 'Iluphi uhlobo lokunambitheka oluzwakala lungcono kakhulu?', options: [
                            { t: 'Sweet and fruity', z: 'Kumnandi kunezithelo', s: { white: 2, cream: 2, vodka: 1 } },
                            { t: 'Smooth and rich', z: 'Kushelelayo kunothile', s: { whisky: 2, red: 2, cream: 2 } },
                            { t: 'Crisp and refreshing', z: 'Kuyakhahlela kuyaqabula', s: { gin: 2, beer: 2, white: 1, bubbly: 1 } },
                            { t: 'Bold and smoky', z: 'Kunamandla kunentuthu', s: { whisky: 3, red: 1 } }
                        ]
                    },
                    {
                        q: 'How would you describe your style?', z: 'Ungachaza kanjani isitayela sakho?', options: [
                            { t: 'Classic and timeless', z: 'Sakudala nesingapheli', s: { whisky: 2, red: 1, bubbly: 1 } },
                            { t: 'Adventurous', z: 'Othanda ukuzama', s: { gin: 2, vodka: 1, whisky: 1 } },
                            { t: 'Relaxed and easy-going', z: 'Okhululekile futhi ongenazinkinga', s: { beer: 2, white: 1, cream: 1 } },
                            { t: 'Life of the party', z: 'Umphefumulo womcimbi', s: { vodka: 2, bubbly: 2, beer: 1 } }
                        ]
                    },
                    {
                        q: 'How strong do you like it?', z: 'Uyithanda inamandla kangakanani?', options: [
                            { t: 'Light and easy', z: 'Kulula futhi kuncane', s: { beer: 2, white: 2, bubbly: 1 } },
                            { t: 'Somewhere in the middle', z: 'Maphakathi', s: { red: 2, gin: 1, cream: 1, vodka: 1 } },
                            { t: 'Bring on the strong stuff', z: 'Letha okunamandla', s: { whisky: 3, vodka: 1, gin: 1 } },
                            { t: 'No alcohol for me', z: 'Angisiphuzi utshwala', s: { zero: 20 } }
                        ]
                    },
                    {
                        q: 'What\'s your budget per bottle?', z: 'Ubhajethi yakho ngebhodlela ingakanani?', options: [
                            { t: 'Under R100', z: 'Ngaphansi kuka-R100', s: { beer: 2, white: 2, red: 1 } },
                            { t: 'R100 – R250', z: 'R100 – R250', s: { cream: 2, red: 1, white: 1, gin: 1, zero: 1 } },
                            { t: 'R250 – R500', z: 'R250 – R500', s: { whisky: 2, gin: 2, vodka: 1 } },
                            { t: 'R500 and up', z: 'R500 nangaphezulu', s: { bubbly: 2, whisky: 1, vodka: 1 } }
                        ]
                    }
                ];

                var answers = [];
                var ranked = [];

                function $(id) { return document.getElementById(id); }
                function zu() { return window.currentLang === 'zu'; }
                function tr(en, z) { return zu() && z ? z : en; }

                function renderQuestion() {
                    var step = answers.length;
                    var q = questions[step];
                    $('quizStepLabel').textContent = zu()
                        ? 'Umbuzo ' + (step + 1) + ' kwemi-' + questions.length
                        : 'Question ' + (step + 1) + ' of ' + questions.length;
                    $('quizProgressFill').style.width = (step / questions.length * 100) + '%';
                    $('quizQuestionText').textContent = tr(q.q, q.z);
                    $('quizBackBtn').style.visibility = step === 0 ? 'hidden' : 'visible';

                    var box = $('quizOptions');
                    box.innerHTML = '';
                    q.options.forEach(function (opt, i) {
                        var b = document.createElement('button');
                        b.type = 'button';
                        b.className = 'quiz-option';
                        b.textContent = tr(opt.t, opt.z);
                        b.addEventListener('click', function () { choose(i); });
                        box.appendChild(b);
                    });
                }

                function choose(i) {
                    answers.push(i);
                    if (answers.length === questions.length) showResult();
                    else renderQuestion();
                }

                function renderResult() {
                    var top = results[ranked[0]];
                    $('quizResultIcon').textContent = top.icon;
                    $('quizResultName').textContent = tr(top.name, top.zName);
                    $('quizResultBlurb').textContent = tr(top.blurb, top.zBlurb);
                    $('quizResultPick').textContent = tr(top.pick, top.zPick);
                    $('quizShopLink').href = 'Shop.aspx?search=' + encodeURIComponent(top.search);
                    $('quizShopLink').textContent = (zu() ? 'Thenga ' : 'Shop ') + tr(top.name, top.zName);

                    var runner = $('quizRunnerUp');
                    if (ranked[1] && ranked[0] !== 'zero') {
                        var next = results[ranked[1]];
                        runner.textContent = (zu() ? 'Ungase uthande nalokhu: ' : 'You might also enjoy: ') + tr(next.name, next.zName);
                        runner.style.display = 'block';
                    } else {
                        runner.style.display = 'none';
                    }
                }

                function showResult() {
                    var scores = {};
                    answers.forEach(function (ai, qi) {
                        var s = questions[qi].options[ai].s;
                        for (var k in s) scores[k] = (scores[k] || 0) + s[k];
                    });
                    ranked = Object.keys(scores).sort(function (a, b) { return scores[b] - scores[a]; });
                    renderResult();
                    $('quizQuestionView').style.display = 'none';
                    $('quizResultView').style.display = 'block';
                }

                window.quizBack = function () {
                    if (answers.length > 0) { answers.pop(); renderQuestion(); }
                };

                window.quizRestart = function () {
                    answers = [];
                    $('quizResultView').style.display = 'none';
                    $('quizQuestionView').style.display = 'block';
                    renderQuestion();
                };

                // Redraw in the new language when the visitor switches
                window.addEventListener('langchange', function () {
                    if (!$('quizOptions')) return;
                    if ($('quizResultView').style.display === 'block') renderResult();
                    else renderQuestion();
                });

                if ($('quizOptions')) renderQuestion();
            })();
        </script>

        <script>
            // Stats count up the first time they scroll into view
            (function () {
                var DURATION_MS = 4000;
                var nums = document.querySelectorAll('.stat-number[data-count]');
                if (!nums.length) return;

                function format(n) {
                    return Math.round(n).toLocaleString('en-ZA').replace(/\u00a0/g, ',');
                }

                function animate(el) {
                    var target = Number(el.getAttribute('data-count'));
                    var suffix = el.getAttribute('data-suffix') || '';
                    var start = null;

                    function step(ts) {
                        if (start === null) start = ts;
                        var p = Math.min((ts - start) / DURATION_MS, 1);
                        var eased = p;   // constant speed
                        el.textContent = format(target * eased) + suffix;
                        if (p < 1) requestAnimationFrame(step);
                    }
                    requestAnimationFrame(step);
                }

                var reduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
                if (reduce || !('IntersectionObserver' in window)) return;   // leave the final numbers showing

                // Start from zero so there's no flash of the final number
                nums.forEach(function (el) {
                    el.textContent = '0' + (el.getAttribute('data-suffix') || '');
                });

                var observer = new IntersectionObserver(function (entries) {
                    entries.forEach(function (entry) {
                        if (!entry.isIntersecting) return;
                        animate(entry.target);
                        observer.unobserve(entry.target);   // only count up once
                    });
                }, { threshold: 0.6 });

                nums.forEach(function (el) { observer.observe(el); });
            })();
        </script>

</asp:Content>