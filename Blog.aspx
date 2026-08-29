<%@ Page Title="Blog & Newsletter" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Blog.aspx.cs" Inherits="SamsLiquorShop.Blog" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ============ PAGE HEADER ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Blog &amp; Newsletter</h2>
      </div>
      <p class="section-subtext">Tips, stories and news from the Sam's team &mdash; plus the easiest way to hear about new arrivals first.</p>
    </section>

    <%-- ============ FEATURED POST ============ --%>
    <section class="shell home-section">
      <a href="#" class="glass blog-feature">
        <img src="Images/blog/post1.jpg" alt="5 Wines to Try This Summer" />
        <div class="blog-feature-body">
          <p class="blog-feature-eyebrow">Latest Post</p>
          <h3>5 Wines to Try This Summer</h3>
          <p>Our top picks for warm Durban evenings, from crisp whites to light rosés &mdash; and what to pair them with.</p>
          <span class="btn-primary" style="width:fit-content;">Read More &rarr;</span>
        </div>
      </a>
    </section>

    <%-- ============ TOPICS ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Browse by Topic</h2>
      </div>
      <div class="topic-tags">
        <span class="topic-tag">Cocktails</span>
        <span class="topic-tag">Wine</span>
        <span class="topic-tag">Whiskey & Spirits</span>
        <span class="topic-tag">Behind the Bar</span>
        <span class="topic-tag">Entertaining</span>
        <span class="topic-tag">Low & No Alcohol</span>
        <span class="topic-tag">Community</span>
      </div>
    </section>

    <%-- ============ ALL POSTS ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>All Posts</h2>
      </div>
      <div class="blog-grid">
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
            <p class="blog-title">Low &amp; No: Our Favourite Alcohol-Free Picks</p>
            <p class="blog-excerpt">Great taste doesn't need proof &mdash; our top zero-proof serves for any occasion.</p>
          </div>
        </a>
      </div>
    </section>

    <%-- ============ NEWSLETTER CTA ============ --%>
    <section class="shell home-section">
      <div class="glass cta-panel">
        <h3>Never miss a drop.</h3>
        <p>Join the newsletter for new arrivals, tastings and subscriber-only discounts &mdash; straight to your inbox.</p>
        <div class="hero-buttons">
          <button type="button" class="btn-primary" onclick="showNewsletter()">Subscribe Now</button>
        </div>
        <p class="hours-note" style="margin-top:16px;">
          By subscribing you agree to receive marketing emails. Read our
          <a href="PrivacyPolicy.aspx">privacy policy</a>.
        </p>
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

</asp:Content>
