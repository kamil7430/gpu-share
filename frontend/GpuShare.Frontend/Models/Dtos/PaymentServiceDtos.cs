using System.ComponentModel;

namespace GpuShare.Frontend.Models.Dtos
{
    public class TransactionQueryParams
    {
        public TransactionType? Type { get; set; }

        public TransactionStatus? Status { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }

    public class TransferRequest
    {
        public int AmountUsdCents { get; set; }
        public PaymentMethod Method { get; set; }
        public TransactionType Type { get; set; }
    }

    public class TopUpRequest : TransferRequest
    {
        public TopUpRequest() : base()
        {
            Type = TransactionType.TOPUP;
        }
    }

    public class WithdrawRequest : TransferRequest
    {
        public WithdrawRequest() : base()
        {
            Type = TransactionType.WITHDRAWAL;
        }
    }

    public class TransferResponse
    {
        public int TransactionId { get; set; }

        public string PaymentUrl { get; set; } = string.Empty;

        public string PaymentProvider { get; set; } = string.Empty;

        public TransactionStatus Status { get; set; } = TransactionStatus.PENDING;
    }

    public class PayoutAccount
    {
        public string BankName { get; set; } = "";
        public string AccountNumber { get; set; } = "";
        public string? PayPalEmail { get; set; }
    }

    public enum PaymentMethod
    {
        [Description("Credit/Debit Card")]
        CARD,
        [Description("Bank Transfer")]
        BANK_TRANSFER,
        [Description("PayPal")]
        PAYPAL
    }
}
