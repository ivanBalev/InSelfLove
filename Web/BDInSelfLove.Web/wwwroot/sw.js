////const cacheDefaultName = 'inselflove';
////const staticCacheName = cacheDefaultName + '-static-v1';
////const dynamicCacheName = cacheDefaultName + '-dyn1mic-v1';
////// TODO: optimize dynamic cache size.
////const dynamicCacheMaxSize = 15;
////const assets = [
////    '/Home/Error',
////    'Custom/js/pwa.js',
////    '/Custom/css/style.css',
////    '/Custom/css/bootstrap.css',
////    '/Custom/icons/bg-icon.png',
////    '/Custom/icons/en-icon.png',
////    '/Custom/icons/flower.svg',
////    '/Custom/img/wonder.png',
////    'https://use.fontawesome.com/releases/v5.0.13/css/all.css',
////    'https://res.cloudinary.com/dzcajpx0y/image/upload/v1613661144/ttttt_cqyaap.jpg',
////];

////self.addEventListener('install', evt => {
////    evt.waitUntil(
////        caches.open(staticCacheName).then(cache => {
////            cache.addAll(assets);
////        })
////    );
////});

////self.addEventListener('activate', evt => {
////    evt.waitUntil(
////        caches.keys().then(keys => {
////            return Promise.all(keys
////                .filter(key => key.startsWith(cacheDefaultName) &&
////                    key !== staticCacheName && key !== dynamicCacheName)
////                .map(key => caches.delete(key))
////            )
////        })
////    )
////});

////// Cache size limit function
////const limitCacheSize = (cacheName, maximumSize) => {
////    caches.open(cacheName).then(cache => {
////        cache.keys().then(keys => {
////            if (keys.length > maximumSize) {
////                cache.delete(keys[0])
////                    .then(limitCacheSize(cacheName, maximumSize))
////            }
////        })
////    })
////};

////self.addEventListener('fetch', evt => {
////    evt.respondWith(
////        caches.match(evt.request).then(cacheRes => {
////            return cacheRes || fetch(evt.request).then(fetchRes => {
////                return caches.open(dynamicCacheName).then(cache => {
////                    cache.put(evt.request.url, fetchRes.clone());
////                    limitCacheSize(dynamicCacheName, dynamicCacheMaxSize);
////                    return fetchRes;
////                })
////            });
////        }).catch(() => {
////            let requestUrlEndpoint = evt.request.url.split('/').pop();
////            if (!requestUrlEndpoint.includes('.')) {
////                return caches.match('/Home/Error');
////            }
////        })
////    );
////});