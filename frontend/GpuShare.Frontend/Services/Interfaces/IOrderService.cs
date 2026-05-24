namespace GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

public interface IOrderService
{
    /// <summary>
    /// POST /api/orders
    /// Creates a new GPU order and returns connection details.
    /// </summary>
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest cmd);

    /// <summary>
    /// GET /api/orders/{id}
    /// Returns current order state and session details.
    /// </summary>
    Task<Order> GetOrderAsync(int orderId);

    /// <summary>
    /// GET /api/orders
    /// Returns paginated order history with status filtering.
    /// </summary>
    Task<PagedResult<Order>> ListOrdersAsync(OrderQueryParams parameters);

    /// <summary>
    /// POST /api/orders/{id}/end
    /// Ends active session before scheduled finish time.
    /// </summary>
    Task EndOrderAsync(int orderId);
}