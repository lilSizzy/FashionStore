using FashionStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FashionStore.Controllers
{
    public class CartController : Controller
    {
        private readonly FashionStoreWebEntities db = new FashionStoreWebEntities();

        // Hiển thị giỏ hàng
        public ActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public ActionResult AddItem(int productId, int quantity = 1)
        {
            try
            {
                var product = db.Products.Find(productId);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Sản phẩm không tồn tại!";
                    return RedirectToAction("Index", "Home");
                }

                var cart = GetCart();
                var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    TempData["SuccessMessage"] = $"Đã cập nhật số lượng {product.Name}";
                }
                else
                {
                    cart.Items.Add(new CartItemModel
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = quantity,
                        Image = product.MainImage
                    });
                    TempData["SuccessMessage"] = $"Đã thêm {product.Name} vào giỏ hàng";
                }

                SaveCart(cart);
                return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Index", "Home"));
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm vào giỏ hàng!";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            try
            {
                var cart = GetCart();
                var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

                if (item != null)
                {
                    if (quantity <= 0)
                    {
                        cart.Items.Remove(item);
                        TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng";
                    }
                    else
                    {
                        item.Quantity = quantity;
                        TempData["SuccessMessage"] = "Đã cập nhật số lượng";
                    }
                    SaveCart(cart);
                }

                return RedirectToAction("Index");
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật số lượng!";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult RemoveItem(int productId)
        {
            try
            {
                var cart = GetCart();
                var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);

                if (item != null)
                {
                    cart.Items.Remove(item);
                    SaveCart(cart);
                    TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng";
                }

                return RedirectToAction("Index");
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa sản phẩm!";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult ClearCart()
        {
            try
            {
                var cart = GetCart();
                cart.Items.Clear();
                SaveCart(cart);
                TempData["SuccessMessage"] = "Đã xóa toàn bộ giỏ hàng";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa giỏ hàng!";
                return RedirectToAction("Index");
            }
        }

        private CartModel GetCart()
        {
            var cart = Session["Cart"] as CartModel;
            if (cart == null)
            {
                cart = new CartModel();
                Session["Cart"] = cart;
            }
            return cart;
        }

        private void SaveCart(CartModel cart)
        {
            Session["Cart"] = cart;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}