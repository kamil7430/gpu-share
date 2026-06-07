namespace GpuShare.Frontend.Models.Dtos
{
    public class DeviceSearchFilters
    {
        public string? Name { get; set; }

        public string? GpuModel { get; set; }

        public int? MinVramMb { get; set; }

        public int? MaxVramMb { get; set; }

        public int? MinCudaCores { get; set; }

        public int? MaxCudaCores { get; set; }

        public int? MinPricePerHourUsdCents { get; set; }

        public int? MaxPricePerHourUsdCents { get; set; }

        public string? MinDriverVersion { get; set; }
        public string? MaxDriverVersion { get; set; }

        public bool? AvailableOnly { get; set; }

        public List<string> Frameworks { get; set; } = [];

        public int Limit { get; set; } = 25;
    }

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

    public class DeviceAgentInfo
    {
        public string InstallScriptUrl { get; set; } = "";
        public string AgentToken { get; set; } = "";
    }
}
