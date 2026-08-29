<%@ Page Title="Sustainability" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Sustainability.aspx.cs" Inherits="SamsLiquorShop.Sustainability" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- Page-scoped styles: kept here so this page renders correctly even if
         Content/Site.css hasn't been updated with the shared About-Us classes. --%>
    <style>
        .sus-value-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(230px, 1fr));
            gap: 20px;
        }

        .sus-value-card {
            padding: 28px 24px;
        }

        .sus-value-icon {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 52px;
            height: 52px;
            border-radius: 50%;
            background: var(--bg-tint-blue, rgba(90, 200, 250, 0.08));
            font-size: 1.4rem;
            margin-bottom: 16px;
        }

        .sus-value-card h3 {
            font-size: 1.05rem;
            margin: 0 0 8px;
        }

        .sus-value-card p {
            margin: 0;
            font-size: 0.9rem;
            color: var(--text-secondary, #6e6e73);
            line-height: 1.6;
        }

        .sus-two-col {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
        }

        .sus-panel {
            padding: 26px;
        }

        .sus-panel h3 {
            margin: 0 0 14px;
            font-size: 1.1rem;
        }

        .sus-panel p {
            font-size: 0.9rem;
            color: var(--text-secondary, #6e6e73);
            line-height: 1.6;
            margin: 0 0 10px;
        }

        .sus-panel ul {
            margin: 0 0 10px;
            padding-left: 20px;
            font-size: 0.9rem;
            color: var(--text-secondary, #6e6e73);
            line-height: 1.7;
        }

        .sus-cta {
            padding: 40px;
            text-align: center;
            border-radius: var(--radius-lg, 28px);
        }

        .sus-cta h3 {
            margin: 0 0 8px;
            font-size: 1.3rem;
        }

        .sus-cta p {
            margin: 0 0 20px;
            color: var(--text-secondary, #6e6e73);
        }

        .sus-cta .hero-buttons {
            justify-content: center;
        }

        [data-theme="dark"] .sus-value-icon {
            background: rgba(90, 200, 250, 0.14);
        }
    </style>

    <%-- ============ PAGE HEADER ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Sustainability</h2>
      </div>
      <p class="section-subtext">How Sam's is working toward a greener, more responsible way to enjoy a drink.</p>
    </section>

    <%-- ============ AT A GLANCE ============ --%>
    <section class="shell home-section">
      <%-- [CONFIRM] All figures below are placeholders --%>
      <div class="glass stats-bar">
        <div class="stat-item">
          <p class="stat-number">3,000kg+</p>
          <p class="stat-label">Glass &amp; cardboard recycled each year</p>
        </div>
        <div class="stat-item">
          <p class="stat-number">40%</p>
          <p class="stat-label">Range sourced from local &amp; South African producers</p>
        </div>
        <div class="stat-item">
          <p class="stat-number">R50,000+</p>
          <p class="stat-label">Invested in community programmes yearly</p>
        </div>
      </div>
    </section>

    <%-- ============ PILLARS ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Our Pillars</h2>
      </div>
      <p class="section-subtext">Four areas we keep coming back to when we make decisions.</p>

      <div class="sus-value-grid">
        <div class="card-surface sus-value-card">
          <div class="sus-value-icon">&#127807;</div>
          <h3>Responsible Sourcing</h3>
          <p>We prioritise local and regional wineries, breweries and distilleries, cutting down on transport miles and backing South African producers.</p>
        </div>
        <div class="card-surface sus-value-card">
          <div class="sus-value-icon">&#9851;&#65039;</div>
          <h3>Reducing Waste</h3>
          <p>In-store recycling for glass and cardboard, a move toward lighter and recyclable packaging, and less single-use plastic wherever we can manage it.</p>
        </div>
        <div class="card-surface sus-value-card">
          <div class="sus-value-icon">&#127968;</div>
          <h3>Community Investment</h3>
          <p>Sponsoring local events, supporting Tongaat community initiatives, and putting money back into the neighbourhood that's supported us for over 20 years.</p>
        </div>
        <div class="card-surface sus-value-card">
          <div class="sus-value-icon">&#128737;&#65039;</div>
          <h3>Responsible Drinking</h3>
          <p>Strict age verification both online and in-store, clear moderation messaging, and support resources for anyone who needs them.</p>
        </div>
      </div>
    </section>

    <%-- ============ COMMITMENTS ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Our Commitments</h2>
      </div>

      <div class="sus-two-col">
        <div class="card-surface sus-panel">
          <h3>Packaging &amp; Recycling</h3>
          <%-- [CONFIRM] Specific packaging commitments below are placeholders --%>
          <p>We're working to reduce the amount of single-use plastic in our packaging, from delivery boxes to gift sets, and we recycle glass, cardboard and shrink wrap in-store.</p>
          <ul>
            <li>Recyclable cardboard boxes for online orders</li>
            <li>In-store glass and cardboard recycling</li>
            <li>Reduced plastic in gift set packaging</li>
          </ul>
        </div>
        <div class="card-surface sus-panel">
          <h3>Store Operations</h3>
          <%-- [CONFIRM] Operational commitments below are placeholders --%>
          <p>Small changes in how we run the store add up over 20-plus years of trading.</p>
          <ul>
            <li>Energy-efficient LED lighting throughout the store</li>
            <li>Delivery routes planned to reduce unnecessary trips</li>
            <li>Ongoing supplier reviews to favour lower-waste packaging</li>
          </ul>
        </div>
      </div>
    </section>

    <%-- ============ RESPONSIBLE DRINKING TIE-IN ============ --%>
    <section class="shell home-section">
      <div class="card-surface ship-notice">
        <span class="ship-notice-icon">18+</span>
        <div>
          <h3>Sustainability includes responsible drinking.</h3>
          <p>
            Being a responsible retailer isn't just about packaging and recycling &mdash; it's
            about the way we sell alcohol, too. We check ID every time, we never serve
            anyone who appears intoxicated, and we support customers who want help
            moderating or stepping back from drinking.
          </p>
          <p class="ship-notice-links">
            <a href="ResponsibleDrinking.aspx">Responsible Drinking Support &rarr;</a>
          </p>
        </div>
      </div>
    </section>

    <%-- ============ CTA ============ --%>
    <section class="shell home-section">
      <div class="glass sus-cta">
        <h3>Have an idea or a question?</h3>
        <p>We're always looking for better ways to do this &mdash; let us know if you've got a suggestion.</p>
        <div class="hero-buttons">
          <a href="mailto:info@samsliquorshop.co.za" class="btn-primary">Email Us</a>
          <a href="Contact.aspx" class="btn-ghost">Contact Us</a>
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
        <a href="Gallery.aspx" class="card-surface guide-card">
          <p>Gallery</p><span class="arrow">&rarr;</span>
        </a>
        <a href="ResponsibleDrinking.aspx" class="card-surface guide-card">
          <p>Responsible Drinking Support</p><span class="arrow">&rarr;</span>
        </a>
      </div>
    </section>

</asp:Content>
