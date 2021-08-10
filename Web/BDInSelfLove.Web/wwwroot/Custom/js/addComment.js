import { addReplyButtonFunctionality } from './commentsHideDisplayItems.js';
import { editCommentDisplay, saveCommentEdit } from './editComment.js';
import { openDeleteConfirmModal } from './deleteComment.js';

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
            // TODO: why do I need this wrapper but I don't with ajax .replace in search.js file
            // Create wrapper element
            var element = document.createElement('div');
            element.innerHTML = data;

            addReplyButtonFunctionality(element.querySelector('.reply-button'));
            element.querySelector('.addCommentBtn')
                .addEventListener('click', ev => addAddCommentButtonFunctionality(ev));

            element.querySelector('.editCommentBtn').addEventListener('click', e => editCommentDisplay(e.target));
            element.querySelector('.save-edit-btn').addEventListener('click', e => saveCommentEdit(e));

            element.querySelector('.deleteCommentBtn').addEventListener('click', e => openDeleteConfirmModal(e));

            if (ParentCommentId === null) {
                // Stick at top of comments list
                element.classList.add('main-comment');
                // Fix img sizing
                element.getElementsByTagName('img')[0].width = '64';
                element.getElementsByTagName('img')[0].height = '64';
                // Add new comment to comments list
                document.querySelector('#all-comments').prepend(element);
            } else {
                // Subcomment
                element.classList.add('align-self-end');
                element.style.width = '90%';

                let parentComment = document.querySelector(`#${CSS.escape(ParentCommentId)}`);
                let isParentCommentMainComment = parentComment.parentElement.classList.contains('main-comment');
                if (isParentCommentMainComment) {
                    element.classList.add('main-subcomment');
                    // Fix img sizing
                    element.getElementsByTagName('img')[0].width = '40';
                    element.getElementsByTagName('img')[0].height = '40';
                    // Add new comment to comments list
                } else {
                    element.getElementsByTagName('img')[0].width = '30';
                    element.getElementsByTagName('img')[0].height = '30';
                }

                parentComment.querySelector('.card').appendChild(element);
            }
        });
}