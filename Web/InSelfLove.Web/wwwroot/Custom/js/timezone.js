const consentCookie = getCookie(".AspNet.Consent");

if (consentCookie) {
    const timezoneCookieName = "timezoneIANA";
    const rawTimezoneCookie = getCookie(timezoneCookieName);
    const timezoneCookie = rawTimezoneCookie ? decodeURIComponent(rawTimezoneCookie) : undefined;
    const currentTimezone = Intl.DateTimeFormat().resolvedOptions().timeZone;

    if (!timezoneCookie || timezoneCookie.localeCompare(currentTimezone) !== 0) {
        setCookie(timezoneCookieName, currentTimezone);
    }
} else {
    const appointmentsAnchor = document.querySelector('a[href$="api/Appointments"]');
    appointmentsAnchor.href = appointmentsAnchor.href + '?timezone=' + currentTimezoneValue;
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
