const paginationScripts = function (refresherId) {
    if (refresherId === undefined) {
        refresherId = '';
    }

    document.querySelectorAll(refresherId + ' .page-link').forEach(pl => pl.addEventListener('click', e => {
        e.preventDefault();
        let hrefArray = e.target.getAttribute('href').split('/').filter(x => x);

        let searchTerm = document.querySelector('#search-term').value;
        let page = hrefArray[1].split('=')[1];
        let action = hrefArray[1].split('?')[0];
        let controller = hrefArray[0];

        $.ajax({
            type: "GET",
            url: `/api/${controller}/${action}`,
            data: {
                page: page,
                searchTerm: searchTerm,
            },
            success: function (data) {
                if (action === "Video") {
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