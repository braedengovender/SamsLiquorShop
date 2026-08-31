<%@ Page Title="Contact Us" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="SamsLiquorShop.Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ============ PAGE HEADER ============ --%>
    <section class="shell home-section">
      <div class="section-header">
        <h2>Contact Us</h2>
      </div>
      <p class="section-subtext">Questions, feedback, or enquiries of any kind &mdash; we would be glad to hear from you.</p>
    </section>

    <%-- ============ FORM + INFO ============ --%>
    <section class="shell home-section">
      <div class="contact-grid">

        <%-- ---- Contact form ---- --%>
        <div class="card-surface contact-form-card">
          <h3>Send us a message</h3>
          <p class="contact-lead">We typically respond within one business day.</p>

          <div class="form-group">
            <input type="text" id="contactName" placeholder="Your name" autocomplete="name" />
            <span class="field-error" id="contactNameError"></span>
          </div>

          <div class="form-group">
            <input type="email" id="contactEmail" placeholder="Email address" autocomplete="email" />
            <span class="field-error" id="contactEmailError"></span>
          </div>

          <div class="form-group">
            <select id="contactSubject">
              <option value="">Please select a subject</option>
              <option value="general">General enquiry</option>
              <option value="order">Order support</option>
              <option value="delivery">Delivery question</option>
              <option value="returns">Returns &amp; refunds</option>
              <option value="feedback">Feedback</option>
            </select>
            <span class="field-error" id="contactSubjectError"></span>
          </div>

          <div class="form-group">
            <textarea id="contactMessage" rows="5" placeholder="Your message"></textarea>
            <span class="field-error" id="contactMessageError"></span>
          </div>

          <button type="button" class="btn-primary" onclick="submitContactForm(event)">Send Message</button>
          <div class="form-success" id="contactFormSuccess">Thank you &mdash; your message has been sent. We will be in touch shortly.</div>
        </div>

        <%-- ---- Store info ---- --%>
        <div class="card-surface contact-info-card">
          <h3>Visit or reach us directly</h3>
          <p class="contact-lead">Durban's trusted liquor store.</p>

          <ul class="contact-info-list">
            <li>
              <span class="contact-info-icon">&#8962;</span>
              <div>
                <strong>Address</strong>
                <span>21 Coronation Road, Tongaat, Durban, 4399, KwaZulu-Natal</span>
              </div>
            </li>
            <li>
              <span class="contact-info-icon">&#9742;</span>
              <div>
                <strong>Phone</strong>
                <a href="tel:+27824055932">082 405 5932</a>
              </div>
            </li>
            <li>
              <span class="contact-info-icon">&#9993;</span>
              <div>
                <strong>Email</strong>
                <a href="mailto:info@samsliquorshop.co.za">info@samsliquorshop.co.za</a>
              </div>
            </li>
          </ul>

          <ul class="hours-list">
            <li><span>Mon &ndash; Thu</span><span>09:00 &ndash; 18:00</span></li>
            <li><span>Fri &ndash; Sat</span><span>09:00 &ndash; 20:00</span></li>
            <li><span>Sunday</span><span>10:00 &ndash; 15:00</span></li>
            <li><span>Public Holidays</span><span>10:00 &ndash; 15:00</span></li>
          </ul>

          <div class="contact-map">
            <iframe
              src="https://www.google.com/maps?q=21+Coronation+Road,+Tongaat,+4399&output=embed"
              loading="lazy"
              referrerpolicy="no-referrer-when-downgrade">
            </iframe>
          </div>

          <div class="contact-social">
            <a href="#" aria-label="Facebook"><img src="Images/social/facebook.png" alt="Facebook" /></a>
            <a href="#" aria-label="Instagram"><img src="Images/social/instagram.png" alt="Instagram" /></a>
            <a href="#" aria-label="X"><img src="Images/social/x.png" alt="Twitter" /></a>
            <a href="#" aria-label="YouTube"><img src="Images/social/youtube.png" alt="YouTube" /></a>
            <a href="#" aria-label="TikTok"><img src="Images/social/tiktok.png" alt="TikTok" /></a>
          </div>
        </div>

      </div>
    </section>

    <%-- ============ QUICK LINK ============ --%>
    <section class="shell home-section">
      <div class="glass ship-cta">
        <h3>Looking for a quick answer instead?</h3>
        <p>Most common questions are already addressed in our FAQ.</p>
        <div class="hero-buttons">
          <a href="FAQ.aspx" class="btn-primary">Browse the FAQ</a>
          <a href="Shipping.aspx" class="btn-ghost">Shipping &amp; Delivery</a>
        </div>
      </div>
    </section>

    <%-- ============ CONTACT FORM SCRIPT ============ --%>
    <script>
        function submitContactForm(e) {
            e.preventDefault();

            var name = document.getElementById('contactName');
            var email = document.getElementById('contactEmail');
            var subject = document.getElementById('contactSubject');
            var message = document.getElementById('contactMessage');
            var emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            var valid = true;

            document.getElementById('contactNameError').textContent = '';
            document.getElementById('contactEmailError').textContent = '';
            document.getElementById('contactSubjectError').textContent = '';
            document.getElementById('contactMessageError').textContent = '';
            name.classList.remove('invalid');
            email.classList.remove('invalid');
            subject.classList.remove('invalid');
            message.classList.remove('invalid');

            if (!name.value.trim()) {
                document.getElementById('contactNameError').textContent = 'Please enter your name.';
                name.classList.add('invalid');
                valid = false;
            }
            if (!emailPattern.test(email.value.trim())) {
                document.getElementById('contactEmailError').textContent = 'Please enter a valid email address.';
                email.classList.add('invalid');
                valid = false;
            }
            if (!subject.value) {
                document.getElementById('contactSubjectError').textContent = 'Please select a subject.';
                subject.classList.add('invalid');
                valid = false;
            }
            if (!message.value.trim()) {
                document.getElementById('contactMessageError').textContent = 'Please enter a message.';
                message.classList.add('invalid');
                valid = false;
            }

            if (!valid) {
                document.getElementById('contactFormSuccess').classList.remove('is-visible');
                return false;
            }

            // NOTE: this only clears the form and shows a confirmation message.
            // Nothing is emailed or saved yet — wiring this up to a real mail
            // service or database is a later step, same as the newsletter forms.
            name.value = '';
            email.value = '';
            subject.value = '';
            message.value = '';
            document.getElementById('contactFormSuccess').classList.add('is-visible');
            return false;
        }
    </script>

</asp:Content>