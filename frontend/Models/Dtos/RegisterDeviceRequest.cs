namespace GpuShare.Frontend.Models.Dtos;

public class RegisterDeviceRequest
{
    public string Name { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int VramMb { get; set; }

    public int CudaCores { get; set; }

    public string DriverVersion { get; set; } = string.Empty;

    public decimal PricePerHour { get; set; }

    public List<string> Frameworks { get; set; } = new();
}