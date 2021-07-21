const targetActionNames = ["Appointment"];
let timezoneCookieValue = getCookie("timezoneIANA");
let currentTimezoneIANA = Intl.DateTimeFormat().resolvedOptions().timeZone;
// If timezone cookie doesn't exist || it doesn't match the current timezone
if (!timezoneCookieValue || timezoneCookieValue.localeCompare(currentTimezoneIANA) !== 0) {
    // Get all anchor elements leading to the targeted controllers and attach the timezone to their href values
    Array.from(document.getElementsByTagName('a')).filter(a =>
        targetActionNames.some(cn => a.href.includes(cn)))
        .forEach(e => {
            e.href = e.href + "?timezoneIANA=" + currentTimezoneIANA;
        })
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}
