using Blazorise;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services
{
    public class MockOrderService : IOrderService
    {
        public async Task<Order> CreateOrderAsync(CreateOrderRequest cmd)
        {
            var conn = new ConnectionDetailsDto() { 
                Host = "host",
                Port = 420,
                Protocol = "WSS"
            };

            return new Order()
            {
                OrderId = 1,
                ConnectionDetails = conn
            };
        }

        public Task EndOrderAsync(int orderId)
        {
            return Task.CompletedTask;
        }

        public async Task<Order> GetOrderAsync(int orderId)
        {
            var conn = new ConnectionDetailsDto()
            {
                Host = "host",
                Port = 420,
                Protocol = "WSS"
            };

            return new Order()
            {
                OrderId = orderId,
                DeviceId = 1,
                TotalReservedCostCents = 5,
                Username = "user",
                EndDate = DateTime.Now.AddHours(12),
                StartDate = DateTime.Now,
                Status = OrderStatus.RUNNING,
                ConnectionDetails = conn 
            };
        }

        public Task<PagedResult<Order>> ListOrdersAsync(OrderQueryParams parameters)
        {
            var orders = GenerateMockOrders(parameters.StartDate ?? DateTime.Today);
            var pagedResult = new PagedResult<Order>
            {
                Items = [.. orders.Take(parameters.Limit)],
                TotalCount = orders.Count,
                PageSize = parameters.Limit,
            };
            return Task.FromResult(pagedResult);
        }

        // ===== MOCK DATA =====
        private static List<Order> GenerateMockOrders(DateTime weekStart)
        {
            return [
        
            new Order
            {
                Username = "LLM Training",
                Status = OrderStatus.RUNNING,
                StartDate = weekStart.AddDays(1).AddHours(9).AddMinutes(30),
                EndDate = weekStart.AddDays(1).AddHours(13)
            },

            new Order
            {
                Username = "Stable Diffusion",
                Status = OrderStatus.WAITING_FOR_START,
                StartDate = weekStart.AddDays(2).AddHours(14),
                EndDate = weekStart.AddDays(2).AddHours(18)
            },

            new Order
            {
                Username = "CUDA Rendering",
                Status = OrderStatus.WAITING_FOR_START,
                StartDate = weekStart.AddDays(4).AddHours(8),
                EndDate = weekStart.AddDays(4).AddHours(11)
            },

            new Order
            {
                Username = "Fine-Tuning",
                Status = OrderStatus.RUNNING,
                StartDate = weekStart.AddDays(5).AddHours(16),
                EndDate = weekStart.AddDays(5).AddHours(22)
            }
        ];
        }
    }
}
