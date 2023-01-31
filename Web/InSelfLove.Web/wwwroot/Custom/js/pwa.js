if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('/sw.js');
}

window.addEventListener('beforeinstallprompt', (e) => {
    // Prevent Chrome 67 and earlier from automatically showing installation prompt
    // Provides better UX. At this point, not many people would like to install the PWA
    e.preventDefault();
});