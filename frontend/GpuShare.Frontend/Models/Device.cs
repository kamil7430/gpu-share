using System.ComponentModel.DataAnnotations;

namespace GpuShare.Frontend.Models;

public class Device
{
    public int DeviceId { get; set; }
    public string OwnerUsername { get; set; } = "";
    public string Name { get; set; } = "";
    public string GpuModel { get; set; } = "";
    public DeviceState State { get; set; } = DeviceState.AVAILABLE;
    public int VramMb { get; set; }
    public int CudaCores { get; set; }
    public string DriverVersion { get; set; } = "";
    public List<string> Frameworks { get; set; } = [];
    public int PricePerHourUsdCents { get; set; }

    public bool IsAvailable => State == DeviceState.AVAILABLE;
}

public enum DevicePageMode
{
    View,
    Edit,
    Add
}

public enum DeviceState
{
    [Display(Name = "Available")]
    AVAILABLE,

    [Display(Name = "Unavailable")]
    UNAVAILABLE,

    [Display(Name = "Rented")]
    RENTED,

    [Display(Name = "Reported")]
    REPORTED
}