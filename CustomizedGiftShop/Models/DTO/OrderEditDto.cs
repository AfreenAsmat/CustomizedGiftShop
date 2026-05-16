using CustomizedGiftShop.Models.DTO;

namespace CustomizedGiftShop.Models.DTO
{
    public class OrderEditDto
    {
        public int OrderId { get; set; }

        public string OrderType { get; set; } = string.Empty;

        public string? DeliveryAddress { get; set; }

        public string Status { get; set; } = "Pending";
    }

}
