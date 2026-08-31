<%@ Page Title="Rewards" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Rewards.aspx.cs" Inherits="SamsLiquorShop.Rewards" %>
<%--
  Loyalty rules on this page mirror the M2 POS implementation
  (TheByteClubPOS / POSForm.cs, approx. lines 1125-1145):
      earn   : (int)Math.Floor(total / 10)   -> 1 point per R10, rounded down
      redeem : (int)Math.Ceiling(total)      -> 1 point = R1, rounded up
      paying with points sets loyaltyPointsEarned = 0
      Customer_Status "Inactive" disables accrual
  Do not change these numbers without changing the POS too.
--%>
 
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
 
    <%-- ============ PAGE HEADER ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Sam's Rewards</h2>
      </div>
      <p class="section-subtext">Earn a point for every R10 you spend. Spend them like cash.</p>
    </section>
 
    <%-- ============ YOUR BALANCE ============ --%>
    <section class="shell home-section">
 
      <%-- Signed OUT --%>
      <asp:PlaceHolder runat="server" ID="phSignedOut">
        <div class="glass rw-balance">
          <div class="rw-balance-main">
            <p class="rw-balance-eyebrow">Your points</p>
            <h3>Sign in to see your balance</h3>
            <p>Points are added to your account automatically on every order. Joining is free.</p>
          </div>
          <%-- Registration isn't wired up yet, so these point at Login / Contact --%>
          <div class="hero-buttons">
            <a href="Login.aspx" class="btn-primary">Log in</a>
            <a href="Contact.aspx" class="btn-ghost">Join in store</a>
          </div>
        </div>
      </asp:PlaceHolder>
 
      <%-- Signed IN --%>
      <asp:PlaceHolder runat="server" ID="phSignedIn" Visible="false">
        <div class="glass rw-balance">
          <div class="rw-balance-main">
            <p class="rw-balance-eyebrow">Your points</p>
            <%-- Static until this reads Customer_LoyaltyPointsBalance from the database --%>
            <p class="rw-balance-value">0 <span>points &middot; worth R0</span></p>
            <p>Your balance updates after your first order. Points usually appear within 24 hours.</p>
          </div>
          <div class="hero-buttons">
            <a href="Shop.aspx" class="btn-primary">Start shopping</a>
            <a href="Shipping.aspx" class="btn-ghost">Delivery info</a>
          </div>
        </div>
      </asp:PlaceHolder>
 
    </section>
 
    <%-- ============ HOW IT WORKS ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>How It Works</h2>
      </div>
      <p class="section-subtext">Three steps. No forms, no fuss.</p>
 
      <div class="guides-grid">
        <div class="card-surface no-lift rw-step">
          <span class="rw-step-num">1</span>
          <h3>Sign in and shop</h3>
          <p>Log in before you check out, or give your details at the till, so your points land on the right account.</p>
        </div>
        <div class="card-surface no-lift rw-step">
          <span class="rw-step-num">2</span>
          <h3>Earn automatically</h3>
          <p>One point for every R10 you spend, added to your balance on every completed order. Nothing to claim.</p>
        </div>
        <div class="card-surface no-lift rw-step">
          <span class="rw-step-num">3</span>
          <h3>Pay with points</h3>
          <p>Choose Loyalty Points as your payment method at the till. One point covers R1 of your total.</p>
        </div>
      </div>
    </section>
 
    <%-- ============ THE NUMBERS ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Earning &amp; Spending</h2>
      </div>
      <p class="section-subtext">The whole scheme, in three numbers.</p>
 
      <div class="glass stats-bar">
        <div class="stat-item">
          <div class="stat-number">R10 = 1</div>
          <div class="stat-label">point earned per R10 spent</div>
        </div>
        <div class="stat-item">
          <div class="stat-number">1 = R1</div>
          <div class="stat-label">each point is worth one rand</div>
        </div>
        <div class="stat-item">
          <div class="stat-number">3 ways</div>
          <div class="stat-label">pay by cash, card or points</div>
        </div>
      </div>
    </section>
 
    <%-- ============ WORKED EXAMPLE ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>A Worked Example</h2>
      </div>
 
      <div class="ship-two-col">
        <div class="card-surface no-lift ship-panel">
          <h3>Spending R450</h3>
          <ul class="hours-list">
            <li><span>Your total</span><span>R450.00</span></li>
            <li><span>Points earned</span><span>45 points</span></li>
            <li><span>Worth</span><span>R45.00</span></li>
          </ul>
          <p class="hours-note">
            R450 &divide; 10 = 45. Part-points are rounded down, so R459 still earns 45 points.
          </p>
        </div>
 
        <div class="card-surface no-lift ship-panel">
          <h3>Paying with 45 points</h3>
          <ul class="hours-list">
            <li><span>Points available</span><span>45 points</span></li>
            <li><span>Covers</span><span>R45.00 of your bill</span></li>
            <li><span>Points earned on it</span><span>None</span></li>
          </ul>
          <p class="hours-note">
            Points spent are rounded up to the nearest rand, and a purchase paid with points doesn't earn new points.
          </p>
        </div>
      </div>
    </section>
 
    <%-- ============ THE RULES ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Good to Know</h2>
      </div>
 
      <div class="card-surface no-lift ship-notice">
        <span class="ship-notice-icon">i</span>
        <div>
          <h3>The fine print, in plain language.</h3>
          <p>
            <strong>You can't earn and spend on the same purchase.</strong> If you pay with points,
            that order doesn't earn new points. Pay by cash or card and you earn as normal.
          </p>
          <p>
            <strong>Points are earned on what you actually pay</strong> &mdash; the total after any
            discounts have come off.
          </p>
          <p>
            <strong>Your account needs to be active.</strong> Accounts marked inactive don't accumulate
            points. If you think that's happened in error, speak to us in store.
          </p>
          <p>
            <strong>Points aren't transferable</strong> between accounts and can't be exchanged for cash.
          </p>
        </div>
      </div>
    </section>
 
    <%-- ============ LOYALTY CARD  (footer anchor target: Loyalty.aspx#card) ============ --%>
    <section class="shell home-section" id="card">
      <div class="section-header">
        <h2>Get the Loyalty Card</h2>
      </div>
 
      <div class="card-surface no-lift rw-card-section">
        <img src="Images/about/loyalty-card.jpg" alt="Sam's Liquor Shop loyalty card" />
        <div class="rw-card-body">
          <h3>One balance. In store and online.</h3>
          <p>
            Your card is linked to the same account you use online, so points earned at the till and
            points earned on the website land in one balance.
          </p>
          <p>
            Give your name or phone number at the counter at 21 Coronation Road and we'll pull up your
            profile. Your points balance is shown on your receipt after every sale.
          </p>
          <%-- [CONFIRM] Whether a physical card is actually issued, or the account IS the card --%>
          <div class="hero-buttons">
            <a href="Login.aspx" class="btn-primary">Join free</a>
            <a href="Contact.aspx" class="btn-ghost">Ask in store</a>
          </div>
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
        <a href="Terms.aspx" class="card-surface guide-card">
          <p>Terms &amp; Conditions</p><span class="arrow">&rarr;</span>
        </a>
        <a href="PrivacyPolicy.aspx" class="card-surface guide-card">
          <p>Privacy Policy</p><span class="arrow">&rarr;</span>
        </a>
        <a href="FAQ.aspx" class="card-surface guide-card">
          <p>FAQ</p><span class="arrow">&rarr;</span>
        </a>
      </div>
    </section>
 
    <%-- ============ CTA ============ --%>
    <section class="shell home-section">
      <div class="glass ship-cta">
        <h3>Questions about your points?</h3>
        <p>We're happy to check your balance in store or over the phone.</p>
        <div class="hero-buttons">
          <a href="tel:+27824055932" class="btn-primary">082 405 5932</a>
          <a href="mailto:info@samsliquorshop.co.za" class="btn-ghost">Email us</a>
        </div>
      </div>
    </section>
 
</asp:Content>