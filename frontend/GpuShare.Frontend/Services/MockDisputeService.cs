using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using System.Net;

namespace GpuShare.Frontend.Services;

public class MockDisputeService : IDisputeService
{
    public Task<Dispute> OpenDisputeAsync(OpenDisputeRequest cmd)
    {
        var order = MockStore.Orders.FirstOrDefault(o => o.OrderId == cmd.OrderId)
            ?? throw new ApiException("Order not found.", HttpStatusCode.NotFound);

        var device = MockStore.Devices.FirstOrDefault(d => d.DeviceId == order.DeviceId);

        var dispute = new Dispute
        {
            DisputeId = MockStore.NextId(),
            OrderId = cmd.OrderId,
            CustomerUsername = MockStore.CurrentUser.Username,
            OwnerUsername = device?.OwnerUsername ?? "unknown",
            Reason = cmd.Reason,
            Description = cmd.Description,
            Attachments = cmd.Attachments,
            Status = DisputeStatus.OPEN,
            CreatedAt = DateTime.UtcNow,
        };
        MockStore.Disputes.Add(dispute);
        return Task.FromResult(dispute);
    }

    public Task<Dispute> GetDisputeAsync(int disputeId)
    {
        var dispute = MockStore.Disputes.FirstOrDefault(d => d.DisputeId == disputeId)
            ?? throw new ApiException("Dispute not found.", HttpStatusCode.NotFound);
        return Task.FromResult(dispute);
    }

    public Task<PagedResult<Dispute>> ListDisputesAsync(DisputeQueryParams parameters)
    {
        var query = MockStore.Disputes.AsEnumerable();

        if (parameters.Status.HasValue)
            query = query.Where(d => d.Status == parameters.Status.Value);
        if (!string.IsNullOrWhiteSpace(parameters.Search))
            query = query.Where(d =>
                d.Reason.Contains(parameters.Search, StringComparison.OrdinalIgnoreCase) ||
                d.Description.Contains(parameters.Search, StringComparison.OrdinalIgnoreCase) ||
                d.CustomerUsername.Contains(parameters.Search, StringComparison.OrdinalIgnoreCase));
        if (parameters.From.HasValue)
            query = query.Where(d => d.CreatedAt >= parameters.From.Value);
        if (parameters.To.HasValue)
            query = query.Where(d => d.CreatedAt <= parameters.To.Value);

        var all = query.OrderByDescending(d => d.CreatedAt).ToList();
        var limit = parameters.Limit > 0 ? parameters.Limit : 25;
        return Task.FromResult(new PagedResult<Dispute>
        {
            Items = all.Take(limit).ToList(),
            TotalCount = all.Count,
            PageSize = limit,
        });
    }

    public Task SubmitClarificationAsync(int disputeId, SubmitClarificationRequest payload)
    {
        var dispute = MockStore.Disputes.FirstOrDefault(d => d.DisputeId == disputeId)
            ?? throw new ApiException("Dispute not found.", HttpStatusCode.NotFound);

        if (dispute.Status == DisputeStatus.OPEN)
            dispute.Status = DisputeStatus.UNDER_REVIEW;

        return Task.CompletedTask;
    }

    public Task ResolveDisputeAsync(int disputeId, ResolveDisputeRequest decision)
    {
        var dispute = MockStore.Disputes.FirstOrDefault(d => d.DisputeId == disputeId)
            ?? throw new ApiException("Dispute not found.", HttpStatusCode.NotFound);

        dispute.Status = decision.Decision == "CustomerFavor" ? DisputeStatus.RESOLVED : DisputeStatus.REJECTED;

        if (decision.RefundAmountUsdCents.HasValue && decision.RefundAmountUsdCents > 0)
        {
            MockStore.Wallet.TotalUsdCents += decision.RefundAmountUsdCents.Value;
            MockStore.Transactions.Add(new Transaction
            {
                TransactionId = MockStore.NextId(),
                Type = TransactionType.REFUND,
                AmountUsdCents = decision.RefundAmountUsdCents.Value,
                Status = TransactionStatus.COMPLETED,
                CreatedAtUtc = DateTime.UtcNow,
                Description = $"Refund for dispute #{disputeId}",
            });
        }

        return Task.CompletedTask;
    }
}
