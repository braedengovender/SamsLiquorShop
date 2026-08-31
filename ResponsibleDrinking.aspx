<%@ Page Title="Responsible Drinking Support" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ResponsibleDrinking.aspx.cs" Inherits="SamsLiquorShop.ResponsibleDrinking" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ============ PAGE HEADER ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Responsible Drinking Support</h2>
      </div>
      <p class="section-subtext">Selling alcohol is a responsibility we take seriously. Below is an outline of our commitments, along with resources should you or someone you know require support.</p>
    </section>

    <%-- ============ OUR COMMITMENT ============ --%>
    <section class="shell home-section">
      <div class="glass hero-card">
        <div class="hero-text">
          <p class="hero-eyebrow">Our commitment</p>
          <h1 class="hero-heading">Responsible retailing is not optional &mdash; it is central to how we operate.</h1>
          <p class="hero-sub">
            Under South African law, alcohol may only be sold or supplied to individuals aged 18 and
            over. We verify age at checkout and again on delivery, and we do not knowingly serve anyone
            who appears intoxicated. We would rather decline a sale than serve irresponsibly, and we ask
            all our customers to help us uphold that standard.
          </p>
        </div>
      </div>
    </section>

    <%-- ============ STANDARD DRINK SIZES ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Understanding a Standard Serving</h2>
      </div>
      <%-- [CONFIRM] These are general reference measures, not an official SA regulatory standard --%>
      <p class="section-subtext">Alcohol content varies considerably between products. The following is a general guide to a standard serving of each.</p>

      <div class="rd-units-grid">
        <div class="card-surface rd-unit-card">
          <span class="ship-step-num">&#127866;</span>
          <h3>Beer / Cider</h3>
          <p>Approximately 340ml at around 5% alcohol by volume &mdash; equivalent to one standard can or bottle.</p>
        </div>
        <div class="card-surface rd-unit-card">
          <span class="ship-step-num">&#127863;</span>
          <h3>Wine</h3>
          <p>Approximately 150ml, or one standard glass, at around 12&ndash;13% alcohol by volume.</p>
        </div>
        <div class="card-surface rd-unit-card">
          <span class="ship-step-num">&#127864;</span>
          <h3>Spirits</h3>
          <p>Approximately a 25ml measure at around 40% alcohol by volume, served neat or mixed.</p>
        </div>
      </div>
    </section>

    <%-- ============ SAFETY NOTICE ============ --%>
    <section class="shell home-section">
      <div class="card-surface ship-notice">
        <span class="ship-notice-icon">&#9888;</span>
        <div>
          <h3>A few reminders worth repeating</h3>
          <p>
            Never drink and drive &mdash; arrange transport before you begin drinking, not after. Avoid
            alcohol entirely if you are pregnant or trying to conceive. Do not combine alcohol with
            medication without first consulting a pharmacist or doctor, and always eat before and during
            drinking. Pace yourself with water between drinks, and be aware that coffee or a cold shower
            will not accelerate sobriety &mdash; only time will.
          </p>
        </div>
      </div>
    </section>

    <%-- ============ SIGNS & SUPPORT ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Recognising the Signs, in Yourself or Others</h2>
      </div>

      <div class="ship-two-col">
        <div class="card-surface ship-panel">
          <h3>Signs it may be more than social drinking</h3>
          <p>
            Requiring more alcohol to achieve the same effect, drinking to manage stress or low mood,
            missing work, family, or social obligations as a result of drinking, or being unable to
            reduce consumption despite wanting to &mdash; these are all worth taking seriously, whether
            in yourself or someone you care about.
          </p>
        </div>
        <div class="card-surface ship-panel">
          <h3>Supporting someone you are concerned about</h3>
          <p>
            Choose a calm moment to raise the subject, listen without judgement, and avoid confrontation
            while they have been drinking. You are not expected to have all the answers &mdash; offering
            to help them access support, or reaching out to one of the organisations below together, is
            often a meaningful first step.
          </p>
        </div>
      </div>
    </section>

    <%-- ============ RESOURCES ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Where to Find Support</h2>
      </div>
      <p class="section-subtext">Free, confidential support is available across South Africa.</p>

      <div class="rd-resource-grid">
        <div class="card-surface rd-resource-card">
          <h3>SANCA National</h3>
          <p>The South African National Council on Alcoholism and Drug Dependence provides prevention, counselling, and treatment services in every province.</p>
          <a href="tel:+27118923829">011 892 3829</a>
        </div>
        <div class="card-surface rd-resource-card">
          <h3>SADAG Substance Abuse Helpline</h3>
          <p>The South African Depression and Anxiety Group operates a free, 24-hour helpline for substance-related support.</p>
          <a href="tel:0800121314">0800 12 13 14</a>
        </div>
        <div class="card-surface rd-resource-card">
          <h3>LifeLine Southern Africa</h3>
          <p>Free, confidential telephone counselling, available around the clock, for a wide range of personal difficulties.</p>
          <a href="tel:0861322322">0861 322 322</a>
        </div>
        <div class="card-surface rd-resource-card">
          <h3>Alcoholics Anonymous South Africa</h3>
          <p>A peer support fellowship with meetings held across the country. The only requirement for membership is a desire to stop drinking.</p>
          <a href="https://www.aasouthafrica.org.za" target="_blank" rel="noopener">aasouthafrica.org.za</a>
        </div>
      </div>
    </section>

    <%-- ============ CLOSING CTA ============ --%>
    <section class="shell home-section">
      <div class="glass ship-cta">
        <h3>Valid identification is required for every delivery</h3>
        <p>Learn more about how age verification is enforced on delivery.</p>
        <div class="hero-buttons">
          <a href="Shipping.aspx" class="btn-primary">Shipping &amp; Delivery Info</a>
          <a href="Contact.aspx" class="btn-ghost">Contact us</a>
        </div>
      </div>
    </section>

</asp:Content>