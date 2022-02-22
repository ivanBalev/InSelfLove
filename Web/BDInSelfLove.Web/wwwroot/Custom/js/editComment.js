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
    let csfrToken = e.target.parentElement.parentElement.parentElement.parentElement
        .querySelector('input[name=__RequestVerificationToken]').value;
    let Id = e.target.parentElement.parentElement.querySelector('#Id').value;
    let Content = e.target.parentElement.parentElement.querySelector('[name=Content]').value;
    let ArticleId = e.target.parentElement.parentElement.querySelector('#ArticleId').value;

    putData(
        '/api/EditComment',
        'put',
        { Id, Content, ArticleId },
        csfrToken)
        .then(response => {
            if (!response.ok) {
                console.log('server error');
            };

            // Update comment content and return to default view
            e.target.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.querySelector('.comment-content').textContent = Content;
            e.target.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.querySelector('.edit-comment').style.display = 'none';
            e.target.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.querySelector('.comment-buttons').style.display = 'flex';
            e.target.parentElement.parentElement.parentElement.parentElement.parentElement.querySelector('.comment-content').style.display = 'block';
        })
        .catch(error => {
            console.log('fetch error');
        });


    async function putData(url = '', method, data = {}, csfrToken) {
        const response = await fetch(url, {
            method,
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': csfrToken,
            },
            body: JSON.stringify(data)
        });
        return response;
    }
}

export { editCommentDisplay, saveCommentEdit }