using BeverageBasket.API.Models;

namespace BeverageBasket.API.Services.Interfaces
{
    public interface IBasketService
    {
        public List<BasketItem> GetBasketItems();

        public Task<bool> AddItemsAsync(string productId, int quantity);

        public Task<bool> RemoveItemsAsync(string productId, int quantity);

        public bool RemoveAllItems();

        public Guid GetBasketId();
    }
}
