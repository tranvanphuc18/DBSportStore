using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using DBSportStore.Models;

namespace DBSportStore.Controllers
{
    public class CategoriesController : Controller
    {
        private DBSportStoreEntities db = new DBSportStoreEntities();

        // Kiểm tra tên danh mục: bắt buộc, tối đa 100 ký tự, không trùng (cột UNIQUE)
        private void CheckCategory(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "Vui lòng nhập tên danh mục.");
                return;
            }

            category.CategoryName = category.CategoryName.Trim();

            if (category.CategoryName.Length > 100)
            {
                ModelState.AddModelError("CategoryName", "Tên danh mục tối đa 100 ký tự.");
            }

            bool duplicated = db.Category.Any(c =>
                c.CategoryName == category.CategoryName &&
                c.CategoryID != category.CategoryID);
            if (duplicated)
            {
                ModelState.AddModelError("CategoryName", "Tên danh mục đã tồn tại.");
            }
        }

        // Đếm số sản phẩm của một danh mục
        private int CountProducts(int categoryId)
        {
            return db.Product.Count(p => p.CategoryID == categoryId);
        }

        // ---------------- INDEX ----------------
        public ActionResult Index()
        {
            // Dictionary: CategoryID -> số sản phẩm, để View hiển thị cột "Số sản phẩm"
            Dictionary<int, int> counts = db.Product
                .GroupBy(p => p.CategoryID)
                .Select(g => new { g.Key, Total = g.Count() })
                .ToDictionary(x => x.Key, x => x.Total);

            ViewBag.ProductCounts = counts;
            return View(db.Category.OrderBy(c => c.CategoryID).ToList());
        }

        // ---------------- DETAILS ----------------
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Category.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProductCount = CountProducts(category.CategoryID);
            return View(category);
        }

        // ---------------- CREATE ----------------
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryName")] Category category)
        {
            CheckCategory(category);

            if (ModelState.IsValid)
            {
                db.Category.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // ---------------- EDIT ----------------
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Category.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            CheckCategory(category);

            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // ---------------- DELETE ----------------
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Category.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProductCount = CountProducts(category.CategoryID);
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Category.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            int productCount = CountProducts(id);
            if (productCount > 0)
            {
                ViewBag.ProductCount = productCount;
                ModelState.AddModelError("",
                    "Không thể xóa vì danh mục này đang có " + productCount + " sản phẩm.");
                return View("Delete", category);
            }

            try
            {
                db.Category.Remove(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (DbUpdateException)
            {
                db.Entry(category).State = EntityState.Unchanged;
                ViewBag.ProductCount = CountProducts(id);
                ModelState.AddModelError("", "Không thể xóa danh mục này vì đang được sử dụng.");
                return View("Delete", category);
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
