namespace DBSportStore.Models
{
    public class ProductCardViewModel
    {
        public int ProductID { get; set; }          // mã sản phẩm
        public string NamePro { get; set; }         // tên sản phẩm
        public string CategoryName { get; set; }    // tên danh mục
        public decimal Price { get; set; }          // đơn giá
        public string ImagePro { get; set; }        // tên file ảnh trong Content/images
        public string ShortDescription { get; set; } // Bài 2: mô tả ngắn
    }
}