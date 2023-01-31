const equalizeVideoHeight = function () {
    // Get all preview items
    let DOMvideos = document.querySelectorAll('.video-resize');
    let videosArray = Array.from(DOMvideos);
    if (videosArray.length === 0) {
        return;
    }

    // Get one of their heights
    let randomVideoHeight = videosArray[0].offsetHeight;

    // If any of the others' heights are different
    if (videosArray.some(e => e.offsetHeight != randomVideoHeight)) {
        let maxHeight = 0;
        // Get the tallest preview item
        videosArray.forEach(e => e.offsetHeight > maxHeight ? maxHeight = e.offsetHeight : null);

        // Set all items' heights to equal that
        for (let i = 0; i < videosArray.length; i++) {
            DOMvideos[i].style.height = maxHeight + 'px';
        }
    }
};

equalizeVideoHeight();