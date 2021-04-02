const cacheDefaultName = 'inselflove';
const staticCacheName = cacheDefaultName + '-static-v4';
const dynamicCacheName = cacheDefaultName + '-dynamic-v4';
const assets = [
    'Custom/js/pwa.js',
    '/Custom/css/style.css',
    '/Custom/css/bootstrap.css',
    '/Custom/icons/bg-icon.png',
    '/Custom/icons/en-icon.png',
    '/Custom/icons/flower.svg',
    'https://use.fontawesome.com/releases/v5.0.13/css/all.css',
    'https://res.cloudinary.com/dzcajpx0y/image/upload/v1613661144/ttttt_cqyaap.jpg',
];

self.addEventListener('install', evt => {
    evt.waitUntil(
        caches.open(staticCacheName).then(cache => {
            console.log('caching shell assets!');
            cache.addAll(assets);
        })
    );
});

self.addEventListener('activate', evt => {
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

self.addEventListener('fetch', evt => {
    console.log(evt.request);
    evt.respondWith(
        caches.match(evt.request).then(cacheRes => {
            return cacheRes || fetch(evt.request).then(fetchRes => {
                return caches.open(dynamicCacheName).then(cache => {
                    cache.put(evt.request.url, fetchRes.clone());
                    return fetchRes;
                })
            });
        })
    );
});