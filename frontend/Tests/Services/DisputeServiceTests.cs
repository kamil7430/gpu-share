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
    public class DisputeServiceTests
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly HttpClient _http;
        private readonly ApiClient _apiClient;
        private readonly ILogger<DisputeService> _logger;
        private readonly DisputeService _sut;
        private readonly JsonSerializerOptions _options;

        public DisputeServiceTests()
        {
            _mockHttp = new MockHttpMessageHandler();
            _http = _mockHttp.ToHttpClient();
            _http.BaseAddress = new Uri("https://localhost:5001");
            _logger = NullLogger<DisputeService>.Instance;

            _apiClient = new ApiClient(_http, NullLogger<ApiClient>.Instance);
            _sut = new DisputeService(_apiClient, _logger);

            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private readonly OpenDisputeRequest _openDisputeRequest = new()
        {
            OrderId = 123,
            Reason = "Hardware mismatch",
            Description = "GPU not as describedddddddddddddddd",
            Attachments = [new() { FileName = "screenshot.png", Url = "https://example.com/screenshot.png" }]
        };

        private readonly string _openDisputeResponseJson = """
            {
                "disputeId": 10,
                "orderId": 123,
                "status": "OPEN",
                "createdAt": "2026-06-06T00:00:00Z",
                "customerUsername": "customer1",
                "ownerUsername": "owner1"
            }
            """;

        private readonly Dispute _dispute = new()
        {
            DisputeId = 10,
            OrderId = 123,
            CustomerUsername = "customer1",
            OwnerUsername = "owner1",
            Reason = "Hardware mismatch",
            Status = DisputeStatus.OPEN,
            Description = "GPU not as describedddddddddddddddd",
            CreatedAt = DateTime.UtcNow,
            Attachments = [new() { FileName = "screenshot.png", Url = "https://example.com/screenshot.png" }]
        };

        private readonly DisputeQueryParams _queryParams = new()
        {
            Status = DisputeStatus.OPEN,
            Limit = 10
        };

        private readonly string _disputeJson = """
            
            {
                "disputeId": 10,
                "orderId": 123,
                "customerUsername": "customer1",
                "ownerUsername": "owner1",
                "reason": "Hardware mismatch",
                "status": "OPEN",
                "description": "GPU not as describedddddddddddddddd",
                "createdAt": "2026-06-06T00:00:00Z",
                "attachments": [
                    {
                        "fileName": "screenshot.png",
                        "url": "https://example.com/screenshot.png"
                    }
                ]
            }
            
            """;

        private readonly string _disputeListResponseJson = """
            [
                {
                    "disputeId": 10,
                    "orderId": 123,
                    "customerUsername": "customer1",
                    "ownerUsername": "owner1",
                    "reason": "Hardware mismatch",
                    "status": "OPEN",
                    "description": "GPU not as describedddddddddddddddd",
                    "createdAt": "2026-06-06T00:00:00Z",
                    "attachments": [
                        {
                            "fileName": "screenshot.png",
                            "url": "https://example.com/screenshot.png"
                        }
                    ]
                }
            ]
            """;

        private readonly SubmitClarificationRequest _submitClarificationRequest = new()
        {
            Message = "Here is additional proof",
            Attachments = [new() { FileName = "screenshot.png", Url = "https://example.com/screenshot.png" }]
        };

        private readonly ResolveDisputeRequest _resolveDisputeRequest = new()
        {
            Decision = "CustomerFavor",
            Justification = "Customer was correct",
            RefundAmountUsdCents = 10000
        };

        // =====================================================
        // OPEN DISPUTE
        // =====================================================

        [Fact]
        public async Task OpenDisputeAsync_Should_Send_Request()
        {
            await ApiContract<OpenDisputeRequest, Dispute>
                .Post(_mockHttp, () => _sut.OpenDisputeAsync(_openDisputeRequest))
                .To("/disputes")
                .Returns(_openDisputeResponseJson)
                .ShouldSendBody(body =>
                {
                    body.OrderId.Should().Be(_openDisputeRequest.OrderId);
                    body.Description.Should().Be(_openDisputeRequest.Description);
                    body.Reason.Should().Be(_openDisputeRequest.Reason);
                });
        }

        [Fact]
        public async Task OpenDisputeAsync_Should_Map_Response()
        {
            await ApiContract<OpenDisputeRequest, Dispute>
                .Post(_mockHttp, () => _sut.OpenDisputeAsync(_openDisputeRequest))
                .To("/disputes")
                .Returns(_openDisputeResponseJson)
                .ShouldMapTo(_dispute);
        }

        [Fact]
        public async Task OpenDisputeAsync_Should_Throw_On_BadRequest()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/disputes")
                .Respond(HttpStatusCode.BadRequest);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.OpenDisputeAsync(_openDisputeRequest),
                HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task OpenDisputeAsync_Should_Throw_On_Conflict()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/disputes")
                .Respond(HttpStatusCode.Conflict);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.OpenDisputeAsync(_openDisputeRequest),
                HttpStatusCode.Conflict);
        }

        // =====================================================
        // GET DISPUTE
        // =====================================================

        [Fact]
        public async Task GetDisputeAsync_Should_Map_Response()
        {
            await ApiContract<object, Dispute>
                .Get(_mockHttp, () => _sut.GetDisputeAsync(10))
                .To("/disputes/10")
                .Returns(_disputeJson)
                .ShouldMapTo(_dispute);
        }

        [Fact]
        public async Task GetDisputeAsync_Should_Throw_When_NotFound()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/disputes/10")
                .Respond(HttpStatusCode.NotFound);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.GetDisputeAsync(10),
                HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetDisputeAsync_Should_Throw_When_Unauthorized()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/disputes/10")
                .Respond(HttpStatusCode.Unauthorized);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.GetDisputeAsync(10),
                HttpStatusCode.Unauthorized);
        }

        // =====================================================
        // LIST DISPUTES
        // =====================================================

        [Fact]
        public async Task ListDisputesAsync_Should_Send_Query_Parameters()
        {
            await ApiContract<object, PagedResult<Dispute>>
                .Get(_mockHttp, () => _sut.ListDisputesAsync(_queryParams))
                .To("/disputes")
                .Returns(_disputeListResponseJson)
                .ExpectQuery(q =>
                {
                    q["status"].Should().Be(_queryParams.Status.ToString());
                    q["limit"].Should().Be(_queryParams.Limit.ToString());
                })
                .ExecuteAction();
        }

        [Fact]
        public async Task ListDisputesAsync_Should_Map_Response()
        {
            await ApiContract<object, PagedResult<Dispute>>
                .Get(_mockHttp, () => _sut.ListDisputesAsync(_queryParams))
                .To("/disputes")
                .Returns(_disputeListResponseJson)
                .ShouldMapTo(new PagedResult<Dispute>()
                {
                    Items = [_dispute],
                    TotalCount = 1,
                    Page = 1,
                    PageSize = 1
                });
        }

        [Fact]
        public async Task ListDisputesAsync_Should_Throw_When_BadRequest()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/disputes")
                .Respond(HttpStatusCode.BadRequest);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.ListDisputesAsync(_queryParams),
                HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ListDisputesAsync_Should_Throw_When_NotFound()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/disputes")
                .Respond(HttpStatusCode.NotFound);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.ListDisputesAsync(_queryParams),
                HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ListDisputesAsync_Should_Throw_When_Unauthorized()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/disputes")
                .Respond(HttpStatusCode.Unauthorized);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.ListDisputesAsync(_queryParams),
                HttpStatusCode.Unauthorized);
        }

        // =====================================================
        // SUBMIT CLARIFICATION
        // =====================================================

        [Fact]
        public async Task SubmitClarificationAsync_Should_Send_Request()
        {
            await ApiContract<SubmitClarificationRequest, object>
                .Post(_mockHttp, () => _sut.SubmitClarificationAsync(10, _submitClarificationRequest))
                .To("/disputes/10/clarification")
                .NoReturn()
                .ShouldSendBodyVoid(body =>
                {
                    body.Message.Should().Be(_submitClarificationRequest.Message);
                    body.Attachments.Should().Equal(_submitClarificationRequest.Attachments);
                });
        }

        [Fact]
        public async Task SubmitClarificationAsync_Should_Throw_On_BadRequest()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/disputes/10/clarification")
                .Respond(HttpStatusCode.BadRequest);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.SubmitClarificationAsync(10, _submitClarificationRequest),
                HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SubmitClarificationAsync_Should_Throw_On_Unauthorized()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/disputes/10/clarification")
                .Respond(HttpStatusCode.Unauthorized);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.SubmitClarificationAsync(10, _submitClarificationRequest),
                HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SubmitClarificationAsync_Should_Throw_On_NotFound()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/disputes/10/clarification")
                .Respond(HttpStatusCode.NotFound);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.SubmitClarificationAsync(10, _submitClarificationRequest),
                HttpStatusCode.NotFound);
        }

        // =====================================================
        // RESOLVE DISPUTE
        // =====================================================

        [Fact]
        public async Task ResolveDisputeAsync_Should_Send_Request()
        {
            await ApiContract<ResolveDisputeRequest, object>
                .Post(_mockHttp, () => _sut.ResolveDisputeAsync(10, _resolveDisputeRequest))
                .To("/disputes/10/resolve")
                .NoReturn()
                .ShouldSendBodyVoid(body =>
                {
                    body.Decision.Should().Be(_resolveDisputeRequest.Decision);
                    body.Justification.Should().Be(_resolveDisputeRequest.Justification);
                    body.RefundAmountUsdCents.Should().Be(_resolveDisputeRequest.RefundAmountUsdCents);
                });
        }

        [Fact]
        public async Task ResolveDisputeAsync_Should_Throw_On_BadRequest()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/disputes/10/resolve")
                .Respond(HttpStatusCode.BadRequest);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.ResolveDisputeAsync(10, _resolveDisputeRequest),
                HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ResolveDisputeAsync_Should_Throw_On_Unauthorized()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/disputes/10/resolve")
                .Respond(HttpStatusCode.Unauthorized);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.ResolveDisputeAsync(10, _resolveDisputeRequest),
                HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ResolveDisputeAsync_Should_Throw_On_NotFound()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/disputes/10/resolve")
                .Respond(HttpStatusCode.NotFound);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.ResolveDisputeAsync(10, _resolveDisputeRequest),
                HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ResolveDisputeAsync_Should_Throw_On_Forbidden()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/disputes/10/resolve")
                .Respond(HttpStatusCode.Forbidden);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.ResolveDisputeAsync(10, _resolveDisputeRequest),
                HttpStatusCode.Forbidden);
        }
    }
}