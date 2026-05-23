namespace GpuShare.Frontend.Models.Dtos;

public class UpdateDeviceRequest
{
    public string Name { get; set; } = string.Empty;

    public decimal PricePerHour { get; set; }

    public bool IsAvailable { get; set; }

    public List<string> Frameworks { get; set; } = new();
}