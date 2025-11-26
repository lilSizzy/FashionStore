using FashionStore.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FashionStore.Models.ViewModels;
namespace FashionStore.Areas.Admin.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private FashionStoreWebEntities db = new FashionStoreWebEntities();


        // GET: Admin/Products
        public ActionResult Index(int page = 1, string searchTerm = "", decimal? minPrice = null, decimal? maxPrice = null, string sortBy = "name", string sortOrder = "asc")
        {
            int pageSize = 5;
            var products = db.Products.AsQueryable();

            // Áp dụng filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.Name.Contains(searchTerm));
            }
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            // Sắp xếp
            switch (sortBy.ToLower())
            {
                case "price":
                    products = sortOrder == "asc" ? products.OrderBy(p => p.Price) : products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = sortOrder == "asc" ? products.OrderBy(p => p.Name) : products.OrderByDescending(p => p.Name);
                    break;
            }

            // QUAN TRỌNG: Lấy toàn bộ sản phẩm (không phân trang ở đây)
            var allProducts = products.ToList();
            int totalCount = allProducts.Count;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var model = new ProductSearchVM
            {
                Products = allProducts, // Gửi toàn bộ sản phẩm đến View
                TotalCount = totalCount,
                Page = page,
                SearchTerm = searchTerm,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            return View(model);
        }

        // Hàm sắp xếp
        private IQueryable<Product> ApplySorting(IQueryable<Product> products, string sortBy, string sortOrder)
        {
            // Mặc định sắp xếp theo ngày tạo mới nhất
            if (string.IsNullOrEmpty(sortBy))
            {
                return products.OrderByDescending(p => p.CreatedDate);
            }

            // Xử lý sắp xếp theo từng trường
            switch (sortBy.ToLower())
            {
                case "price":
                    if (sortOrder == "desc")
                        return products.OrderByDescending(p => p.Price);
                    else
                        return products.OrderBy(p => p.Price);

                case "category":
                    if (sortOrder == "desc")
                        return products.OrderByDescending(p => p.Category.Name);
                    else
                        return products.OrderBy(p => p.Category.Name);

                case "name":
                default:
                    if (sortOrder == "desc")
                        return products.OrderByDescending(p => p.Name);
                    else
                        return products.OrderBy(p => p.Name);
            }
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.PageTitle = "Thêm sản phẩm mới";
            ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name), "Id", "Name");
            return View(new Product());
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product, HttpPostedFileBase MainImageFile, IEnumerable<HttpPostedFileBase> ImageFiles)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
                return View(product);
            }

            // Upload ảnh chính
            if (MainImageFile != null && MainImageFile.ContentLength > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(MainImageFile.FileName);
                string path = Path.Combine(Server.MapPath("~/Uploads/Products"), fileName);
                Directory.CreateDirectory(Server.MapPath("~/Uploads/Products"));
                MainImageFile.SaveAs(path);
                product.MainImage = "/Uploads/Products/" + fileName;
            }

            product.CreatedDate = DateTime.Now;
            db.Products.Add(product);
            db.SaveChanges();

            // Upload nhiều ảnh phụ
            if (ImageFiles != null)
            {
                foreach (var file in ImageFiles)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        string path = Path.Combine(Server.MapPath("~/Uploads/ProductImages"), fileName);
                        Directory.CreateDirectory(Server.MapPath("~/Uploads/ProductImages"));
                        file.SaveAs(path);

                        db.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = "/Uploads/ProductImages/" + fileName
                        });
                    }
                }
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // GET: Admin/Products/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var product = db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
                return HttpNotFound();

            var categoryItems = db.Categories
     .OrderBy(c => c.Name)
     .Select(c => new SelectListItem
     {
         Value = c.Id.ToString(),
         Text = c.Name,
         Selected = (c.Id == product.CategoryId)
     }).ToList();

            ViewBag.CategoryId = categoryItems;


            // ✅ Xoá CategoryId trong ModelState để không bị override
            if (ModelState.ContainsKey("CategoryId"))
                ModelState.Remove("CategoryId");

            foreach (var c in db.Categories)
            {
                System.Diagnostics.Debug.WriteLine($" - {c.Id}: {c.Name}");
            }

            return View(product);
        }

        // POST: Admin/Products/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product model, HttpPostedFileBase MainImageFile, IEnumerable<HttpPostedFileBase> ImageFiles)
        {
            ModelState.Remove("MainImage");
            ModelState.Remove("ProductImages");

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name), "Id", "Name", model.CategoryId);
                return View(model);
            }

            var product = db.Products.Include(p => p.ProductImages).FirstOrDefault(p => p.Id == model.Id);
            if (product == null)
                return HttpNotFound();

            // Cập nhật thông tin
            product.Name = model.Name;
            product.Price = model.Price;
            product.Description = model.Description;
            product.Color = model.Color;
            product.Size = model.Size;
            product.CategoryId = model.CategoryId;

            // Upload ảnh chính (nếu có)
            if (MainImageFile != null && MainImageFile.ContentLength > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(MainImageFile.FileName);
                string saveDir = Server.MapPath("~/Uploads/Products");
                Directory.CreateDirectory(saveDir);
                string path = Path.Combine(saveDir, fileName);
                MainImageFile.SaveAs(path);
                product.MainImage = "/Uploads/Products/" + fileName;
            }

            // Upload thêm ảnh phụ (nếu có)
            if (ImageFiles != null)
            {
                foreach (var f in ImageFiles)
                {
                    if (f != null && f.ContentLength > 0)
                    {
                        string fn = Guid.NewGuid() + Path.GetExtension(f.FileName);
                        string saveDir = Server.MapPath("~/Uploads/ProductImages");
                        Directory.CreateDirectory(saveDir);
                        string p = Path.Combine(saveDir, fn);
                        f.SaveAs(p);

                        db.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = "/Uploads/ProductImages/" + fn
                        });
                    }
                }
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Admin/Products/Delete
        public ActionResult Delete(int id)
        {
            var product = db.Products.Include(p => p.ProductImages).FirstOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();

            // Xóa ảnh phụ
            foreach (var img in product.ProductImages.ToList())
            {
                string path = Server.MapPath("~" + img.ImageUrl);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                db.ProductImages.Remove(img);
            }

            // Xóa ảnh chính
            if (!string.IsNullOrEmpty(product.MainImage))
            {
                string path = Server.MapPath("~" + product.MainImage);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult DeleteImage(int id)
        {
            var image = db.ProductImages.Find(id);
            if (image != null)
            {
                string path = Server.MapPath("~" + image.ImageUrl);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                db.ProductImages.Remove(image);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
