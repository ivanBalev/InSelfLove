// Collapse navbar when screen is narrow and navbar is expanded but user clicks somewhere outside the expanded navbar
$(window).on('click', function (event) {
    var clickOver = $(event.target)
    if ($('.navbar .navbar-toggler').attr('aria-expanded') == 'true' && clickOver.closest('.navbar').length === 0) {
        // Click the navbar toggler button
        $('button[aria-expanded="true"]').click();
    }
});