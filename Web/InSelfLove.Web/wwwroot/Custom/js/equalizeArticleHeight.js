const equalizeArticleHeight = function () {
    // Get all preview items
    let DOMarticlesAndAboutPreview = document.querySelectorAll('.article-resize');
    let articlesArray = Array.from(DOMarticlesAndAboutPreview);
    if (articlesArray.length === 0) {
        return;
    }
        
    // Get one of their heights
    let randomArticleHeight = articlesArray[0].offsetHeight;

    // If any of the others' heights are different
    if (articlesArray.some(e => e.offsetHeight != randomArticleHeight)) {
        let maxHeight = 0;
        // Get the tallest preview item
        articlesArray.forEach(e => e.offsetHeight > maxHeight ? maxHeight = e.offsetHeight : null);

         //Set all items' heights to equal that
        for (let i = 0; i < articlesArray.length; i++) {
            DOMarticlesAndAboutPreview[i].style.height = maxHeight + 'px';
        }
    }
}

// Fix for home page bug where just calling the function gives inaccurate
// values for .offsetHeight of .article-resize elements. 2+ hours spent debugging - not worth it
setTimeout(equalizeArticleHeight, 300);
