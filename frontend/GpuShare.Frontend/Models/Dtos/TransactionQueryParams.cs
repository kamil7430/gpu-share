namespace GpuShare.Frontend.Models;

public class TransactionQueryParams
{
    public string? Type { get; set; }

    public string? Status { get; set; }

    public DateTime? FromUtc { get; set; }

    public DateTime? ToUtc { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}