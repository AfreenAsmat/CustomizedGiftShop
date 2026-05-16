using System.Collections.Generic;

namespace CustomizedGiftShop.Models.DTO
{
    public class POSOrderDto
    {
        public List<POSItem> Items { get; set; } = new(); // initialize to avoid null
    }

    public class POSItem
    {
        public int id { get; set; }
        public int qty { get; set; }
    }
}
