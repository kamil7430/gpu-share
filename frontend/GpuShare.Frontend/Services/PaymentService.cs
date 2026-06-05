using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services
{
    public class PaymentService : IPaymentService
    {
        public Task<WalletBalance> GetBalanceAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<Transaction>> GetTransactionsAsync(TransactionQueryParams parameters)
        {
            throw new NotImplementedException();
        }

        public Task<TransferResponse> TransferAsync(decimal amount, PaymentMethod method)
        {
            throw new NotImplementedException();
        }
    }
}
