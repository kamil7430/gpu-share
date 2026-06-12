using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services
{
    public class PaymentService(IApiClient api, ILogger<PaymentService> logger) : IPaymentService
    {
        private readonly IApiClient _api = api;
        private readonly ILogger<PaymentService> _logger = logger;

        public async Task<PayoutAccount> GetPayoutAccountAsync()
        {
            var account = await _api.GetAsync<PayoutAccount>($"/api/wallet/payout-account");
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Got account information with bank name {name}", account!.BankName);

            return account!;
        }

        public async Task<PagedResult<Transaction>> GetTransactionsAsync(TransactionQueryParams parameters)
        {
            var transactions = await _api.GetAsync<List<Transaction>>($"/api/wallet/transactions", parameters);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Got transactions.");

            return new PagedResult<Transaction>
            {
                Items = transactions!,
                TotalCount = transactions!.Count,
                Page = 1,
                PageSize = transactions!.Count
            };
        }

        public async Task<WalletBalance> GetWalletBalanceAsync()
        {
            var wallet = await _api.GetAsync<WalletBalance>($"/api/wallet");
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Got wallet information with total USD cents {total} and locked USD cents {locked}", 
                    wallet!.TotalUsdCents, wallet!.LockedUsdCents);

            return wallet!;
        }

        public async Task SavePayoutAccountAsync(PayoutAccount request)
        {
            await _api.PostAsync($"/api/wallet/payout-account", request);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Sent payout account information with bank name {name}.", request.BankName);
        }

        public async Task<TransferResponse> TopUpAsync(TopUpRequest request)
        {
            var response = await _api.PostAsync<TransferRequest, TransferResponse>($"/api/wallet/transfer", (TransferRequest)request);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Sent top-up request with amount {amount}.", request.AmountUsdCents);
            return response!;
        }

        public async Task<TransferResponse> WithdrawAsync(WithdrawRequest request)
        {
            var response = await _api.PostAsync<TransferRequest, TransferResponse>($"/api/wallet/transfer", (TransferRequest)request);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Sent withdraw request with amount {amount}.", request.AmountUsdCents);
            return response!;
        }
    }
}
