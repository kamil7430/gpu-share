namespace GpuShare.Frontend.Models;

public class SearchFilter
{
    public string? Term { get; set; }
    public List<string> VRAM_MB { get; set; } = [];
    public (decimal Start, decimal End)? PricePerHour { get; set; }
    public (int Start, int End)? Cores { get; set; }
    public List<string> SupportedFrameworks { get; set; } = [];
    public bool AvailableOnly { get; set; } = false;

    public SortOption SortBy { get; set; } = SortOption.None;
    public bool Descending { get; set; } = false;
}

public enum SortOption
    {
        
    None,
    Name,
    Model,
    Price
}