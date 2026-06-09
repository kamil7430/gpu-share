namespace GpuShare.Frontend.Models;

public class PlatformStats
{
    public int ActiveSessions { get; set; }

    public int RegisteredUsers { get; set; }

    public int DevicesInCatalog { get; set; }

    public int OpenDisputes { get; set; }

    public decimal TotalRevenue { get; set; }
}

public enum VerificationVerdict
{
    Approved,
    Rejected
}