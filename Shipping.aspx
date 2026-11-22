<%@ Page Title="Shipping & Delivery" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Shipping.aspx.cs" Inherits="SamsLiquorShop.Shipping" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ============ PAGE HEADER ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Shipping &amp; Delivery</h2>
      </div>
      <p class="section-subtext">Getting your order to your door, safely and on time.</p>
    </section>

       <%-- ============ OPTIONS AT A GLANCE ============ --%>
    <section class="shell home-section">
      <div class="glass stats-bar">
        <div class="stat-item">
          <div class="stat-number">Next day</div>
          <div class="stat-label">Durban area delivery</div>
        </div>
        <div class="stat-item">
          <div class="stat-number">R70</div>
          <div class="stat-label">Flat-rate delivery &mdash; free over R500</div>
        </div>
        <div class="stat-item">
          <div class="stat-number">Free</div>
          <div class="stat-label">Click &amp; Collect in Tongaat</div>
        </div>
      </div>
    </section>

    <%-- ============ HOW IT WORKS ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>How It Works</h2>
      </div>
      <p class="section-subtext">From checkout to your doorstep in four steps.</p>

      <div class="ship-steps">
        <div class="card-surface no-lift ship-step">
          <span class="ship-step-num">1</span>
          <h3>Place your order</h3>
          <p>Shop online and check out. You'll get an email confirmation with your order number straight away.</p>
        </div>
        <div class="card-surface no-lift ship-step">
          <span class="ship-step-num">2</span>
          <h3>We pack it</h3>
          <p>Our team picks and packs your order in store, with bottles wrapped and boxed to survive the trip.</p>
        </div>
        <div class="card-surface no-lift ship-step">
          <span class="ship-step-num">3</span>
          <h3>Out for delivery</h3>
          <p>You'll receive a tracking link by SMS and email as soon as your order leaves our store.</p>
        </div>
        <div class="card-surface no-lift ship-step">
          <span class="ship-step-num">4</span>
          <h3>ID on arrival</h3>
          <p>Have your ID ready. Our driver must verify you're 18 or older before handing over any alcohol.</p>
        </div>
      </div>
    </section>

      <%-- ============ AREAS & COSTS ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Delivery Areas &amp; Costs</h2>
      </div>
      <p class="section-subtext">Flat R70 delivery &mdash; free on all orders over R500.</p>

      <div class="card-surface no-lift ship-table-wrap">
        <table class="ship-table">
          <thead>
            <tr>
              <th scope="col">Area</th>
              <th scope="col">Delivery time</th>
              <th scope="col">Cost</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>
                <strong>Durban &amp; surrounds</strong>
                <span class="ship-table-note">Tongaat, Umhlanga, Ballito, Durban North, CBD</span>
              </td>
              <td>Next day</td>
              <td>R70 (free over R500)</td>
            </tr>
            <tr>
              <td>
                <strong>Click &amp; Collect</strong>
                <span class="ship-table-note">21 Coronation Road, Tongaat</span>
              </td>
              <td>Ready within 2 hours</td>
              <td>Free</td>
            </tr>
          </tbody>
        </table>
      </div>

      <p class="hours-note">
        Minimum order value for delivery is R250. We currently deliver within the Durban area only,
        and we do not offer same-day delivery. There is no minimum order for Click &amp; Collect.
      </p>
    </section>

    <%-- ============ AGE VERIFICATION ============ --%>
    <section class="shell home-section">
      <div class="card-surface no-lift ship-notice">
        <span class="ship-notice-icon">18+</span>
        <div>
          <h3>We check ID at the door. Every time.</h3>
          <p>
            South African law prohibits the sale or supply of alcohol to anyone under the age of 18.
            The person accepting the delivery must be 18 or older and must produce a valid ID,
            driver's licence or passport.
          </p>
          <p>
            Our driver will refuse the handover if nobody of legal age is available, if no valid ID is
            produced, or if the recipient appears intoxicated. In these cases the order returns to our
            store and a re-delivery fee may apply. We cannot leave alcohol unattended at a door,
            with a neighbour, or in a safe place.
          </p>
          <p class="ship-notice-links">
            <a href="ResponsibleDrinking.aspx">Responsible drinking support &rarr;</a>
          </p>
        </div>
      </div>
    </section>

    <%-- ============ CUT-OFFS & WINDOWS ============ --%>
    <section class="shell home-section">
      <div class="section-header">
       <h2>Delivery Windows</h2>
      </div>
      <p class="section-subtext">When to order, and when to expect us.</p>

      <div class="ship-two-col">
                <div class="card-surface no-lift ship-panel">
          <h3>Next-day delivery</h3>
          <p>Every delivery order is delivered the next day, inside the delivery windows shown here. We don't offer same-day delivery.</p>
          <p class="hours-note">Need it sooner? Collect from our Tongaat store &mdash; Click &amp; Collect is free and ready within 2 hours.</p>
        </div>

        <div class="card-surface no-lift ship-panel">
          <h3>Delivery windows</h3>
          <%-- [CONFIRM] Windows chosen to sit inside footer trading hours --%>
          <ul class="hours-list">
            <li><span>Mon &ndash; Thu</span><span>10:00 &ndash; 17:30</span></li>
            <li><span>Fri &ndash; Sat</span><span>10:00 &ndash; 19:30</span></li>
            <li><span>Sunday</span><span>11:00 &ndash; 14:30</span></li>
            <li><span>Public Holidays</span><span>11:00 &ndash; 14:30</span></li>
          </ul>
          <p class="hours-note">We don't deliver outside our trading hours.</p>
        </div>
      </div>
    </section>

    <%-- ============ TRACKING & MISSED DELIVERY ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Tracking Your Order</h2>
      </div>

      <div class="ship-two-col">
        <div class="card-surface no-lift ship-panel">
          <h3>Where's my order?</h3>
          <%-- [CONFIRM] No SMS or email service is wired up yet - this describes
               intended behaviour, not something the site currently does --%>
          <p>
            Every order gets a tracking link by SMS and email once it leaves our store. You can also
            see the live status of any order once you're signed in.
          </p>
          <%-- Account/Orders.aspx is not built yet - pointing at Login.aspx until it exists --%>
          <p><a href="Login.aspx" class="btn-primary">Sign in to track</a></p>
        </div>

        <div class="card-surface no-lift ship-panel">
          <h3>Missed your delivery?</h3>
          <p>
            Our driver will call you on the number from your order. If we can't reach you, the order
            comes back to the store and we'll contact you to rebook &mdash; no need to reorder.
          </p>
          <%-- [CONFIRM] Re-delivery fee --%>
          <p class="hours-note">A second delivery attempt to the same address is charged at R70.</p>
        </div>
      </div>
    </section>

    <%-- ============ RELATED ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Related Information</h2>
      </div>
      <div class="guides-grid">
        <a href="Legal.aspx#terms" class="card-surface guide-card">
          <p>Terms &amp; Conditions</p><span class="arrow">&rarr;</span>
        </a>
        <a href="ReturnPolicy.aspx" class="card-surface guide-card">
          <p>Returns &amp; Refunds</p><span class="arrow">&rarr;</span>
        </a>
        <a href="FAQ.aspx" class="card-surface guide-card">
          <p>FAQ</p><span class="arrow">&rarr;</span>
        </a>
        <a href="Contact.aspx" class="card-surface guide-card">
          <p>Contact Us</p><span class="arrow">&rarr;</span>
        </a>
      </div>
    </section>

    <%-- ============ HELP CTA ============ --%>
    <section class="shell home-section">
      <div class="glass ship-cta">
        <h3>Still have a delivery question?</h3>
        <p>Our team is here during trading hours.</p>
        <div class="hero-buttons">
          <a href="tel:+27824055932" class="btn-primary">082 405 5932</a>
          <a href="Contact.aspx" class="btn-ghost">Email us</a>
        </div>
      </div>
    </section>

</asp:Content>