using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using System.Net;

namespace GpuShare.Frontend.Services;

public class MockPaymentService : IPaymentService
{
    public Task<WalletBalance> GetWalletBalanceAsync() => Task.FromResult(MockStore.Wallet);

    public Task<TransferResponse> TopUpAsync(TopUpRequest request)
    {
        MockStore.Wallet.TotalUsdCents += request.AmountUsdCents;

        var tx = new Transaction
        {
            TransactionId = MockStore.NextId(),
            Type = TransactionType.TOPUP,
            AmountUsdCents = request.AmountUsdCents,
            Status = TransactionStatus.COMPLETED,
            CreatedAtUtc = DateTime.UtcNow,
            Description = $"Top up via {request.Method}",
        };
        MockStore.Transactions.Add(tx);

        return Task.FromResult(new TransferResponse
        {
            TransactionId = tx.TransactionId,
            PaymentUrl = "https://pay.mock.gpushare.io/checkout/mock_session",
            PaymentProvider = request.Method == PaymentMethod.PAYPAL ? "PayPal" : "Stripe",
            Status = TransactionStatus.COMPLETED,
        });
    }

    public Task<TransferResponse> WithdrawAsync(WithdrawRequest request)
    {
        if (request.AmountUsdCents > MockStore.Wallet.AvailableUsdCents)
            throw new ApiException("Insufficient available balance.", HttpStatusCode.UnprocessableEntity);

        MockStore.Wallet.TotalUsdCents -= request.AmountUsdCents;

        var tx = new Transaction
        {
            TransactionId = MockStore.NextId(),
            Type = TransactionType.WITHDRAWAL,
            AmountUsdCents = request.AmountUsdCents,
            Status = TransactionStatus.PENDING,
            CreatedAtUtc = DateTime.UtcNow,
            Description = $"Withdrawal via {request.Method}",
        };
        MockStore.Transactions.Add(tx);

        return Task.FromResult(new TransferResponse
        {
            TransactionId = tx.TransactionId,
            PaymentUrl = "",
            PaymentProvider = request.Method == PaymentMethod.PAYPAL ? "PayPal" : "Bank",
            Status = TransactionStatus.PENDING,
        });
    }

    public Task<PagedResult<Transaction>> GetTransactionsAsync(TransactionQueryParams query)
    {
        var q = MockStore.Transactions.AsEnumerable();

        if (query.Type.HasValue)
            q = q.Where(t => t.Type == query.Type.Value);
        if (query.Status.HasValue)
            q = q.Where(t => t.Status == query.Status.Value);
        if (query.From.HasValue)
            q = q.Where(t => t.CreatedAtUtc >= query.From.Value);
        if (query.To.HasValue)
            q = q.Where(t => t.CreatedAtUtc <= query.To.Value);

        var all = q.OrderByDescending(t => t.CreatedAtUtc).ToList();
        return Task.FromResult(new PagedResult<Transaction>
        {
            Items = all.Take(query.Limit).ToList(),
            TotalCount = all.Count,
            PageSize = query.Limit,
        });
    }

    public Task<PayoutAccount> GetPayoutAccountAsync()
    {
        if (MockStore.PayoutAccount is null)
            throw new ApiException("No payout account configured.", HttpStatusCode.NotFound);
        return Task.FromResult(MockStore.PayoutAccount);
    }

    public Task SavePayoutAccountAsync(PayoutAccount request)
    {
        MockStore.PayoutAccount = request;
        return Task.CompletedTask;
    }
}
