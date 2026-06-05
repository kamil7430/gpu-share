using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services
{
    public class OrderService : IOrderService
    {
        public Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest cmd)
        {
            throw new NotImplementedException();
        }

        public Task EndOrderAsync(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task<Order> GetOrderAsync(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<Order>> ListOrdersAsync(OrderQueryParams parameters)
        {
            throw new NotImplementedException();
        }
    }
}
