document.querySelectorAll('.deleteCommentBtn').forEach(btn =>
    btn.addEventListener('click', e => addCommentIdToConfirmationModalClasslist(e)));

document.querySelector('.delete-comment-confirm-btn')
    .addEventListener('click', (e) => confirmCommentDelete(e));

function addCommentIdToConfirmationModalClasslist(e) {
    let commentId = e.target.closest('div[class*=-comment]').id;
    document.querySelector('.delete-comment-confirm-btn').classList.add(commentId);
}

function confirmCommentDelete(e) {
    let commentId = e.target.classList[e.target.classList.length - 1];
    let csrfToken = document.querySelector('input[name=__RequestVerificationToken]').value;
    // Return btn classlist to default state
    e.target.classList.remove(commentId);

    fetch('/api/DeleteComment?id=' + commentId, {
        method: 'delete',
        headers: {
            'X-CSRF-TOKEN': csrfToken,
        },
    })
    .then(response => {
        if (!response.ok) {
            console.log('server error');
        };
        // Remove comment from DOM
        let commentElement = document.getElementById(commentId);
        commentElement.parentElement.removeChild(commentElement);
        // If no comments left on page
        if (document.querySelectorAll('#comments-section .comment').length === 0) {
            // Hide 'Comments' subtitle
            document.querySelectorAll('#comments-section .subtitle')[1].style.display = 'none';
        }
    })
    .catch(error => {
        console.log('fetch error');
    });

    // Hide confirmation modal
    let modalElement = document.getElementById('confirm-comment-delete');
    bootstrap.Modal.getInstance(modalElement).hide();
}
