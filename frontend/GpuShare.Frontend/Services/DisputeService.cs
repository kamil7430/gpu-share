using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services
{
    public class DisputeService(IApiClient api, ILogger<DisputeService> logger) : IDisputeService
    {
        private readonly IApiClient _api = api;
        private readonly ILogger<DisputeService> _logger = logger;

        public async Task<Dispute> GetDisputeAsync(int disputeId)
        {
            var dispute = await _api.GetAsync<Dispute>($"/disputes/{disputeId}");
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Got dispute with id {Id}", disputeId);

            return dispute!;
        }

        public async Task<PagedResult<Dispute>> ListDisputesAsync(DisputeQueryParams parameters)
        {
            var disputes = await _api.GetAsync<List<Dispute>>($"/disputes", parameters);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Got {num} disputes.", disputes!.Count);

            return new PagedResult<Dispute>
            {
                Items = disputes!,
                TotalCount = disputes!.Count,
                Page = 1,
                PageSize = disputes!.Count
            };
        }

        public async Task<Dispute> OpenDisputeAsync(OpenDisputeRequest cmd)
        {
            var response = await _api.PostAsync<OpenDisputeRequest, OpenDisputeResponse>($"/disputes", cmd);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Created dispute for order {orderId}. Got ID {disputeId} for it.",
                    cmd.OrderId, response!.DisputeId);

            return new Dispute
            {
                DisputeId = response!.DisputeId,
                OrderId = cmd.OrderId,
                Status = response.Status,
                Attachments = cmd.Attachments,
                CreatedAt = response.CreatedAt ?? DateTime.UtcNow,
                Description = cmd.Description,
                CustomerUsername = response.CustomerUsername,
                OwnerUsername = response.OwnerUsername,
                Reason = cmd.Reason
            };
        }

        public async Task ResolveDisputeAsync(int disputeId, ResolveDisputeRequest decision)
        {
            await _api.PostAsync($"/disputes/{disputeId}/resolve", decision);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Resolved dispute with ID {id}.", disputeId);
        }

        public async Task SubmitClarificationAsync(int disputeId, SubmitClarificationRequest payload)
        {
            await _api.PostAsync($"/disputes/{disputeId}/clarification", payload);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Submitted clarification for dispute with ID {id}.", disputeId);
        }
    }
}
