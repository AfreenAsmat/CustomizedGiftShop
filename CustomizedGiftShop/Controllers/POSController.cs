using CustomizedGiftShop.Data;
using CustomizedGiftShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomizedGiftShop.Models.DTO;


namespace CustomizedGiftShop.Controllers
{
    [Authorize(Roles = "Admin, Staff")]
    public class POSController : Controller
    {
        private readonly ApplicationDbContext _db;

        public POSController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var products = _db.Products.ToList();
            return View(products);
        }

        [HttpPost]
        public IActionResult Checkout([FromBody] POSOrderDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return BadRequest("Cart is empty");

            var walkInCustomer = _db.Customers
                .FirstOrDefault(c => c.Name == "Walk-in Customer");

            if (walkInCustomer == null)
                return BadRequest("Walk-in customer not found.");

            // 🔹 Validate stock BEFORE creating order
            foreach (var item in dto.Items)
            {
                var product = _db.Products.Find(item.id);

                if (product == null)
                    return BadRequest("Product not found.");

                if (product.Stock < item.qty)
                    return BadRequest($"Not enough stock for {product.Name}");
            }

            // ✅ Create order
            var order = new Order
            {
                CustomerId = walkInCustomer.CustomerId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                OrderType = "Walk-in",
                TotalPrice = 0
            };

            _db.Orders.Add(order);
            _db.SaveChanges();

            decimal total = 0;

            foreach (var item in dto.Items)
            {
                var product = _db.Products.Find(item.id)!;

                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = product.ProductId,
                    Quantity = item.qty,
                    Price = product.Price
                };

                _db.OrderDetails.Add(orderDetail);

                // 🔻 Reduce stock
                product.Stock -= item.qty;
                _db.Products.Update(product);

                total += product.Price * item.qty;
            }

            order.TotalPrice = total;
            _db.SaveChanges();

            return Json(new { orderId = order.OrderId });
        }




    }
}
