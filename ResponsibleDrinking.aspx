<%@ Page Title="Responsible Drinking Support" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ResponsibleDrinking.aspx.cs" Inherits="SamsLiquorShop.ResponsibleDrinking" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ============ PAGE HEADER ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Responsible Drinking Support</h2>
      </div>
      <p class="section-subtext">We sell alcohol, so we think about how it gets used. Here is what we do on our side, and where to find help if you or someone close to you is struggling.</p>
    </section>

    <%-- ============ OUR COMMITMENT ============ --%>
    <section class="shell home-section">
      <div class="glass hero-card">
        <div class="hero-text">
          <p class="hero-eyebrow">Our commitment</p>
          <h1 class="hero-heading">We take selling alcohol seriously.</h1>
          <p class="hero-sub">
            South African law says alcohol can only be sold to people aged 18 and over. We check
            your age when you order and again at your door, and we won't serve anyone who looks
            intoxicated. We would rather turn down a sale than serve someone who shouldn't be
            drinking, and we ask our customers to help us keep it that way.
          </p>
        </div>
      </div>
    </section>

    <%-- ============ STANDARD DRINK SIZES ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>How Much Is One Drink?</h2>
      </div>
      <%-- [CONFIRM] These are general reference measures, not an official SA regulatory standard --%>
      <p class="section-subtext">Strength varies a lot from one product to the next. This is a rough guide to what counts as a single serving.</p>

      <div class="rd-units-grid">
        <div class="card-surface rd-unit-card">
          <span class="ship-step-num">&#127866;</span>
          <h3>Beer / Cider</h3>
          <p>About 340ml at around 5% alcohol, so roughly one can or bottle.</p>
        </div>
        <div class="card-surface rd-unit-card">
          <span class="ship-step-num">&#127863;</span>
          <h3>Wine</h3>
          <p>About 150ml, which is a standard glass, at around 12 to 13% alcohol.</p>
        </div>
        <div class="card-surface rd-unit-card">
          <span class="ship-step-num">&#127864;</span>
          <h3>Spirits</h3>
          <p>About a 25ml tot at around 40% alcohol, neat or in a mixer.</p>
        </div>
      </div>
    </section>

    <%-- ============ SAFETY NOTICE ============ --%>
    <section class="shell home-section">
      <div class="card-surface ship-notice">
        <span class="ship-notice-icon">&#9888;</span>
        <div>
          <h3>A few basics</h3>
          <p>
            Don't drive after drinking. Sort out a lift or an e-hailing ride before your first
            drink, not after your last one. Skip alcohol completely if you are pregnant or trying
            to fall pregnant. Check with a pharmacist or doctor before mixing alcohol with any
            medication, and try to eat while you drink. Have water between drinks. Coffee and cold
            showers don't sober you up. Only time does.
          </p>
        </div>
      </div>
    </section>

    <%-- ============ SIGNS & SUPPORT ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Noticing the Signs, in Yourself or Someone Else</h2>
      </div>

      <div class="ship-two-col">
        <div class="card-surface ship-panel">
          <h3>When it might be more than social drinking</h3>
          <p>
            Needing more to get the same effect, drinking to cope with stress or a low mood,
            missing work or family plans because of it, or not being able to cut down even though
            you want to. Any of these is worth taking seriously, whether it's you or someone you
            care about.
          </p>
        </div>
        <div class="card-surface ship-panel">
          <h3>If you're worried about someone</h3>
          <p>
            Pick a calm moment to bring it up, listen without judging, and don't try to have the
            conversation while they've been drinking. You don't need all the answers. Offering to
            help them find support, or to phone one of the organisations below with them, is a
            good place to start.
          </p>
        </div>
      </div>
    </section>

    <%-- ============ RESOURCES ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Where to Find Support</h2>
      </div>
      <p class="section-subtext">These services are free and confidential, and they work across South Africa.</p>

      <div class="rd-resource-grid">
        <div class="card-surface rd-resource-card">
          <h3>SANCA National</h3>
          <p>The South African National Council on Alcoholism and Drug Dependence offers prevention, counselling and treatment services in every province.</p>
          <a href="tel:+27118923829">011 892 3829</a>
        </div>
        <div class="card-surface rd-resource-card">
          <h3>SADAG Substance Abuse Helpline</h3>
          <p>The South African Depression and Anxiety Group runs a free helpline for substance-related problems, open 24 hours a day.</p>
          <a href="tel:0800121314">0800 12 13 14</a>
        </div>
        <div class="card-surface rd-resource-card">
          <h3>LifeLine Southern Africa</h3>
          <p>Free, confidential phone counselling at any hour, for whatever you're going through.</p>
          <a href="tel:0861322322">0861 322 322</a>
        </div>
        <div class="card-surface rd-resource-card">
          <h3>Alcoholics Anonymous South Africa</h3>
          <p>A peer support group with meetings across the country. All you need is a wish to stop drinking.</p>
          <a href="https://www.aasouthafrica.org.za" target="_blank" rel="noopener">aasouthafrica.org.za</a>
        </div>
      </div>
    </section>

    <%-- ============ CLOSING CTA ============ --%>
    <section class="shell home-section">
      <div class="glass ship-cta">
        <h3>We ask for ID on every delivery</h3>
        <p>Read how age checks work when your order arrives.</p>
        <div class="hero-buttons">
          <a href="Shipping.aspx" class="btn-primary">Shipping &amp; Delivery Info</a>
          <a href="Contact.aspx" class="btn-ghost">Contact us</a>
        </div>
      </div>
    </section>

</asp:Content>