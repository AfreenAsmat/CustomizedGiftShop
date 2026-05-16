using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace CustomizedGiftShop.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        [Required]
        public string OrderType { get; set; } = string.Empty;

        public string? DeliveryAddress { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;


        // Navigation property
        public List<OrderDetail> OrderDetails { get; set; } = new();
    }
}
