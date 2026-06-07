using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.Extensions.Logging;

namespace GpuShare.Frontend.Services
{
    public class OrderService(IApiClient api, ILogger<OrderService> logger, IAuthState state) : IOrderService
    {
        private readonly IApiClient _api = api;
        private readonly ILogger<OrderService> _logger = logger;
        private readonly IAuthState _authState = state;

        public async Task<Order> CreateOrderAsync(CreateOrderRequest cmd)
        {
            var response = await _api.PostAsync<CreateOrderRequest, CreateOrderResponse>($"/orders", cmd);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Created order for device {id}. Got ID {id} for it.", 
                    cmd.DeviceId, response!.OrderId);

            return new Order
            {
                OrderId = response!.OrderId,
                DeviceId = cmd.DeviceId,
                Username = _authState.User!.Username,
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
            var orders = await _api.GetAsync<List<Order>>($"/orders", parameters);
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
