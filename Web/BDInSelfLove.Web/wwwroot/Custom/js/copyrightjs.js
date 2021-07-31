// Get the current year for the copyright
$('#year').text(new Date().getFullYear());

$('.carousel').carousel({
    interval: false
});

$("#popover").popover({ trigger: "hover" });