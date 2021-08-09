// Display 'Add comment box' if user is logged in
let addCommentElement = document.querySelector('.comment-box');
if (addCommentElement != undefined) {
    document.querySelector('.comment-box').style.display = 'block';
}
// Add reply buttons funcitonality if user is logged in
if (addCommentElement != undefined) {
    document.querySelectorAll('.comment .reply-button').forEach(btn => addReplyButtonsFunctionality(btn));
    // Hide reply buttons if user is not logged in
} else {
    document.querySelectorAll('.comment .reply-button').forEach(btn => {
        btn.style.display = 'none';
    });
}
// Hide last comments
const mainCommentsPerPage = 3;
var currentPage = 1;
document.querySelectorAll('.main-comment').forEach((c, i) => {
    if ((i + 1) > mainCommentsPerPage) {
        c.style.display = 'none';
    }
})
// Load next batch of comments
document.querySelector('#load-comments-btn')?.addEventListener('click', e => {
    currentPage++;
    document.querySelectorAll('.main-comment').forEach((c, i) => {
        if ((i + 1) > (currentPage - 1) * mainCommentsPerPage &&
            (i + 1) <= currentPage * mainCommentsPerPage) {
            c.style.display = 'block';
        }
    })
    // Hide load comments btn if all comments are already visible
    if (!Array.from(document.querySelectorAll('.main-comment')).some(c => c.style.display === 'none')) {
        document.querySelector('#load-comments-btn').style.display = 'none';
    }
})
// Initial hide of subcomments
document.querySelectorAll('.main-subcomment').forEach(sc => sc.style.display = 'none');
// Show subcomments
document.querySelectorAll('.btn-subcomments').forEach(btn => btn.addEventListener('click', e => {
    e.target.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement
        .querySelector('.main-subcomment').style.display = 'block';
}))
// Hide subcomments
document.querySelectorAll('.hide-subcomments').forEach(btn => btn.addEventListener('click', e => {
    // TODO: Error Why??
    e.target.parentElement.style.display = 'none'
}));

function addReplyButtonsFunctionality(btn) {
    // Hide/show text box
    btn.addEventListener('click', e => {
        //  Hide comment boxes and display all reply buttons
        document.querySelectorAll('.comment-box').forEach((cb, i) => i > 0 ? cb.style.display = "none" : null);
        document.querySelectorAll('.comment-buttons').forEach(cb => cb.style.display = "flex");
        // Hide reply and edit buttons
        e.target.parentElement.parentElement.parentElement.parentElement.parentElement.querySelector('.comment-buttons').style.display = 'none';
        // Display comment form
        btn.parentElement.parentElement.parentElement.parentElement.parentElement
            .querySelector('.comment-box').style.display = 'block';
    })
}

export { addReplyButtonsFunctionality };

