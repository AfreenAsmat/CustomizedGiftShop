using CustomizedGiftShop.Data;
using CustomizedGiftShop.Models;
using CustomizedGiftShop.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CustomizedGiftShop.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize(Roles = "Admin, Staff")]
        public IActionResult Index()
        {
            var orders = _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ToList();
            return View(orders);
        }

        public IActionResult Insert()
        {
            ViewBag.Customers = new SelectList(_db.Customers, "CustomerId", "Name");
            ViewBag.Products = new SelectList(_db.Products, "ProductId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Insert(OrderInsertDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var order = new Order
            {
                CustomerId = model.CustomerId,
                OrderType = model.OrderType,
                DeliveryAddress = model.DeliveryAddress,
                Status = "Pending",
                TotalPrice = 0
            };

            _db.Orders.Add(order);
            _db.SaveChanges(); // Save to get OrderId

            decimal totalPrice = 0;

            foreach (var item in model.Items)
            {
                var product = _db.Products.Find(item.ProductId);
                if (product == null)
                {
                    ModelState.AddModelError("", "Product not found.");
                    return View(model);
                }

                if (item.Quantity <= 0 || item.Quantity > product.Stock)
                {
                    ModelState.AddModelError("", $"Invalid quantity for {product.Name}");
                    return View(model);
                }

                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                };

                _db.OrderDetails.Add(orderDetail);

                product.Stock -= item.Quantity;
                _db.Products.Update(product);

                totalPrice += item.Quantity * product.Price;
            }

            order.TotalPrice = totalPrice;
            _db.Orders.Update(order);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var order = _db.Orders.Find(id);
            if (order == null || order.Status != "Pending")
                return RedirectToAction("Index");

            var dto = new OrderEditDto
            {
                OrderId = order.OrderId,
                OrderType = order.OrderType,
                DeliveryAddress = order.DeliveryAddress,
                Status = order.Status
            };

            return View(dto); 
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(OrderEditDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var order = _db.Orders.Find(model.OrderId);
            if (order == null || order.Status != "Pending")
                return RedirectToAction("Index");

            if (model.OrderType == "Delivery" &&
                string.IsNullOrWhiteSpace(model.DeliveryAddress))
            {
                ModelState.AddModelError("DeliveryAddress",
                    "Delivery address is required for delivery orders.");
                return View(model);
            }

            order.OrderType = model.OrderType;
            order.DeliveryAddress = model.DeliveryAddress;
            order.Status = model.Status;

            _db.SaveChanges();

            return RedirectToAction("Index");
        }


        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var order = _db.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var order = _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null) return NotFound();

            foreach (var od in order.OrderDetails)
            {
                var product = _db.Products.Find(od.ProductId);
                if (product != null)
                {
                    product.Stock += od.Quantity;
                    _db.Products.Update(product);
                }
            }

            _db.OrderDetails.RemoveRange(order.OrderDetails);
            _db.Orders.Remove(order);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Details(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var order = _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult SalesReport()
        {
            var orders = _db.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.Customer)
                .Where(o => o.Status == "Completed")
                .ToList();

            decimal totalRevenue = orders.Sum(o => o.TotalPrice);
            int totalOrders = orders.Count;
            int totalItemsSold = orders.Sum(o => o.OrderDetails.Sum(od => od.Quantity));

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalItemsSold = totalItemsSold;

            return View(orders);
        }

        [Authorize(Roles = "Staff")]
        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            var order = _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null) return NotFound();

            if (status == "Cancelled" && order.Status == "Pending")
            {
                foreach (var od in order.OrderDetails)
                {
                    var product = _db.Products.Find(od.ProductId);
                    if (product != null)
                    {
                        product.Stock += od.Quantity;
                        _db.Products.Update(product);
                    }
                }
            }

            order.Status = status;
            _db.Orders.Update(order);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
