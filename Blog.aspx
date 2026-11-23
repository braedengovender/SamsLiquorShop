<%@ Page Title="Blog & Newsletter" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Blog.aspx.cs" Inherits="SamsLiquorShop.Blog" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

<% if (Current != null) { %>

    <%-- ============ ARTICLE ============ --%>
    <section class="shell home-section">
      <a href="Blog.aspx" class="blog-back-link">&larr; Back to all posts</a>
      <article class="glass blog-article">
        <div class="blog-article-text">
          <p class="blog-feature-eyebrow"><%: Current.Category %> &middot; 2 min read</p>
          <h1><%: Current.Title %></h1>
          <% foreach (string para in Current.Paragraphs) { %>
          <p><%: para %></p>
          <% } %>
          <p class="hours-note">Please enjoy responsibly. You must be 18 or older to buy alcohol.</p>
        </div>
        <div class="blog-article-image">
          <img src="<%: Current.Image %>" alt="<%: Current.Title %>" />
        </div>
      </article>
    </section>

<% } else { %>

    <%-- ============ PAGE HEADER ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Blog &amp; Newsletter</h2>
      </div>
      <p class="section-subtext">Tips, stories and news from the Sam's team &mdash; plus the easiest way to hear about new arrivals first.</p>
    </section>

    <%-- ============ FEATURED POST ============ --%>
    <section class="shell home-section">
      <a href="Blog.aspx?post=<%: Posts[0].Id %>" class="glass blog-feature">
        <img src="<%: Posts[0].Image %>" alt="<%: Posts[0].Title %>" />
        <div class="blog-feature-body">
          <p class="blog-feature-eyebrow">Latest Post</p>
          <h3><%: Posts[0].Title %></h3>
          <p><%: Posts[0].Excerpt %></p>
          <span class="btn-primary" style="width:fit-content;">Read More &rarr;</span>
        </div>
      </a>
    </section>

    <%-- ============ ALL POSTS ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>All Posts</h2>
      </div>
      <div class="blog-grid">
        <% foreach (SamsLiquorShop.BlogPost p in Posts) { %>
        <a href="Blog.aspx?post=<%: p.Id %>" class="card-surface blog-card">
          <img src="<%: p.Image %>" alt="<%: p.Title %>" />
          <div class="blog-card-body">
            <p class="blog-title"><%: p.Title %></p>
            <p class="blog-excerpt"><%: p.Excerpt %></p>
          </div>
        </a>
        <% } %>
      </div>
    </section>

<% } %>

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
          <a href="/Legal.aspx#privacy">privacy policy</a>.
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
        <a href="Contact.aspx" class="card-surface guide-card">
          <p>Contact Us</p><span class="arrow">&rarr;</span>
        </a>
      </div>
    </section>

</asp:Content>