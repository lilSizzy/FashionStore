$(document).ready(function () {
    $("#registerForm").on("submit", function (e) {
        e.preventDefault();
        $("#registerError").addClass("d-none");
        $("#registerSuccess").addClass("d-none");

        var form = $(this);

        $.ajax({
            url: form.attr("action"),
            type: "POST",
            data: form.serialize(),
            success: function (res) {
                if (res.success) {
                    $("#registerSuccess").removeClass("d-none").text("Đăng ký thành công! Hệ thống sẽ tự đăng nhập...");
                    setTimeout(() => {
                        const modal = bootstrap.Modal.getInstance(document.getElementById("registerModal"));
                        if (modal) modal.hide();
                        location.reload();
                    }, 1000);
                } else {
                    $("#registerError").removeClass("d-none").text(res.message);
                }
            },
            error: function () {
                $("#registerError").removeClass("d-none").text("Có lỗi xảy ra, vui lòng thử lại!");
            }
        });
    });
});
