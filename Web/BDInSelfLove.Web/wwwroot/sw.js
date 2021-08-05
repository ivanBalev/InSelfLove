﻿const cacheDefaultName = 'inselflove';
const domainNames = ['inselflove', 'localhost'];
const staticCacheName = cacheDefaultName + '-static-v122.22';
const dynamicCacheName = cacheDefaultName + '-dynamic-v122.22';
// TODO: optimize dynamic cache size.
const dynamicCacheMaxSize = 20;
const assets = [
    '/Home/Error',
    'https://res.cloudinary.com/dzcajpx0y/image/upload/v1620506959/aididie6_ke76hz.jpg',
];

// The first event a service worker gets, and it only happens once.
self.addEventListener('install', evt => {
    evt.waitUntil(
        caches.open(staticCacheName).then(cache => {
            cache.addAll(assets);
        })
    );
});

// This gets fired after the service worker has installed and the user closes and reopens the page
// (does not activate on refresh.Refresh and the old worker still controls the page).Upon activation,
// the old cache is deleted. It is not deleted while installing, because the old service worker needs the old
// cache in order to control the page.
self.addEventListener('activate', evt => {
    // This causes the service worker to activate immediately after it's installed isntead of waiting for the page to close.
    // It might break things though, so use with caution
    self.skipWaiting();

    // Delete all previous versions of our cache
    evt.waitUntil(
        caches.keys().then(keys => {
            return Promise.all(keys
                .filter(key => key.startsWith(cacheDefaultName) &&
                    key !== staticCacheName && key !== dynamicCacheName)
                .map(key => caches.delete(key))
            )
        })
    )
});

// Cache size limit function
const limitCacheSize = (cacheName, maximumSize) => {
    caches.open(cacheName).then(cache => {
        cache.keys().then(keys => {
            if (keys.length > maximumSize) {
                cache.delete(keys[0])
                    .then(limitCacheSize(cacheName, maximumSize))
            }
        })
    })
};

self.addEventListener('fetch', evt => {
    evt.respondWith(
        caches.match(evt.request).then(cacheRes => {
            return cacheRes || fetch(evt.request).then(fetchRes => {
                return caches.open(dynamicCacheName).then(cache => {
                    // Add to dynamic cache only if request is not handled by server
                    // Server-handled requests are already cached adequately
                    if (!domainNames.some(dn => evt.request.url.includes(dn))) {
                        cache.put(evt.request.url, fetchRes.clone());
                        limitCacheSize(dynamicCacheName, dynamicCacheMaxSize);
                    }
                    return fetchRes;
                })
            });
        }).catch(() => {
            let requestUrlEndpoint = evt.request.url.split('/').pop();
            if (!requestUrlEndpoint.includes('.')) {
                return caches.match('/Home/Error');
            }
        })
    );
});