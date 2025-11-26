$(document).ready(function () {
    $("#loginForm").on("submit", function (e) {
        e.preventDefault();

        $("#loginError").addClass("d-none");
        var form = $(this);

        $.ajax({
            url: form.attr("action"),
            type: "POST",
            data: form.serialize(), // Gửi dữ liệu form-encoded
            success: function (res) {
                if (res.success) {
                    // Ẩn modal
                    const modal = bootstrap.Modal.getInstance(document.getElementById("loginModal"));
                    if (modal) modal.hide();

                    // Điều hướng
                    if (res.role === "Admin") {
                        window.location.href = "/Admin/Dashboard";
                    } else {
                        location.reload();
                    }
                } else {
                    $("#loginError").removeClass("d-none").text(res.message);
                }
            },
            error: function () {
                $("#loginError").removeClass("d-none").text("Có lỗi xảy ra, vui lòng thử lại!");
            }
        });
    });
});
