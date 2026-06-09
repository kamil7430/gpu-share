using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "Top Up")]
        TOPUP,
        [Display(Name = "Reservation")]
        RESERVATION,
        [Display(Name = "Settlement")]
        SETTLEMENT,
        [Display(Name = "Refund")]
        REFUND,
        [Display(Name = "Withdrawal")]
        WITHDRAWAL
    }

    public enum TransactionStatus
    {
        [Display(Name = "Pending")]
        PENDING,
        [Display(Name = "Completed")]
        COMPLETED,
        [Display(Name = "Failed")]
        FAILED,
        [Display(Name = "Cancelled")]
        CANCELLED
    }
}