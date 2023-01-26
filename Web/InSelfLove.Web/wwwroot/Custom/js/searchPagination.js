const paginationScripts = function (refresherId) {
    // script is intended only for the search page for now. Articles' & Videos' pagination is not loaded dynamically
    if (!document.getElementById('search-page')) {
        return;
    }

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

        fetch(`/api/${controller}/${action}?page=${page}&searchTerm=${searchTerm}`)
            .then(response => {
                if (!response.ok) {
                    console.log('server error');
                    return;
                };

                return response.text();
            })
            .then(data => {
                // Replace data
                if (action === "Video") {
                    let allVideos = document.getElementById('all-videos');
                    allVideos.innerHTML = '';
                    allVideos.innerHTML = data;
                    equalizeVideoHeight();
                    paginationScripts('#all-videos');
                } else {
                    let allArticles= document.getElementById('all-articles');
                    allArticles.innerHTML = '';
                    allArticles.innerHTML = data;
                    equalizeArticleHeight();
                    paginationScripts('#all-articles');
                }
            });
    }));
}

paginationScripts();