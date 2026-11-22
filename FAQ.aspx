<%@ Page Title="FAQ" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FAQ.aspx.cs" Inherits="SamsLiquorShop.FAQ" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ============ PAGE HEADER ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Frequently Asked Questions</h2>
      </div>
      <p class="section-subtext">Our most commonly asked questions, organised by category so you can find your answer quickly.</p>
    </section>

    <%-- ============ FAQ GROUPS ============ --%>
    <section class="shell home-section">

      <div class="faq-group">
        <h3>Ordering &amp; Payment</h3>
        <div class="faq-list">
          <details class="card-surface faq-item">
            <summary>What is the minimum order value?</summary>
            <%-- [CONFIRM] --%>
            <p>Our minimum order value for delivery is R250. There is no minimum order value for Click &amp; Collect.</p>
          </details>
          <details class="card-surface faq-item">
            <summary>Which payment methods do you accept?</summary>
            <p>We accept major debit and credit cards, as well as <strong>loyalty points</strong>. Every point in your Rewards balance is worth R1 and can be used to pay for all or part of your order &mdash; simply choose Loyalty Points as your payment method. See our <a href="Rewards.aspx">Loyalty &amp; Rewards page</a> for details.</p>
          </details>
          <details class="card-surface faq-item">
            <summary>Can I amend or cancel my order after placing it?</summary>
            <p>If your order has not yet been packed, we can usually amend or cancel it. Please contact us as soon as possible with your order number and we will assist where we can.</p>
          </details>
        </div>
      </div>

      <div class="faq-group">
        <h3>Delivery &amp; Collection</h3>
        <div class="faq-list">
          <details class="card-surface faq-item">
            <summary>How long does delivery take?</summary>
                       <p>Delivery is next day, within the Durban area only. We don't offer same-day delivery or delivery outside Durban. Click &amp; Collect from our Tongaat store is ready within 2 hours. Please refer to our <a href="Shipping.aspx">Shipping &amp; Delivery page</a> for full details.</p>
          </details>
          <details class="card-surface faq-item">
            <summary>Is delivery free?</summary>
                       <p>Delivery is a flat R70, and it's free on orders over R500. Click &amp; Collect is always free.</p>
          </details>
          <details class="card-surface faq-item">
            <summary>Can I collect my order in store instead?</summary>
            <p>Yes. Click &amp; Collect is free of charge, and orders are typically ready within two hours. Our store is located at 21 Coronation Road, Tongaat.</p>
          </details>
        </div>
      </div>

      <div class="faq-group">
        <h3>Age Verification &amp; ID</h3>
        <div class="faq-list">
          <details class="card-surface faq-item">
            <summary>Why must I verify my age before browsing the site?</summary>
            <p>South African law prohibits the sale or supply of alcohol to anyone under the age of 18. Our age verification step simply confirms this requirement is met before you may view or purchase any product.</p>
          </details>
          <details class="card-surface faq-item">
            <summary>What identification is required on delivery?</summary>
            <p>The individual accepting delivery must be 18 or older and must present a valid ID, driver's licence, or passport. Alcohol cannot be handed over without valid identification, without exception.</p>
          </details>
          <details class="card-surface faq-item">
            <summary>May someone else accept delivery on my behalf?</summary>
            <p>Yes, provided they are 18 or older and can present valid identification when the driver arrives.</p>
          </details>
        </div>
      </div>

      <div class="faq-group">
        <h3>Account &amp; Rewards</h3>
        <div class="faq-list">
          <details class="card-surface faq-item">
            <summary>Do I need an account to place an order?</summary>
            <p>Yes. You need an account to place an order. Your account lets you track orders, save your details for future purchases, and collect and spend loyalty points. Use the Register link in the top menu to get started.</p>
          </details>
          <details class="card-surface faq-item">
            <summary>Who can create an account?</summary>
            <p>Accounts are only available to South Africans. You will need a valid South African ID number to register, and you must be 18 or older.</p>
          </details>
          <details class="card-surface faq-item">
            <summary>I have forgotten my password &mdash; what should I do?</summary>
            <p>Use the "Forgot Password" link on the login page and we will assist you in resetting it.</p>
          </details>
          <details class="card-surface faq-item">
            <summary>How does the Loyalty &amp; Rewards programme work?</summary>
            <p>Every purchase earns points that convert into real discounts. Points can also be used as a method of payment, with every point worth R1. Please visit our <a href="Rewards.aspx">Loyalty &amp; Rewards page</a> for full details.</p>
          </details>
        </div>
      </div>

      <div class="faq-group">
        <h3>Returns &amp; Refunds</h3>
        <div class="faq-list">
          <details class="card-surface faq-item">
            <summary>Can I return a product I simply changed my mind about?</summary>
            <p>Unfortunately not once a seal has been broken, for hygiene and licensing reasons. Unopened, unwanted items are treated differently. Please see our <a href="ReturnPolicy.aspx">Returns &amp; Refunds page</a> for full details.</p>
          </details>
          <details class="card-surface faq-item">
            <summary>My order arrived damaged or incorrect &mdash; what should I do?</summary>
            <p>Please contact us within 48 hours with a photo of the item and your order number, and we will resolve this at no cost to you.</p>
          </details>
        </div>
      </div>

    </section>

    <%-- ============ CLOSING CTA ============ --%>
    <section class="shell home-section">
      <div class="glass ship-cta">
        <h3>Still have a question?</h3>
        <p>Send us a message and we will respond as soon as possible.</p>
        <div class="hero-buttons">
          <a href="Contact.aspx" class="btn-primary">Contact us</a>
          <a href="tel:+27824055932" class="btn-ghost">082 405 5932</a>
        </div>
      </div>
    </section>

</asp:Content>