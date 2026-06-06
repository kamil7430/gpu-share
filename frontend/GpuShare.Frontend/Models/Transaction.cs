using System.ComponentModel;

namespace GpuShare.Frontend.Models
{

    public class Transaction
    {
        public int TransactionId { get; set; }

        public TransactionType Type { get; set; } = TransactionType.TOPUP;

        public int AmountUsdCents { get; set; }

        public TransactionStatus Status { get; set; } = TransactionStatus.PENDING;

        public DateTime CreatedAtUtc { get; set; }

        public string? Description { get; set; }
    }

    public enum TransactionType
    {
        [Description("Top Up")]
        TOPUP,
        [Description("Reservation")]
        RESERVATION,
        [Description("Settlement")]
        SETTLEMENT,
        [Description("Refund")]
        REFUND,
        [Description("Withdrawal")]
        WITHDRAWAL
    }

    public enum TransactionStatus
    {
        [Description("Pending")]
        PENDING,
        [Description("Completed")]
        COMPLETED,
        [Description("Failed")]
        FAILED,
        [Description("Cancelled")]
        CANCELLED
    }
}