// Save cookie on client after user agrees
var acceptCookiesBtn = document.querySelector("#cookieConsent button[data-cookie-string]");
acceptCookiesBtn?.addEventListener("click", function () {
    document.cookie = acceptCookiesBtn.dataset.cookieString;
}, false);
