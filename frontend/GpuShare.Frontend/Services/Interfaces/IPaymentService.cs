namespace GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

public interface IPaymentService
{
    /// <summary>
    /// GET /api/wallet
    /// Returns current wallet balance and locked funds.
    /// </summary>
    Task<WalletBalance> GetWalletBalanceAsync();

    /// <summary>
    /// POST /api/wallet/transfer
    /// Initiates wallet topup via payment provider.
    /// </summary>
    Task<TransferResponse> TopUpAsync(TopUpRequest request);

    /// <summary>
    /// POST /api/wallet/transfer
    /// Initiates wallet withdrawal via payment provider.
    /// </summary>
    Task<TransferResponse> WithdrawAsync(WithdrawRequest request);

    /// <summary>
    /// GET /api/wallet/transactions
    /// Returns paginated transaction history.
    /// </summary>
    Task<PagedResult<Transaction>> GetTransactionsAsync(TransactionQueryParams query);

    /// <summary>
    /// GET /api/wallet/payout-account
    /// Returns the user's payout account information.
    /// </summary>
    /// <returns></returns>
    Task<PayoutAccount> GetPayoutAccountAsync();

    /// <summary>
    /// POST /api/wallet/payout-account
    /// Saves the user's payout account information.
    /// </summary>
    /// <returns></returns>
    Task SavePayoutAccountAsync(PayoutAccount request);

}