namespace GpuShare.Frontend.Models;

public class WalletBalance
{
    public decimal AvailableBalance { get; set; }

    public decimal LockedFunds { get; set; }

    public string Currency { get; set; } = "USD";
}