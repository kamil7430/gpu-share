using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using System.Net;

namespace GpuShare.Frontend.Services;

public class MockAdminService : IAdminService
{
    public Task<PagedResult<User>> ListUsersAsync(UserQueryParams parameters)
    {
        var query = MockStore.Users.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
            query = query.Where(u => u.Username.Contains(parameters.Search, StringComparison.OrdinalIgnoreCase));

        var all = query.ToList();
        var page = parameters.Page > 0 ? parameters.Page : 1;
        var size = parameters.PageSize > 0 ? parameters.PageSize : 20;

        return Task.FromResult(new PagedResult<User>
        {
            Items = all.Skip((page - 1) * size).Take(size).ToList(),
            TotalCount = all.Count,
            Page = page,
            PageSize = size,
        });
    }

    public Task VerifyUserAsync(int userId, VerificationVerdict verdict) => Task.CompletedTask;

    public Task BlockUserAsync(int userId, string reason)
    {
        var user = MockStore.Users.FirstOrDefault(u => u.Id == userId)
            ?? throw new ApiException("User not found.", HttpStatusCode.NotFound);
        MockStore.Users.Remove(user);
        return Task.CompletedTask;
    }

    public Task<PlatformStats> GetPlatformStatsAsync()
    {
        return Task.FromResult(new PlatformStats
        {
            ActiveSessions = MockStore.Orders.Count(o => o.Status == OrderStatus.RUNNING),
            RegisteredUsers = MockStore.Users.Count,
            DevicesInCatalog = MockStore.Devices.Count,
            OpenDisputes = MockStore.Disputes.Count(d => d.Status is DisputeStatus.OPEN or DisputeStatus.UNDER_REVIEW),
            TotalRevenue = MockStore.Transactions
                .Where(t => t.Type == TransactionType.SETTLEMENT && t.Status == TransactionStatus.COMPLETED)
                .Sum(t => (decimal)t.AmountUsdCents) / 100m,
        });
    }

    public Task VerifyDeviceAsync(int deviceId, VerificationVerdict verdict) => Task.CompletedTask;
}
