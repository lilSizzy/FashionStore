using FashionStore.Models;
using FashionStore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace FashionStore.Controllers
{
    public class HomeController : Controller
    {
        private FashionStoreWebEntities db = new FashionStoreWebEntities();
        public ActionResult Index(string keyword, int? pageFeatured, int? pageNew)
        {
            ViewBag.Title = "Trang chủ - Fashion Store";

            // ⚠️ Nếu có thông báo từ TempData (ví dụ: chưa login, sai quyền)
            if (TempData["AuthError"] != null)
            {
                ViewBag.AuthError = TempData["AuthError"].ToString();
            }

            // 🖼 Banner hiển thị ở đầu
            var banners = db.Banners
                .Where(b => b.IsActive == true)
                .OrderByDescending(b => b.OrderNo)
                .ToList();

            // 🔍 Tìm kiếm sản phẩm theo tên, mô tả, hoặc danh mục
            var products = db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim().ToLower();
                products = products.Where(p =>
                    p.Name.ToLower().Contains(keyword) ||
                    p.Description.ToLower().Contains(keyword) ||
                    p.Category.Name.ToLower().Contains(keyword)
                );
                ViewBag.SearchKeyword = keyword;
            }
            int pageSize = 5; // Chỉ hiển thị 5 sản phẩm mỗi trang

            // 🌟 Sản phẩm nổi bật - phân trang
            int pageNumberFeatured = (pageFeatured ?? 1);
            var featuredProducts = products
                .OrderByDescending(p => p.SoldCount)
                .Skip((pageNumberFeatured - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Tổng số sản phẩm nổi bật để tính số trang
            int totalFeatured = products.Count();
            ViewBag.FeaturedPageNumber = pageNumberFeatured;
            ViewBag.FeaturedTotalPages = (int)Math.Ceiling(totalFeatured / (double)pageSize);

            // 🆕 Sản phẩm mới - phân trang
            int pageNumberNew = (pageNew ?? 1);
            var newProducts = products
                .OrderByDescending(p => p.CreatedDate)
                .Skip((pageNumberNew - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalNew = products.Count();
            ViewBag.NewPageNumber = pageNumberNew;
            ViewBag.NewTotalPages = (int)Math.Ceiling(totalNew / (double)pageSize);

            //// 🌟 Sản phẩm nổi bật: top 10 có SoldCount cao nhất
            //var featuredProducts = products
            //    .OrderByDescending(p => p.SoldCount)
            //    .Take(10)
            //    .ToList();

            //// 🆕 Sản phẩm mới: 10 sản phẩm mới nhất
            //var newProducts = products
            //    .OrderByDescending(p => p.CreatedDate)
            //    .Take(10)
            //    .ToList();

            // Gửi dữ liệu qua ViewModel
            var vm = new HomeViewModel
            {
                Banners = banners,
                FeaturedProducts = featuredProducts,
                NewProducts = newProducts
            };

            return View(vm);
        }

        public ActionResult ProductDetail(int id)
        {
            var product = db.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
                return HttpNotFound();

            // ✅ Phòng null CategoryId
            var categoryId = product.CategoryId;

            var similarProducts = db.Products
                .Where(p => p.CategoryId == categoryId && p.Id != product.Id)
                .OrderByDescending(p => p.CreatedDate)
                .Take(8)
                .ToList();

            var topDeals = db.Products
                .OrderByDescending(p => (p.SoldCount ?? 0))
                .Take(9)
                .ToList();

            ViewBag.SimilarProducts = similarProducts;
            ViewBag.TopDeals = topDeals;

            return View(product);
        }
        // GET: Products/GetSearchSuggestions
        [HttpGet]
        public JsonResult GetSearchSuggestions(string query)
        {
            if (string.IsNullOrEmpty(query) || query.Length < 2)
            {
                return Json(new List<string>());
            }

            var suggestions = db.Products
                .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
                .Select(p => p.Name)
                .Distinct()
                .Take(8)
                .ToList();

            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }



    }
}