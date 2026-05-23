namespace GpuShare.Frontend.Models;

public class TransferResponse
{
    public string PaymentUrl { get; set; } = string.Empty;

    public string PaymentProvider { get; set; } = string.Empty;

    public Guid TransactionId { get; set; }
}

public enum PaymentMethod
{
    Card,
    BankTransfer,
    PayPal
}