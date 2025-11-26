using FashionStore.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace FashionStore.Controllers
{
    public class AccountController : Controller
    {
        private FashionStoreWebEntities db = new FashionStoreWebEntities();

        // 🟩 LOGIN (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

            if (user == null)
            {
                return Json(new { success = false, message = "Sai tên đăng nhập hoặc mật khẩu!" });
            }

            FormsAuthentication.SetAuthCookie(user.Username, false);
            Session["Username"] = user.Username;
            Session["Role"] = user.Role;

            return Json(new { success = true, role = user.Role });
        }

        // 🟩 REGISTER (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string username, string password)
        {
            // Kiểm tra trùng tên đăng nhập
            var exists = db.Users.Any(u => u.Username == username);
            if (exists)
            {
                return Json(new { success = false, message = "Tên đăng nhập đã tồn tại!" });
            }

            // Tạo user mới (cho phép password đơn giản)
            var newUser = new User
            {
                Username = username,
                PasswordHash = password, // 🔥 Không mã hóa để tiện test
                Role = "Customer",
                CreatedDate = DateTime.Now
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            // Đăng nhập luôn sau khi đăng ký
            FormsAuthentication.SetAuthCookie(newUser.Username, false);
            Session["Username"] = newUser.Username;
            Session["Role"] = newUser.Role;

            return Json(new { success = true });
        }

        // 🟩 LOGOUT (GET)
        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
