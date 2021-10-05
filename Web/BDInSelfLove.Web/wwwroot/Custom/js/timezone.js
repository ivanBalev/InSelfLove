let timezoneCookieName = "timezoneIANA";
let timezoneCookieValue = decodeURIComponent(getCookie(timezoneCookieName));
let currentTimezoneValue = Intl.DateTimeFormat().resolvedOptions().timeZone;
// If timezone cookie doesn't exist || it doesn't match the current timezone
if (!timezoneCookieValue || timezoneCookieValue.localeCompare(currentTimezoneValue) !== 0) {
    setCookie(timezoneCookieName, currentTimezoneValue);
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) {
        return parts.pop().split(';').shift();
    }
}

function setCookie(name, value, days = 60, path = '/') {
    const expires = new Date(Date.now() + days * 864e5).toUTCString();
    document.cookie = name + '=' + encodeURIComponent(value) + '; expires=' + expires + '; path=' + path;
}
