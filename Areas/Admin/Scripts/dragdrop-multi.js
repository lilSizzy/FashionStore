// dragdrop-multi.js  (multi files dropzone)
document.addEventListener("DOMContentLoaded", function () {
    const dropZoneSub = document.getElementById("dropZoneSub");
    const inputFiles = document.getElementById("ImageFiles");
    const subPreview = document.getElementById("subPreviewContainer");

    // 2 vùng hiển thị thông tin (nếu chưa có, tự tạo)
    let subCountEl = document.getElementById("subCount");
    let subListEl = document.getElementById("subFileList");

    if (!dropZoneSub || !inputFiles || !subPreview) return;

    if (!subCountEl) {
        subCountEl = document.createElement("div");
        subCountEl.id = "subCount";
        subCountEl.className = "sub-info-count";
        dropZoneSub.parentNode.appendChild(subCountEl);
    }
    if (!subListEl) {
        subListEl = document.createElement("ul");
        subListEl.id = "subFileList";
        subListEl.className = "sub-file-list";
        dropZoneSub.parentNode.appendChild(subListEl);
    }

    // Kéo vào
    dropZoneSub.addEventListener("dragover", function (e) {
        e.preventDefault();
        dropZoneSub.classList.add("dragover");
    });

    // Rời khỏi
    dropZoneSub.addEventListener("dragleave", function () {
        dropZoneSub.classList.remove("dragover");
    });

    // Thả file
    dropZoneSub.addEventListener("drop", function (e) {
        e.preventDefault();
        dropZoneSub.classList.remove("dragover");
        if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
            appendFilesToInput(e.dataTransfer.files);
            renderFromInput();
        }
    });

    // Chọn bằng dialog
    inputFiles.addEventListener("change", function () {
        if (inputFiles.files && inputFiles.files.length > 0) {
            // Khi dùng dialog, đã gán sẵn → chỉ cần render
            renderFromInput();
        }
    });

    // Gộp file mới vào input[type=file] bằng DataTransfer
    function appendFilesToInput(newFiles) {
        const dt = new DataTransfer();
        // giữ file cũ
        for (let f of Array.from(inputFiles.files)) dt.items.add(f);
        // thêm file mới
        for (let f of Array.from(newFiles)) {
            if (f.type && f.type.startsWith("image/")) dt.items.add(f);
        }
        inputFiles.files = dt.files;
        dropZoneSub.classList.add("success");
    }

    // Render preview + danh sách + số lượng từ inputFiles.files
    function renderFromInput() {
        // preview
        subPreview.innerHTML = "";
        for (let f of Array.from(inputFiles.files)) {
            const img = document.createElement("img");
            img.src = URL.createObjectURL(f);
            img.title = f.name;
            subPreview.appendChild(img);
        }

        // count
        subCountEl.textContent = `Đã chọn ${inputFiles.files.length} ảnh`;

        // list tên
        subListEl.innerHTML = "";
        for (let f of Array.from(inputFiles.files)) {
            const li = document.createElement("li");
            li.textContent = f.name;
            subListEl.appendChild(li);
        }
    }
});
