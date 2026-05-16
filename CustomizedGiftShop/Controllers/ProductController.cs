using CustomizedGiftShop.Data;
using CustomizedGiftShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace CustomizedGiftShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize(Roles = "Admin, Staff")]
        public IActionResult Index()
        {
            IEnumerable<Product> products = _db.Products;
            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Insert()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Insert (Product product, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/products"
                    );

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fileName = Guid.NewGuid().ToString()
                    + Path.GetExtension(imageFile.FileName);

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                product.ImageUrl = "/images/products/" + fileName;
            }
            if (ModelState.IsValid)
            {
                _db.Products.Add(product);
                _db.SaveChanges();
                ViewData["Message"] = "Your record saved successfully!";
                return RedirectToAction("Index");
            }
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var product = _db.Products.Find(id);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product, IFormFile imageFile)
        {
            var existingProduct = _db.Products.Find(product.ProductId);
            if (existingProduct == null)
                return NotFound();

            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/products"
                    );

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid().ToString()
                    + Path.GetExtension(imageFile.FileName);

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                existingProduct.ImageUrl = "/images/products/" + fileName;
            }

            // 🔹 Update other fields
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Category = product.Category;
            existingProduct.Stock = product.Stock;
            existingProduct.Description = product.Description;

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null) return NotFound();

            _db.Products.Remove(product);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
