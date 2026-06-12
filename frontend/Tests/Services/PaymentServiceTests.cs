using FluentAssertions;
using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GpuShare.Frontend.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly HttpClient _http;
        private readonly ApiClient _apiClient;
        private readonly ILogger<PaymentService> _logger;
        private readonly PaymentService _sut;
        private readonly JsonSerializerOptions _options;

        public PaymentServiceTests()
        {
            _mockHttp = new MockHttpMessageHandler();
            _http = _mockHttp.ToHttpClient();
            _http.BaseAddress = new Uri("https://localhost:5001");
            _logger = NullLogger<PaymentService>.Instance;

            _apiClient = new ApiClient(_http, NullLogger<ApiClient>.Instance);
            _sut = new PaymentService(_apiClient, _logger);

            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private readonly string _walletJson = """
            {
                "totalUsdCents": 12550,
                "lockedUsdCents": 3500
            }
            """;

        private readonly WalletBalance _walletBalance = new()
        {
            TotalUsdCents = 12550,
            LockedUsdCents = 3500
        };

        private readonly TopUpRequest _topUpRequest = new()
        {
            AmountUsdCents = 10000,
            Method = PaymentMethod.CARD
        };

        private readonly WithdrawRequest _withdrawRequest = new()
        {
            AmountUsdCents = 5000,
            Method = PaymentMethod.BANK_TRANSFER
        };

        private readonly TransferResponse _transferResponse = new()
        {
            TransactionId = 123,
            Status = TransactionStatus.PENDING,
            PaymentProvider = "Stripe",
            PaymentUrl = "https://paymentprovider.com/pay/123"
        };

        private readonly string _transferJson = """
            {
                "transactionId": 123,
                "paymentUrl": "https://paymentprovider.com/pay/123",
                "paymentProvider": "Stripe",
                "status": "PENDING"
            }
            """;

        private readonly TransactionQueryParams _query = new()
        {
            Type = TransactionType.RESERVATION,
            Status = TransactionStatus.COMPLETED,
            Limit = 66
        };

        private readonly List<Transaction> _transactions = [
            new(){
                TransactionId = 1,
                AmountUsdCents = 10000,
                Type = TransactionType.TOPUP,
                Status = TransactionStatus.COMPLETED
            },
            new(){
                TransactionId = 2,
                AmountUsdCents = 5000,
                Type = TransactionType.WITHDRAWAL,
                Status = TransactionStatus.PENDING
            }
            ];

        private readonly string _transactionsJson = """
            [
            {
                "transactionId": 1,
                "amountUsdCents": 10000,
                "type": "TOPUP",
                "status": "COMPLETED"
            },
            {
                "transactionId": 2,
                "amountUsdCents": 5000,
                "type": "WITHDRAWAL",
                "status": "PENDING"
            }
            ]
            """;

        private readonly PayoutAccount _payoutAccount = new()
        {
            BankName = "Scamtander",
            AccountNumber = "12311234"
        };

        private readonly string _payoutJson = """
            {
                "bankName": "Scamtander",
                "accountNumber": "12311234"
            }
            """;

        // =====================================================
        // GET BALANCE
        // =====================================================

        [Fact]
        public async Task GetWalletBalanceAsync_Should_Map_Response()
        {
            await ApiContract<object, WalletBalance>
                .Get(_mockHttp, () => _sut.GetWalletBalanceAsync())
                .To("/api/wallet")
                .Returns(_walletJson)
                .ShouldMapTo(_walletBalance);
        }

        [Fact]
        public async Task GetWalletBalanceAsync_Should_Throw_When_Unauthorized()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/wallet")
                .Respond(HttpStatusCode.Unauthorized);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.GetWalletBalanceAsync(),
                HttpStatusCode.Unauthorized);
        }

        // =====================================================
        // GET TRANSACTIONS
        // =====================================================

        [Fact]
        public async Task GetTransactionsAsync_Should_Send_Query_Parameters()
        {
            await ApiContract<object, PagedResult<Transaction>>
                .Get(_mockHttp, () => _sut.GetTransactionsAsync(_query))
                .To("/api/wallet/transactions")
                .Returns(_transactionsJson)
                .ExpectQuery(q =>
                {
                    q["limit"].Should().Be("66");
                    q["type"].Should().Be("Deposit");
                })
                .ExecuteAction();
        }

        [Fact]
        public async Task GetTransactionsAsync_Should_Map_Response()
        {
            await ApiContract<object, PagedResult<Transaction>>
                .Get(_mockHttp, () => _sut.GetTransactionsAsync(_query))
                .To("/api/wallet/transactions")
                .Returns(_transactionsJson)
                .ShouldMapTo(new PagedResult<Transaction>()
                {
                    Items = _transactions,
                    Page = 1,
                    TotalCount = 2,
                    PageSize = 2
                });
        }

        [Fact]
        public async Task GetTransactionsAsync_Should_Throw_When_Unauthorized()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/wallet/transactions*")
                .Respond(HttpStatusCode.Unauthorized);

            await ApiErrorAssertions.ShouldFailWith(
                () => _sut.GetTransactionsAsync(_query),
                HttpStatusCode.Unauthorized);
        }

        // =====================================================
        // TOP UP
        // =====================================================

        [Fact]
        public async Task TopUpAsync_Should_Send_Correct_Request()
        {
            await ApiContract<TopUpRequest, TransferResponse>
                .Post(_mockHttp, () => _sut.TopUpAsync(_topUpRequest))
                .To("/api/wallet/transfer")
                .Returns(_transferJson)
                .ShouldSendBody(req =>
                {
                    req.AmountUsdCents.Should().Be(10000);
                    req.Method.Should().Be(PaymentMethod.CARD);
                });
        }

        [Fact]
        public async Task TopUpAsync_Should_Map_Response()
        {
            await ApiContract<TopUpRequest, TransferResponse>
                .Post(_mockHttp, () => _sut.TopUpAsync(_topUpRequest))
                .To("/api/wallet/transfer")
                .Returns(_transferJson)
                .ShouldMapTo(_transferResponse);
        }

        [Fact]
        public async Task TopUpAsync_Should_Throw_On_Invalid_Amount()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/api/wallet/transfer")
                .Respond(HttpStatusCode.BadRequest);

            await ApiErrorAssertions.ShouldFailWith(
                () => _sut.TopUpAsync(new TopUpRequest
                {
                    AmountUsdCents = -1,
                    Method = PaymentMethod.CARD
                }),
                HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task TopUpAsync_Should_Throw_When_Provider_Rejected()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/api/wallet/transfer")
                .Respond(HttpStatusCode.PaymentRequired);

            await ApiErrorAssertions.ShouldFailWith(
                () => _sut.TopUpAsync(_topUpRequest),
                HttpStatusCode.PaymentRequired);
        }

        // =====================================================
        // WITHDRAW
        // =====================================================

        [Fact]
        public async Task WithdrawAsync_Should_Send_Correct_Request()
        {
            await ApiContract<WithdrawRequest, TransferResponse>
                .Post(_mockHttp, () => _sut.WithdrawAsync(_withdrawRequest))
                .To("/api/wallet/transfer")
                .Returns(_transferJson)
                .ShouldSendBody(req =>
                {
                    req.AmountUsdCents.Should().Be(5000);
                    req.Method.Should().Be(PaymentMethod.BANK_TRANSFER);
                });
        }

        [Fact]
        public async Task WithdrawAsync_Should_Map_Response()
        {
            await ApiContract<WithdrawRequest, TransferResponse>
                .Post(_mockHttp, () => _sut.WithdrawAsync(_withdrawRequest))
                .To("/api/wallet/transfer")
                .Returns(_transferJson)
                .ShouldMapTo(_transferResponse);
        }

        [Fact]
        public async Task WithdrawAsync_Should_Throw_When_Insufficient_Balance()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/api/wallet/transfer")
                .Respond(HttpStatusCode.PaymentRequired);

            await ApiErrorAssertions.ShouldFailWith(
                () => _sut.WithdrawAsync(_withdrawRequest),
                HttpStatusCode.PaymentRequired);
        }

        // =====================================================
        // GET PAYOUT ACCOUNT
        // =====================================================

        [Fact]
        public async Task GetPayoutAccountAsync_Should_Map_Response()
        {
            await ApiContract<object, PayoutAccount>
                .Get(_mockHttp, () => _sut.GetPayoutAccountAsync())
                .To("/api/wallet/payout-account")
                .Returns(_payoutJson)
                .ShouldMapTo(_payoutAccount);
        }

        [Fact]
        public async Task GetPayoutAccountAsync_Should_Throw_On_Unauthorized()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/wallet/payout-account")
                .Respond(HttpStatusCode.Unauthorized);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.GetPayoutAccountAsync(),
                HttpStatusCode.Unauthorized);
        }

        // =====================================================
        // SET PAYOUT ACCOUNT
        // =====================================================

        [Fact]
        public async Task SavePayoutAccountAsync_Should_Send_Request()
        {
            await ApiContract<PayoutAccount, object>
                .Post(_mockHttp, () => _sut.SavePayoutAccountAsync(_payoutAccount))
                .To("/api/wallet/payout-account")
                .NoReturn()
                .ShouldSendBodyVoid(req =>
                {
                    req.BankName.Should().Be("Scamtander");
                    req.AccountNumber.Should().Be("12311234");
                });
        }
            
        [Fact]
        public async Task SavePayoutAccountAsync_Should_Throw_On_Invalid_Data()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/api/wallet/payout-account")
                .Respond(HttpStatusCode.BadRequest);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.SavePayoutAccountAsync(new PayoutAccount()),
                HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SavePayoutAccountAsync_Should_Throw_On_Unauthorized()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/api/wallet/payout-account")
                .Respond(HttpStatusCode.Unauthorized);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.SavePayoutAccountAsync(new PayoutAccount()),
                HttpStatusCode.Unauthorized);
        }
    }
}
