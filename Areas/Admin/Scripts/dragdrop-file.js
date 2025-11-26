// dragdrop-file.js  (single file dropzone)
document.addEventListener("DOMContentLoaded", function () {
    // Hỗ trợ cả 2 naming: Banners (cũ) và Products (mới)
    const dropZone = document.getElementById("dropZoneMain") || document.getElementById("dropZone");
    const inputFile = document.getElementById("MainImageFile") || document.getElementById("ImageFile");
    const fileNameTxt = document.getElementById("fileNameMain") || document.getElementById("fileName");
    const previewImg = document.getElementById("previewMain") || document.getElementById("previewImage");

    if (!dropZone || !inputFile) return;

    // Kéo vào
    dropZone.addEventListener("dragover", function (e) {
        e.preventDefault();
        dropZone.classList.add("dragover");
    });

    // Rời ra
    dropZone.addEventListener("dragleave", function () {
        dropZone.classList.remove("dragover");
    });

    // Thả file
    dropZone.addEventListener("drop", function (e) {
        e.preventDefault();
        dropZone.classList.remove("dragover");
        if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
            // gán vào input (rất quan trọng để form submit có file)
            inputFile.files = e.dataTransfer.files;
            handleFile(inputFile.files[0]);
        }
    });

    // Chọn file bằng dialog
    inputFile.addEventListener("change", function () {
        if (inputFile.files && inputFile.files.length > 0) {
            handleFile(inputFile.files[0]);
        }
    });

    function handleFile(file) {
        if (!file) return;
        if (fileNameTxt) {
            fileNameTxt.textContent = "✔ " + file.name;
            fileNameTxt.style.color = "#28a745";
        }
        dropZone.classList.add("success");
        showPreview(file);
    }

    function showPreview(file) {
        if (!previewImg) return;
        const url = URL.createObjectURL(file);
        previewImg.src = url;
        previewImg.style.display = "block";
    }
});
