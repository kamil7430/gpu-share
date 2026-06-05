namespace GpuShare.Frontend.Models.Dtos
{
    public class CreateOrderRequest
    {
        public int DeviceId { get; set; }

        public DateTime StartTimeUtc { get; set; }

        public int DurationHours { get; set; }

        public string DockerImage { get; set; } = string.Empty;
    }

    public class CreateOrderResponse
    {
        public int OrderId { get; set; }

        public ConnectionDetailsDto ConnectionDetails { get; set; } = new();
    }

    public class ConnectionDetailsDto
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public string Protocol { get; set; } = "WSS";

        public string ConnectionString { get; set; } = "";

        public string AccessToken { get; set; } = string.Empty;
    }

    public class OrderQueryParams
    {
        public int? DeviceId { get; set; } = null;

        public string? OwnerUsername { get; set; } = null;

        public DateTime? StartDate { get; set; } = null;

        public DateTime? EndDate { get; set; } = null;

        public string? Status { get; set; } = null;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }


}
