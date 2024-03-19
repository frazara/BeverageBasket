using BeverageBasket.API.Entities;

namespace BeverageBasket.API.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        public Task AddAsync(Order order);

        public Task<Order?> GetOrderByBasketIdAsync(Guid basketId);

        public Task UpdateAsync(Guid orderId, decimal totalPrice);

        public Task SoftDeleteAsync(Guid orderId);
    }
}
