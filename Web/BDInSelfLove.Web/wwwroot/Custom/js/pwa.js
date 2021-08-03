if ('serviceworker' in navigator) {
    navigator.serviceworker.register('/sw.js')
        .then((reg) => {
            reg.update();
            console.log('registered', reg);
        }) // remove reg.update later. it forces an update immediately, needed just now.
        .catch((err) => console.log('rejected', err))
}
