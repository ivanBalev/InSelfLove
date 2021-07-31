document.querySelector('.search').addEventListener('click', e => {
    // Show search modal
    $('#search-modal').modal();
    // Set cursor to text field
    $('#search-modal').on('shown.bs.modal', function () {
        $('#searchTerm').focus()
    });
    $('.search-btn').on('click', function (e) {
        let searchTermValue = e.target.parentElement.querySelector('#searchTerm').value;
        if (searchTermValue === '' || !searchTermValue) {
            e.preventDefault();
            alert('Please enter valid data.');
        }
    });
});