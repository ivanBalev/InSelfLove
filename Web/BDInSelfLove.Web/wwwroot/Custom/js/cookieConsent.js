(function () {
    var button = document.querySelector("#cookieConsent button[data-cookie-string]");
    button.addEventListener("click", function () {
        document.cookie = button.dataset.cookieString;
    }, false);
})();