using DBSportStore.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace DBSportStore.Controllers
{
    public class StoreController : Controller
    {
        // URL: /Store/Index
        public ActionResult Index()
        {
            var products = new List<ProductCardViewModel>
            {
                new ProductCardViewModel { ProductID = 1, NamePro = "Nike Air Zoom", CategoryName = "Giày thể thao", Price = 1290000, ImagePro = "shoe-01.jpg", ShortDescription = "Giày chạy bộ êm ái, đế đàn hồi tốt." },
                new ProductCardViewModel { ProductID = 2, NamePro = "Adidas Training Tee", CategoryName = "Áo quần thi đấu", Price = 590000, ImagePro = "shirt-01.jpg", ShortDescription = "Áo thun cotton thoáng mát, thấm hút mồ hôi." },
                new ProductCardViewModel { ProductID = 3, NamePro = "Gym Performance Set", CategoryName = "Dụng cụ thể thao", Price = 890000, ImagePro = "gym-01.jpg", ShortDescription = "Bộ tạ tay và dụng cụ tập gym tại nhà." },
                new ProductCardViewModel { ProductID = 4, NamePro = "Running Shoes Pro", CategoryName = "Giày thể thao", Price = 1490000, ImagePro = "shoe-02.jpg", ShortDescription = "Giày knit nhẹ, ôm chân, chạy đường dài." },

                // Bài 1: 2 sản phẩm mới
                new ProductCardViewModel { ProductID = 5, NamePro = "Bóng Tập Yoga 65cm", CategoryName = "Dụng cụ thể thao", Price = 350000, ImagePro = "ball-01.jpg", ShortDescription = "Bóng chống nổ, hỗ trợ tập core và thăng bằng." },
                new ProductCardViewModel { ProductID = 6, NamePro = "Bình Nước Thể Thao 750ml", CategoryName = "Phụ kiện thể thao", Price = 180000, ImagePro = "bottle-01.jpg", ShortDescription = "Nhựa không BPA, nắp bật tiện lợi." },

                // Bài 3: sản phẩm thuộc danh mục Phụ kiện thể thao
                new ProductCardViewModel { ProductID = 7, NamePro = "Dây Kháng Lực Mini Band", CategoryName = "Phụ kiện thể thao", Price = 120000, ImagePro = "band-01.jpg", ShortDescription = "Bộ 3 dây kháng lực, mang theo mọi nơi." }
            };

            return View(products); // gửi danh sách sang Views/Store/Index.cshtml
        }

        // Bài 4: URL /Store/AboutStore
        public ActionResult AboutStore()
        {
            return View();
        }
    }
}