using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

namespace GpuShare.Frontend.Services;

/// <summary>
/// Shared in-memory state for all mock services.
/// </summary>
public static class MockStore
{
    private static int _nextId = 100;
    public static int NextId() => _nextId++;

    public static User CurrentUser { get; set; } = new() { Id = 1, Username = "alice", Admin = false };

    public static List<User> Users { get; } = [];

    private static readonly List<User> _seedUsers =
    [
        new() { Id = 1, Username = "alice", Admin = false },
        new() { Id = 2, Username = "bob",   Admin = false },
        new() { Id = 3, Username = "charlie", Admin = false },
        new() { Id = 4, Username = "diana", Admin = true  },
    ];

    public static List<Device> Devices { get; } = [];

    private static readonly List<Device> _seedDevices =
    [
        new()
        {
            DeviceId = 1, OwnerUsername = "bob", Name = "RTX 4090 Workstation",
            GpuModel = "NVIDIA RTX 4090", VramMb = 24576, CudaCores = 16384,
            DriverVersion = "545.92", PricePerHourUsdCents = 350,
            Frameworks = ["PyTorch", "TensorFlow"], State = DeviceState.AVAILABLE
        },
        new()
        {
            DeviceId = 2, OwnerUsername = "charlie", Name = "A100 Server Node",
            GpuModel = "NVIDIA A100", VramMb = 81920, CudaCores = 6912,
            DriverVersion = "535.104", PricePerHourUsdCents = 1200,
            Frameworks = ["PyTorch", "TensorFlow", "MXNet"], State = DeviceState.RENTED
        },
        new()
        {
            DeviceId = 3, OwnerUsername = "bob", Name = "RTX 3080 Rig",
            GpuModel = "NVIDIA RTX 3080", VramMb = 10240, CudaCores = 8704,
            DriverVersion = "535.104", PricePerHourUsdCents = 150,
            Frameworks = ["PyTorch", "Keras"], State = DeviceState.AVAILABLE
        },
        new()
        {
            DeviceId = 4, OwnerUsername = "diana", Name = "H100 Training Cluster",
            GpuModel = "NVIDIA H100", VramMb = 81920, CudaCores = 14592,
            DriverVersion = "545.92", PricePerHourUsdCents = 2500,
            Frameworks = ["PyTorch", "TensorFlow", "MXNet", "Caffe"], State = DeviceState.AVAILABLE
        },
        new()
        {
            DeviceId = 5, OwnerUsername = "charlie", Name = "GTX 1080 Ti Budget",
            GpuModel = "NVIDIA GTX 1080 Ti", VramMb = 11264, CudaCores = 3584,
            DriverVersion = "470.182", PricePerHourUsdCents = 80,
            Frameworks = ["TensorFlow", "Caffe"], State = DeviceState.UNAVAILABLE
        },
    ];

    public static List<Order> Orders { get; } = [];

    private static readonly List<Order> _seedOrders =
    [
        new()
        {
            OrderId = 1, DeviceId = 2, Username = "alice",
            Status = OrderStatus.RUNNING,
            StartDate = DateTime.UtcNow.AddHours(-2),
            EndDate = DateTime.UtcNow.AddHours(6),
            TotalReservedCostCents = 9600,
            ConnectionDetails = new()
            {
                Host = "gpu2.gpushare.io", Port = 22, Protocol = "SSH",
                ConnectionUrl = "ssh://gpu2.gpushare.io:22"
            }
        },
        new()
        {
            OrderId = 2, DeviceId = 1, Username = "alice",
            Status = OrderStatus.COMPLETED,
            StartDate = DateTime.UtcNow.AddDays(-3),
            EndDate = DateTime.UtcNow.AddDays(-3).AddHours(4),
            TotalReservedCostCents = 1400,
        },
        new()
        {
            OrderId = 3, DeviceId = 3, Username = "bob",
            Status = OrderStatus.WAITING_FOR_START,
            StartDate = DateTime.UtcNow.AddHours(3),
            EndDate = DateTime.UtcNow.AddHours(7),
            TotalReservedCostCents = 600,
        },
        new()
        {
            OrderId = 4, DeviceId = 4, Username = "charlie",
            Status = OrderStatus.COMPLETED,
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow.AddDays(-7).AddHours(12),
            TotalReservedCostCents = 30000,
        },
    ];

    public static List<Review> Reviews { get; } =
    [
        new()
        {
            ReviewId = 1, OrderId = 2,
            Rating = 5, Comment = "Outstanding performance! The RTX 4090 handled my training job flawlessly.",
            CreatedAt = DateTime.UtcNow.AddDays(-2), AuthorUsername = "alice"
        },
        new()
        {
            ReviewId = 2, OrderId = 4,
            Rating = 4, Comment = "Excellent throughput on H100. Minor network hiccup but overall great.",
            CreatedAt = DateTime.UtcNow.AddDays(-6), AuthorUsername = "charlie"
        },
    ];

    public static List<Transaction> Transactions { get; } =
    [
        new()
        {
            TransactionId = 1, Type = TransactionType.TOPUP, AmountUsdCents = 5000,
            Status = TransactionStatus.COMPLETED, CreatedAtUtc = DateTime.UtcNow.AddDays(-10),
            Description = "Top up via Credit Card"
        },
        new()
        {
            TransactionId = 2, Type = TransactionType.RESERVATION, AmountUsdCents = 1400,
            Status = TransactionStatus.COMPLETED, CreatedAtUtc = DateTime.UtcNow.AddDays(-3),
            Description = "Reservation for RTX 4090 Workstation"
        },
        new()
        {
            TransactionId = 3, Type = TransactionType.TOPUP, AmountUsdCents = 10000,
            Status = TransactionStatus.COMPLETED, CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            Description = "Top up via PayPal"
        },
        new()
        {
            TransactionId = 4, Type = TransactionType.RESERVATION, AmountUsdCents = 9600,
            Status = TransactionStatus.PENDING, CreatedAtUtc = DateTime.UtcNow.AddHours(-2),
            Description = "Reservation for A100 Server Node"
        },
    ];

    public static List<Dispute> Disputes { get; } =
    [
        new()
        {
            DisputeId = 1, OrderId = 2, CustomerUsername = "alice", OwnerUsername = "bob",
            Reason = "Service interruption",
            Description = "The GPU session was interrupted multiple times during my training job, causing significant data loss and wasted compute time.",
            Status = DisputeStatus.UNDER_REVIEW, CreatedAt = DateTime.UtcNow.AddDays(-1)
        },
        new()
        {
            DisputeId = 2, OrderId = 4, CustomerUsername = "charlie", OwnerUsername = "diana",
            Reason = "Performance below advertised specs",
            Description = "The H100 device consistently delivered only 60% of the expected throughput during my entire session despite no other concurrent users.",
            Status = DisputeStatus.OPEN, CreatedAt = DateTime.UtcNow.AddHours(-5)
        },
    ];

    public static WalletBalance Wallet { get; set; } = new() { TotalUsdCents = 14000, LockedUsdCents = 9600 };

    public static PayoutAccount? PayoutAccount { get; set; } = null;

    static MockStore()
    {
        Reset();
    }
    
    public static void Reset()
    {
        _nextId = 100;
        CurrentUser = new() { Id = 1, Username = "alice", Admin = false };

        Users.Clear();
        Users.AddRange(_seedUsers);

        Devices.Clear();
        Devices.AddRange(_seedDevices);

        Orders.Clear();
        Orders.AddRange(_seedOrders);

        // ... repeat for Reviews, Transactions, Disputes
        Wallet = new() { TotalUsdCents = 14000, LockedUsdCents = 9600 };
        PayoutAccount = null;
    }
}
