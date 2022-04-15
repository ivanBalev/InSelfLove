(function () {
    const input = document.getElementById('CreatedOn');
    const datepicker = new TheDatepicker.Datepicker(input);
    datepicker.render();
    datepicker.options.setInitialDate(new Date());
})();