// Shared password and form helpers: show/hide password, strength meter, "Suggest password"
// and the "?" field-help popovers. Same behaviour as Register.aspx.
// Used by ForgotPassword.aspx, Customer/Account.aspx and Manager/Account.aspx.
(function () {
    'use strict';

    function el(id) { return document.getElementById(id); }

    // ---------- Show / hide password ----------
    window.toggleFieldVisibility = function (fieldId, button) {
        var field = el(fieldId);
        if (!field) return;
        var icon = button.querySelector('.eye-icon');
        if (field.type === 'password') {
            field.type = 'text';
            button.setAttribute('aria-label', 'Hide password');
            icon.innerHTML = '<path d="M3 3l18 18"></path><path d="M10.6 10.6a2 2 0 0 0 2.8 2.8"></path>'
                + '<path d="M9.9 5.1A10.8 10.8 0 0 1 12 5c6 0 9.5 7 9.5 7a16.5 16.5 0 0 1-3.2 3.8"></path>'
                + '<path d="M6.2 6.2C3.9 7.8 2.5 12 2.5 12a16.5 16.5 0 0 0 6.4 5.5A10.8 10.8 0 0 0 12 19c1.7 0 3.2-.5 4.5-1.2"></path>';
        } else {
            field.type = 'password';
            button.setAttribute('aria-label', 'Show password');
            icon.innerHTML = '<path d="M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6z"></path><circle cx="12" cy="12" r="2.5"></circle>';
        }
    };

    // ---------- Strength meter (expects #passwordStrengthFill, #passwordStrengthText and #req* rows) ----------
    window.checkPasswordStrength = function (password) {
        var checks = {
            reqLength: password.length >= 8,
            reqLower: /[a-z]/.test(password),
            reqUpper: /[A-Z]/.test(password),
            reqNumber: /[0-9]/.test(password),
            reqSpecial: /[^A-Za-z0-9]/.test(password)
        };
        var score = 0;
        for (var id in checks) {
            var row = el(id);
            if (row) row.classList.toggle('valid', checks[id]);
            if (checks[id]) score++;
        }

        var fill = el('passwordStrengthFill');
        var text = el('passwordStrengthText');
        if (!fill || !text) return;

        var cls = ['strength-weak', 'strength-medium', 'strength-strong'];
        cls.forEach(function (c) { fill.classList.remove(c); text.classList.remove(c); });

        if (password.length === 0) { fill.style.width = '0%'; text.textContent = 'Password strength'; return; }

        var level = score <= 2 ? 0 : (score <= 4 ? 1 : 2);
        fill.style.width = ['25%', '60%', '100%'][level];
        text.textContent = ['Weak password', 'Medium password', 'Strong password'][level];
        fill.classList.add(cls[level]);
        text.classList.add(cls[level]);
    };

    // ---------- Suggest password ----------
    // No < > & characters: ASP.NET request validation rejects some combinations of them.
    var PW_SETS = {
        lower: 'abcdefghijkmnopqrstuvwxyz',
        upper: 'ABCDEFGHJKLMNPQRSTUVWXYZ',
        digit: '23456789',
        special: '!@#$%*?-_=+'
    };

    function randomIndex(max) {
        var buf = new Uint32Array(1);
        var limit = Math.floor(0x100000000 / max) * max;
        do { window.crypto.getRandomValues(buf); } while (buf[0] >= limit);
        return buf[0] % max;
    }

    function pick(chars) { return chars.charAt(randomIndex(chars.length)); }

    window.generatePassword = function (length) {
        var all = PW_SETS.lower + PW_SETS.upper + PW_SETS.digit + PW_SETS.special;
        // One of each required type, the rest from everything, then shuffled.
        var chars = [pick(PW_SETS.lower), pick(PW_SETS.upper), pick(PW_SETS.digit), pick(PW_SETS.special)];
        while (chars.length < length) chars.push(pick(all));
        for (var i = chars.length - 1; i > 0; i--) {
            var j = randomIndex(i + 1);
            var t = chars[i]; chars[i] = chars[j]; chars[j] = t;
        }
        return chars.join('');
    };

    // Fills both fields, shows the password and the "save it somewhere safe" note (#suggestedPasswordNote).
    window.suggestPassword = function (passwordId, confirmId) {
        var pw = el(passwordId);
        var confirm = el(confirmId);
        if (!pw || !confirm) return;
        var value = window.generatePassword(16);

        [pw, confirm].forEach(function (f) {
            f.value = value;
            f.classList.remove('invalid');
            f.dispatchEvent(new Event('input', { bubbles: true }));
            f.dispatchEvent(new Event('change', { bubbles: true })); // re-runs any ASP.NET validators
        });

        if (pw.type === 'password') {
            window.toggleFieldVisibility(pw.id, pw.parentNode.querySelector('.login-eye-btn'));
        }
        var note = el('suggestedPasswordNote');
        if (note) {
            note.hidden = false;
            note.setAttribute('data-for', passwordId);
        }
    };

    window.copySuggestedPassword = function (btn, passwordId) {
        var note = el('suggestedPasswordNote');
        var field = el(passwordId || (note && note.getAttribute('data-for')));
        var value = field ? field.value : '';
        function done(ok) {
            btn.textContent = ok ? 'Copied' : 'Copy failed';
            setTimeout(function () { btn.textContent = 'Copy'; }, 1800);
        }
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(value).then(function () { done(true); }, function () { done(false); });
        } else {
            done(false);
        }
    };

    // Hide the "suggested" note once the user types their own password
    function watchSuggestedNote() {
        var note = el('suggestedPasswordNote');
        if (!note) return;
        document.addEventListener('input', function (e) {
            if (e.isTrusted && e.target && e.target.id && e.target.id === note.getAttribute('data-for')) note.hidden = true;
        });
    }

    // ---------- Field help "?" popovers (buttons with class field-help and data-rules="rule|rule|...") ----------
    function initFieldHelp() {
        var buttons = document.querySelectorAll('.field-help[data-rules]');
        var openBtn = null;

        function position(btn) {
            var tip = btn._tip;
            var r = btn.getBoundingClientRect();
            tip.style.left = '0px';
            tip.style.top = '0px';
            var w = tip.offsetWidth, h = tip.offsetHeight;
            var left = Math.min(Math.max(8, r.left + r.width / 2 - w / 2), window.innerWidth - w - 8);
            var top = r.bottom + 8;
            if (top + h > window.innerHeight - 8) top = Math.max(8, r.top - h - 8);
            tip.style.left = left + 'px';
            tip.style.top = top + 'px';
        }

        function show(btn) {
            if (openBtn && openBtn !== btn) hide(openBtn, true);
            btn._tip.classList.add('open');
            position(btn);
        }

        function hide(btn, force) {
            if (!force && btn._pinned) return;
            btn._pinned = false;
            btn._tip.classList.remove('open');
            btn.setAttribute('aria-expanded', 'false');
            if (openBtn === btn) openBtn = null;
        }

        Array.prototype.forEach.call(buttons, function (btn, i) {
            if (btn._tip) return;   // already set up
            var label = btn.closest('label');
            var name = label && label.firstChild ? label.firstChild.textContent.trim() : 'this field';
            if (!name || name.length > 40) name = 'this field';

            var tip = document.createElement('div');
            tip.className = 'field-help-tip';
            tip.id = 'fieldHelpTip' + i;
            tip.setAttribute('role', 'tooltip');
            var title = document.createElement('div');
            title.className = 'field-help-title';
            title.textContent = name;
            tip.appendChild(title);
            var list = document.createElement('ul');
            btn.getAttribute('data-rules').split('|').forEach(function (rule) {
                var li = document.createElement('li');
                li.textContent = rule;
                list.appendChild(li);
            });
            tip.appendChild(list);
            document.body.appendChild(tip);

            btn._tip = tip;
            btn.textContent = '?';
            btn.setAttribute('aria-label', 'Rules for ' + name);
            btn.setAttribute('aria-describedby', tip.id);
            btn.setAttribute('aria-expanded', 'false');

            btn.addEventListener('mouseenter', function () { show(btn); });
            btn.addEventListener('mouseleave', function () { hide(btn); });
            btn.addEventListener('focus', function () { show(btn); });
            btn.addEventListener('blur', function () { hide(btn, true); });
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                if (btn._pinned) { hide(btn, true); return; }
                show(btn);
                btn._pinned = true;
                btn.setAttribute('aria-expanded', 'true');
                openBtn = btn;
            });
        });

        document.addEventListener('click', function () { if (openBtn) hide(openBtn, true); });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && openBtn) hide(openBtn, true);
        });
        window.addEventListener('scroll', function () {
            document.querySelectorAll('.field-help-tip.open').forEach(function (t) { t.classList.remove('open'); });
            if (openBtn) hide(openBtn, true);
        }, { passive: true });
        window.addEventListener('resize', function () { if (openBtn) position(openBtn); });
    }

    window.addEventListener('load', function () {
        initFieldHelp();
        watchSuggestedNote();
    });
})();
