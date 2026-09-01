<%@ Page Title="Advice & Help" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AdviceAndHelp.aspx.cs" Inherits="SamsLiquorShop.AdviceAndHelp" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ============ PAGE HEADER ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Advice &amp; Help</h2>
      </div>
      <p class="section-subtext">Everything you need for an easy, safe experience shopping with Sam's Liquor &mdash; from responsible drinking guidance to smooth returns.</p>      
    </section>

    <%-- ============ HUB TILES ============ --%>
    <section class="shell home-section">
      <div class="advice-grid">
        <a href="ResponsibleDrinking.aspx" class="card-surface advice-tile">
          <span class="ship-notice-icon">18+</span>
          <h3>Responsible Drinking Support</h3>
          <p>Standard drink sizes, staying safe, and where to turn if drinking has become a worry for you or someone close to you.</p>
          <span class="arrow">Learn more &rarr;</span>
        </a>

        <a href="FAQ.aspx" class="card-surface advice-tile">
          <span class="ship-notice-icon">?</span>
          <h3>FAQ</h3>
          <p>Quick answers on ordering, delivery, age verification, accounts and more &mdash; the questions we get asked most.</p>
          <span class="arrow">Learn more &rarr;</span>
        </a>

        <a href="Shipping.aspx" class="card-surface advice-tile">
          <span class="ship-notice-icon">&#128666;</span>
          <h3>Shipping &amp; Delivery Info</h3>
          <p>Delivery areas, costs, cut-off times and what to expect from our drivers on the day.</p>
          <span class="arrow">Learn more &rarr;</span>
        </a>

        <a href="ReturnPolicy.aspx" class="card-surface advice-tile">
          <span class="ship-notice-icon">&#8617;</span>
          <h3>Returns &amp; Refunds</h3>
          <p>What can be returned, how to start a return, and how refunds are processed.</p>
          <span class="arrow">Learn more &rarr;</span>
        </a>

      </div>
    </section>

    <%-- ============ CLOSING CTA ============ --%>
    <section class="shell home-section">
      <div class="glass ship-cta">
        <h3>Can't find what you're looking for?</h3>
        <p>Our team is happy to help during trading hours.</p>
        <div class="hero-buttons">
          <a href="tel:+27824055932" class="btn-primary">082 405 5932</a>
          <a href="mailto:info@samsliquorshop.co.za" class="btn-ghost">Email us</a>
        </div>
      </div>
    </section>

</asp:Content>