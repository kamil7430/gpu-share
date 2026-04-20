namespace GpuShare.Frontend.Models;

public class SearchFilter
{
    public string? Term { get; set; }
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public DateTime? CreatedAfter { get; set; }

    public SortOption SortBy { get; set; } = SortOption.None;
}

public enum SortOption
    {
        
    None,
    NameAsc,
    NameDesc,
    PriceAsc,
    PriceDesc,
    DateNewest,
    DateOldest
}