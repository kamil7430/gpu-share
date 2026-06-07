namespace GpuShare.Frontend.Models.Dtos
{
    public class CreateOrderRequest
    {
        public int DeviceId { get; set; }

        public DateTime StartTime { get; set; }

        public double DurationHours { get; set; }

        public string DockerImage { get; set; } = string.Empty;
    }

    public class CreateOrderResponse
    {
        public int OrderId { get; set; }

        public OrderStatus Status { get; set; }

        public ConnectionDetailsDto ConnectionDetails { get; set; } = new();

        public int TotalReservedCostCents { get; set; }
    }

    public class ConnectionDetailsDto
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public string Protocol { get; set; } = "WSS";

        public string ConnectionString => $"wss://{Host}:{Port}/connect/session_x82A";
    }

    public class OrderQueryParams
    {
        public int? DeviceId { get; set; } = null;

        public string? Username { get; set; } = null;

        public DateTime? StartDate { get; set; } = null;

        public DateTime? EndDate { get; set; } = null;

        public OrderStatus? Status { get; set; } = null;

        //public int Page { get; set; } = 1;

        //public int PageSize { get; set; } = 20;

        public int Limit { get; set; } = 25;
    }


}
