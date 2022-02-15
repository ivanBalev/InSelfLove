let uploadField = document.getElementById("originalPreviewPic");

uploadField.onchange = function () {

    let filesSelected = this.files;
    // Something needs to be uploaded in order to proceed
    if (filesSelected.length === 0) {
        return;
    }

    let fileToLoad = filesSelected[0];
    // Max file size is 10 mb
    if (this.files[0].size > (10 * 1024 * 1024)) {
        alert("File needs to be less than 10 MB.");
        this.value = "";
    };

    // Set file name so user knows the upload is successful
    $('#upload-previewPic-info').html(fileToLoad.name);

    let fileReader = new FileReader();
    fileReader.onload = function (fileLoadedEvent) {

        let imgBase64 = fileLoadedEvent.target.result;
        const maxWidth = 500;
        const maxHeight = 200;

        compressImage(imgBase64, maxWidth, maxHeight)
            .then(function (newImg) {
                document.querySelector('#PreviewImage').value = newImg;
            });
    }
    fileReader.readAsDataURL(fileToLoad);
};

function compressImage(base64, maxWidth, maxHeight) {
    const canvas = document.createElement('canvas')
    const img = document.createElement('img')

    return new Promise((resolve, reject) => {
        img.onload = function () {
            let width = img.width
            let height = img.height

            // Scaling according to img size
            if (width > height) {
                if (width > maxWidth) {
                    height = Math.round((height *= maxWidth / width))
                    width = maxWidth
                }
            } else {
                if (height > maxHeight) {
                    width = Math.round((width *= maxHeight / height))
                    height = maxHeight
                }
            }
            canvas.width = width
            canvas.height = height

            const ctx = canvas.getContext('2d')
            ctx.drawImage(img, 0, 0, width, height)

            resolve(canvas.toDataURL('image/jpeg', 0.9))
        }
        img.onerror = function (err) {
            reject(err)
        }
        img.src = base64;
    })
}