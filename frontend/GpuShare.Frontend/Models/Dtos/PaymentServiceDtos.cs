namespace GpuShare.Frontend.Models.Dtos
{
    public class TransactionQueryParams
    {
        public string? Type { get; set; }

        public string? Status { get; set; }

        public DateTime? FromUtc { get; set; }

        public DateTime? ToUtc { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }

    public class TransferResponse
    {
        public string PaymentUrl { get; set; } = string.Empty;

        public string PaymentProvider { get; set; } = string.Empty;

        public Guid TransactionId { get; set; }
    }

    public enum PaymentMethod
    {
        CARD,
        BANK_TRANSFER,
        PAYPAL,
        BLIK
    }
}
