using CustomizedGiftShop.Models.DTO;

namespace CustomizedGiftShop.Models.DTO
{
    public class OrderInsertDto
    {
        public int CustomerId { get; set; }
        public string OrderType { get; set; } = string.Empty;
        public string? DeliveryAddress { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
