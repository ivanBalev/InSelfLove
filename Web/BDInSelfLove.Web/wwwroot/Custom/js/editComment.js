document.querySelectorAll('.editCommentBtn').forEach(b =>
    b.addEventListener('click', e =>
        editCommentDisplay(e.target)));

document.querySelectorAll('.save-edit-btn').forEach(b =>
    b.addEventListener('click', e =>
        saveCommentEdit(e)));

function editCommentDisplay(btn) {
    // Show edit comment box and hide reply and edit comment buttons and comment content box
    btn.parentElement.parentElement.parentElement.parentElement.parentElement.querySelector('.edit-comment').style.display = 'block';
    btn.parentElement.parentElement.parentElement.parentElement.parentElement.querySelector('.comment-buttons').style.display = 'none';
    btn.parentElement.parentElement.parentElement.parentElement.parentElement.querySelector('.comment-content').style.display = 'none';
}

function saveCommentEdit(e) {
    e.preventDefault();
    // Gather data
    let token = e.target.parentElement.parentElement.parentElement.parentElement
        .querySelector('input[name=__RequestVerificationToken]').value;
    let commentId = e.target.parentElement.parentElement.querySelector('#Id').value;
    let content = e.target.parentElement.parentElement.querySelector('[name=Content]').value;
    let articleId = e.target.parentElement.parentElement.querySelector('#ArticleId').value;


    fetch('/api/EditComment', {
        method: 'put',
        headers: {
            'X-CSRF-TOKEN': token,
            'Content-type': 'application/x-www-form-urlencoded; charset=UTF-8',
        },
        body: `Id=${commentId}&Content=${content}&ArticleId=${articleId}`,
    })
        .then(response => {
            if (!response.ok) {
                console.log('server error');
            };

            // Update comment content and return to default view
            e.target.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.querySelector('.comment-content').textContent = content;
            e.target.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.querySelector('.edit-comment').style.display = 'none';
            e.target.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.querySelector('.comment-buttons').style.display = 'flex';
            e.target.parentElement.parentElement.parentElement.parentElement.parentElement.querySelector('.comment-content').style.display = 'block';
        })
        .catch(error => {
            console.log('fetch error');
        });

}

export { editCommentDisplay, saveCommentEdit }