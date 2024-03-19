using BeverageBasket.API.Models;
using BeverageBasket.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BeverageBasket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketService _basketService;

        public BasketController(IBasketService basketService)
        {
            _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
        }

        [HttpGet("GetBasket")]
        public ActionResult<List<BasketItem>> GetBasketItems()
        {
            List<BasketItem> basket = _basketService.GetBasketItems();
            return basket != null ? Ok(basket) : NotFound();
        }

        [HttpPost("AddItems")]
        public async Task<ActionResult<bool>> AddItemsAsync(string productId, int quantity = 1)
        {
            return Ok(await _basketService.AddItemsAsync(productId, quantity));
        }

        [HttpPost("RemoveItems")]
        public async Task<ActionResult<bool>> RemoveItemsAsync(string productId, int quantity = 1)
        {
            return Ok(await _basketService.RemoveItemsAsync(productId, quantity));
        }

        [HttpPost("RemoveAll")]
        public ActionResult<bool> RemoveAllItems()
        {
            return Ok(_basketService.RemoveAllItems());
        }
    }
}
