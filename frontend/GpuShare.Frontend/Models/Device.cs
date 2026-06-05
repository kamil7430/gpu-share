using System.ComponentModel.DataAnnotations;

namespace GpuShare.Frontend.Models;

public class Device
{
    public int DeviceId { get; set; }
    public string OwnerUsername { get; set; } = "";
    public string Name { get; set; } = "";
    public string GpuModel { get; set; } = "";
    public DeviceState State { get; set; } = DeviceState.Available;
    public int VramMb { get; set; }
    public int CudaCores { get; set; }
    public string DriverVersion { get; set; } = "";
    public List<string> Frameworks { get; set; } = [];
    public decimal PricePerHour { get; set; }

    public bool IsAvailable => State == DeviceState.Available;
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
    Available,

    [Display(Name = "Unavailable")]
    Unavailable,

    [Display(Name = "Rented")]
    Rented,

    [Display(Name = "Reported")]
    Reported
}