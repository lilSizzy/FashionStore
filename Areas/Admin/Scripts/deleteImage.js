document.addEventListener("DOMContentLoaded", function () {
    // Lấy URL Action từ thuộc tính data-delete-url của một phần tử nào đó trên trang (Xem giải thích ở dưới)
    const deleteUrl = document.querySelector('[data-delete-url]').dataset.deleteUrl;

    document.querySelectorAll(".btn-delete-img").forEach(btn => {
        btn.addEventListener("click", function () {
            const id = this.dataset.id;

            // 1. Xác nhận xóa
            if (!confirm("Xóa ảnh này?")) {
                return;
            }

            // 2. Gửi yêu cầu AJAX (fetch)
            fetch(deleteUrl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({ id: id })
            })
                .then(res => {
                    // Kiểm tra HTTP status và trả về JSON
                    if (!res.ok) {
                        throw new Error("Network response was not ok");
                    }
                    return res.json();
                })
                .then(data => {
                    // 3. Xử lý kết quả trả về
                    if (data.success) {
                        // Xóa phần tử cha chứa nút bấm (là div chứa ảnh)
                        this.closest("div").remove();
                    } else {
                        alert("Không thể xóa ảnh! Lỗi: " + (data.message || "Unknown error."));
                    }
                })
                .catch(error => {
                    console.error("Fetch error:", error);
                    alert("Đã xảy ra lỗi trong quá trình xóa.");
                });
        });
    });
});