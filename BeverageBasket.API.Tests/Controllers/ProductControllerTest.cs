using BeverageBasket.API.Controllers;
using BeverageBasket.API.Entities;
using BeverageBasket.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BeverageBasket.API.Tests.Controllers
{
    public class ProductControllerTest
    {
        private Mock<IProductRepository> mockRepository = new Mock<IProductRepository>();

        [Fact]
        public async Task GetProductList_ReturnsOkResult_WithAListOfProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product("WW1ITC", "Regular Coffee", 1.00m, 8),
                new Product("TZ2AMC", "Chinese Tea", 8.20m, 12),
                new Product("BBTEA2", "Bubble Tea", 3.50m, 7),
                new Product("A1CCHC", "Dark Chocolate", 3.50m, 17)
            };
            mockRepository.Setup(repo => repo.GetProductsAsync())
                .ReturnsAsync(products);
            var controller = new ProductController(mockRepository.Object);

            // Act
            var result = await controller.GetProductList();

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetProductList_ReturnsNotFoundResult_WhenNoProducts()
        {
            // Arrange
            var mockRepository = new Mock<IProductRepository>();
            mockRepository.Setup(repo => repo.GetProductsAsync())
                .ReturnsAsync((List<Product>)null);
            var controller = new ProductController(mockRepository.Object);

            // Act
            var result = await controller.GetProductList();

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
