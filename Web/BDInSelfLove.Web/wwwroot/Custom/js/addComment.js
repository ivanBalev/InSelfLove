import { addReplyButtonsFunctionality } from './commentsHideDisplayItems.js';

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
        addAddCommentButtonFunctionality(e);
    }));

function addAddCommentButtonFunctionality(e) {
    e.preventDefault();
    // Get data
    let form = e.target.closest('form');
    let csfrToken = form.querySelector('[name=__RequestVerificationToken]').value;
    let Content = form.querySelector('[name=Content]').value;
    // if any of the below's value is "", set value to null to allow backend to bind model correctly
    let ParentCommentId = form.querySelector('[name=ParentCommentId]').value || null;
    let ArticleId = form.querySelector('[name=ArticleId]').value || null;
    let VideoId = form.querySelector('[name=VideoId]').value || null;
    // Validate input
    if (Content.length < 2 || (ArticleId === null && VideoId === null)) {
        alert("Моля, въведете повече от 2 символа.");
        return;
    }

    postData('/api/CreateComment', { Content, ParentCommentId, ArticleId, VideoId }, csfrToken)
        .then(data => {
            // Main Comment
            // Stick at top of comments list
            if (ParentCommentId === null) {
                // Create wrapper element
                var wrapper = document.createElement('div');
                wrapper.classList.add('main-comment');
                // Add response to it and fix img sizing
                wrapper.innerHTML = data;
                wrapper.getElementsByTagName('img')[0].width = '64';
                wrapper.getElementsByTagName('img')[0].height = '64';

                addReplyButtonsFunctionality(wrapper.querySelector('.reply-button'));
                let addCommentBtn = wrapper.querySelector('.addCommentBtn')
                addCommentBtn.addEventListener('click', ev => addAddCommentButtonFunctionality(ev));

                let commentsSection = document.querySelector('#all-comments');
                commentsSection.prepend(wrapper);
                // TODO: execute js to activate buttons
            }

            // Subcomment
            // Find parent comment
            // Stick at bottom of card element
            // Fix Image width and height (if parent comment is also subcomment - 30x30, else 40x40)
        });
}