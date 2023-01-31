document.querySelectorAll('.video-title')?.forEach(t => {
    let innerText = t.innerText;
    let windowWidth = window.innerWidth;

    // Decrease text size depending on available space
    if (windowWidth < '768' && innerText.length > 24) {
        t.innerText = innerText.substr(0, 24) + '...';
    } else if (windowWidth <= '992' && innerText.length > 40) {
        t.innerText = innerText.substr(0, 40) + '...';
    } else if (windowWidth <= '1200' && innerText.length > 55) {
        t.innerText = innerText.substr(0, 55) + '...';
    } else if (innerText.length > 65) {
        t.innerText = innerText.substr(0, 65) + '...';
    }
})