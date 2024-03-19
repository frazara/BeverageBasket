using BeverageBasket.API.Entities;
using BeverageBasket.API.Enum;
using BeverageBasket.API.Repositories.Interfaces;
using BeverageBasket.API.Services.Interfaces;
using System.Transactions;

namespace BeverageBasket.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ILogger<PaymentService> _logger;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IBasketService _basketService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const decimal cashPaymentThreshold = 10;

        public PaymentService(ILogger<PaymentService> logger, IProductRepository productRepository, IOrderRepository orderRepository, IBasketService basketService, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<List<PaymentMethodEnum>?> CheckoutAsync()
        {
            if (!await IsBasketValidAsync())
            {
                return null;
            }

            var availablePaymentMethods = new List<PaymentMethodEnum> { PaymentMethodEnum.Card };
            var totalPrice = await GetTotalPriceAsync();
            Guid basketId = _basketService.GetBasketId();

            if (totalPrice < cashPaymentThreshold)
            {
                availablePaymentMethods.Add(PaymentMethodEnum.Cash);
            }

            var order = await _orderRepository.GetOrderByBasketIdAsync(basketId);
            if (order == null)
            {
                order = new Order()
                {
                    BasketId = basketId,
                    OrderDate = DateTime.UtcNow,
                    HasBeenCompleted = false,
                    TotalPrice = totalPrice
                };
                await _orderRepository.AddAsync(order);
            }
            else
            {
                await _orderRepository.UpdateAsync(order.Id, totalPrice);
            }

            return availablePaymentMethods;
        }

        public async Task<bool> PayAsync()
        {
            await CheckoutAsync();

            Guid basketId = _basketService.GetBasketId();
            var order = await _orderRepository.GetOrderByBasketIdAsync(basketId);
            var basketItems = _basketService.GetBasketItems();

            if (basketItems == null || basketItems.Count == 0 || order == null || order.HasBeenCompleted)
            {
                return false;
            }

            using (var scope = new TransactionScope())
            {
                try
                {
                    if (!await IsBasketValidAsync())
                    {
                        return false;
                    }

                    if (order.PaymentMethod == PaymentMethodEnum.Card)
                    {
                        // call external payment API
                    }

                    await _productRepository.DeleteProductsAsync(basketItems);
                    await _orderRepository.SoftDeleteAsync(order.Id);
                    _basketService.RemoveAllItems();
                    _httpContextAccessor?.HttpContext?.Session.Clear();

                    scope.Complete();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Payment not completed: error {ex.Message}.");
                    throw new Exception($"Payment not completed: error {ex.Message}.");
                }
            }

            return true;
        }

        public async Task<Order?> GetOrderByBasketIdAsync()
        {
            Guid basketId = _basketService.GetBasketId();
            return await _orderRepository.GetOrderByBasketIdAsync(basketId);
        }

        public async Task<bool> IsBasketValidAsync()
        {
            var basketItems = _basketService.GetBasketItems();

            foreach (var item in basketItems)
            {
                var products = await _productRepository.GetProductsAsync();
                if (!products.Any(product => product.Id == item.ProductId && item.Quantity <= product.Quantity))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<decimal> GetTotalPriceAsync()
        {
            var basketItems = _basketService.GetBasketItems();

            decimal totalPrice = 0;
            foreach (var item in basketItems)
            {
                var product = await _productRepository.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    totalPrice += item.Quantity * product.Price;
                }
            }
            return totalPrice;
        }
    }
}
