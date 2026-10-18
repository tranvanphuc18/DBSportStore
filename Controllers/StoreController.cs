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
        public ActionResult Index()
        {
            var products = db.Product
        .Where(p => p.IsActive)
        .Select(p => new ProductCardViewModel
        {
            ProductID = p.ProductID,
            NamePro = p.ProductName,
            CategoryName = p.Category.CategoryName,
            Price = p.Price,
            ImagePro = p.ImageUrl
        })
        .ToList();

            return View(products);
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