using BeverageBasket.API.Models;
using BeverageBasket.API.Repositories.Interfaces;
using BeverageBasket.API.Services.Interfaces;
using Newtonsoft.Json;

namespace BeverageBasket.API.Services
{
    public class BasketService : IBasketService
    {
        private readonly ILogger<BasketService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductRepository _productRepository;
        private const string BasketSessionKey = "BasketId";

        public BasketService(ILogger<BasketService> logger, IHttpContextAccessor httpContextAccessor, IProductRepository productRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public List<BasketItem> GetBasketItems()
        {
            Guid basketId = GetBasketId();
            var basketItemsJson = _httpContextAccessor?.HttpContext?.Session.GetString(basketId.ToString());
            
            if (string.IsNullOrEmpty(basketItemsJson))
            {
                return new List<BasketItem>();
            }

            try
            {
                var basketItemDeserialized = JsonConvert.DeserializeObject<List<BasketItem>>(basketItemsJson);
                return basketItemDeserialized ?? new List<BasketItem>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Couldn't parse basket {basketId}");
                return new List<BasketItem>();
            }
        }

        public async Task<bool> AddItemsAsync(string productId, int quantity)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);

            if (product == null)
            {
                _logger.LogError($"Product with id {productId} wasn't found.");
                return false;
            }

            Guid basketId = GetBasketId();
            var basketItems = GetBasketItems();
            var basketItem = basketItems.FirstOrDefault(item => item.ProductId == productId);

            var currentQuantity = basketItem?.Quantity ?? 0;
            if (currentQuantity + quantity > product.Quantity)
            {
                _logger.LogError($"Quantity {currentQuantity} not available for product {productId}. Tot remaining items: {product.Quantity}");
                return false;
            }

            if (basketItem == null)
            {
                basketItem = new BasketItem(Guid.NewGuid(), basketId, productId, quantity, DateTime.UtcNow);
                basketItems.Add(basketItem);
            }
            else
            {
                basketItem.Quantity += quantity;
                basketItem.LastUpdate = DateTime.UtcNow;
            }

            _httpContextAccessor?.HttpContext?.Session.SetString(basketId.ToString(), JsonConvert.SerializeObject(basketItems));
            return true;
        }

        public async Task<bool> RemoveItemsAsync(string productId, int quantity)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);

            if (product == null)
            {
                _logger.LogError($"Product with id {productId} wasn't found.");
                return false;
            }

            Guid basketId = GetBasketId();
            var basketItems = GetBasketItems();
            var basketItem = basketItems.FirstOrDefault(item => item.ProductId == productId);

            if (basketItem == null)
            {
                _logger.LogError($"Product with id {productId} wasn't associated to basket {basketId}.");
                return false;
            }

            basketItem.Quantity -= quantity;

            if (basketItem.Quantity <= 0)
            {
                basketItems.Remove(basketItem);
            }

            _httpContextAccessor?.HttpContext?.Session.SetString(basketId.ToString(), JsonConvert.SerializeObject(basketItems));
            return true;
        }

        public bool RemoveAllItems()
        {
            Guid basketId = GetBasketId();
            _httpContextAccessor?.HttpContext?.Session.Remove(basketId.ToString());
            return true;
        }

        public Guid GetBasketId()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null || context.Session == null)
            {
                return Guid.NewGuid();
            }

            if (context.Session.GetString(BasketSessionKey) == null)
            {
                if (!string.IsNullOrWhiteSpace(context.User?.Identity?.Name))
                {
                    context.Session.SetString(BasketSessionKey, context.User.Identity.Name);
                }
                else
                {
                    Guid tempBasketId = Guid.NewGuid();
                    context.Session.SetString(BasketSessionKey, tempBasketId.ToString());
                }
            }

            return Guid.Parse(context.Session.GetString(BasketSessionKey) ?? string.Empty);
        }
    }
}
