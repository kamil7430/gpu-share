using Blazorise;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services
{
    public class MockOrderService : IOrderService
    {
        public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest cmd)
        {
            var conn = new ConnectionDetailsDto() { 
                AccessToken = "token",
                Host = "host",
                Port = 420,
                Protocol = "WSS"
            };

            return new CreateOrderResponse()
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
                AccessToken = "token",
                Host = "host",
                Port = 420,
                Protocol = "WSS"
            };

            return new Order()
            {
                Id = orderId,
                DeviceId = 1,
                Cost = 5,
                OwnerUsername = "user",
                EndDate = DateTime.Now.AddHours(12),
                StartDate = DateTime.Now,
                Status = OrderStatus.Running,
                ConnectionDetails = conn 
            };
        }

        public Task<PagedResult<Order>> ListOrdersAsync(OrderQueryParams parameters)
        {
            var orders = GenerateMockOrders(parameters.StartDate ?? DateTime.Today);
            var pagedResult = new PagedResult<Order>
            {
                Items = orders.Take(parameters.PageSize).ToList(),
                TotalCount = orders.Count,
                PageSize = parameters.PageSize,
                Page = parameters.Page,
            };
            return Task.FromResult(pagedResult);
        }

        // ===== MOCK DATA =====
        private List<Order> GenerateMockOrders(DateTime weekStart)
        {
            return new()
        {
            new Order
            {
                OwnerUsername = "LLM Training",
                Status = OrderStatus.Running,
                StartDate = weekStart.AddDays(1).AddHours(9).AddMinutes(30),
                EndDate = weekStart.AddDays(1).AddHours(13)
            },

            new Order
            {
                OwnerUsername = "Stable Diffusion",
                Status = OrderStatus.WaitingForStart,
                StartDate = weekStart.AddDays(2).AddHours(14),
                EndDate = weekStart.AddDays(2).AddHours(18)
            },

            new Order
            {
                OwnerUsername = "CUDA Rendering",
                Status = OrderStatus.WaitingForStart,
                StartDate = weekStart.AddDays(4).AddHours(8),
                EndDate = weekStart.AddDays(4).AddHours(11)
            },

            new Order
            {
                OwnerUsername = "Fine-Tuning",
                Status = OrderStatus.Running,
                StartDate = weekStart.AddDays(5).AddHours(16),
                EndDate = weekStart.AddDays(5).AddHours(22)
            }
        };
        }
    }
}
