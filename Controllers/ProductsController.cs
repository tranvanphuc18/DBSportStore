using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DBSportStore.Models;

namespace DBSportStore.Controllers
{
    public class ProductsController : Controller
    {
        // Đối tượng kết nối CSDL (do EF sinh ra từ file .edmx)
        private DBSportStoreEntities db = new DBSportStoreEntities();

        // =====================================================
        // HÀM DÙNG CHUNG
        // =====================================================

        // Lưu ảnh vào Content/images, giữ tên gốc.
        // Nếu trùng tên thì thêm -1, -2, ... để không ghi đè.
        // Chỉ trả về TÊN FILE (không trả về đường dẫn).
        private string SaveImage(HttpPostedFileBase imageFile)
        {
            string originalFileName = Path.GetFileName(imageFile.FileName);
            string fileNameWithoutExtension =
                Path.GetFileNameWithoutExtension(originalFileName);
            string extension =
                Path.GetExtension(originalFileName).ToLower();

            string folder = Server.MapPath("~/Content/images");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fileName = fileNameWithoutExtension + extension;
            string path = Path.Combine(folder, fileName);

            int count = 1;
            while (System.IO.File.Exists(path))
            {
                fileName = fileNameWithoutExtension + "-" + count + extension;
                path = Path.Combine(folder, fileName);
                count++;
            }

            imageFile.SaveAs(path);
            return fileName;
        }

        // Kiểm tra file ảnh. Trả về null nếu hợp lệ, ngược lại trả về câu báo lỗi.
        private string CheckImage(HttpPostedFileBase imageFile)
        {
            string extension = Path.GetExtension(imageFile.FileName).ToLower();
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(extension))
            {
                return "Chỉ chấp nhận file JPG, JPEG hoặc PNG.";
            }
            if (imageFile.ContentLength > 2 * 1024 * 1024)
            {
                return "Dung lượng ảnh tối đa là 2 MB.";
            }
            return null;
        }

        // Kiểm tra các trường mà CSDL ràng buộc (NOT NULL, độ dài, CHECK >= 0)
        private void CheckProductFields(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                ModelState.AddModelError("ProductName", "Vui lòng nhập tên sản phẩm.");
            }
            else if (product.ProductName.Length > 150)
            {
                ModelState.AddModelError("ProductName", "Tên sản phẩm tối đa 150 ký tự.");
            }

            if (product.Description != null && product.Description.Length > 500)
            {
                ModelState.AddModelError("Description", "Mô tả tối đa 500 ký tự.");
            }
            if (product.Price < 0)
            {
                ModelState.AddModelError("Price", "Đơn giá không được âm.");
            }
            if (product.StockQuantity < 0)
            {
                ModelState.AddModelError("StockQuantity", "Số lượng tồn kho không được âm.");
            }
        }

        // =====================================================
        // INDEX - Danh sách sản phẩm (trang quản lý)
        // =====================================================
        public ActionResult Index()
        {
            // Include: nạp luôn Category để View hiển thị CategoryName
            var product = db.Product.Include(p => p.Category);
            return View(product.ToList());
        }

        // =====================================================
        // DETAILS - Chi tiết
        // =====================================================
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // =====================================================
        // CREATE
        // =====================================================
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName");
            // Sản phẩm mới mặc định ở trạng thái "Đang bán"
            return View(new Product { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "ProductName,Description,CategoryID,Price,StockQuantity,IsActive")]
            Product product,
            HttpPostedFileBase imageFile)
        {
            CheckProductFields(product);

            bool hasImage = imageFile != null && imageFile.ContentLength > 0;
            if (hasImage)
            {
                string error = CheckImage(imageFile);
                if (error != null)
                {
                    ModelState.AddModelError("ImageUrl", error);
                }
            }

            if (ModelState.IsValid)
            {
                // Chỉ lưu file khi mọi dữ liệu đã hợp lệ
                if (hasImage)
                {
                    product.ImageUrl = SaveImage(imageFile);
                }
                db.Product.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(
                db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // =====================================================
        // EDIT
        // =====================================================
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(
                db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "ProductID,ProductName,Description,CategoryID,Price,StockQuantity,IsActive")]
            Product product,
            HttpPostedFileBase imageFile)
        {
            // Lấy Product cũ để biết ảnh hiện tại.
            // AsNoTracking: chỉ đọc, EF không theo dõi object này
            // (nếu theo dõi sẽ bị xung đột với object product bên dưới).
            var oldProduct = db.Product
                .AsNoTracking()
                .FirstOrDefault(p => p.ProductID == product.ProductID);

            if (oldProduct == null)
            {
                return HttpNotFound();
            }

            // Mặc định giữ ảnh cũ
            product.ImageUrl = oldProduct.ImageUrl;

            CheckProductFields(product);

            bool hasNewImage = imageFile != null && imageFile.ContentLength > 0;
            if (hasNewImage)
            {
                string error = CheckImage(imageFile);
                if (error != null)
                {
                    ModelState.AddModelError("ImageUrl", error);
                }
            }

            if (ModelState.IsValid)
            {
                if (hasNewImage)
                {
                    product.ImageUrl = SaveImage(imageFile);
                }
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(
                db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // =====================================================
        // DELETE
        // =====================================================
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            try
            {
                // Buổi 05: chỉ xóa bản ghi, KHÔNG xóa file ảnh trong Content/images
                db.Product.Remove(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (DbUpdateException)
            {
                // Sản phẩm đã nằm trong OrderDetail (khóa ngoại) nên không xóa được.
                db.Entry(product).State = EntityState.Unchanged;
                ModelState.AddModelError("",
                    "Không thể xóa vì sản phẩm này đã có trong đơn hàng. " +
                    "Hãy vào Sửa và bỏ chọn \"Trạng thái\" để chuyển sang Ngừng bán.");
                return View("Delete", product);
            }
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
