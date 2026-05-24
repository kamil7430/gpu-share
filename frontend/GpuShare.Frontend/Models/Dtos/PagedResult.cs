namespace GpuShare.Frontend.Models.Dtos;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}