namespace GpuShare.Frontend.Models;

public class Transaction
{
    public Guid Id { get; set; }

    public TransactionType Type { get; set; } = TransactionType.TopUp;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "USD";

    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    public DateTime CreatedAtUtc { get; set; }

    public string? Description { get; set; }
}

public enum TransactionType
{
    TopUp,
    Reservation,
    Settlement,
    Refund,
    Withdrawal
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled
}