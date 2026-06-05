namespace GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

public class Order
{
    public int OrderId { get; set; }
    public int DeviceId { get; set; } = 0;
    public string Username { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; } = null;
    public DateTime? EndDate { get; set; } = null;
    public int TotalReservedCostCents { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.WAITING_FOR_START;
    public ConnectionDetailsDto? ConnectionDetails { get; set; }

    public Order(){}
    
    public Order(int id, int deviceId, string username, DateTime? startDate, DateTime? endDate, int totalReservedCostCents, OrderStatus status)
    {
        OrderId = id;
        DeviceId = deviceId;
        Username = username;
        StartDate = startDate;
        EndDate = endDate;
        TotalReservedCostCents = totalReservedCostCents;
        Status = status;
    }
};

public enum OrderStatus
{
    WAITING_FOR_START,
    RUNNING,
    COMPLETED,
    SUSPENDED,
    FAILURE
}