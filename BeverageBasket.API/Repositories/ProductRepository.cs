using BeverageBasket.API.DbContexts;
using BeverageBasket.API.Entities;
using BeverageBasket.API.Models;
using BeverageBasket.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BeverageBasket.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductContext _context;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(ProductContext context, ILogger<ProductRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (!_context.Products.Any())
            {
                var products = new HashSet<Product>()
                {
                    new Product("AA1ITC", "Italian Coffee", 1.10m, 10),
                    new Product("AA2AMC", "American Coffee", 2.20m, 15),
                    new Product("BB2TEA", "Tea", 3.00m, 1),
                    new Product("C1CCHC", "Chocolate", 3.50m, 17)
                };

                _context.Products.AddRange(products);
                _context.SaveChanges();
            }
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(string productId)
        {
            return await _context.Products.FirstOrDefaultAsync(product => product.Id == productId);
        }

        public async Task DeleteProductsAsync(List<BasketItem> basketItems)
        {
            foreach (BasketItem item in basketItems)
            {
                var product = await _context.Products.FirstOrDefaultAsync(product => product.Id == item.ProductId);
                if (product != null)
                {
                    product.Quantity -= item.Quantity;
                }
                else
                {
                    _logger.LogError($"Attempt to delete product {item.ProductId} failed, product not found.");
                }
            }
            await _context.SaveChangesAsync();
            return;
        }
    }
}
