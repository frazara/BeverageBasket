using BeverageBasket.API.Controllers;
using BeverageBasket.API.Models;
using BeverageBasket.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BeverageBasket.API.Tests.Controllers
{
    public class BasketControllerTest
    {
        private Mock<IBasketService> basketService = new Mock<IBasketService>();

        [Fact]
        public void GetBasketItems_ReturnsNotFound_WhenNoItemsAreFound()
        {
            // Arrange
            var basketController = new BasketController(basketService.Object);
            basketService.Setup(x => x.GetBasketItems()).Returns((List<BasketItem>)null);

            // Act
            var result = basketController.GetBasketItems();

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void GetBasketItems_ReturnsOk_WhenItemsAreFound()
        {
            // Arrange
            var basketController = new BasketController(basketService.Object);
            basketService.Setup(x => x.GetBasketItems()).Returns(new List<BasketItem> { new BasketItem(Guid.NewGuid(), Guid.NewGuid(), "WW1ITC", 1, DateTime.Now) });

            // Act
            var result = basketController.GetBasketItems();

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task AddItemsAsync_ReturnsOk_WhenItemsAreAdded()
        {
            // Arrange
            var basketController = new BasketController(basketService.Object);
            basketService.Setup(x => x.AddItemsAsync("WW1ITC", 1)).ReturnsAsync(true);

            // Act
            var result = await basketController.AddItemsAsync("WW1ITC", 1);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task RemoveItemsAsync_ReturnsOk_WhenItemsAreRemoved()
        {
            // Arrange
            var basketController = new BasketController(basketService.Object);
            basketService.Setup(x => x.RemoveItemsAsync("WW1ITC", 1)).ReturnsAsync(true);

            // Act
            var result = await basketController.RemoveItemsAsync("WW1ITC1", 1);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public void RemoveAllItems_ReturnsOk_WhenItemsAreRemoved()
        {
            // Arrange
            var basketController = new BasketController(basketService.Object);
            basketService.Setup(x => x.RemoveAllItems()).Returns(true);

            // Act
            var result = basketController.RemoveAllItems();

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}
