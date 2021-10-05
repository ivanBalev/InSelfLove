let timezoneIANA = Intl.DateTimeFormat().resolvedOptions().timeZone;
Array.from(document.getElementsByClassName('timezone-IANA'))
    .forEach(e => e.value = timezoneIANA);