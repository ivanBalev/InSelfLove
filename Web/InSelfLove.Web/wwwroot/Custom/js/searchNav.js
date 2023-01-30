let searchModal = document.getElementById('search-modal');
let searchField = document.getElementById('searchTerm');

// User can start typing immediately once the modal appears
searchModal.addEventListener('shown.bs.modal', function () {
    // No need to click the search field first
    searchField.focus();
});

document.getElementsByClassName('search-btn')[0].addEventListener('click', function (e) {
    // Validation
    let searchTermValue = searchField.value;
    if (searchTermValue === '' || !searchTermValue) {
        e.preventDefault();
        alert('Please enter valid data.');
    }
});