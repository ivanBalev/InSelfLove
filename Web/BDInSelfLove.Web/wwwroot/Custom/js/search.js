const paginationScripts = function (refresherId) {
    if (refresherId === undefined) {
        refresherId = '';
    }

    document.querySelectorAll(refresherId + ' .page-link').forEach(pl => pl.addEventListener('click', e => {
        e.preventDefault();
        let hrefArray = e.target.getAttribute('href').split('/');

        let searchTerm = document.querySelector('#search-term').value;
        let page = hrefArray[2].split('=')[1];
        let controller = hrefArray[1];

        $.ajax({
            type: "GET",
            url: '/api/Search/' + controller,
            data: {
                page: page,
                searchTerm: searchTerm,
            },
            success: function (data) {
                if (controller === "Video") {
                    $('#all-videos').replaceWith(data);
                    equalizeVideoPreviewHeight();
                    paginationScripts('#all-videos');
                } else {
                    $('#all-articles').replaceWith(data);
                    equalizeArticlePreviewHeight();
                    paginationScripts('#all-articles');
                }
            },
            error: function (err) {
                alert('Error');
            }
        })
    }));
}

paginationScripts();