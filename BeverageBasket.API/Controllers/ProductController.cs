using BeverageBasket.API.Entities;
using BeverageBasket.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BeverageBasket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetProductList()
        {
            var products = await _productRepository.GetProductsAsync();
            return products != null ? Ok(products) : NotFound();
        }
    }
}
