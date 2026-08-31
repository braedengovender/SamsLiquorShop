<%@ Page Title="Returns & Refunds" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReturnPolicy.aspx.cs" Inherits="SamsLiquorShop.ReturnPolicy" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ============ PAGE HEADER ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Returns &amp; Refunds</h2>
      </div>
      <p class="section-subtext">A fair and straightforward process, in line with what South African liquor licensing regulations permit.</p>
    </section>

    <%-- ============ AT A GLANCE ============ --%>
    <section class="shell home-section">
      <div class="glass stats-bar">
        <div class="stat-item">
          <%-- [CONFIRM] Return window --%>
          <div class="stat-number">7 days</div>
          <div class="stat-label">To request a return from date of delivery</div>
        </div>
        <div class="stat-item">
          <div class="stat-number">Unopened</div>
          <div class="stat-label">Seal must be intact to qualify</div>
        </div>
        <div class="stat-item">
          <%-- [CONFIRM] Refund processing time --%>
          <div class="stat-number">5&ndash;7 days</div>
          <div class="stat-label">For your refund to reflect</div>
        </div>
      </div>
    </section>

    <%-- ============ ELIGIBILITY ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>What Can Be Returned</h2>
      </div>
      <p class="section-subtext">This policy operates alongside your rights under the Consumer Protection Act.</p>

      <div class="ship-two-col">
        <div class="card-surface ship-panel">
          <h3>Eligible for return</h3>
          <ul class="hours-list">
            <li><span>Unopened, sealed items</span><span>&#10003;</span></li>
            <li><span>Within 7 days of delivery</span><span>&#10003;</span></li>
            <li><span>Accompanied by order number or receipt</span><span>&#10003;</span></li>
            <li><span>Damaged or incorrect items</span><span>&#10003;</span></li>
          </ul>
        </div>
        <div class="card-surface ship-panel">
          <h3>Not eligible for return</h3>
          <ul class="hours-list">
            <li><span>Opened or broken-seal alcohol</span><span>&times;</span></li>
            <li><span>Items on final clearance</span><span>&times;</span></li>
            <li><span>Free promotional or gift items</span><span>&times;</span></li>
          </ul>
        </div>
      </div>
    </section>

    <%-- ============ HOW TO START A RETURN ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>How to Initiate a Return</h2>
      </div>
      <p class="section-subtext">Four steps from request to refund.</p>

      <div class="ship-steps">
        <div class="card-surface ship-step">
          <span class="ship-step-num">1</span>
          <h3>Contact us</h3>
          <p>Provide your order number and the reason for the return within 7 days of delivery.</p>
        </div>
        <div class="card-surface ship-step">
          <span class="ship-step-num">2</span>
          <h3>We confirm eligibility</h3>
          <p>We will verify that the item qualifies and send you confirmation along with return instructions.</p>
        </div>
        <div class="card-surface ship-step">
          <span class="ship-step-num">3</span>
          <h3>Return the item</h3>
          <p>Send the item back via courier, or drop it off at our Tongaat store &mdash; whichever is more convenient.</p>
        </div>
        <div class="card-surface ship-step">
          <span class="ship-step-num">4</span>
          <h3>Refund issued</h3>
          <p>Once received and inspected, your refund is issued to your original payment method.</p>
        </div>
      </div>
    </section>

    <%-- ============ DAMAGED / INCORRECT ITEMS ============ --%>
    <section class="shell home-section">
      <div class="card-surface ship-notice">
        <span class="ship-notice-icon">&#128230;</span>
        <div>
          <h3>Damaged or incorrect item? We will resolve it at no cost to you.</h3>
          <p>
            Occasionally bottles do not survive transit, or a picking error occurs. Should this happen,
            please contact us within 48 hours of delivery with a photo of the item and your order number,
            and we will arrange a replacement or refund at no charge, including return shipping.
          </p>
        </div>
      </div>
    </section>

    <%-- ============ REFUND METHODS ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>How Refunds Are Processed</h2>
      </div>

      <div class="ship-two-col">
        <div class="card-surface ship-panel">
          <h3>Refund method</h3>
          <p>
            Refunds are issued to the original payment method used for the order. We are unable to refund
            to a different card, account, or as cash.
          </p>
        </div>
        <div class="card-surface ship-panel">
          <h3>Processing time</h3>
          <%-- [CONFIRM] --%>
          <p>
            Once your return has been received and approved, refunds are processed within 5&ndash;7
            business days. Depending on your bank, it may take a further few days to reflect in your
            account.
          </p>
        </div>
      </div>
    </section>

    <%-- ============ RELATED ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Related Information</h2>
      </div>
      <div class="guides-grid">
        <a href="Shipping.aspx" class="card-surface guide-card">
          <p>Shipping &amp; Delivery</p><span class="arrow">&rarr;</span>
        </a>
        <a href="FAQ.aspx" class="card-surface guide-card">
          <p>FAQ</p><span class="arrow">&rarr;</span>
        </a>
        <a href="ResponsibleDrinking.aspx" class="card-surface guide-card">
          <p>Responsible Drinking Support</p><span class="arrow">&rarr;</span>
        </a>
        <a href="Contact.aspx" class="card-surface guide-card">
          <p>Contact Us</p><span class="arrow">&rarr;</span>
        </a>
      </div>
    </section>

    <%-- ============ CLOSING CTA ============ --%>
    <section class="shell home-section">
      <div class="glass ship-cta">
        <h3>Need to initiate a return?</h3>
        <p>Get in touch and we will guide you through the process.</p>
        <div class="hero-buttons">
          <a href="mailto:info@samsliquorshop.co.za" class="btn-primary">Email us</a>
          <a href="tel:+27824055932" class="btn-ghost">082 405 5932</a>
        </div>
      </div>
    </section>

</asp:Content>