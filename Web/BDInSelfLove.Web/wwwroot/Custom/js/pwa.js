if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('/sw.js');
}

window.addEventListener('beforeinstallprompt', (e) => {
    // Prevent Chrome 67 and earlier from automatically showing installation prompt
    e.preventDefault();
});