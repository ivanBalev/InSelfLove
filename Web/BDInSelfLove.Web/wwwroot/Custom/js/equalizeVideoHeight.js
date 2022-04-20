// Declared as const since it's being used on the search page multiple times
const equalizeVideoHeight = function () {
    let DOMvideos = document.querySelectorAll('.video-resize');
    let videosArray = Array.from(DOMvideos);
    let randomVideoHeight = videosArray[0].offsetHeight;

    if (videosArray.some(e => e.offsetHeight != randomVideoHeight)) {
        let maxHeight = 0;
        videosArray.forEach(e => e.offsetHeight > maxHeight ? maxHeight = e.offsetHeight : null);

        for (let i = 0; i < videosArray.length; i++) {
            DOMvideos[i].style.height = maxHeight + 'px';
        }
    }
};

equalizeVideoHeight();