async function postData(url = '', data = {}, csfrToken) {
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': csfrToken,
        },
        body: JSON.stringify(data)
    });
    return response.text();
}

document.querySelectorAll('.addCommentBtn')
    .forEach(b => b.addEventListener('click', e => {
        e.preventDefault();

        // Get data
        let form = e.target.closest('form');
        let csfrToken = form.querySelector('[name=__RequestVerificationToken]').value;
        let Content = form.querySelector('[name=Content]').value;
        // if any of the below's value is "", set value to null to allow backend to bind model correctly
        let ParentCommentId = form.querySelector('[name=ParentCommentId]').value || null;
        let ArticleId = form.querySelector('[name=ArticleId]').value || null;
        let VideoId = form.querySelector('[name=VideoId]').value || null;

        if (Content.length < 2 || (ArticleId === null && VideoId === null)) {
            alert("Моля, въведете повече от 2 символа.");
            return;
        }

        postData('/api/CreateComment', { Content, ParentCommentId, ArticleId, VideoId }, csfrToken)
            .then(data => {
                console.log(data);
            });
}));