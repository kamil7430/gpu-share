namespace GpuShare.Frontend.Models;

public class DeviceSearchFilters
{
    public string? Query { get; set; }

    public int? MinVramMb { get; set; }

    public int? MaxPricePerHour { get; set; }

    public bool? AvailableOnly { get; set; }

    public List<string> Frameworks { get; set; } = new();

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}