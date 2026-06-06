using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Text;

namespace GpuShare.Frontend.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly HttpClient _http;
        private readonly ApiClient _apiClient;
        private readonly ILogger<PaymentService> _logger;
        private readonly PaymentService _sut;

        public PaymentServiceTests()
        {
            _mockHttp = new MockHttpMessageHandler();
            _http = _mockHttp.ToHttpClient();
            _http.BaseAddress = new Uri("https://localhost:5001");
            _logger = NullLogger<PaymentService>.Instance;

            _apiClient = new ApiClient(_http, NullLogger<ApiClient>.Instance);
            _sut = new PaymentService(_apiClient, _logger);
        }

        // =====================================================
        // CREATE REVIEW
        // =====================================================
    }
}
