namespace GpuShare.Frontend.Models;

public class DeviceStatus
{
    public bool Online { get; set; }

    public double GpuUtilizationPercent { get; set; }

    public double VramUtilizationPercent { get; set; }

    public double TemperatureCelsius { get; set; }

    public DateTime LastHeartbeatUtc { get; set; }
}