this.window.addEventListener('click', function (e) {
    var navIsExpanded = document.querySelector('.navbar .navbar-toggler').getAttribute('aria-expanded');
    var navbarContainsTarget = document.querySelector('.navbar').contains(e.target);

    // Collapse navbar when user clicks somewhere else on the page
    if (navIsExpanded && !navbarContainsTarget) {
        document.querySelector('button[aria-expanded="true"]')?.click();
    }
});

