using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GpuShare.Frontend.Services
{
    public class OrderService : IOrderService
    {
        private readonly IApiClient _api;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IApiClient api, ILogger<OrderService> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task<Order> CreateOrderAsync(CreateOrderRequest cmd)
        {
            var response = await _api.PostAsync<CreateOrderRequest, CreateOrderResponse>($"/orders", cmd);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Created order for device {id} and user {username}. Got ID {id} for it.", 
                    cmd.DeviceId, cmd.Username, response!.OrderId);

            return new Order
            {
                OrderId = response!.OrderId,
                DeviceId = cmd.DeviceId,
                Username = cmd.Username,
                StartDate = cmd.StartTime,
                EndDate = cmd.StartTime.AddHours(cmd.DurationHours),
                Status = response.Status,
                TotalReservedCostCents = response.TotalReservedCostCents,
                ConnectionDetails = response.ConnectionDetails
            };
        }

        public async Task EndOrderAsync(int orderId)
        {
            await _api.PostAsync<object>($"/orders/{orderId}/end", new { });
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Ended order with ID {id}.", orderId);
        }

        public async Task<Order> GetOrderAsync(int orderId)
        {
            var order = await _api.GetAsync<Order>($"/orders/{orderId}");
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Got order with id {Id}", orderId);
            return order!;
        }

        public async Task<PagedResult<Order>> ListOrdersAsync(OrderQueryParams parameters)
        {
            var orders = await _api.GetAsync<List<Order>>($"/orders");
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Got orders.");
            return new PagedResult<Order>
            {
                Items = orders!,
                TotalCount = orders!.Count,
                Page = 1,
                PageSize = parameters.Limit
            };
        }
    }
}
