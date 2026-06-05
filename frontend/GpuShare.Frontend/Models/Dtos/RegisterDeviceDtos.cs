namespace GpuShare.Frontend.Models.Dtos
{
    public class RegisterDeviceRequest
    {
        public string Name { get; set; } = string.Empty;

        public string GpuModel { get; set; } = string.Empty;

        public int VramMb { get; set; }

        public int CudaCores { get; set; }

        public string DriverVersion { get; set; } = string.Empty;

        public int PricePerHourUsdCents { get; set; }

        public List<string> Frameworks { get; set; } = [];
    }

    public class RegisterDeviceResponse
    {
        public int DeviceId { get; set; }
        public string OwnerUsername { get; set; } = null!;
        public DeviceState State { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
