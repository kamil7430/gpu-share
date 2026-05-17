namespace GpuShare.Frontend.Models;

public class Order
{
    public int Id { get; set; }
    public string Device { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; } = null;
    public DateTime? EndDate { get; set; } = null;
    public int Cost { get; set; }
    public string Status { get; set; } = string.Empty;

    public Order(){}
    
    public Order(int id, string device, string owner, DateTime? startDate, DateTime? endDate, int cost, string status)
    {
        Id = id;
        Device = device;
        Owner = owner;
        StartDate = startDate;
        EndDate = endDate;
        Cost = cost;
        Status = status;
    }
};