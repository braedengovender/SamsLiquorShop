<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="SamsLiquorShop.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main aria-labelledby="title">

        <section class="hero-wrap">
          <div class="glass hero-card">
            <div class="hero-text">
              <p class="hero-eyebrow">Our Story</p>
              <h1 id="title" class="hero-heading">21 years of good<br>times in Tongaat.</h1>
              <p class="hero-sub">From a small corner shop to Durban's trusted name in spirits, wine and more — Sam's Liquor Shop has been part of the neighbourhood since day one.</p>
              <div class="hero-buttons">
                <a href="Shop.aspx" class="btn-primary">Shop Now</a>
                <a href="Contact.aspx" class="btn-ghost">Get in Touch</a>
              </div>
            </div>
            <div class="hero-image-wrap">
              <img src="Images/about/store-history.jpg" alt="Sam's Liquor Shop storefront" class="hero-image" />
            </div>
          </div>
        </section>

        <section class="shell home-section about-story">
          <div class="section-header">
            <h2>Our Story</h2>
          </div>
          <p class="section-subtext">How a corner shop became a Tongaat institution</p>
          <div class="about-story-copy">
            <p>Sam's Liquor Shop first opened its doors on Coronation Road with little more than a small stockroom and a big idea: give the Tongaat community a liquor store that treats every customer like a regular. Two decades on, that idea hasn't changed — even as our shelves have grown to cover everything from local favourites to rare spirits.</p>
            <p>Today, we're proud to be one of Durban's trusted names for spirits, wine, beer and more. Whether you're stocking up for a braai, picking out a gift, or just grabbing your usual order, our team is here with the same recommendations and friendly service that got us here in the first place.</p>
          </div>
        </section>

        <section class="shell home-section">
          <div class="section-header">
            <h2>What Sets Us Apart</h2>
          </div>
          <div class="values-grid">
            <div class="card-surface value-card">
              <div class="value-icon">&#127866;</div>
              <h3>Wide Selection</h3>
              <p>From everyday favourites to hard-to-find spirits, wines and craft beers, there's something for every occasion.</p>
            </div>
            <div class="card-surface value-card">
              <div class="value-icon">&#128172;</div>
              <h3>Expert Advice</h3>
              <p>Our team knows the shelves inside out and is always happy to help you find the right bottle or pairing.</p>
            </div>
            <div class="card-surface value-card">
              <div class="value-icon">&#127942;</div>
              <h3>Loyalty That Pays</h3>
              <p>Earn points on every order and unlock extra discounts and offers through our rewards programme.</p>
            </div>
            <div class="card-surface value-card">
              <div class="value-icon">&#9989;</div>
              <h3>Responsible Service</h3>
              <p>We're committed to responsible trading and helping our customers drink safely and in moderation.</p>
            </div>
          </div>
        </section>

        <section class="shell home-section">
          <div class="glass stats-bar">
            <div class="stat-item">
              <p class="stat-number">21+</p>
              <p class="stat-label">Years trading in Tongaat</p>
            </div>
            <div class="stat-item">
              <p class="stat-number">100+</p>
              <p class="stat-label">Unique products listed on our website</p>
            </div>
            <div class="stat-item">
              <p class="stat-number">10,000+</p>
              <p class="stat-label">Sales made</p>
            </div>
          </div>
        </section>

        <section class="shell home-section">
          <div class="glass hero-card loyalty-banner">
            <div class="hero-image-wrap">
              <img src="Images/about/loyalty-card.jpg" alt="Sam's Liquor Shop loyalty card" class="hero-image" />
            </div>
            <div class="hero-text">
              <p class="hero-eyebrow">Rewards</p>
              <h2 class="hero-heading loyalty-heading">Join the Sam's<br>rewards family.</h2>
              <p class="hero-sub">Sign up for a free loyalty card and start earning points on every order — redeemable for discounts on your favourites.</p>
              <div class="hero-buttons">
                <a href="Loyalty.aspx" class="btn-primary">Learn About Rewards</a>
              </div>
            </div>
          </div>
        </section>

        <section class="shell home-section">
          <div class="section-header">
            <h2>Visit Our Store</h2>
          </div>
          <p class="section-subtext">Drop by for expert recommendations and same-day collection</p>
          <div class="ship-two-col">
            <div class="card-surface ship-panel">
              <h3>Sam's Liquor Shop</h3>
              <address>
                21 Coronation Road<br />
                Tongaat, Durban, 4399<br />
                KwaZulu-Natal
              </address>
              <p><a href="tel:+27824055932">082 405 5932</a><br />
              <a href="mailto:info@samsliquorshop.co.za">info@samsliquorshop.co.za</a></p>
              <ul class="hours-list">
                <li><span>Mon &ndash; Thu</span><span>09:00 &ndash; 18:00</span></li>
                <li><span>Fri &ndash; Sat</span><span>09:00 &ndash; 20:00</span></li>
                <li><span>Sunday</span><span>10:00 &ndash; 15:00</span></li>
                <li><span>Public Holidays</span><span>10:00 &ndash; 15:00</span></li>
              </ul>
              <p class="hours-note">Trading hours may vary in accordance with liquor licensing regulations.</p>
            </div>
            <div class="card-surface footer-map about-map">
              <iframe
                src="https://www.google.com/maps?q=21+Coronation+Road,+Tongaat,+4399&output=embed"
                loading="lazy"
                referrerpolicy="no-referrer-when-downgrade">
              </iframe>
            </div>
          </div>
        </section>

        <section class="shell home-section">
          <div class="glass ship-cta">
            <h3>Ready to Shop?</h3>
            <p>Browse our full range or check out this week's specials.</p>
            <div class="hero-buttons">
              <a href="Shop.aspx" class="btn-primary">Shop Now</a>
              <a href="Specials.aspx" class="btn-ghost">View Specials</a>
            </div>
          </div>
        </section>

    </main>
</asp:Content>

