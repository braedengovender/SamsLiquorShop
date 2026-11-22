<%@ Page Title="Legal & Policies" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Legal.aspx.cs" Inherits="SamsLiquorShop.Legal" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ============ PAGE HEADER ============ --%>
    <section class="shell home-section" id="top">
      <div class="section-header">
        <h2>Legal &amp; Policies</h2>
      </div>
      <p class="section-subtext">Our Privacy Policy, Cookies Policy, Terms &amp; Conditions and Website Terms of Use, all in one place.</p>
    </section>

    <section class="shell home-section">
      <div class="legal-layout">

        <%-- ============ CONTENTS ============ --%>
        <nav class="card-surface legal-toc" aria-label="On this page">
          <h3>On this page</h3>
          <ol>
            <li><a href="#privacy">Privacy Policy</a></li>
            <li><a href="#cookies">Cookies Policy</a></li>
            <li><a href="#terms">Terms &amp; Conditions</a></li>
            <li><a href="#website-terms">Website Terms of Use</a></li>
          </ol>
        </nav>

        <div class="legal-sections">

          <%-- ============ 1. PRIVACY POLICY ============ --%>
          <article class="card-surface legal-section" id="privacy">
            <h2>1. Privacy Policy</h2>
            <p class="legal-updated">Last updated: 21 November 2026</p>

            <p>
              Sam's Liquor Shop ("we", "us", "our") respects your privacy. This policy explains what personal information we collect,
              why we collect it, and how we protect it, in line with the Protection of Personal Information Act 4 of 2013 (POPIA).
            </p>

            <h3>1.1 Information we collect</h3>
            <ul>
              <li><strong>Account details:</strong> your name, South African ID number, date of birth, email address, phone number and password (stored in encrypted form).</li>
              <li><strong>Delivery details:</strong> your delivery address and any delivery instructions you give us.</li>
              <li><strong>Order and loyalty history:</strong> the products you buy, order totals, payment method used and your loyalty points balance.</li>
              <li><strong>Technical information:</strong> your IP address, browser type and how you use our website (see the Cookies Policy below).</li>
            </ul>
            <p>We do not store full debit or credit card numbers on our systems.</p>

            <h3>1.2 Why we use your information</h3>
            <ul>
              <li>To verify that you are 18 or older and a South African, as required to hold an account and buy alcohol.</li>
              <li>To process, deliver and support your orders, including returns and refunds.</li>
              <li>To run the Loyalty &amp; Rewards programme and let you pay with loyalty points.</li>
              <li>To send you marketing emails and our newsletter, but only if you have opted in. You can unsubscribe at any time.</li>
              <li>To keep our website secure, prevent fraud and meet our legal and liquor licensing obligations.</li>
            </ul>

            <h3>1.3 Who we share it with</h3>
            <p>
              We only share your information with service providers who help us run the shop (such as our delivery partners, payment
              processor and email provider) and only as much as they need to do that job. We may also disclose information where the
              law requires it. We never sell your personal information.
            </p>

            <h3>1.4 How long we keep it</h3>
            <p>
              We keep your account information for as long as your account is open, and order records for as long as tax and liquor
              licensing laws require. After that, information is securely deleted or anonymised.
            </p>

            <h3>1.5 Your rights</h3>
            <p>
              Under POPIA you may ask to see the personal information we hold about you, ask us to correct or delete it, object to us
              using it for marketing, and lodge a complaint with the Information Regulator. To make a request, email
              <a href="mailto:info@samsliquorshop.co.za">info@samsliquorshop.co.za</a>.
            </p>

            <a href="#top" class="legal-back-top">&uarr; Back to top</a>
          </article>

          <%-- ============ 2. COOKIES POLICY ============ --%>
          <article class="card-surface legal-section" id="cookies">
            <h2>2. Cookies Policy</h2>
            <p class="legal-updated">Last updated: 21 November 2026</p>

            <p>
              Cookies are small text files that a website saves in your browser. We use them to make our website work properly and to
              remember your choices between visits.
            </p>

            <h3>2.1 Cookies we use</h3>
            <table>
              <thead>
                <tr><th>Type</th><th>What it does</th><th>Can I turn it off?</th></tr>
              </thead>
              <tbody>
                <tr><td>Essential</td><td>Keeps you logged in, remembers your cart and confirms you have passed the age check.</td><td>No &mdash; the site will not work without these.</td></tr>
                <tr><td>Preferences</td><td>Remembers settings such as light or dark mode.</td><td>Yes</td></tr>
                <tr><td>Analytics</td><td>Helps us understand which pages are popular so we can improve the site. The data is not used to identify you.</td><td>Yes</td></tr>
              </tbody>
            </table>

            <h3>2.2 Third-party content</h3>
            <p>
              Some pages show content from other services, such as the Google Maps location map in our footer. These services may set
              their own cookies, which are covered by their own privacy policies.
            </p>

            <h3>2.3 Managing cookies</h3>
            <p>
              You can block or delete cookies in your browser settings at any time. If you block essential cookies, you will not be able
              to log in, keep items in your cart or check out.
            </p>

            <a href="#top" class="legal-back-top">&uarr; Back to top</a>
          </article>

          <%-- ============ 3. TERMS & CONDITIONS ============ --%>
          <article class="card-surface legal-section" id="terms">
            <h2>3. Terms &amp; Conditions</h2>
            <p class="legal-updated">Last updated: 21 November 2026</p>

            <p>
              These terms apply to every order placed with Sam's Liquor Shop, online or in store. By placing an order you agree to them.
              Nothing in these terms limits your rights under the Consumer Protection Act 68 of 2008.
            </p>

            <h3>3.1 Who may buy from us</h3>
            <ul>
              <li>You must be 18 years or older. We do not sell or supply alcohol to anyone under 18.</li>
              <li>You need an account to place an order, and accounts are only available to South Africans with a valid South African ID number.</li>
              <li>Each account is for one person. You are responsible for keeping your password safe and for all orders placed on your account.</li>
            </ul>

            <h3>3.2 Orders and prices</h3>
            <ul>
              <li>All prices are in South African Rand (ZAR) and include VAT.</li>
              <li>An order is only accepted once we confirm it by email. We may cancel an order if a product is out of stock, a price was shown incorrectly, or we cannot verify your age or identity. If we cancel, you get a full refund.</li>
              <li>Specials and discounts are valid while stock lasts and for the dates shown.</li>
            </ul>

            <h3>3.3 Payment</h3>
            <p>
              We accept debit and credit cards and loyalty points. Each loyalty point is worth R1 and can be used for all or part of an
              order. Loyalty points have no cash value, cannot be transferred, and are not earned on the portion of an order paid with points.
            </p>

            <h3>3.4 Delivery and collection</h3>
            <ul>
              <li>Delivery is free on orders over R500. Smaller orders pay the delivery fee shown at checkout.</li>
              <li>We aim to deliver the next day; please allow 1 to 3 business days as an estimate. Delivery times are not guaranteed.</li>
              <li>The person receiving the order must be 18 or older and show a valid ID, driver's licence or passport. If no one can show valid ID, the driver will not hand over the order.</li>
              <li>Click &amp; Collect orders can be collected from 21 Coronation Road, Tongaat, during trading hours.</li>
            </ul>

            <h3>3.5 Returns and refunds</h3>
            <p>
              Returns are handled under our <a href="ReturnPolicy.aspx">Returns &amp; Refunds policy</a>. In short, unopened items may
              be returned within 7 days of delivery, and damaged or incorrect items must be reported within 48 hours.
            </p>

            <h3>3.6 Responsible trading</h3>
            <p>
              We trade in line with the Liquor Act and our liquor licence. We may refuse a sale to anyone who appears intoxicated, and
              liquor cannot be sold outside the trading hours our licence allows.
            </p>

            <a href="#top" class="legal-back-top">&uarr; Back to top</a>
          </article>

          <%-- ============ 4. WEBSITE TERMS OF USE ============ --%>
          <article class="card-surface legal-section" id="website-terms">
            <h2>4. Website Terms of Use</h2>
            <p class="legal-updated">Last updated: 21 November 2026</p>

            <p>
              These terms cover your use of the samsliquorshop.co.za website. By using the website you agree to them, in addition to the
              Electronic Communications and Transactions Act 25 of 2002 (ECTA) disclosures below.
            </p>

            <h3>4.1 Using the website</h3>
            <ul>
              <li>You must be 18 or older to browse or use this website.</li>
              <li>You may not use the website for anything unlawful, try to gain unauthorised access to it, or interfere with how it works.</li>
              <li>We may suspend or close an account that breaks these terms.</li>
            </ul>

            <h3>4.2 Content and intellectual property</h3>
            <p>
              The website's text, images, logos and design belong to Sam's Liquor Shop or are used with permission. Product names and
              brands belong to their owners. You may not copy or reuse our content for commercial purposes without written permission.
            </p>

            <h3>4.3 Accuracy and availability</h3>
            <p>
              We try to keep product information, pictures and prices accurate, but mistakes can happen. The website may occasionally be
              unavailable for maintenance. Links to other websites are provided for convenience and we are not responsible for their content.
            </p>

            <h3>4.4 Liability</h3>
            <p>
              To the extent the law allows, we are not liable for indirect loss caused by using the website. This does not affect any
              rights you have under the Consumer Protection Act.
            </p>

            <h3>4.5 Business details</h3>
            <p>
              Sam's Liquor Shop, 21 Coronation Road, Tongaat, Durban, 4399, KwaZulu-Natal &middot;
              <a href="tel:+27824055932">082 405 5932</a> &middot;
              <a href="mailto:info@samsliquorshop.co.za">info@samsliquorshop.co.za</a>
            </p>

            <h3>4.6 Changes to these policies</h3>
            <p>
              We may update any of the policies on this page. The "Last updated" date shows when each section last changed. Continuing
              to use the website after a change means you accept the updated version. These policies are governed by the laws of the
              Republic of South Africa.
            </p>

            <a href="#top" class="legal-back-top">&uarr; Back to top</a>
          </article>

        </div>
      </div>
    </section>

    <%-- ============ CLOSING CTA ============ --%>
    <section class="shell home-section">
      <div class="glass ship-cta">
        <h3>Questions about our policies?</h3>
        <p>Get in touch and we will be happy to help.</p>
        <div class="hero-buttons">
          <a href="Contact.aspx" class="btn-primary">Contact us</a>
          <a href="Contact.aspx" class="btn-ghost">Email us</a>
        </div>
      </div>
    </section>

</asp:Content>
