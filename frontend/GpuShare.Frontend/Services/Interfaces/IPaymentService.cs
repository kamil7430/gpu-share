namespace GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

public interface IPaymentService
{
    /// <summary>
    /// GET /api/wallet
    /// Returns current wallet balance and locked funds.
    /// </summary>
    Task<WalletBalance> GetBalanceAsync();

    /// <summary>
    /// POST /api/wallet/transfer
    /// Initiates wallet transfer via payment provider.
    /// </summary>
    Task<TransferResponse> TransferAsync(decimal amount, PaymentMethod method);

    /// <summary>
    /// GET /api/wallet/transactions
    /// Returns paginated transaction history.
    /// </summary>
    Task<PagedResult<Transaction>> GetTransactionsAsync(TransactionQueryParams parameters);

}