document.querySelectorAll('.delete-comment-btn').forEach(btn => btn.addEventListener('click', (e) => {
    // Get comment id
    let commentId = e.target.parentElement.parentElement.parentElement.parentElement
        .parentElement.parentElement.parentElement.parentElement.parentElement.id;
    // Add id to confirm button classlist to use after confirmation
    document.querySelector('.delete-comment-confirm-btn').classList.add(commentId);
    $('#confirm-comment-delete').modal();
}));

document.querySelector('.delete-comment-confirm-btn').addEventListener('click', (e) => {
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
            // Hide deleted comment which will be removed entirely once page reloads
            $('#' + commentId).hide();
        },
        error: function () {
            console.log('error')
        }
    })

    $('#confirm-comment-delete').modal('hide');
})