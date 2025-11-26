using FashionStore.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FashionStore.Areas.Admin.Controllers
{
    [Authorize]
    public class BannersController : Controller
    {
        // GET: Admin/Banners
        private FashionStoreWebEntities db = new FashionStoreWebEntities();

        public ActionResult Index()
        {
            var banners = db.Banners.OrderBy(b => b.OrderNo).ToList();
            ViewBag.PageTitle = "Quản lý Banner";
            return View(banners);
        }

        public ActionResult Create()
        {
            ViewBag.PageTitle = "Thêm Banner";
            return View(new Banner { IsActive = true, OrderNo = 0 }); // mặc định bật
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Banner banner, HttpPostedFileBase ImageFile)
        {
            // Đọc IsActive chắc chắn đúng (xử lý các case "true", "on", "true,false"...)
            var raw = Request.Form.GetValues("IsActive");
            bool postedActive = raw != null && raw.Any(v =>
                v.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                v.Equals("on", StringComparison.OrdinalIgnoreCase));

            banner.IsActive = postedActive;           // ✅ không còn NULL
                                                      // --- upload ảnh ---
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                var fileName = Guid.NewGuid() + System.IO.Path.GetExtension(ImageFile.FileName);
                var path = Server.MapPath("~/Uploads/banners/" + fileName);
                ImageFile.SaveAs(path);
                banner.ImageUrl = "/Uploads/banners/" + fileName;
            }

            db.Banners.Add(banner);
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        public ActionResult Edit(int id)
        {
            var banner = db.Banners.Find(id);
            if (banner == null) return HttpNotFound();

            ViewBag.PageTitle = "Sửa Banner";
            return View(banner);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Banner banner, HttpPostedFileBase ImageFile)
        {
            var item = db.Banners.Find(banner.Id);
            if (item == null) return HttpNotFound();

            // IsActive chắc chắn đúng
            var raw = Request.Form.GetValues("IsActive");
            bool postedActive = raw != null && raw.Any(v =>
                v.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                v.Equals("on", StringComparison.OrdinalIgnoreCase));
            item.IsActive = postedActive;

            item.OrderNo = banner.OrderNo;

            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                if (!string.IsNullOrEmpty(item.ImageUrl))
                {
                    var old = Server.MapPath("~" + item.ImageUrl);
                    if (System.IO.File.Exists(old)) System.IO.File.Delete(old);
                }
                var fileName = Guid.NewGuid() + System.IO.Path.GetExtension(ImageFile.FileName);
                var path = Server.MapPath("~/Uploads/banners/" + fileName);
                ImageFile.SaveAs(path);
                item.ImageUrl = "/Uploads/banners/" + fileName;
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }



        public ActionResult Delete(int id)
        {
            var banner = db.Banners.Find(id);
            if (banner == null) return HttpNotFound();

            if (!string.IsNullOrEmpty(banner.ImageUrl))
            {
                string path = Server.MapPath("~" + banner.ImageUrl);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            db.Banners.Remove(banner);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}