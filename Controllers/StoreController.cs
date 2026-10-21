using System;
using System.Linq;
using DBSportStore.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace DBSportStore.Controllers
{
    public class StoreController : Controller
    {
        private DBSportStoreEntities db = new DBSportStoreEntities();

        // URL: /Store/Index
        // Gộp đủ B06: danh mục động + lọc Category + tìm kiếm + lọc giá + phân trang
        // (+ Bài tập 1: TotalItems, Bài tập 2: sortOrder)
        public ActionResult Index(
            int? categoryId,
            string keyword,
            decimal? minPrice,
            decimal? maxPrice,
            string sortOrder,
            int page = 1)
        {
            // Số sản phẩm mỗi trang.
            const int pageSize = 6;

            // Không cho page nhỏ hơn 1.
            if (page < 1)
            {
                page = 1;
            }

            // Bắt đầu từ các sản phẩm đang bán.
            var products = db.Product
                .Where(p => p.IsActive);

            // Lọc theo Category.
            if (categoryId.HasValue)
            {
                products = products.Where(
                    p => p.CategoryID == categoryId.Value);
            }

            // Tìm kiếm theo tên hoặc mô tả.
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                products = products.Where(
                    p => p.ProductName.Contains(keyword)
                      || p.Description.Contains(keyword));
            }

            // Lọc cận dưới của giá.
            if (minPrice.HasValue)
            {
                products = products.Where(
                    p => p.Price >= minPrice.Value);
            }

            // Lọc cận trên của giá.
            if (maxPrice.HasValue)
            {
                products = products.Where(
                    p => p.Price <= maxPrice.Value);
            }

            // Đếm SAU khi đã áp dụng Category + Search + Price Filter.
            int totalItems = products.Count();

            // Tính số trang.
            int totalPages =
                (int)Math.Ceiling((double)totalItems / pageSize);

            if (totalPages == 0)
            {
                totalPages = 1;
            }

            // Nếu page quá lớn thì đưa về trang cuối.
            if (page > totalPages)
            {
                page = totalPages;
            }

            // Bài tập 2: sắp xếp (phải đặt TRƯỚC Skip/Take).
            IQueryable<Product> sorted;
            switch (sortOrder)
            {
                case "price_asc":
                    sorted = products
                        .OrderBy(p => p.Price)
                        .ThenByDescending(p => p.ProductID);
                    break;
                case "price_desc":
                    sorted = products
                        .OrderByDescending(p => p.Price)
                        .ThenByDescending(p => p.ProductID);
                    break;
                default:
                    sorted = products
                        .OrderByDescending(p => p.ProductID);
                    break;
            }

            var model = new StoreHomeViewModel
            {
                Categories = db.Category
                    .OrderBy(c => c.CategoryName)
                    .ToList(),

                Products = sorted
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProductCardViewModel
                    {
                        ProductID = p.ProductID,
                        NamePro = p.ProductName,
                        CategoryName = p.Category.CategoryName,
                        Price = p.Price,
                        ImagePro = p.ImageUrl
                    })
                    .ToList(),

                // Giữ lại toàn bộ điều kiện hiện tại để View hiển thị lại.
                SelectedCategoryId = categoryId,
                SearchTerm = keyword,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sortOrder,

                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems
            };

            return View(model);
        }

        // Bài 4: URL /Store/AboutStore
        public ActionResult AboutStore()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
