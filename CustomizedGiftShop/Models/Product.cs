using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CustomizedGiftShop.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Precision(18, 2)]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public int Stock { get; set; }

        public string? ImageUrl { get; set; }

        // Navigation property
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
