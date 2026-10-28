using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DBSportStore.Models;

namespace DBSportStore.Controllers
{
    public class ShoppingCartController : Controller
    {
        // Kết nối CSDL để đọc Product, ghi Customer/OrderPro/OrderDetail.
        private DBSportStoreEntities db = new DBSportStoreEntities();

        // Lấy Cart hiện tại từ Session. Chưa có thì tạo mới.
        // Đây là điểm DUY NHẤT quản lý Session["Cart"].
        private Cart GetCart()
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }

        // ==================== CHỨC NĂNG 2: ADD TO CART ====================

        // URL: /ShoppingCart/ShowCart
        public ActionResult ShowCart()
        {
            return View(GetCart());
        }

        // Nhận ProductID từ nút Mua.
        public ActionResult AddToCart(int id)
        {
            // Chỉ cho thêm Product đang tồn tại và đang bán.
            var product = db.Product
                .FirstOrDefault(p => p.ProductID == id && p.IsActive);

            if (product == null)
            {
                TempData["CartError"] =
                    "Sản phẩm không tồn tại hoặc đã ngừng bán.";
                return RedirectToAction("Index", "Store");
            }

            // Bài tập 2: hết hàng thì không cho thêm vào giỏ.
            if (product.StockQuantity <= 0)
            {
                TempData["CartError"] =
                    "Sản phẩm \"" + product.ProductName + "\" đã hết hàng.";
                return RedirectToAction("ShowCart");
            }

            // Cart.Add tự kiểm tra tồn kho.
            bool added = GetCart().Add(product);

            if (!added)
            {
                TempData["CartError"] =
                    "Không thể thêm sản phẩm. "
                    + "Số lượng đã đạt giới hạn tồn kho.";
            }

            // Sau khi thêm, mở giỏ để người dùng thấy kết quả.
            return RedirectToAction("ShowCart");
        }

        // ==================== CHỨC NĂNG 3: UPDATE / REMOVE ====================

        // Cập nhật số lượng - dùng POST vì làm thay đổi dữ liệu giỏ.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            // Đọc lại Product từ CSDL để lấy tồn kho MỚI NHẤT,
            // không chỉ dựa vào tồn kho cũ đã lưu trong Session.
            var product = db.Product.Find(productId);

            if (product == null)
            {
                TempData["CartError"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("ShowCart");
            }

            // Update() kiểm tra: quantity > 0 và không vượt tồn kho.
            bool updated = GetCart().Update(
                productId, quantity, product.StockQuantity);

            if (!updated)
            {
                TempData["CartError"] =
                    "Số lượng phải lớn hơn 0 và không vượt tồn kho.";
            }

            return RedirectToAction("ShowCart");
        }

        // Xóa một sản phẩm khỏi giỏ.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int productId)
        {
            GetCart().Remove(productId);
            return RedirectToAction("ShowCart");
        }

        // Bài tập 1: Xóa toàn bộ giỏ hàng.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearCart()
        {
            GetCart().Clear();
            return RedirectToAction("ShowCart");
        }

        // ==================== CHỨC NĂNG 4: CART TRÊN NAVBAR ====================

        // Chỉ được gọi từ View/Layout, không truy cập trực tiếp bằng URL.
        [ChildActionOnly]
        public PartialViewResult BagCart()
        {
            // Dùng Session trực tiếp (không GetCart) để không tạo Cart mới
            // chỉ vì render navbar.
            var cart = Session["Cart"] as Cart;

            ViewBag.CartQty = cart == null ? 0 : cart.TotalQuantity();
            ViewBag.CartTotal = cart == null ? 0 : cart.TotalMoney();

            return PartialView();
        }

        // ==================== CHỨC NĂNG 5 + 6: CHECKOUT ====================

        // GET: hiển thị form nhập thông tin đặt hàng.
        public ActionResult Checkout()
        {
            var cart = GetCart();

            // Giỏ trống thì quay lại ShowCart.
            if (!cart.Items.Any())
            {
                return RedirectToAction("ShowCart");
            }

            var model = new CheckoutViewModel
            {
                Cart = cart
            };

            return View(model);
        }

        // POST: kiểm tra dữ liệu + lưu Customer -> OrderPro -> OrderDetail.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutViewModel model)
        {
            var cart = GetCart();

            // Cart không được gửi từ form lên nên phải gắn lại
            // để View vẫn hiển thị tóm tắt đơn hàng.
            model.Cart = cart;

            // Không cho đặt hàng nếu giỏ đã trống.
            if (!cart.Items.Any())
            {
                return RedirectToAction("ShowCart");
            }

            // Kiểm tra DataAnnotations trong CheckoutViewModel.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra lại tồn kho ngay trước khi tạo đơn
            // (tồn kho có thể đã đổi sau lúc thêm vào giỏ).
            foreach (var item in cart.Items)
            {
                var product = db.Product.Find(item.ProductID);

                if (product == null
                    || !product.IsActive
                    || product.StockQuantity < item.Quantity)
                {
                    ModelState.AddModelError("",
                        "Sản phẩm " + item.ProductName
                        + " không còn đủ tồn kho.");
                    return View(model);
                }
            }

            // Transaction: Customer + OrderPro + OrderDetail + tồn kho
            // phải cùng thành công, lỗi thì rollback tất cả.
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Customer mới cho khách vãng lai (UserID = NULL).
                    var customer = new Customer
                    {
                        UserID = null,
                        FullName = model.FullName,
                        Phone = model.Phone,
                        Email = model.Email,
                        Address = model.Address
                    };
                    db.Customer.Add(customer);
                    db.SaveChanges(); // EF điền customer.CustomerID

                    // 2. OrderPro (snapshot thông tin giao hàng).
                    var order = new OrderPro
                    {
                        OrderDate = DateTime.Now,
                        CustomerID = customer.CustomerID,
                        RecipientName = model.FullName,
                        ShippingPhone = model.Phone,
                        ShippingAddress = model.ShippingAddress,
                        TotalAmount = cart.TotalMoney(),
                        Status = "Chờ xử lý"
                    };
                    db.OrderPro.Add(order);
                    db.SaveChanges(); // EF điền order.OrderID

                    // 3. OrderDetail cho từng dòng trong giỏ + trừ tồn kho.
                    foreach (var item in cart.Items)
                    {
                        var product = db.Product.Find(item.ProductID);

                        db.OrderDetail.Add(new OrderDetail
                        {
                            OrderID = order.OrderID,
                            ProductID = item.ProductID,
                            Quantity = item.Quantity,
                            // Lưu đơn giá tại thời điểm đặt hàng.
                            UnitPrice = item.UnitPrice
                        });

                        product.StockQuantity -= item.Quantity;
                    }
                    db.SaveChanges();

                    // Mọi bước thành công -> xác nhận.
                    transaction.Commit();

                    // Chỉ xóa giỏ SAU KHI commit thành công.
                    cart.Clear();

                    return RedirectToAction("OrderSuccess",
                        new { id = order.OrderID });
                }
                catch
                {
                    transaction.Rollback();

                    ModelState.AddModelError("",
                        "Có lỗi khi lưu đơn hàng. Vui lòng thử lại.");

                    // Giữ nguyên Cart và dữ liệu người dùng đã nhập.
                    return View(model);
                }
            }
        }

        // ==================== CHỨC NĂNG 7: ORDER SUCCESS ====================

        public ActionResult OrderSuccess(int id)
        {
            // Include để nạp sẵn Customer, OrderDetail và Product của từng dòng.
            var order = db.OrderPro
                .Include(o => o.Customer)
                .Include(o => o.OrderDetail.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
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
