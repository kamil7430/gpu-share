using FluentAssertions;
using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services;
using MudBlazor.Charts;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GpuShare.Frontend.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly HttpClient _http;
        private readonly ApiClient _apiClient;
        private readonly ILogger<OrderService> _logger;
        private readonly OrderService _sut;

        public OrderServiceTests()
        {
            _mockHttp = new MockHttpMessageHandler();
            _http = _mockHttp.ToHttpClient();
            _http.BaseAddress = new Uri("https://localhost:5001");
            _logger = NullLogger<OrderService>.Instance;

            _apiClient = new ApiClient(_http, NullLogger<ApiClient>.Instance);
            _sut = new OrderService(_apiClient, _logger);
        }

        private readonly CreateOrderRequest _createOrderRequest = new()
        {
            DeviceId = 123,
            Username = "john",
            StartTime = DateTime.UtcNow.AddHours(1),
            DurationHours = 2,
            DockerImage = "gpu-image:latest"
        };

        private readonly CreateOrderResponse _createOrderResponse = new()
        {
            OrderId = 456,
            Status = OrderStatus.WAITING_FOR_START,
            ConnectionDetails = new ConnectionDetailsDto
            {
                Host = "gpu-server",
                Port = 12345,
                Protocol = "WSS"
            },
            TotalReservedCostCents = 900
        };

        private readonly string _createOrderJson = """
            {
                "orderId": "456",
                "status": "WAITING_FOR_START",
                "connectionDetails": {
                    "host": "gpu-server",
                    "port": "12345",
                    "protocol": "WSS"
                },
                "totalReservedCostCents": 900
            }
            """;

        private readonly Order _order = new()
        {
            OrderId = 456,
            DeviceId = 123,
            Username = "john",
            StartDate = DateTime.Now.AddHours(1),
            EndDate = DateTime.Now.AddHours(3),
            TotalReservedCostCents = 900,
            Status = OrderStatus.WAITING_FOR_START,
            ConnectionDetails = new()
            {
                Host = "gpu-server",
                Port = 12345,
                Protocol = "WSS"
            }
        };

        private readonly string _orderJson = """
            {
                "orderId": "456",
                "deviceId": "123",
                "status": "WAITING_FOR_START",
                "username": "john",
                "startDate": "2026-06-05T21:55:14.013Z",
                "endDate": "2026-06-05T21:55:14.013Z",
                "connectionDetails": {
                    "host": "gpu-server",
                    "port": "12345",
                    "protocol": "WSS"
                },
                "totalReservedCostCents": 900
            }
            """;

        private readonly List<Order> _orders = [
            new Order()
            {
                OrderId = 456,
                DeviceId = 123,
                Username = "john",
                TotalReservedCostCents = 900,
                Status = OrderStatus.WAITING_FOR_START,
                ConnectionDetails = new()
                {
                    Host = "gpu-server",
                    Port = 12345,
                    Protocol = "WSS"
                }
            },
            new Order()
            {
                OrderId = 457,
                DeviceId = 124,
                Username = "julie",
                TotalReservedCostCents = 1000,
                Status = OrderStatus.RUNNING,
                ConnectionDetails = new()
                {
                    Host = "gpu-server1",
                    Port = 12346,
                    Protocol = "WSS"
                }
            }
        ];

        private readonly string _ordersJson = """
            [{
                "orderId": "456",
                "deviceId": "123",
                "status": "WAITING_FOR_START",
                "username": "john",
                "connectionDetails": {
                    "host": "gpu-server",
                    "port": "12345",
                    "protocol": "WSS"
                },
                "totalReservedCostCents": 900
            },
            {
                "orderId": "457",
                "deviceId": "124",
                "status": "RUNNING",
                "username": "julie",
                "connectionDetails": {
                    "host": "gpu-server1",
                    "port": "12346",
                    "protocol": "WSS"
                },
                "totalReservedCostCents": 1000
            }]
            """;

        private readonly OrderQueryParams _filters = new()
        {
            DeviceId = 123,
            Username = "john",
            Status = OrderStatus.WAITING_FOR_START,

        };

        // =====================================================
        // CREATE ORDER
        // =====================================================

        [Fact]
        public async Task CreateOrderAsync_Should_Send_Correct_Request()
        {
            await ApiContract<CreateOrderRequest, Order>
                .Post(_mockHttp, () => _sut.CreateOrderAsync(_createOrderRequest))
                .To("/orders")
                .Returns(_createOrderJson)
                .ShouldSendBody(body =>
                {
                    body.DeviceId = 123;
                    body.DurationHours = 2;
                    body.DockerImage = "gpu-image:latest";
                });

            _mockHttp.VerifyNoOutstandingExpectation();
            _mockHttp.VerifyNoOutstandingRequest();
        }

        [Fact]
        public async Task CreateOrderAsync_Should_Map_Response()
        {
            await ApiContract<CreateOrderRequest, Order>
                .Post(_mockHttp, () => _sut.CreateOrderAsync(_createOrderRequest))
                .To("/orders")
                .Returns(_createOrderJson)
                .ShouldMapTo(_order);
        }

        [Fact]
        public async Task CreateOrderAsync_Should_Throw_On_400()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/orders")
                .Respond(HttpStatusCode.BadRequest);

            var act = async () => await _sut.CreateOrderAsync(_createOrderRequest);

            var exception = await act.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateOrderAsync_Should_Throw_On_402()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/orders")
                .Respond(HttpStatusCode.PaymentRequired);

            var act = async () => await _sut.CreateOrderAsync(_createOrderRequest);

            var exception = await act.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.PaymentRequired);
        }

        // =====================================================
        // GET ORDER
        // =====================================================

        [Fact]
        public async Task GetOrderAsync_Should_Call_Correct_Endpoint()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/orders/456")
                .Respond("application/json", _orderJson);

            var act = async () => await _sut.GetOrderAsync(456);

            _mockHttp.VerifyNoOutstandingExpectation();
            _mockHttp.VerifyNoOutstandingRequest();
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetOrderAsync_Should_Map_Order()
        {
            await ApiContract<object, Order>
                .Get(_mockHttp, () => _sut.GetOrderAsync(456))
                .To("/orders/456")
                .Returns(_orderJson)
                .ShouldMapTo(_order);
        }

        [Fact]
        public async Task GetOrderAsync_Should_Throw_On_404()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/orders/456")
                .Respond(HttpStatusCode.NotFound);

            var act = async () => await _sut.GetOrderAsync(456);

            var exception = await act.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =====================================================
        // LIST ORDERS
        // =====================================================

        [Fact]
        public async Task ListOrdersAsync_Should_Call_Devices_Endpoint()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/orders")
                .Respond("application/json", _ordersJson);

            var act = async () => await _sut.ListOrdersAsync(new OrderQueryParams());

            _mockHttp.VerifyNoOutstandingExpectation();
            _mockHttp.VerifyNoOutstandingRequest();
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ListOrdersAsync_Should_Convert_Filters_To_Query_Parameters()
        {
            await ApiContract<object, PagedResult<Order>>
                .Get(_mockHttp, () => _sut.ListOrdersAsync(_filters))
                .To("/orders")
                .Returns(_ordersJson)
                .ExpectQuery(q =>
                {
                    q["limit"].Should().Be("25");
                    q["deviceId"].Should().Be("123");
                    q["Status"].Should().Be("WAITING_FOR_START");
                    q["username"].Should().Be("john");
                }).ExecuteAction();
        }

        [Fact]
        public async Task ListOrdersAsync_Should_Map_Response_To_Orders()
        {
            await ApiContract<object, PagedResult<Order>>
                .Get(_mockHttp, () => _sut.ListOrdersAsync(_filters))
                .To("/orders")
                .Returns(_ordersJson)
                .ShouldMapTo(new PagedResult<Order>()
                {
                    Items = _orders,
                    Page = 1,
                    TotalCount = 2,
                    PageSize = 25
                });
        }

        [Fact]
        public async Task ListOrdersAsync_Should_Throw_On_400()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/orders")
                .Respond(HttpStatusCode.BadRequest);

            var act = async () => await _sut.ListOrdersAsync(_filters);

            var exception = await act.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ListOrdersAsync_Should_Throw_When_No_Orders_Found()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/orders")
                .Respond(HttpStatusCode.NotFound);

            var act = async () => await _sut.ListOrdersAsync(_filters);

            var exception = await act.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =====================================================
        // END ORDER
        // =====================================================

        [Fact]
        public async Task EndOrderAsync_Should_Call_Delete_Endpoint()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/orders/456/end")
                .Respond(HttpStatusCode.OK);

            var act = async () => await _sut.EndOrderAsync(456);

            _mockHttp.VerifyNoOutstandingExpectation();
            _mockHttp.VerifyNoOutstandingRequest();
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task EndOrderAsync_Should_Throw_On_404()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/orders/456/end")
                .Respond(HttpStatusCode.NotFound);

            var act = async () => await _sut.EndOrderAsync(456);

            var exception = await act.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
