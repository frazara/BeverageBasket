using BeverageBasket.API.DbContexts;
using BeverageBasket.API.Entities;
using BeverageBasket.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BeverageBasket.API.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ProductContext _context;

        public OrderRepository(ProductContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return;
        }

        public async Task UpdateAsync(Guid orderId, decimal totalPrice)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(el => el.Id == orderId);
            if (order != null)
            {
                order.OrderDate = DateTime.Now;
                order.TotalPrice = totalPrice;
                await _context.SaveChangesAsync();
            }
            return;
        }

        public async Task SoftDeleteAsync(Guid orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(el => el.Id == orderId);
            if (order != null)
            {
                order.HasBeenCompleted = true;
                await _context.SaveChangesAsync();
            }
            return;
        }

        public async Task<Order?> GetOrderByBasketIdAsync(Guid basketId)
        {
            return await _context.Orders.FirstOrDefaultAsync(order => order.BasketId == basketId);
        }
    }
}
