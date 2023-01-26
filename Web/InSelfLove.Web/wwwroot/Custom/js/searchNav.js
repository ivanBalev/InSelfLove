let searchModal = document.getElementById('search-modal');
let searchField = document.getElementById('searchTerm');

searchModal.addEventListener('shown.bs.modal', function () {
    searchField.focus();
});

document.getElementsByClassName('search-btn')[0].addEventListener('click', function (e) {
    let searchTermValue = searchField.value;
    if (searchTermValue === '' || !searchTermValue) {
        e.preventDefault();
        alert('Please enter valid data.');
    }
});