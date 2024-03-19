using BeverageBasket.API.Entities;
using BeverageBasket.API.Enum;
using BeverageBasket.API.Models;
using BeverageBasket.API.Repositories.Interfaces;
using BeverageBasket.API.Services;
using BeverageBasket.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace BeverageBasket.API.Tests.Services
{
    public class PaymentServiceTest
    {
        private Mock<ILogger<PaymentService>> logger = new Mock<ILogger<PaymentService>>();
        private Mock<IProductRepository> productRepository = new Mock<IProductRepository>();
        private Mock<IOrderRepository> orderRepository = new Mock<IOrderRepository>();
        private Mock<IBasketService> basketService = new Mock<IBasketService>();
        private Mock<IHttpContextAccessor> httpContextAccessor = new Mock<IHttpContextAccessor>();
        
        [Fact]
        public async Task CheckoutAsync_WhenBasketIsInvalid_WrongProductId_ReturnsNull()
        {
            //Arrange
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);
            
            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "ERROR", 1, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 1, DateTime.Now)
            };
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);

            var products = new List<Product>
            {
                new Product("WW1ITC", "Regular Coffee", 1.00m, 8),
                new Product("TZ2AMC", "Chinese Tea", 8.20m, 12),
                new Product("BBTEA2", "Bubble Tea", 3.50m, 7),
                new Product("A1CCHC", "Dark Chocolate", 3.50m, 17)
            };
            productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);

            //Act
            var result = await paymentService.CheckoutAsync();

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CheckoutAsync_WhenBasketIsInvalid_WrongQuantity_ReturnsNull()
        {
            //Arrange
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);

            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "WW1ITC", 100, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 1, DateTime.Now)
            };
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);

            var products = new List<Product>
            {
                new Product("WW1ITC", "Regular Coffee", 1.00m, 8),
                new Product("TZ2AMC", "Chinese Tea", 8.20m, 12),
                new Product("BBTEA2", "Bubble Tea", 3.50m, 7),
                new Product("A1CCHC", "Dark Chocolate", 3.50m, 17)
            };
            productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);

            //Act
            var result = await paymentService.CheckoutAsync();

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CheckoutAsync_WhenBasketIsValid_TotalPriceGreaterThanThreshold_ReturnsOnlyCardPaymentMethods()
        {
            //Arrange
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);

            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "WW1ITC", 3, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 1, DateTime.Now)
            };
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);

            var products = new List<Product>
            {
                new Product("WW1ITC", "Regular Coffee", 1.00m, 8),
                new Product("TZ2AMC", "Chinese Tea", 8.20m, 12),
                new Product("BBTEA2", "Bubble Tea", 3.50m, 7),
                new Product("A1CCHC", "Dark Chocolate", 3.50m, 17)
            };
            productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);
            productRepository.Setup(x => x.GetProductByIdAsync("WW1ITC")).ReturnsAsync(products[0]);
            productRepository.Setup(x => x.GetProductByIdAsync("TZ2AMC")).ReturnsAsync(products[1]);

            //Act
            var result = await paymentService.CheckoutAsync();

            //Assert
            Assert.Equal(1, result.Count);
            Assert.Contains(PaymentMethodEnum.Card, result);
        }

        [Fact]
        public async Task CheckoutAsync_WhenBasketIsValid_TotalPriceLessThanThreshold_ReturnsCardAndCashPaymentMethods()
        {
            //Arrange
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);

            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "BBTEA2", 1, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "A1CCHC", 1, DateTime.Now)
            };
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);

            var products = new List<Product>
            {
                new Product("WW1ITC", "Regular Coffee", 1.00m, 8),
                new Product("TZ2AMC", "Chinese Tea", 8.20m, 12),
                new Product("BBTEA2", "Bubble Tea", 3.50m, 7),
                new Product("A1CCHC", "Dark Chocolate", 3.50m, 17)
            };
            productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);
            productRepository.Setup(x => x.GetProductByIdAsync("BBTEA2")).ReturnsAsync(products[2]);
            productRepository.Setup(x => x.GetProductByIdAsync("A1CCHC")).ReturnsAsync(products[3]);

            //Act
            var result = await paymentService.CheckoutAsync();

            //Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(PaymentMethodEnum.Card, result);
            Assert.Contains(PaymentMethodEnum.Cash, result);
        }

        [Fact]
        public async Task GetTotalPriceAsync_ReturnsCorrectPrice()
        {
            // Arrange
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);
            var expectedResult = 304.5m;
            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "WW1ITC", 3, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 102, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "BBTEA2", 70, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "A1CCHC", 17, DateTime.Now),

            };
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);

            var products = new List<Product>
            {
                new Product("WW1ITC", "Regular Coffee", 1.01m, 8),
                new Product("TZ2AMC", "Chinese Tea", 8.20m, 120),
                new Product("BBTEA2", "Bubble Tea", 3.50m, 70),
                new Product("A1CCHC", "Dark Chocolate", 3.50m, 17)
            };
            productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);
            productRepository.Setup(x => x.GetProductByIdAsync("BBTEA2")).ReturnsAsync(products[2]);
            productRepository.Setup(x => x.GetProductByIdAsync("A1CCHC")).ReturnsAsync(products[3]);

            // Act
            var result = await paymentService.GetTotalPriceAsync();

            // Assert
            Assert.Equal(result, expectedResult);
        }

        [Fact]
        public async Task GetTotalPriceAsync_WhenBasketIsEmpty_ReturnsZeroPrice()
        {
            // Arrange
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);
            var expectedResult = 0m;
            var basketItems = new List<BasketItem> {};
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);

            var products = new List<Product>
            {
                new Product("WW1ITC", "Regular Coffee", 1.01m, 8),
                new Product("TZ2AMC", "Chinese Tea", 8.20m, 120),
                new Product("BBTEA2", "Bubble Tea", 3.50m, 70),
                new Product("A1CCHC", "Dark Chocolate", 3.50m, 17)
            };
            productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);
            productRepository.Setup(x => x.GetProductByIdAsync("BBTEA2")).ReturnsAsync(products[2]);
            productRepository.Setup(x => x.GetProductByIdAsync("A1CCHC")).ReturnsAsync(products[3]);

            // Act
            var result = await paymentService.GetTotalPriceAsync();

            // Assert
            Assert.Equal(result, expectedResult);
        }

        [Fact]
        public async Task PayAsync_WhenBasketIsEmpty_ReturnsFalse()
        {
            // Arrange
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);
            var basketItems = new List<BasketItem> { };
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);

            // Act
            var result = await paymentService.PayAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PayAsync_WhenBasketIsInvalid_ReturnsFalse()
        {
            // Arrange
            var logger = new Mock<ILogger<PaymentService>>();
            var productRepository = new Mock<IProductRepository>();
            var orderRepository = new Mock<IOrderRepository>();
            var basketService = new Mock<IBasketService>();
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);
            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "WW1ITC", 10, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 1, DateTime.Now)
            };
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);

            var products = new List<Product>
            {
                new Product("WW1ITC", "Regular Coffee", 1.00m, 8),
                new Product("TZ2AMC", "Chinese Tea", 8.20m, 12),
                new Product("BBTEA2", "Bubble Tea", 3.50m, 7),
                new Product("A1CCHC", "Dark Chocolate", 3.50m, 17)
            };
            productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);

            var order = new Order
            {
                BasketId = Guid.NewGuid(),
                OrderDate = DateTime.Now,
                HasBeenCompleted = false,
                TotalPrice = 304.5m
            };
            orderRepository.Setup(x => x.GetOrderByBasketIdAsync(It.IsAny<Guid>())).ReturnsAsync(order);

            // Act
            var result = await paymentService.PayAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PayAsync_WhenOrderIsCompleted_ReturnsFalse()
        {
            // Arrange
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);
            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "WW1ITC", 3, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 1, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "BBTEA2", 7, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "A1CCHC", 1, DateTime.Now),
            };
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);
            var order = new Order
            {
                BasketId = Guid.NewGuid(),
                OrderDate = DateTime.Now,
                HasBeenCompleted = true,
                TotalPrice = 304.5m
            };
            orderRepository.Setup(x => x.GetOrderByBasketIdAsync(It.IsAny<Guid>())).ReturnsAsync(order);

            // Act
            var result = await paymentService.PayAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PayAsync_WhenOrderIsNotCompleted_ReturnsTrue()
        {
            // Arrange
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);
            
            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "WW1ITC", 3, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 1, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "BBTEA2", 7, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "A1CCHC", 1, DateTime.Now),
            };
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);

            var products = new List<Product>
            {
                new Product("WW1ITC", "Regular Coffee", 1.00m, 8),
                new Product("TZ2AMC", "Chinese Tea", 8.20m, 12),
                new Product("BBTEA2", "Bubble Tea", 3.50m, 7),
                new Product("A1CCHC", "Dark Chocolate", 3.50m, 17)
            };
            productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);

            var order = new Order
            {
                BasketId = Guid.NewGuid(),
                OrderDate = DateTime.Now,
                HasBeenCompleted = false,
                TotalPrice = 304.5m
            };
            orderRepository.Setup(x => x.GetOrderByBasketIdAsync(It.IsAny<Guid>())).ReturnsAsync(order);

            // Act
            var result = await paymentService.PayAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task PayAsync_WhenOrderIsSuccessful_RemoveProductsFromRepositorySoftDeleteOrderEmptyBasket()
        {
            // Arrange
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);

            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "WW1ITC", 3, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 1, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "BBTEA2", 7, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "A1CCHC", 1, DateTime.Now),
            };
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);

            var products = new List<Product>
            {
                new Product("WW1ITC", "Regular Coffee", 1.00m, 8),
                new Product("TZ2AMC", "Chinese Tea", 8.20m, 12),
                new Product("BBTEA2", "Bubble Tea", 3.50m, 7),
                new Product("A1CCHC", "Dark Chocolate", 3.50m, 17)
            };
            productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);

            var order = new Order
            {
                BasketId = Guid.NewGuid(),
                OrderDate = DateTime.Now,
                HasBeenCompleted = false,
                TotalPrice = 304.5m
            };
            orderRepository.Setup(x => x.GetOrderByBasketIdAsync(It.IsAny<Guid>())).ReturnsAsync(order);

            // Act
            var result = await paymentService.PayAsync();

            // Assert
            productRepository.Verify(x => x.DeleteProductsAsync(It.IsAny<List<BasketItem>>()), Times.Exactly(1));
            orderRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<Guid>()), Times.Exactly(1));
            basketService.Verify(x => x.RemoveAllItems(), Times.Exactly(1));
        }

        [Fact]
        public async Task PayAsync_WhenExceptionIsThrown_LogsException()
        {
            // Arrange
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);
            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "WW1ITC", 3, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 1, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "BBTEA2", 7, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "A1CCHC", 1, DateTime.Now),
            };
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);

            var products = new List<Product>
            {
                new Product("WW1ITC", "Regular Coffee", 1.00m, 8),
                new Product("TZ2AMC", "Chinese Tea", 8.20m, 12),
                new Product("BBTEA2", "Bubble Tea", 3.50m, 7),
                new Product("A1CCHC", "Dark Chocolate", 3.50m, 17)
            };
            productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);

            var order = new Order
            {
                BasketId = Guid.NewGuid(),
                OrderDate = DateTime.Now,
                HasBeenCompleted = false,
                TotalPrice = 304.5m
            };
            orderRepository.Setup(x => x.GetOrderByBasketIdAsync(It.IsAny<Guid>())).ReturnsAsync(order);
            productRepository.Setup(x => x.DeleteProductsAsync(It.IsAny<List<BasketItem>>())).ThrowsAsync(new Exception());

            // Act
            await Assert.ThrowsAsync<Exception>(async () => await paymentService.PayAsync());

            // Assert
            logger.Verify(x => x.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.IsAny<It.IsAnyType>(),
                   It.IsAny<Exception>(),
                   (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(1));
        }

        [Fact]
        public async Task IsBasketValidAsync_WhenProductIsInvalid_ReturnsFalse()
        {
            // Arrange
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);
            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "ERROR", 3, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 1, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "BBTEA2", 7, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "A1CCHC", 1, DateTime.Now),
            };
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);

            var products = new List<Product>
            {
                new Product("WW1ITC", "Regular Coffee", 1.00m, 8),
                new Product("TZ2AMC", "Chinese Tea", 8.20m, 12),
                new Product("BBTEA2", "Bubble Tea", 3.50m, 7),
                new Product("A1CCHC", "Dark Chocolate", 3.50m, 17)
            };
            productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);

            // Act
            var result = await paymentService.IsBasketValidAsync();

            // Assert
            Assert.False(result);
        }


        [Fact]
        public async Task IsBasketValidAsync_WhenProductQuantityIsInvalid_ReturnsFalse()
        {
            // Arrange
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);
            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "WW1ITC", 100, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 1, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "BBTEA2", 7, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "A1CCHC", 1, DateTime.Now),
            };
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);

            var products = new List<Product>
            {
                new Product("WW1ITC", "Regular Coffee", 1.00m, 8),
                new Product("TZ2AMC", "Chinese Tea", 8.20m, 12),
                new Product("BBTEA2", "Bubble Tea", 3.50m, 7),
                new Product("A1CCHC", "Dark Chocolate", 3.50m, 17)
            };
            productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);

            // Act
            var result = await paymentService.IsBasketValidAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsBasketValidAsync_WhenBasketIsValid_ReturnsTrue()
        {
            // Arrange
            var paymentService = new PaymentService(logger.Object, productRepository.Object, orderRepository.Object, basketService.Object, httpContextAccessor.Object);
            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "WW1ITC", 3, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 1, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "BBTEA2", 7, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "A1CCHC", 1, DateTime.Now),
            };
            basketService.Setup(x => x.GetBasketItems()).Returns(basketItems);

            var products = new List<Product>
            {
                new Product("WW1ITC", "Regular Coffee", 1.00m, 8),
                new Product("TZ2AMC", "Chinese Tea", 8.20m, 12),
                new Product("BBTEA2", "Bubble Tea", 3.50m, 7),
                new Product("A1CCHC", "Dark Chocolate", 3.50m, 17)
            };
            productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);

            // Act
            var result = await paymentService.IsBasketValidAsync();

            // Assert
            Assert.True(result);
        }
    }
}
