const CACHE_NAME = "alerta-estafa-v11";

const STATIC_ASSETS = [
    "/offline.html",
    "/manifest.webmanifest",
    "/css/site.css",
    "/css/functional-fixes.css",
    "/css/shark-hosting.css",
    "/js/site.js",
    "/js/functional-fixes.js",
    "/js/pwa.js",
    "/lib/bootstrap/dist/css/bootstrap.min.css",
    "/lib/bootstrap/dist/js/bootstrap.bundle.min.js",
    "/lib/jquery/dist/jquery.min.js",
    "/icons/icon-192.png",
    "/icons/icon-512.png",
    "/icons/icon-maskable-512.png",
    "/icons/apple-touch-icon.png"
];

self.addEventListener("install", event => {
    event.waitUntil(
        caches
            .open(CACHE_NAME)
            .then(cache => cache.addAll(STATIC_ASSETS))
            .then(() => self.skipWaiting())
    );
});

self.addEventListener("activate", event => {
    event.waitUntil(
        caches
            .keys()
            .then(cacheNames =>
                Promise.all(
                    cacheNames
                        .filter(cacheName => cacheName !== CACHE_NAME)
                        .map(cacheName => caches.delete(cacheName))
                )
            )
            .then(() => self.clients.claim())
    );
});

self.addEventListener("fetch", event => {
    const request = event.request;

    if (request.method !== "GET") {
        return;
    }

    const requestUrl = new URL(request.url);

    if (requestUrl.origin !== self.location.origin) {
        return;
    }

    if (request.mode === "navigate") {
        event.respondWith(
            fetch(request).catch(() => caches.match("/offline.html"))
        );

        return;
    }

    const isLiveAsset =
        requestUrl.pathname.startsWith("/css/") ||
        requestUrl.pathname.startsWith("/js/");

    if (isLiveAsset) {
        event.respondWith(
            fetch(request)
                .then(networkResponse => {
                    if (networkResponse && networkResponse.status === 200) {
                        const responseCopy = networkResponse.clone();
                        caches.open(CACHE_NAME).then(cache => {
                            cache.put(request, responseCopy);
                        });
                    }

                    return networkResponse;
                })
                .catch(async () =>
                    (await caches.match(request)) ||
                    (await caches.match(requestUrl.pathname)))
        );

        return;
    }

    event.respondWith(
        caches.match(request).then(cachedResponse => {
            if (cachedResponse) {
                return cachedResponse;
            }

            return fetch(request).then(networkResponse => {
                if (!networkResponse || networkResponse.status !== 200) {
                    return networkResponse;
                }

                const responseCopy = networkResponse.clone();

                caches.open(CACHE_NAME).then(cache => {
                    cache.put(request, responseCopy);
                });

                return networkResponse;
            });
        })
    );
});
