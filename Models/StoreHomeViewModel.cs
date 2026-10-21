using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBSportStore.Models
{
    public class StoreHomeViewModel
    {
        // ===== Chức năng 1: danh mục động =====
        // Danh sách danh mục lấy trực tiếp từ bảng Category.
        public List<Category> Categories { get; set; }

        // Danh sách sản phẩm của trang hiện tại.
        public List<ProductCardViewModel> Products { get; set; }

        // ===== Chức năng 2: lọc theo danh mục =====
        // int? cho phép null = không lọc danh mục (Tất cả sản phẩm).
        public int? SelectedCategoryId { get; set; }

        // ===== Chức năng 3: tìm kiếm =====
        public string SearchTerm { get; set; }

        // ===== Chức năng 4: phân trang =====
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // ===== Chức năng 5: lọc khoảng giá =====
        // decimal? vì người dùng có thể để trống ô giá.
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int TotalItems { get; set; }

        // "price_asc" = thấp → cao, "price_desc" = cao → thấp, null = mặc định (mới nhất)
        public string SortOrder { get; set; }
    }
}
