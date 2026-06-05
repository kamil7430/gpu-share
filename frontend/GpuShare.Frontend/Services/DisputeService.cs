using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services
{
    public class DisputeService : IDisputeService
    {
        public Task<Dispute> GetDisputeAsync(int disputeId)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<Dispute>> ListDisputesAsync(DisputeQueryParams parameters)
        {
            throw new NotImplementedException();
        }

        public Task<Dispute> OpenDisputeAsync(OpenDisputeRequest cmd)
        {
            throw new NotImplementedException();
        }

        public Task ResolveDisputeAsync(int disputeId, ResolveDisputeRequest decision)
        {
            throw new NotImplementedException();
        }

        public Task SubmitClarificationAsync(int disputeId, SubmitClarificationRequest payload)
        {
            throw new NotImplementedException();
        }
    }
}
