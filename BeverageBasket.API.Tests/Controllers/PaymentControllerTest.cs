using BeverageBasket.API.Controllers;
using BeverageBasket.API.Entities;
using BeverageBasket.API.Enum;
using BeverageBasket.API.Services;
using BeverageBasket.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BeverageBasket.API.Tests.Controllers
{
    public class PaymentControllerTest
    {
        private Mock<IPaymentService> paymentService = new Mock<IPaymentService>();
        private Mock<ILogger<BasketService>> logger = new Mock<ILogger<BasketService>>();

        [Fact]
        public async Task CheckoutAsync_ReturnsNotFound_WhenNoPaymentMethodsAreFound()
        {
            // Arrange
            var paymentController = new PaymentController(logger.Object, paymentService.Object);
            paymentService.Setup(x => x.CheckoutAsync()).ReturnsAsync((List<PaymentMethodEnum>)null);

            // Act
            var result = await paymentController.CheckoutAsync();

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CheckoutAsync_ReturnsOk_WhenPaymentMethodsAreFound()
        {
            // Arrange
            var paymentController = new PaymentController(logger.Object, paymentService.Object);
            paymentService.Setup(x => x.CheckoutAsync()).ReturnsAsync(new List<PaymentMethodEnum> { PaymentMethodEnum.Card });

            // Act
            var result = await paymentController.CheckoutAsync();

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetOrderByBasketIdAsync_ReturnsNotFound_WhenNoOrdersAreFound()
        {
            // Arrange
            var paymentController = new PaymentController(logger.Object, paymentService.Object);
            paymentService.Setup(x => x.GetOrderByBasketIdAsync()).ReturnsAsync((Order)null);

            // Act
            var result = await paymentController.GetOrderByBasketIdAsync();

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetOrderByBasketIdAsync_ReturnsOk_WhenOrdersAreFound()
        {
            // Arrange
            var paymentController = new PaymentController(logger.Object, paymentService.Object);
            paymentService.Setup(x => x.GetOrderByBasketIdAsync()).ReturnsAsync(new Order());

            // Act
            var result = await paymentController.GetOrderByBasketIdAsync();

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task PayAsync_ReturnsStatusCode500_WhenExceptionIsThrown()
        {
            // Arrange
            var paymentController = new PaymentController(logger.Object, paymentService.Object);
            paymentService.Setup(x => x.PayAsync()).ThrowsAsync(new Exception());

            // Act
            var result = await paymentController.PayAsync();

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, (result.Result as ObjectResult).StatusCode);
        }

        [Fact]
        public async Task PayAsync_ReturnsOk_WhenPaymentIsSuccessful()
        {
            // Arrange
            var paymentController = new PaymentController(logger.Object, paymentService.Object);
            paymentService.Setup(x => x.PayAsync()).ReturnsAsync(true);

            // Act
            var result = await paymentController.PayAsync();

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}
