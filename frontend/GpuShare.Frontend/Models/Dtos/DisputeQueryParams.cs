namespace GpuShare.Frontend.Models.Dtos;

public class DisputeQueryParams
{
    public string? Status { get; set; }

    public string? Search { get; set; }

    public DateTime? FromUtc { get; set; }

    public DateTime? ToUtc { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}