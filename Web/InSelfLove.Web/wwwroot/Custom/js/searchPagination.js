const paginationScripts = function (refresherId) {
    // script is intended only for the search page for now.
    // Articles' & Videos' pagination is not loaded dynamically
    if (!document.getElementById('search-page')) {
        return;
    }

    // refresherId indicates a fetch request & helps
    // identify which resource to refresh (articles or videos)
    if (refresherId === undefined) {
        refresherId = '';
    }

    // Get all items in pagination
    document.querySelectorAll(refresherId + ' .page-link').forEach(pl => pl.addEventListener('click', e => {
        e.preventDefault();
        // Remove 0, null and ""
        let hrefArray = e.target.getAttribute('href').split('/').filter(x => x);

        // Gather data for fetch request
        let searchTerm = document.querySelector('#search-term').value;
        let page = hrefArray[1].split('=')[1];
        let action = hrefArray[1].split('?')[0];
        let controller = hrefArray[0];

        fetch(`/api/${controller}/${action}?page=${page}&searchTerm=${searchTerm}`)
            .then(response => {
                if (!response.ok) {
                    console.log('server error');
                    return;
                };

                return response.text();
            })
            .then(data => {
                if (action === "Video") {
                    // Replace data
                    let allVideos = document.getElementById('all-videos');
                    allVideos.innerHTML = '';
                    allVideos.innerHTML = data;

                    // Equalize preview items' heights
                    equalizeVideoHeight();

                    // Attach listeners to newly-attached pagination
                    paginationScripts('#all-videos');
                } else {
                    // Replace data
                    let allArticles = document.getElementById('all-articles');
                    allArticles.innerHTML = '';
                    allArticles.innerHTML = data;

                    // Equalize preview items' heights
                    equalizeArticleHeight();

                    // Attach listeners to newly-attached pagination
                    paginationScripts('#all-articles');
                }
            });
    }));
}

paginationScripts();