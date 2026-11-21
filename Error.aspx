<%@ Page Title="Something went wrong" Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="SamsLiquorShop.ErrorPage" %>

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Something went wrong | Sam's Liquor Shop</title>
    <script>
        (function () {
            var t = 'light';
            try { t = localStorage.getItem('theme') || 'light'; } catch (e) { }
            document.documentElement.setAttribute('data-theme', t);
        })();
    </script>
    <style>
        :root { --bg: #eef3f9; --card: #ffffff; --text: #1d1d1f; --muted: #6e6e73; --accent: #0057b8; }
        [data-theme="dark"] { --bg: #11151f; --card: #1a1f2c; --text: #f5f5f7; --muted: #9a9aa2; --accent: #3987e5; }
        * { box-sizing: border-box; }
        body { margin: 0; min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 24px;
               background: var(--bg); color: var(--text); font-family: "Segoe UI", Helvetica, Arial, sans-serif; }
        .card { width: 100%; max-width: 520px; background: var(--card); border-radius: 18px; padding: 44px 36px; text-align: center;
                box-shadow: 0 10px 40px rgba(0,0,0,0.10); }
        .eyebrow { margin: 0 0 12px; font-size: 13px; letter-spacing: 1.5px; text-transform: uppercase; color: var(--muted); }
        h1 { margin: 0 0 12px; font-size: 1.8rem; color: var(--accent); }
        p { margin: 0 0 8px; line-height: 1.6; color: var(--muted); }
        .actions { margin-top: 28px; display: flex; flex-wrap: wrap; gap: 12px; justify-content: center; }
        a.btn { display: inline-block; padding: 13px 28px; border-radius: 999px; text-decoration: none; font-weight: 600; font-size: 0.95rem; }
        a.primary { background: var(--accent); color: #fff; }
        a.ghost { border: 1.5px solid var(--accent); color: var(--accent); }
    </style>
</head>
<body>
    <div class="card">
        <p class="eyebrow">Sam's Liquor Shop</p>
        <h1>Something went wrong</h1>
        <p>We hit an unexpected problem on our side. It's not you, and it's usually temporary.</p>
        <p>Please try again in a moment. If it keeps happening, contact us on 082 405 5932.</p>
        <div class="actions">
            <a href="/Default.aspx" class="btn primary">Back to Home</a>
            <a href="javascript:history.back()" class="btn ghost">Go Back</a>
        </div>
    </div>
</body>
</html>