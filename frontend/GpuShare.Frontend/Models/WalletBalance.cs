namespace GpuShare.Frontend.Models;

public class WalletBalance
{
    public int TotalUsdCents { get; set; }

    public int LockedUsdCents { get; set; }

    public int AvailableUsdCents => TotalUsdCents - LockedUsdCents;
}