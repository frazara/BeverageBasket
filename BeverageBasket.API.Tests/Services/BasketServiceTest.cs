using BeverageBasket.API.Entities;
using BeverageBasket.API.Models;
using BeverageBasket.API.Repositories.Interfaces;
using BeverageBasket.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Text;

namespace BeverageBasket.API.Tests.Services
{
    public class BasketServiceTest
    {
        private const string BasketSessionKey = "BasketId";
        private Mock<ILogger<BasketService>> logger = new Mock<ILogger<BasketService>>();
        private Mock<IHttpContextAccessor> httpContextAccessor = new Mock<IHttpContextAccessor>();
        private Mock<IProductRepository> productRepository = new Mock<IProductRepository>();

        [Fact]
        public void GetBasketItems_WhenBasketIsEmpty_ReturnsEmptyList()
        {
            //Arrange
            var basketService = new BasketService(logger.Object, httpContextAccessor.Object, productRepository.Object);

            //Act
            var result = basketService.GetBasketItems();

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetBasketItems_WhenCanNotParseBasketItems_ReturnsEmptyList()
        {
            //Arrange
            var basketService = new BasketService(logger.Object, httpContextAccessor.Object, productRepository.Object);
            var basketId = Guid.NewGuid();
            var basketIdToByteArray = Encoding.UTF8.GetBytes(basketId.ToString());
            var basketItemsToArray = Encoding.UTF8.GetBytes("invalid json");

            httpContextAccessor.Setup(x => x.HttpContext.Session.TryGetValue(BasketSessionKey, out basketIdToByteArray)).Returns(true);
            httpContextAccessor.Setup(x => x.HttpContext.Session.TryGetValue(basketId.ToString(), out basketItemsToArray)).Returns(true);

            //Act
            var result = basketService.GetBasketItems();

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetBasketItems_WhenBasketIsNotEmpty_ReturnsBasketItemsFromSession()
        {
            //Arrange
            var basketService = new BasketService(logger.Object, httpContextAccessor.Object, productRepository.Object);
            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "WW1ITC", 1, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 1, DateTime.Now)
            };
            var basketItemsJson = JsonConvert.SerializeObject(basketItems);
            var basketId = Guid.NewGuid();
            var basketIdToByteArray = Encoding.UTF8.GetBytes(basketId.ToString());
            var basketItemsToArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(basketItems));

            httpContextAccessor.Setup(x => x.HttpContext.Session.TryGetValue(BasketSessionKey, out basketIdToByteArray)).Returns(true);
            httpContextAccessor.Setup(x => x.HttpContext.Session.TryGetValue(basketId.ToString(), out basketItemsToArray)).Returns(true);

            //Act
            var result = basketService.GetBasketItems();

            //Assert
            Assert.Equal(basketItemsJson, JsonConvert.SerializeObject(result));
        }

        [Fact]
        public async Task AddItemsAsync_WhenProductIsNotFound_ReturnsFalse()
        {
            //Arrange
            var basketService = new BasketService(logger.Object, httpContextAccessor.Object, productRepository.Object);
            var productId = "WW1ITC";
            var quantity = 1;
            productRepository.Setup(x => x.GetProductByIdAsync(productId)).ReturnsAsync((Product)null);

            //Act
            var result = await basketService.AddItemsAsync(productId, quantity);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddItemsAsync_WhenProductIsFound_ReturnsTrue()
        {
            //Arrange
            var basketService = new BasketService(logger.Object, httpContextAccessor.Object, productRepository.Object);
            var productId = "WW1ITC";
            var quantity = 1;
            productRepository.Setup(x => x.GetProductByIdAsync(productId)).ReturnsAsync(new Product("WW1ITC", "Regular Coffee", 1.00m, 8));

            //Act
            var result = await basketService.AddItemsAsync(productId, quantity);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddItemsAsync_WhenQuantityIsUnavailable_ReturnsFalse()
        {
            //Arrange
            var basketService = new BasketService(logger.Object, httpContextAccessor.Object, productRepository.Object);
            var productId = "WW1ITC";
            var quantity = 10;
            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "WW1ITC", 10, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 1, DateTime.Now)
            };
            var basketItemsJson = JsonConvert.SerializeObject(basketItems);
            var basketId = Guid.NewGuid();
            var basketIdToByteArray = Encoding.UTF8.GetBytes(basketId.ToString());
            var basketItemsToArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(basketItems));
            httpContextAccessor.Setup(x => x.HttpContext.Session.TryGetValue(BasketSessionKey, out basketIdToByteArray)).Returns(true);
            httpContextAccessor.Setup(x => x.HttpContext.Session.TryGetValue(basketId.ToString(), out basketItemsToArray)).Returns(true);
            productRepository.Setup(x => x.GetProductByIdAsync(productId)).ReturnsAsync(new Product("WW1ITC", "Regular Coffee", 1.00m, 8));

            //Act
            var result = await basketService.AddItemsAsync(productId, quantity);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveItemsAsync_WhenProductIsNotFound_ReturnsFalse()
        {
            //Arrange
            var basketService = new BasketService(logger.Object, httpContextAccessor.Object, productRepository.Object);
            var productId = "1";
            var quantity = 1;
            productRepository.Setup(x => x.GetProductByIdAsync(productId)).ReturnsAsync((Product)null);

            //Act
            var result = await basketService.RemoveItemsAsync(productId, quantity);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveItemsAsync_WhenProductIsFound_ReturnsTrue()
        {
            //Arrange
            var basketService = new BasketService(logger.Object, httpContextAccessor.Object, productRepository.Object);
            var productId = "WW1ITC";
            var quantity = 1;

            var basketItems = new List<BasketItem>
            {
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "WW1ITC", 1, DateTime.Now),
                new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "TZ2AMC", 1, DateTime.Now)
            };
            var basketItemsJson = JsonConvert.SerializeObject(basketItems);
            var basketId = Guid.NewGuid();
            var basketIdToByteArray = Encoding.UTF8.GetBytes(basketId.ToString());
            var basketItemsToArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(basketItems));
            httpContextAccessor.Setup(x => x.HttpContext.Session.TryGetValue(BasketSessionKey, out basketIdToByteArray)).Returns(true);
            httpContextAccessor.Setup(x => x.HttpContext.Session.TryGetValue(basketId.ToString(), out basketItemsToArray)).Returns(true);
            productRepository.Setup(x => x.GetProductByIdAsync(productId)).ReturnsAsync(new Product("WW1ITC", "Regular Coffee", 1.00m, 8));

            //Act
            var result = await basketService.RemoveItemsAsync(productId, quantity);

            //Assert
            Assert.True(result);
        }

        [Fact]
        public void GetBasketId_WhenBasketIdIsInSession_ReturnsBasketId()
        {
            //Arrange
            var basketService = new BasketService(logger.Object, httpContextAccessor.Object, productRepository.Object);
            var basketId = Guid.NewGuid();
            var basketIdToByteArray = Encoding.UTF8.GetBytes(basketId.ToString());

            httpContextAccessor.Setup(x => x.HttpContext.Session.TryGetValue(BasketSessionKey, out basketIdToByteArray)).Returns(true);

            //Act
            var result = basketService.GetBasketId();

            //Assert
            Assert.Equal(basketId, result);
        }
    }
}
