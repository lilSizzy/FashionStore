using FashionStore.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace FashionStore.Areas.Admin.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private FashionStoreWebEntities db = new FashionStoreWebEntities();

        // 🟩 GET: Admin/Users
        public ActionResult Index()
        {
            var users = db.Users.OrderByDescending(u => u.CreatedDate).ToList();
            ViewBag.PageTitle = "Quản lý người dùng";

            // Đếm số admin hiện có (phục vụ view ẩn nút Xóa)
            ViewBag.AdminCount = db.Users.Count(u => u.Role == "Admin");

            return View(users);
        }

        // 🟩 GET: Admin/Users/Create
        public ActionResult Create()
        {
            ViewBag.PageTitle = "Thêm người dùng mới";
            return View(new User
            {
                Role = "Customer",
                CreatedDate = DateTime.Now
            });
        }

        // 🟩 POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username))
                ModelState.AddModelError("Username", "Tên đăng nhập không được để trống.");

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                ModelState.AddModelError("PasswordHash", "Mật khẩu không được để trống.");

            if (db.Users.Any(u => u.Username == user.Username))
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");

            if (!ModelState.IsValid)
            {
                ViewBag.PageTitle = "Thêm người dùng mới";
                return View(user);
            }

            user.CreatedDate = DateTime.Now;
            db.Users.Add(user);
            db.SaveChanges();

            TempData["Success"] = "Thêm người dùng thành công!";
            return RedirectToAction("Index");
        }

        // 🟩 GET: Admin/Users/Edit/5
        public ActionResult Edit(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            ViewBag.PageTitle = "Sửa thông tin người dùng";
            return View(user);
        }

        // 🟩 POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User user)
        {
            var existing = db.Users.Find(user.Id);
            if (existing == null) return HttpNotFound();

            // Kiểm tra trùng tên đăng nhập
            if (db.Users.Any(u => u.Username == user.Username && u.Id != user.Id))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                ViewBag.PageTitle = "Sửa thông tin người dùng";
                return View(user);
            }

            // 🚫 Không cho tự hạ quyền Admin của chính mình
            if (existing.Username == User.Identity.Name && user.Role != "Admin")
            {
                ModelState.AddModelError("", "Bạn không thể hạ quyền của chính mình.");
                ViewBag.PageTitle = "Sửa thông tin người dùng";
                return View(user);
            }

            // 🚫 Không cho hạ quyền Admin cuối cùng
            int adminCount = db.Users.Count(u => u.Role == "Admin");
            if (existing.Role == "Admin" && user.Role != "Admin" && adminCount <= 1)
            {
                ModelState.AddModelError("", "Không thể hạ quyền admin cuối cùng của hệ thống.");
                ViewBag.PageTitle = "Sửa thông tin người dùng";
                return View(user);
            }

            // Cập nhật thông tin
            existing.Username = user.Username;
            existing.PasswordHash = user.PasswordHash;
            existing.Role = user.Role;
            db.SaveChanges();

            TempData["Success"] = "Cập nhật thông tin người dùng thành công!";
            return RedirectToAction("Index");
        }

        // 🟩 GET: Admin/Users/Delete/5
        public ActionResult Delete(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            // 🚫 Không cho xóa chính mình
            if (User.Identity.Name == user.Username)
            {
                TempData["Error"] = "Bạn không thể xóa tài khoản của chính mình.";
                return RedirectToAction("Index");
            }

            // 🚫 Không cho xóa admin cuối cùng
            int adminCount = db.Users.Count(u => u.Role == "Admin");
            if (user.Role == "Admin" && adminCount <= 1)
            {
                TempData["Error"] = "Không thể xóa admin cuối cùng của hệ thống.";
                return RedirectToAction("Index");
            }

            db.Users.Remove(user);
            db.SaveChanges();

            TempData["Success"] = "Xóa người dùng thành công!";
            return RedirectToAction("Index");
        }
    }
}
