namespace GpuShare.Frontend.Models.Dtos;

public class UserQueryParams
{
    public string? Search { get; set; }

    public string? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}