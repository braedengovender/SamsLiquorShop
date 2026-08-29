<%@ Page Title="Our Story" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="SamsLiquorShop.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ============ PAGE HEADER ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Our Story</h2>
      </div>
      <p class="section-subtext">From a single Tongaat storefront to Durban's trusted name in drinks.</p>
    </section>

    <%-- ============ FOUNDING STORY ============ --%>
    <section class="shell home-section">
      <div class="glass hero-card">
        <div class="hero-text">
          <%-- [CONFIRM] Founding year --%>
          <p class="hero-eyebrow">Family-owned since 2005</p>
          <h1 class="hero-heading">A neighbourhood shop that grew up with Tongaat.</h1>
          <p class="hero-sub">
            Sam's Liquor Shop started life as a small corner store on Coronation Road,
            stocked with the basics and run on a simple idea: know your customers, know
            your stock, and never rush a recommendation. Two decades later, that idea
            still runs the business &mdash; we've just added a website, a delivery van,
            and a lot more bottles.
          </p>
        </div>
        <div class="hero-image-wrap">
          <img src="Images/about/store-history.jpg" alt="Sam's Liquor Shop storefront in Tongaat" class="hero-image" />
        </div>
      </div>
    </section>

    <%-- ============ TIMELINE ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Our Journey</h2>
      </div>
      <p class="section-subtext">A few of the milestones along the way.</p>

      <%-- [CONFIRM] All milestone years below are placeholders --%>
      <div class="timeline-steps">
        <div class="card-surface timeline-step">
          <span class="timeline-year">2005</span>
          <h3>Doors open in Tongaat</h3>
          <p>Sam's Liquor Shop opens on Coronation Road with a small range of local beer, wine and spirits.</p>
        </div>
        <div class="card-surface timeline-step">
          <span class="timeline-year">2013</span>
          <h3>The range grows up</h3>
          <p>We expand into imported spirits, boutique wine and gifting, and start building relationships with local producers.</p>
        </div>
        <div class="card-surface timeline-step">
          <span class="timeline-year">2019</span>
          <h3>Loyalty & Rewards launches</h3>
          <p>Regulars can finally earn something back &mdash; our loyalty card turns everyday purchases into real discounts.</p>
        </div>
        <div class="card-surface timeline-step">
          <span class="timeline-year">2022</span>
          <h3>Online & delivery</h3>
          <p>Sam's goes digital, bringing same-day Durban delivery and click &amp; collect to the same trusted range.</p>
        </div>
        <div class="card-surface timeline-step">
          <span class="timeline-year">2026</span>
          <h3>21 years in Tongaat</h3>
          <p>Still family-run, still on Coronation Road, and still getting to know every customer who walks through the door.</p>
        </div>
      </div>
    </section>

    <%-- ============ WHAT WE STAND FOR ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>What We Stand For</h2>
      </div>
      <p class="section-subtext">The things we won't compromise on, no matter how big we get.</p>

      <div class="value-grid">
        <div class="card-surface value-card">
          <div class="value-icon">&#129309;</div>
          <h3>Community First</h3>
          <p>We're neighbours before we're a shop. Supporting Tongaat and greater Durban matters as much as selling a bottle.</p>
        </div>
        <div class="card-surface value-card">
          <div class="value-icon">&#127863;</div>
          <h3>Curated, Not Crowded</h3>
          <p>Every product on our shelves is chosen by someone who's tasted it, not just added because it sells.</p>
        </div>
        <div class="card-surface value-card">
          <div class="value-icon">&#128737;&#65039;</div>
          <h3>Responsible Service</h3>
          <p>We check ID, we take drink-driving seriously, and we'd rather lose a sale than serve someone who shouldn't be served.</p>
        </div>
        <div class="card-surface value-card">
          <div class="value-icon">&#127757;</div>
          <h3>Local & Independent</h3>
          <p>We're a family business, not a franchise &mdash; and we back other local and South African producers wherever we can.</p>
        </div>
      </div>
    </section>

    <%-- ============ VISIT US / TODAY ============ --%>
    <section class="shell home-section">
      <div class="two-col-grid">
        <div class="card-surface info-panel">
          <h3>Visit Our Store</h3>
          <p>
            We're still exactly where we started &mdash; 21 Coronation Road, Tongaat &mdash;
            with knowledgeable staff on hand to help you find the right bottle for the
            occasion, or just talk you through what's new.
          </p>
          <address style="font-size:0.9rem; color:var(--text-secondary); margin:0 0 10px;">
            21 Coronation Road, Tongaat<br />
            Durban, 4399, KwaZulu-Natal
          </address>
          <p><a href="Contact.aspx" class="btn-primary">Get Directions &amp; Contact Us</a></p>
        </div>
        <div class="card-surface info-panel">
          <h3>Sam's Today</h3>
          <p>
            More than two decades on, we're still adding to the range and still finding
            new ways to say thank you to the customers who've stuck with us.
          </p>
          <ul>
            <li>100+ products across beer, wine, spirits, RTDs and gifts</li>
            <li>Loyalty points on every order, automatically</li>
            <li>Same-day delivery across the greater Durban area</li>
          </ul>
        </div>
      </div>
    </section>

    <%-- ============ RELATED ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>More About Us</h2>
      </div>
      <div class="guides-grid">
        <a href="Blog.aspx" class="card-surface guide-card">
          <p>Blog &amp; Newsletter</p><span class="arrow">&rarr;</span>
        </a>
        <a href="Sustainability.aspx" class="card-surface guide-card">
          <p>Sustainability</p><span class="arrow">&rarr;</span>
        </a>
        <a href="Gallery.aspx" class="card-surface guide-card">
          <p>Gallery</p><span class="arrow">&rarr;</span>
        </a>
        <a href="Contact.aspx" class="card-surface guide-card">
          <p>Contact Us</p><span class="arrow">&rarr;</span>
        </a>
      </div>
    </section>

    <%-- ============ CLOSING CTA ============ --%>
    <section class="shell home-section">
      <div class="glass cta-panel">
        <h3>Come and say hello.</h3>
        <p>Pop into Tongaat, or shop the full range online.</p>
        <div class="hero-buttons">
          <a href="Shop.aspx" class="btn-primary">Shop Now</a>
          <a href="Contact.aspx" class="btn-ghost">Contact Us</a>
        </div>
      </div>
    </section>

</asp:Content>


