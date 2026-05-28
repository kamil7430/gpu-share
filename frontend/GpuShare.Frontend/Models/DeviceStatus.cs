namespace GpuShare.Frontend.Models;

public class DeviceStatus
{
    public int DeviceId { get; set; }

    public bool Online { get; set; }

    public DeviceState State { get; set; }

    public double GpuUtilizationPercent { get; set; }

    public double MemoryUsedMb { get; set; }

    public double TemperatureCelsius { get; set; }

    public DateTime LastHeartbeat { get; set; }
}

public enum DeviceState
{
    Available,
    Unavailable,
    Rented,
    Reported
}