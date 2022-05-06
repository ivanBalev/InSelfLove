const equalizeArticleHeight = function () {
    let DOMarticlesAndAboutPreview = document.querySelectorAll('.article-resize');
    let articlesArray = Array.from(DOMarticlesAndAboutPreview);
    if (articlesArray.length === 0) {
        return;
    }
    let randomArticleHeight = articlesArray[0].offsetHeight;

    if (articlesArray.some(e => e.offsetHeight != randomArticleHeight)) {
        let maxHeight = 0;
        articlesArray.forEach(e => e.offsetHeight > maxHeight ? maxHeight = e.offsetHeight : null);

        for (let i = 0; i < articlesArray.length; i++) {
            DOMarticlesAndAboutPreview[i].style.height = maxHeight + 'px';
        }
    }
}

equalizeArticleHeight();