namespace GpuShare.Frontend.Models;

public class Gpu
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Model { get; set; } = "";
    public bool IsAvailable { get; set; }
    public int VramMb { get; set; }
    public int CudaCores { get; set; }
    public string DriverVersion { get; set; } = "";
    public List<string> Frameworks { get; set; } = [];
    public decimal PricePerHour { get; set; }
}