using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using System.Net;

namespace GpuShare.Frontend.Services;

public class MockOrderService : IOrderService
{
    public Task<Order> CreateOrderAsync(CreateOrderRequest cmd)
    {
        var device = MockStore.Devices.FirstOrDefault(d => d.DeviceId == cmd.DeviceId)
            ?? throw new ApiException("Device not found.", HttpStatusCode.NotFound);

        if (device.State != DeviceState.AVAILABLE)
            throw new ApiException("Device is not available.", HttpStatusCode.Conflict);

        var endTime = cmd.StartTime.AddHours(cmd.DurationHours);
        var costCents = (int)(device.PricePerHourUsdCents * cmd.DurationHours);

        var order = new Order
        {
            OrderId = MockStore.NextId(),
            DeviceId = cmd.DeviceId,
            Username = MockStore.CurrentUser.Username,
            StartDate = cmd.StartTime,
            EndDate = endTime,
            TotalReservedCostCents = costCents,
            Status = cmd.StartTime <= DateTime.UtcNow ? OrderStatus.RUNNING : OrderStatus.WAITING_FOR_START,
            ConnectionDetails = new ConnectionDetailsDto
            {
                Host = $"gpu{cmd.DeviceId}.gpushare.io",
                Port = 22,
                Protocol = "SSH",
                ConnectionUrl = $"ssh://gpu{cmd.DeviceId}.gpushare.io:22",
            },
        };

        MockStore.Orders.Add(order);
        device.State = DeviceState.RENTED;

        MockStore.Wallet.LockedUsdCents += costCents;
        MockStore.Transactions.Add(new Transaction
        {
            TransactionId = MockStore.NextId(),
            Type = TransactionType.RESERVATION,
            AmountUsdCents = costCents,
            Status = TransactionStatus.PENDING,
            CreatedAtUtc = DateTime.UtcNow,
            Description = $"Reservation for {device.Name}",
        });

        return Task.FromResult(order);
    }

    public Task EndOrderAsync(int orderId)
    {
        var order = MockStore.Orders.FirstOrDefault(o => o.OrderId == orderId)
            ?? throw new ApiException("Order not found.", HttpStatusCode.NotFound);

        order.Status = OrderStatus.COMPLETED;
        order.EndDate = DateTime.UtcNow;

        var device = MockStore.Devices.FirstOrDefault(d => d.DeviceId == order.DeviceId);
        if (device != null) device.State = DeviceState.AVAILABLE;

        var locked = order.TotalReservedCostCents;
        MockStore.Wallet.LockedUsdCents = Math.Max(0, MockStore.Wallet.LockedUsdCents - locked);
        MockStore.Wallet.TotalUsdCents = Math.Max(0, MockStore.Wallet.TotalUsdCents - locked);

        return Task.CompletedTask;
    }

    public Task<Order> GetOrderAsync(int orderId)
    {
        var order = MockStore.Orders.FirstOrDefault(o => o.OrderId == orderId)
            ?? throw new ApiException("Order not found.", HttpStatusCode.NotFound);
        return Task.FromResult(order);
    }

    public Task<PagedResult<Order>> ListOrdersAsync(OrderQueryParams parameters)
    {
        var query = MockStore.Orders.AsEnumerable();

        if (parameters.DeviceId.HasValue)
            query = query.Where(o => o.DeviceId == parameters.DeviceId.Value);
        if (!string.IsNullOrWhiteSpace(parameters.Username))
            query = query.Where(o => o.Username == parameters.Username);
        if (parameters.Status.HasValue)
            query = query.Where(o => o.Status == parameters.Status.Value);
        if (parameters.StartDate.HasValue)
            query = query.Where(o => o.StartDate >= parameters.StartDate.Value);
        if (parameters.EndDate.HasValue)
            query = query.Where(o => o.EndDate <= parameters.EndDate.Value);

        var all = query.OrderByDescending(o => o.StartDate).ToList();
        return Task.FromResult(new PagedResult<Order>
        {
            Items = all.Take(parameters.Limit).ToList(),
            TotalCount = all.Count,
            PageSize = parameters.Limit,
        });
    }
}
