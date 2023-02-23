var cacheName = 'kuvarpa-v1';
var filesToCache = [
    './',
    './index.html',
    './manifest.json',
    './favicon.ico',
    './icons/android-launchericon-48-48.png',
    './icons/android-launchericon-72-72.png',
    './icons/android-launchericon-96-96.png',
    './icons/android-launchericon-144-144.png',
    './icons/android-launchericon-192-192.png',
    './icons/android-launchericon-512-512.png',
    './icons/64.png',
    './icons/144.png',
    './icons/152.png',
    './icons/192.png',
    './icons/256.png',
    './icons/512.png',
    './icons/1024.png'
];

self.addEventListener('install', function (e) {
    console.log('[ServiceWorker] install');
    e.waitUntil(
        caches.open(cacheName).then(function (cache) {
            console.log('[ServiceWorker] caching');
            return cache.addAll(filesToCache);
        })
    );
});

self.addEventListener('activate', function (event) {
    console.log('[ServiceWorker] activating');
    event.waitUntil(
        caches.keys()
            .then(function (cacheNames) {
                return Promise.all(
                    cacheNames.map(function (cName) {
                        if (cName !== cacheName) {
                            return caches.delete(cName);
                        }
                    })
                );
            })
    );
});

self.addEventListener('fetch', event => {
    console.log('[ServiceWorker] fetch event for ', event.request.url);
    event.respondWith(
        caches.match(event.request).then(response => {
            if (response) {
                console.log('Found ', event.request.url, ' in cache');
                return response;
            }
            console.log('Network request for ', event.request.url);
            return fetch(event.request)
        }).catch(error => {
            console.log(error);
        })
    );
});
