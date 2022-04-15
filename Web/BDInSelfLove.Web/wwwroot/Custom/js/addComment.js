import { showAddCommentBox } from './commentsHideDisplayItems.js';
import { editCommentDisplay, saveCommentEdit } from './editComment.js';
import { addCommentIdToConfirmationModalClasslist } from './deleteComment.js';

const cultureIsEn = document.cookie.match('Culture')?.input.substr(-2) === 'en';

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
    let ResourceUrl = window.location.href;
    // Validate input
    if (Content.length < 2 || (ArticleId === null && VideoId === null)) {
        alert("Моля, въведете повече от 2 символа.");
        return;
    }

    postData('/api/CreateComment', { Content, ParentCommentId, ArticleId, VideoId, ResourceUrl }, csfrToken)
        .then(data => {
            var element = htmlToElement(data);

            showAddCommentBox(element.querySelector('.reply-button'));
            element.querySelector('.addCommentBtn')
                .addEventListener('click', ev => addAddCommentButtonFunctionality(ev));

            element.querySelector('.editCommentBtn').addEventListener('click', e => editCommentDisplay(e.target));
            element.querySelector('.save-edit-btn').addEventListener('click', e => saveCommentEdit(e));

            element.querySelector('.deleteCommentBtn').addEventListener('click', e => addCommentIdToConfirmationModalClasslist(e));

            if (!document.querySelector('#all-comments')) {
                // No comments on page
                preparePageForCommentInsertion();
            }

            if (ParentCommentId === null) {
                setUpMainCommentDisplay(element);
                // Stick at top of comments list
                document.querySelector('#all-comments').prepend(element);
            } else {
                // Subcomment
                element.classList.add('align-self-end');
                element.style.width = '90%';

                let parentComment = document.querySelector(`#${CSS.escape(ParentCommentId)}`);
                let isParentCommentMainComment = parentComment.classList.contains('main-comment');
                if (isParentCommentMainComment) {
                    element.classList.add('firstSub-comment');
                    // Fix img sizing
                    element.getElementsByTagName('img')[0].width = '40';
                    element.getElementsByTagName('img')[0].height = '40';
                    // Add new comment to comments list
                } else {
                    element.classList.add('secondSub-comment');

                    element.getElementsByTagName('img')[0].width = '30';
                    element.getElementsByTagName('img')[0].height = '30';
                }

                element.style.display = 'block';
                parentComment.querySelector('.card-body').parentNode
                    .insertBefore(element, parentComment.querySelector('.card-body').nextSibling);
                parentComment.querySelector('.comment-box').style.display = 'none';
                parentComment.querySelector('.comment-buttons').style.display = 'flex';
            }
            // Clear text box
            form.querySelector('[name=Content]').value = '';
        });
}

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

function htmlToElement(html) {
    var template = document.createElement('template');
    html = html.trim();
    template.innerHTML = html;
    return template.content.firstChild;
}

function setUpMainCommentDisplay(element) {
    element.classList.add('main-comment');
    // Fix img sizing
    element.getElementsByTagName('img')[0].width = '64';
    element.getElementsByTagName('img')[0].height = '64';
}

function preparePageForCommentInsertion() {
    let commentsTitleElement = document.createElement('h2');
    commentsTitleElement.className = 'mb-2 mt-5 subtitle';
    commentsTitleElement.textContent = cultureIsEn ? "COMMENTS" : "КОМЕНТАРИ";

    let commentsWrapper = document.createElement('div');
    commentsWrapper.setAttribute("id", "all-comments")

    let commentsSection = document.querySelector('#comments-section');
    commentsSection.append(commentsTitleElement);
    document.querySelector('#comments-section').append(commentsWrapper);
}
