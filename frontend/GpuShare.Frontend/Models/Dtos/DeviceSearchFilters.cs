namespace GpuShare.Frontend.Models.Dtos;

public class DeviceSearchFilters
{
    public string? Name { get; set; }

    public string? GpuModel { get; set; }

    public int? MinVramMb { get; set; }

    public int? MaxVramMb { get; set; }

    public int? MinCudaCores { get; set; }

    public int? MaxCudaCores { get; set; }

    public int? MinPricePerHourUsdCents { get; set; }

    public int? MaxPricePerHourUsdCents { get; set; }

    public string? MinDriverVersion { get; set; }
    public string? MaxDriverVersion { get; set; }

    public bool? AvailableOnly { get; set; }

    public List<string> Frameworks { get; set; } = [];

    public int Limit { get; set; } = 25;
}