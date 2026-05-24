namespace GpuShare.Frontend.Models.Dtos;

public class CreateOrderRequest
{
    public int DeviceId { get; set; }

    public DateTime StartTimeUtc { get; set; }

    public int DurationHours { get; set; }

    public string DockerImage { get; set; } = string.Empty;
}