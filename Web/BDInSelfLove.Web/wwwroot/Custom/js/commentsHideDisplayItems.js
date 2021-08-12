// Display 'Add comment box' if user is logged in
let addCommentElement = document.querySelector('.comment-box');
if (addCommentElement != undefined) {
    document.querySelector('.comment-box').style.display = 'block';
}
// Add reply buttons funcitonality if user is logged in
if (addCommentElement != undefined) {
    document.querySelectorAll('.reply-button').forEach(btn => showAddCommentBox(btn));
    // Hide reply buttons if user is not logged in
} else {
    document.querySelectorAll('.reply-button').forEach(btn => {
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

// Show subcomments
document.querySelectorAll('.showSubcomments').forEach(btn => btn.addEventListener('click', e => {
    // Find all lower-level subcomments and show them
    Array.from(e.target.closest('.card').children).forEach(c => {
        let classList = c.classList.value;
        if (classList.includes('-comment') || classList.includes('hideSubcomments')) {
            c.style.display = 'block';
        }
    });
    // Hide 'showSubcomments' button
    e.target.style.display = 'none';
}))

// Hide subcomments
document.querySelectorAll('.hideSubcomments').forEach(btn => btn.addEventListener('click', e => {
    // Find all lower-level subcomments and hide them
    Array.from(e.target.closest('.card').children).forEach(c => {
        let classList = c.classList.value;
        if (classList.includes('-comment') || classList.includes('hideSubcomments')) {
            c.style.display = 'none';
        } else if (classList.includes('card-body')) {
            // Show 'showSubcomments' button
            c.querySelector('.showSubcomments').style.display = 'block';
        }
    });
}));

function showAddCommentBox(btn) {
    // Hide/show text box
    btn.addEventListener('click', e => {
        //  Hide comment boxes and display all reply buttons
        // TODO: why iterate through all, can't we just find the one that's visible instead?
        document.querySelectorAll('.comment-box').forEach((cb, i) => i > 0 ? cb.style.display = "none" : null);
        document.querySelectorAll('.comment-buttons').forEach(cb => cb.style.display = "flex");
        // Hide reply and edit buttons
        e.target.parentElement.parentElement.parentElement.parentElement.parentElement.querySelector('.comment-buttons').style.display = 'none';
        // Display comment form
        btn.closest('.card').querySelector('.comment-box').style.display = 'block';
    })
}

export { showAddCommentBox };

