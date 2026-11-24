// Service worker for "Install as app". It caches nothing: every request still goes to the server
// (the shop needs live stock, prices and sessions). It only shows a friendly page when a
// page navigation fails because the device is offline.
self.addEventListener('install', function () { self.skipWaiting(); });
self.addEventListener('activate', function (e) { e.waitUntil(self.clients.claim()); });

var OFFLINE_HTML =
    '<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">' +
    '<title>Offline | Sam\'s Liquor Shop</title>' +
    '<style>body{margin:0;min-height:100vh;display:flex;align-items:center;justify-content:center;font-family:-apple-system,Segoe UI,Roboto,sans-serif;background:#f5f7fb;color:#1d1d1f;text-align:center;padding:24px}' +
    'button{margin-top:16px;padding:12px 28px;border:0;border-radius:999px;background:#0a84ff;color:#fff;font:inherit;font-weight:600;cursor:pointer}</style></head>' +
    '<body><div><h1>You\'re offline</h1><p>Sam\'s Liquor Shop needs an internet connection. Check your connection and try again.</p>' +
    '<button onclick="location.reload()">Try again</button></div></body></html>';

self.addEventListener('fetch', function (e) {
    var req = e.request;
    if (req.mode !== 'navigate' || req.method !== 'GET') return; // let the browser handle everything else normally
    e.respondWith(fetch(req).catch(function () {
        return new Response(OFFLINE_HTML, { headers: { 'Content-Type': 'text/html; charset=utf-8' } });
    }));
});
