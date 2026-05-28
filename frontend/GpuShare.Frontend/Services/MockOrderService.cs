using Blazorise;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services
{
    public class MockOrderService : IOrderService
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
                Status = "In Use",
                StartDate = weekStart.AddDays(1).AddHours(9).AddMinutes(30),
                EndDate = weekStart.AddDays(1).AddHours(13)
            },

            new Order
            {
                OwnerUsername = "Stable Diffusion",
                Status = "Reserved",
                StartDate = weekStart.AddDays(2).AddHours(14),
                EndDate = weekStart.AddDays(2).AddHours(18)
            },

            new Order
            {
                OwnerUsername = "CUDA Rendering",
                Status = "Reserved",
                StartDate = weekStart.AddDays(4).AddHours(8),
                EndDate = weekStart.AddDays(4).AddHours(11)
            },

            new Order
            {
                OwnerUsername = "Fine-Tuning",
                Status = "In Use",
                StartDate = weekStart.AddDays(5).AddHours(16),
                EndDate = weekStart.AddDays(5).AddHours(22)
            }
        };
        }
    }
}
