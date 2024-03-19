using BeverageBasket.API.Entities;

namespace BeverageBasket.API.Models
{
    public class BasketItem
    {
        public Guid ItemId { get; set; }
        
        public Guid BasketId { get; set; }

        public string ProductId { get; set; }

        public int Quantity { get; set; }

        public DateTime LastUpdate { get; set; }

        public BasketItem(Guid itemId, Guid basketId, string productId, int quantity, DateTime lastUpdate)
        {
            ItemId = itemId;
            BasketId = basketId;
            ProductId = productId;
            Quantity = quantity;
            LastUpdate = lastUpdate;
        }
    }
}
