namespace GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

public class Order
{
    public int Id { get; set; }
    public int DeviceId { get; set; } = 0;
    public string OwnerUsername { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; } = null;
    public DateTime? EndDate { get; set; } = null;
    public decimal Cost { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.WaitingForStart;
    public ConnectionDetailsDto? ConnectionDetails { get; set; }

    public Order(){}
    
    public Order(int id, int deviceId, string ownerUsername, DateTime? startDate, DateTime? endDate, decimal cost, OrderStatus status)
    {
        Id = id;
        DeviceId = deviceId;
        OwnerUsername = ownerUsername;
        StartDate = startDate;
        EndDate = endDate;
        Cost = cost;
        Status = status;
    }
};

public enum OrderStatus
{
    WaitingForStart,
    Running,
    Completed,
    Suspended,
    Failure
}