using BeverageBasket.API.Entities;
using BeverageBasket.API.Models;

namespace BeverageBasket.API.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetProductsAsync();

        Task<Product?> GetProductByIdAsync(string productId);

        Task DeleteProductsAsync(List<BasketItem> basketItems);
    }
}
