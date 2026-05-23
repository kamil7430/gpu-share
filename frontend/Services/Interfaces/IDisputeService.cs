namespace GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

public interface IDisputeService
{
    /// <summary>
    /// POST /api/disputes
    /// Opens a new dispute for an order.
    /// </summary>
    Task<Dispute> OpenDisputeAsync(OpenDisputeRequest cmd);

    /// <summary>
    /// GET /api/disputes/{id}
    /// Returns dispute details and current status.
    /// </summary>
    Task<Dispute> GetDisputeAsync(int disputeId);

    /// <summary>
    /// GET /api/disputes
    /// Returns paginated dispute list for admin panel.
    /// </summary>
    Task<PagedResult<Dispute>> ListDisputesAsync(DisputeQueryParams parameters);

    /// <summary>
    /// POST /api/disputes/{id}/clarification
    /// Submits clarification or additional evidence.
    /// </summary>
    Task SubmitClarificationAsync(int disputeId, SubmitClarificationRequest payload);

    /// <summary>
    /// POST /api/disputes/{id}/resolve
    /// Resolves dispute (admin only).
    /// </summary>
    Task ResolveDisputeAsync(int disputeId, ResolveDisputeRequest decision);
}