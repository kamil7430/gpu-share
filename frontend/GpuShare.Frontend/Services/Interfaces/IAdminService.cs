namespace GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

public interface IAdminService
{
    /// <summary>
    /// GET /api/admin/users
    /// Returns paginated user list with role and status filters.
    /// </summary>
    Task<PagedResult<User>> ListUsersAsync(UserQueryParams parameters);

    /// <summary>
    /// POST /api/admin/users/{id}/verify
    /// Approves or rejects user verification.
    /// </summary>
    Task VerifyUserAsync(int userId, VerificationVerdict verdict);

    /// <summary>
    /// POST /api/admin/users/{id}/block
    /// Blocks user account with required reason.
    /// </summary>
    Task BlockUserAsync(int userId, string reason);

    /// <summary>
    /// GET /api/admin/stats
    /// Returns platform KPI statistics for admin dashboard.
    /// </summary>
    Task<PlatformStats> GetPlatformStatsAsync();

    /// <summary>
    /// POST /api/admin/devices/{id}/verify
    /// Performs random GPU verification.
    /// </summary>
    Task VerifyDeviceAsync(int deviceId, VerificationVerdict verdict);
}