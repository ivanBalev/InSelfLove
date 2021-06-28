const equalizeVideoPreviewHeight = () => {

    let DOMelements = document.querySelectorAll('.video-preview h1');
    let elements = Array.from(DOMelements);
    let randomElementHeight = elements[0].offsetHeight;

    if (elements.some(e => e.offsetHeight != randomElementHeight)) {
        let maxHeight = 0;
        elements.forEach(e => e.offsetHeight > maxHeight ? maxHeight = e.offsetHeight : null);

        for (let i = 0; i < elements.length; i++) {
            DOMelements[i].style.height = maxHeight + 'px';
        }
    }
}

equalizeVideoPreviewHeight();