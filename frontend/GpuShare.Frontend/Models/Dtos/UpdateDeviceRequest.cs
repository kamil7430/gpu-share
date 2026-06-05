namespace GpuShare.Frontend.Models.Dtos;

public class UpdateDeviceRequest
{
    public string? Name { get; set; } = null;
    public string? GpuModel { get; set; } = null;
    public int? VramMb { get; set; } = null;
    public int? CudaCores { get; set; } = null;
    public string? DriverVersion { get; set; } = null;
    public int? PricePerHourUsdCents { get; set; } = null;
    public DeviceState? State { get; set; } = null;
    public List<string>? Frameworks { get; set; } = null;
}