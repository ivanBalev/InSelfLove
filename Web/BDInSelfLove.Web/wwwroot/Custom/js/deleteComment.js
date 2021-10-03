document.querySelectorAll('.deleteCommentBtn').forEach(btn =>
    btn.addEventListener('click', e => openDeleteConfirmModal(e)));


document.querySelector('.delete-comment-confirm-btn')
    .addEventListener('click', (e) => confirmCommentDelete(e));

function openDeleteConfirmModal(e) {
    // Get comment id
    let commentId = e.target.parentElement.parentElement.parentElement.parentElement
        .parentElement.parentElement.parentElement.parentElement.parentElement.id;
    // Add id to confirm button classlist to use after confirmation
    document.querySelector('.delete-comment-confirm-btn').classList.add(commentId);
    $('#confirm-comment-delete').modal();
}

function confirmCommentDelete(e) {
    let commentId = e.target.classList[e.target.classList.length - 1];
    let token = document.querySelector('input[name=__RequestVerificationToken]').value;
    // Return btn classlist to default state
    e.target.classList.remove(commentId);

    $.ajax({
        type: "POST",
        url: '/api/DeleteComment',
        data: {
            Id: commentId,
        },
        headers: { 'X-CSRF-TOKEN': token },
        success: function () {
            // Remove deleted comment
            $('#' + commentId).remove();

            // If last comment was just deleted(hidden)
            if (document.querySelectorAll('#comments .comment').length === 1) {
                // Hide 'Comments' subtitle
                document.querySelectorAll('#comments .subtitle')[1].style.display = 'none';
            }
        },
        error: function () {
            console.log('error')
        }
    })

    $('#confirm-comment-delete').modal('hide');
}

export {openDeleteConfirmModal }