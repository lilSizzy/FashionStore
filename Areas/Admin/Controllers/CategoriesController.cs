using FashionStore.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace FashionStore.Areas.Admin.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private FashionStoreWebEntities db = new FashionStoreWebEntities();

        // GET: Admin/Categories 
        public ActionResult Index(string q = "")
        {
            ViewBag.PageTitle = "Quản lý danh mục";
            var cats = db.Categories.Include(c => c.Categories1) // parent nav name nếu có
                                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                cats = cats.Where(c => c.Name.Contains(q) || c.Slug.Contains(q));
            }

            return View(cats.OrderBy(c => c.Name).ToList());
        }


        // GET: Admin/Categories/Create
        public ActionResult Create()
        {
            ViewBag.PageTitle = "Thêm danh mục";
            BindParents();
            return View(new Category());
        }

        // POST: Admin/Categories/Create
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Category model)
        {
            ViewBag.PageTitle = "Thêm danh mục";
            // Slug auto nếu bỏ trống
            if (string.IsNullOrWhiteSpace(model.Slug))
                model.Slug = Slugify(model.Name);

            // Unique Name/Slug (đơn giản)
            if (db.Categories.Any(c => c.Name == model.Name))
                ModelState.AddModelError("Name", "Tên danh mục đã tồn tại.");
            if (db.Categories.Any(c => c.Slug == model.Slug))
                ModelState.AddModelError("Slug", "Slug đã tồn tại.");

            if (model.ParentId == 0) model.ParentId = null; // nhận từ dropdown "(Không)"
            if (!ModelState.IsValid)
            {
                BindParents();
                return View(model);
            }

            db.Categories.Add(model);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Admin/Categories/Edit/5
        public ActionResult Edit(int id)
        {
            var cat = db.Categories.Find(id);
            if (cat == null) return HttpNotFound();

            ViewBag.PageTitle = "Sửa danh mục";
            BindParents(id); // loại chính nó khỏi dropdown
            return View(cat);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Category model)
        {
            var cat = db.Categories.Find(model.Id);
            if (cat == null) return HttpNotFound();

            // Slug auto nếu trống
            if (string.IsNullOrWhiteSpace(model.Slug))
                model.Slug = Slugify(model.Name);

            // Unique (trừ chính nó)
            if (db.Categories.Any(c => c.Id != model.Id && c.Name == model.Name))
                ModelState.AddModelError("Name", "Tên danh mục đã tồn tại.");
            if (db.Categories.Any(c => c.Id != model.Id && c.Slug == model.Slug))
                ModelState.AddModelError("Slug", "Slug đã tồn tại.");

            if (model.ParentId == model.Id)
                ModelState.AddModelError("ParentId", "Không thể chọn chính nó làm danh mục cha.");

            if (!ModelState.IsValid)
            {
                ViewBag.PageTitle = "Sửa danh mục";
                BindParents(model.Id);
                return View(model);
            }

            cat.Name = model.Name;
            cat.Slug = model.Slug;
            cat.ParentId = model.ParentId == 0 ? null : model.ParentId;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Admin/Categories/Delete/5
        public ActionResult Delete(int id)
        {
            var cat = db.Categories.Find(id);
            if (cat == null) return HttpNotFound();

            // chặn xóa khi có sản phẩm
            bool hasProducts = db.Products.Any(p => p.CategoryId == id);
            if (hasProducts)
            {
                TempData["Error"] = "Không thể xóa: Danh mục đang có sản phẩm.";
                return RedirectToAction("Index");
            }

            db.Categories.Remove(cat);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Helpers
        private void BindParents(int? excludeId = null)
        {
            var parents = db.Categories.AsQueryable();
            if (excludeId.HasValue)
                parents = parents.Where(c => c.Id != excludeId.Value);

            var list = parents.OrderBy(c => c.Name).ToList();
            ViewBag.ParentId = new SelectList(
                items: list,
                dataValueField: "Id",
                dataTextField: "Name"
            );

            // thêm option "(Không)" ở view (value 0)
            ViewBag.HasParentPlaceholder = true;
        }

        private string Slugify(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            var slug = text.Trim().ToLowerInvariant();

            // bỏ dấu tiếng Việt
            slug = RemoveDiacritics(slug);
            // ký tự không hợp lệ -> "-"
            foreach (var ch in System.IO.Path.GetInvalidFileNameChars())
                slug = slug.Replace(ch.ToString(), "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\- ]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');
            return slug;
        }

        private string RemoveDiacritics(string s)
        {
            var norm = s.Normalize(System.Text.NormalizationForm.FormD);
            var chars = norm.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                                != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(System.Text.NormalizationForm.FormC);
        }
    }
}