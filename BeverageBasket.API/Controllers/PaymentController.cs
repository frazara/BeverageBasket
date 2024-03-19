using BeverageBasket.API.Entities;
using BeverageBasket.API.Enum;
using BeverageBasket.API.Services;
using BeverageBasket.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BeverageBasket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<BasketService> _logger;

        public PaymentController(ILogger<BasketService> logger, IPaymentService paymentService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        }

        [HttpGet("Checkout")]
        public async Task<ActionResult<PaymentMethodEnum>> CheckoutAsync()
        {
            var paymentMethods = await _paymentService.CheckoutAsync();
            return paymentMethods != null ? Ok(paymentMethods) : NotFound();
        }

        [HttpGet("GetOrder")]
        public async Task<ActionResult<Order>> GetOrderByBasketIdAsync()
        {
            var orders = await _paymentService.GetOrderByBasketIdAsync();
            return orders != null ? Ok(orders) : NotFound();
        }

        [HttpPost("Pay")]
        public async Task<ActionResult<bool>> PayAsync()
        {
            try
            {
                var result = await _paymentService.PayAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Payment failed: {ex}.");
                return StatusCode(500, "Payment failed. Please try again later.");
            }
        }
    }
}
