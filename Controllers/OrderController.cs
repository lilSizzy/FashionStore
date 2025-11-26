using FashionStore.Models;
using FashionStore.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;
using PagedList;


namespace FashionStore.Controllers
{
    public class OrderController : Controller
    {
        private readonly FashionStoreWebEntities db = new FashionStoreWebEntities();

        // GET: Order/Checkout
        public ActionResult Checkout()
        {
            var cart = Session["Cart"] as CartModel;
            if (cart == null || !cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống!";
                return RedirectToAction("Index", "Cart");
            }

            if (!User.Identity.IsAuthenticated)
            {
                TempData["WarningMessage"] = "Vui lòng đăng nhập để đặt hàng!";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Order") });
            }

            var model = new OrderModel
            {
                CartItems = cart.Items,
                GrandTotal = cart.GrandTotal,
                CustomerName = User.Identity.Name
            };

            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(OrderModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var cart = Session["Cart"] as CartModel;
                    if (cart != null)
                    {
                        model.CartItems = cart.Items;
                        model.GrandTotal = cart.GrandTotal;
                    }
                    return View(model);
                }

                // SỬA LỖI: Thêm dòng khai báo cartCheck
                var cartCheck = Session["Cart"] as CartModel;

                if (cartCheck == null || !cartCheck.Items.Any())
                {
                    TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
                    return RedirectToAction("Index", "Cart");
                }

                // Lưu đơn hàng
                var orderId = SaveOrder(model, cartCheck,db);

                if (model.PaymentMethod == "PayPal")
                {
                    TempData["InfoMessage"] = "Tính năng PayPal đang được phát triển";
                }

                // Xóa giỏ hàng
                Session["Cart"] = null;
                Session["CartCount"] = 0;

                TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng: #{orderId}";
                return RedirectToAction("OrderSuccess", new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi đặt hàng: " + ex.Message;

                var cart = Session["Cart"] as CartModel;
                if (cart != null)
                {
                    model.CartItems = cart.Items;
                    model.GrandTotal = cart.GrandTotal;
                }

                return View(model);
            }
        }

        // Thay đổi chữ ký phương thức
        private int SaveOrder(OrderModel model, CartModel cart, FashionStoreWebEntities db)
        {
            var currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            // BẮT BUỘC phải có user, nếu không thì ném lỗi
            if (currentUser == null)
                throw new Exception("Không tìm thấy tài khoản người dùng. Vui lòng đăng nhập lại!");
            int? userId = currentUser?.Id;

            var today = DateTime.Today.ToString("yyyyMMdd");
            var countToday = db.Orders.Count(o => o.CreatedDate >= DateTime.Today);
            var orderCode = $"DH{today}{(countToday + 1):D4}";

            var order = new Order
            {
                OrderCode = orderCode,
                UserId = currentUser.Id,
                CustomerName = model.CustomerName,
                Phone = model.Phone,
                Address = model.Address,
                TotalAmount = cart.GrandTotal,
                PaymentMethod = model.PaymentMethod,
                Note = model.Note,
                Status = "Pending",
                CreatedDate = DateTime.Now,
                OrderDate = DateTime.Now
            };

            db.Orders.Add(order);
            db.SaveChanges(); // lúc này order.Id đã có giá trị

            foreach (var item in cart.Items)
            {
                var detail = new OrderDetail
                {
                    OrderId = (int)order.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    //ProductImage = item.Image, // nếu có
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                db.OrderDetails.Add(detail);
            }

            db.SaveChanges();
            return (int)order.Id;
        }

        // GET: Order/OrderSuccess/5
        public ActionResult OrderSuccess(int ? id)
        {
            using (var db = new FashionStoreWebEntities())
            {
                var order = db.Orders.Find(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                    return RedirectToAction("Index", "Home");
                }

                return View(order);
            }
        }

        // GET: Order/MyOrder
        public ActionResult MyOrder(int ? page)
        {
            var userName = User.Identity.Name;
            using (var db = new FashionStoreWebEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.Username == userName);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản!";
                    return RedirectToAction("Index", "Home");
                }

                int pageSize = 10;
                int pageNumber = (page ?? 1);

                var orders = db.Orders
                    .Where(o => o.UserId == user.Id)
                    .OrderByDescending(o => o.CreatedDate)
                    .Select(o => new OrderHistoryViewModel
                    {
                        Id = (int)o.Id,
                        OrderCode = o.OrderCode,
                        CreatedDate = o.CreatedDate,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        PaymentMethod = o.PaymentMethod,
                        ItemCount = o.OrderDetails.Sum(od => od.Quantity)
                    })
                    .ToList()
                    .ToPagedList(pageNumber, pageSize);

                return View(orders);
            }
        }

        // GET: Order/OrderDetail/5
        public ActionResult OrderDetail(int id)
        {
            using (var db = new FashionStoreWebEntities())
            {
                var currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem chi tiết đơn hàng!";
                    return RedirectToAction("Login", "Account");
                }

                var order = db.Orders
                              .Include("OrderDetails")  // Rất quan trọng: load chi tiết sản phẩm
                              .FirstOrDefault(o => o.Id == id && o.UserId == currentUser.Id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem!";
                    return RedirectToAction("MyOrder");
                }

                return View(order);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}