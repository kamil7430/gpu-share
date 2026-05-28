namespace GpuShare.Frontend.Models.Dtos;

public class OrderQueryParams
{
    public int? DeviceId { get; set; } = null;

    public string? OwnerUsername { get; set; } = null;

    public DateTime? StartDate { get; set; } = null;

    public DateTime? EndDate { get; set; } = null;

    public string? Status { get; set; } = null;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}