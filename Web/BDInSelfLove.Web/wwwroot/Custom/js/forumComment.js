function SetReply(id, name) {
    $("#ParentCommentId").val(id);
    if (id == '0') {
        $("#AddCommentText").html('Add New Comment');
    }
    else {
        $("#AddCommentText").html('Add a reply to ' + name + '\'s comment');
    }

    $('html, body').animate({ scrollTop: $("#AddCommentText").offset().top }, 500);
}


(function getButtons() {
    const comments = document.querySelectorAll('.comment');


    comments.forEach(c => {
        const parentCommentUser = c.querySelector('.user-link').textContent;
        const parentCommentId = c.querySelector('.card').getAttribute('id');

        const button = c.querySelector('.reply-button');

        button.addEventListener('click', () => SetReply(parentCommentId, parentCommentUser), false);

    })

    document.querySelectorAll('button')[2].addEventListener('click', () => SetReply(0, null), false);

})();