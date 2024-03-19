using BeverageBasket.API.Entities;
using BeverageBasket.API.Enum;
using BeverageBasket.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace BeverageBasket.API.Services.Interfaces
{
    public interface IPaymentService
    {
        public Task<List<PaymentMethodEnum>?> CheckoutAsync();

        public Task<bool> PayAsync();

        public Task<decimal> GetTotalPriceAsync();

        public Task<Order?> GetOrderByBasketIdAsync();
    }
}
