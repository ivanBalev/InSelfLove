let uploadField = document.getElementById("originalProfilePic");

uploadField.onchange = function () {
    $('#upload-file-info').html(this.files[0].name);
    if (this.files[0].size > (10 * 1024 * 1024)) {
        alert("File is too big, please lower image quality to less than 10 MB.");
        this.value = "";
    };
    let filesSelected = uploadField.files;
    if (filesSelected.length > 0) {
        let fileToLoad = filesSelected[0];

        let fileReader = new FileReader();

        fileReader.onload = function (fileLoadedEvent) {
            let oldBase64 = fileLoadedEvent.target.result; // <--- data: base64
            compressImage(oldBase64).then(function (newImg) {
                let newinput = document.createElement("input");
                newinput.type = 'hidden';
                newinput.name = 'Input.ProfilePicture';
                newinput.id = 'Input_ProfilePicture'
                newinput.value = newImg; // put result from canvas into new hidden input
                document.querySelector('#profile-form').appendChild(newinput);
            });
        }
        fileReader.readAsDataURL(fileToLoad);
    }
};

function compressImage(base64) {
    const canvas = document.createElement('canvas')
    const img = document.createElement('img')

    return new Promise((resolve, reject) => {
        img.onload = function () {
            let width = img.width
            let height = img.height
            const maxHeight = 300
            const maxWidth = 300

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

            resolve(canvas.toDataURL('image/jpeg', 0.7))
        }
        img.onerror = function (err) {
            reject(err)
        }
        img.src = base64;
    })
}