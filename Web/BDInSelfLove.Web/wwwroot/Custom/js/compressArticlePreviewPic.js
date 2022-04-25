document.getElementById("originalPreviewPic").onchange = e => handlePreviewPicture(e);
document.getElementById("Image").onchange = e => {
    document.getElementById('upload-file-info').innerHTML = e.target.files[0].name;
    document.getElementById('ImageUrl').value = '';
    handlePreviewPicture(e);
};

function handlePreviewPicture (e) {
    let filesSelected = e.target.files;
    console.log(filesSelected);
    // Something needs to be uploaded in order to proceed
    if (filesSelected.length === 0) {
        return;
    }

    let fileToLoad = filesSelected[0];
    // Max file size is 10 mb
    if (fileToLoad.size > (10 * 1024 * 1024)) {
        alert("File needs to be less than 10 MB.");
        this.value = "";
    };

    // Set file name so user knows the upload is successful
    document.getElementById('upload-previewPic-info').innerHTML = fileToLoad.name;

    let fileReader = new FileReader();
    fileReader.onload = function (fileLoadedEvent) {

        let imgBase64 = fileLoadedEvent.target.result;
        // 16 x 9, little larger than max size of preview img(bootstrap-controlled)
        const maxWidth = 700;
        const maxHeight = 394;

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

            resolve(canvas.toDataURL('image/jpeg', 1))
        }
        img.onerror = function (err) {
            reject(err)
        }
        img.src = base64;
    })
}