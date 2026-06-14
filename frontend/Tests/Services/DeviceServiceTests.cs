using FluentAssertions;
using GpuShare.Frontend.Auth;
using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;
using Xunit;

namespace GpuShare.Frontend.Tests.Services
{
    public class DeviceServiceTests
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly HttpClient _httpClient;
        private readonly IApiClient _apiClient;
        private readonly ILogger<DeviceService> _logger;
        private readonly DeviceService _sut;

        public DeviceServiceTests()
        {
            _mockHttp = new();
            _httpClient = _mockHttp.ToHttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:5001");
            _apiClient = new ApiClient(_httpClient, NullLogger<ApiClient>.Instance);
            _logger = NullLogger<DeviceService>.Instance;
            _sut = new DeviceService(_apiClient, _logger);
        }

        private readonly List<Device> _devices = [
            new Device()
            {
                DeviceId = 123,
                Name = "Workstation-Alpha",
                OwnerUsername = "julie",
                State = DeviceState.AVAILABLE,
                GpuModel = "RTX 4090",
                VramMb = 24576,
                CudaCores = 16384,
                DriverVersion = "535.104",
                PricePerHourUsdCents = 450,
            },
            new Device()
            {
                DeviceId = 124,
                Name = "Workstation-Alpha 2",
                OwnerUsername = "john",
                State = DeviceState.UNAVAILABLE,
                GpuModel = "RTX 4080",
                VramMb = 24000,
                CudaCores = 16000,
                DriverVersion = "535.105",
                PricePerHourUsdCents = 550,
            },
        ];

        private readonly string _devicesJson = """
                [
                    {
                        "deviceId" : "123",
                        "name" : "Workstation-Alpha",
                        "gpuModel": "RTX 4090",
                        "vramMb": 24576,
                        "cudaCores": 16384,
                        "pricePerHourUsdCents": 450,
                        "driverVersion": "535.104",
                        "state": "AVAILABLE",
                        "ownerUsername": "julie"
                    },
                    {
                        "deviceId" : "124",
                        "name" : "Workstation-Alpha 2",
                        "gpuModel": "RTX 4080",
                        "vramMb": 24000,
                        "cudaCores": 16000,
                        "pricePerHourUsdCents": 550,
                        "driverVersion": "535.105",
                        "state": "UNAVAILABLE",
                        "ownerUsername": "john"
                    }
                ]
                """;

        private readonly string _device1Json = """
                {
                        "deviceId" : "123",
                        "name" : "Workstation-Alpha",
                        "gpuModel": "RTX 4090",
                        "vramMb": 24576,
                        "cudaCores": 16384,
                        "pricePerHourUsdCents": 450,
                        "driverVersion": "535.104",
                        "state": "AVAILABLE",
                        "ownerUsername": "julie"
                    }
                """;

        private readonly DeviceSearchFilters _filters = new()
        {
            Name = "RTX",
            GpuModel = "RTX 4090",
            MinVramMb = 8000,
            MaxVramMb = 24000,
            MinPricePerHourUsdCents = 250,
            MaxPricePerHourUsdCents = 1000,
            MinCudaCores = 16000,
            MaxCudaCores = 24000,
            MinDriverVersion = "535.00",
            MaxDriverVersion = "536.00",
            AvailableOnly = true,
        };

        private static readonly DateTime _lastHeartbeat = DateTime.Now.AddSeconds(-10);

        private readonly DeviceStatus _status = new()
        {
            DeviceId = 123,
            State = DeviceState.AVAILABLE,
            TemperatureCelsius = 0,
            UtilizationPercent = 100,
            MemoryUsedMb = 0,
            LastHeartbeat = _lastHeartbeat
        };

        private readonly string _statusJson = """
                {
                    "deviceId": "123",
                    "state": "AVAILABLE",
                    "temperatureC": 0,
                    "utilizationPercent": 100,
                    "memoryUsedMb": 0,
                    "lastHeartbeat": "2026-06-05T14:49:06.772Z"
                }
            """;

        private readonly RegisterDeviceRequest _registerDeviceRequest = new()
        {
            Name = "Workstation-Alpha",
            GpuModel = "RTX 4090",
            VramMb = 24576,
            CudaCores = 16384,
            DriverVersion = "535.104",
            PricePerHourUsdCents = 450,
        };

        private readonly string _registerJson = """
                {
                    "deviceId": 123,
                    "ownerUsername": "julie",
                    "state": "AVAILABLE"
                }
                """;

        private readonly UpdateDeviceRequest _updateDeviceRequest = new() {
            Name = "Workstation-Alpha 2",
            State = DeviceState.UNAVAILABLE,
            GpuModel = "RTX 4080",
            VramMb = 24000,
            CudaCores = 16000,
            DriverVersion = "535.105",
            PricePerHourUsdCents = 550,
        };

        // =====================================================
        // SEARCH DEVICES
        // =====================================================

        [Fact]
        public async Task SearchDevicesAsync_Should_Call_Devices_Endpoint()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/devices")
                .Respond("application/json", _devicesJson);

            var act = async () => await _sut.SearchDevicesAsync(_filters);

            _mockHttp.VerifyNoOutstandingExpectation();
            _mockHttp.VerifyNoOutstandingRequest();
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SearchDevicesAsync_Should_Convert_Filters_To_Query_Parameters()
        {
            await ApiContract<object, PagedResult<Device>>
                .Get(_mockHttp, () => _sut.SearchDevicesAsync(_filters))
                .To("/api/devices")
                .Returns("[]")
                .ExpectQuery(q =>
                {
                    q["limit"].Should().Be("25");
                    q["name"].Should().Be("RTX");
                    q["gpuModel"].Should().Be("RTX 4090");

                    q["minVramMb"].Should().Be("8000");
                    q["maxVramMb"].Should().Be("24000");

                    q["minPricePerHourUsdCents"].Should().Be("250");
                    q["maxPricePerHourUsdCents"].Should().Be("1000");

                    q["minCudaCores"].Should().Be("16000");
                    q["maxCudaCores"].Should().Be("24000");

                    q["minDriverVersion"].Should().Be("535.00");
                    q["maxDriverVersion"].Should().Be("536.00");
                }).ExecuteAction();
        }

        [Fact]
        public async Task SearchDevicesAsync_Should_Map_Response_To_Devices()
        {
            await ApiContract<object, PagedResult<Device>>
                .Get(_mockHttp, async () => await _sut.SearchDevicesAsync(new DeviceSearchFilters()))
                .To("/api/devices")
                .Returns(_devicesJson)
                .ShouldMapTo(new PagedResult<Device>() { TotalCount = _devices.Count, 
                    Page = 1, PageSize = 2, Items = _devices });
        }

        [Fact]
        public async Task SearchDevicesAsync_Should_Throw_On_400()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/devices")
                .Respond(HttpStatusCode.BadRequest);

            var act = async () => await _sut.SearchDevicesAsync(new DeviceSearchFilters());

            var exception = await act.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SearchDevicesAsync_Should_Throw_When_No_Devices_Found()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/devices")
                .Respond(HttpStatusCode.NotFound);

            var act = async () => await _sut.SearchDevicesAsync(new DeviceSearchFilters());

            var exception = await act.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =====================================================
        // GET DEVICE
        // =====================================================

        [Fact]
        public async Task GetDeviceAsync_Should_Call_Correct_Endpoint()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/devices/123")
                .Respond("application/json", _device1Json);

            var act = async () => await _sut.GetDeviceAsync(123);

            _mockHttp.VerifyNoOutstandingExpectation();
            _mockHttp.VerifyNoOutstandingRequest();
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetDeviceAsync_Should_Return_Device()
        {
            await ApiContract<object, Device>
                .Get(_mockHttp, async () => await _sut.GetDeviceAsync(123))
                .To("https://localhost:5001/api/devices/123")
                .Returns(_device1Json)
                .ShouldMapTo(_devices[0]);
        }

        [Fact]
        public async Task GetDeviceAsync_Should_Throw_On_404()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/devices/123")
                .Respond(HttpStatusCode.NotFound);

            var act = async () => await _sut.GetDeviceAsync(123);

            var exception = await act.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);    
        }

        // =====================================================
        // GET DEVICE STATUS
        // =====================================================

        [Fact]
        public async Task GetDeviceStatusAsync_Should_Call_Status_Endpoint()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/devices/123/status")
                .Respond("application/json", JsonSerializer.Serialize(_status));

            var act = async () => await _sut.GetDeviceStatusAsync(123);

            _mockHttp.VerifyNoOutstandingExpectation();
            _mockHttp.VerifyNoOutstandingRequest();
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetDeviceStatusAsync_Should_Map_Response()
        {
            // Arrange
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/devices/123/status")
                .Respond("application/json", _statusJson);

            // Act
            var status = await _sut.GetDeviceStatusAsync(123);

            // Assert
            //status.Should().NotBeNull();

            //status.DeviceId.Should().Be(123);
            //status.State.Should().Be(DeviceState.AVAILABLE);
            //status.TemperatureCelsius.Should().Be(0);
            //status.MemoryUsedMb.Should().Be(0);
            //status.UtilizationPercent.Should().Be(100);
            //status.LastHeartbeat.Should().Be(DateTime.Parse("2026-06-05T14:49:06.772Z"));

            await ApiContract<object, DeviceStatus>
                .Get(_mockHttp, async () => await _sut.GetDeviceStatusAsync(123))
                .To("https://localhost:5001/api/devices/123/status")
                .Returns(_statusJson)
                .ShouldMapTo(_status);
        }

        [Fact]
        public async Task GetDeviceStatusAsync_Should_Throw_On_404()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/api/devices/123/status")
                .Respond(HttpStatusCode.NotFound);

            var act = async () => await _sut.GetDeviceStatusAsync(123);

            var exception = await act.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // =====================================================
        // REGISTER DEVICE
        // =====================================================

        [Fact]
        public async Task RegisterDeviceAsync_Should_Send_Correct_Request()
        {
            await ApiContract<RegisterDeviceRequest, Device>
                .Post(_mockHttp, () => _sut.RegisterDeviceAsync(_registerDeviceRequest))
                .ExpectStatus(HttpStatusCode.Created)
                .To("https://localhost:5001/api/devices")
                //.WithHeader("Authorization", "Bearer test-token")
                //.WithQuery("source", "frontend")
                .Returns(_registerJson)
                .ShouldSendBody(body =>
                {
                    body.Name.Should().Be("Workstation-Alpha");
                    body.VramMb.Should().Be(24576);
                    body.CudaCores.Should().Be(16384);
                    body.PricePerHourUsdCents.Should().Be(450);
                    body.DriverVersion.Should().Be("535.104");
                });
        }

        [Fact]
        public async Task RegisterDeviceAsync_Should_Return_Created_Device()
        {
           await ApiContract<object, Device>
                .Post(_mockHttp, () => _sut.RegisterDeviceAsync(_registerDeviceRequest))
                .ExpectStatus(HttpStatusCode.Created)
                .WithHeader("Authorization", "Bearer test-token")
                .To("https://localhost:5001/api/devices")
                .Returns(_registerJson)
                .ShouldMapTo(_devices[0]);
        }

        [Fact]
        public async Task RegisterDeviceAsync_Should_Throw_On_400()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/api/devices")
                .Respond(HttpStatusCode.BadRequest);

            var act = async () => await _sut.RegisterDeviceAsync(_registerDeviceRequest);

            var exception = await act.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RegisterDeviceAsync_Should_Throw_On_401()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/api/devices")
                .Respond(HttpStatusCode.Unauthorized);

            var act = async () => await _sut.RegisterDeviceAsync(_registerDeviceRequest);

            var exception = await act.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // =====================================================
        // UPDATE DEVICE
        // =====================================================

        [Fact]
        public async Task UpdateDeviceAsync_Should_Call_Correct_Endpoint() 
        {
            _mockHttp.When(HttpMethod.Patch, "https://localhost:5001/api/devices/123")
                .Respond(HttpStatusCode.OK);

            var act = async () => await _sut.UpdateDeviceAsync(123, _devices[0], _updateDeviceRequest);

            _mockHttp.VerifyNoOutstandingExpectation();
            _mockHttp.VerifyNoOutstandingRequest();
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task UpdateDeviceAsync_Should_Send_Correct_Payload()
        {
            //_mockHttp.When(HttpMethod.Patch, "https://localhost:5001/api/devices/123")
            //    .Respond(HttpStatusCode.OK);

            await ApiContract<UpdateDeviceRequest, Device>
                .Patch(_mockHttp, () => _sut.UpdateDeviceAsync(123, _devices[0], _updateDeviceRequest))
                .To("https://localhost:5001/api/devices/123")
                .ExpectStatus(HttpStatusCode.Created)
                //.WithHeader("Authorization", "Bearer test-token")
                .Returns("")
                .ShouldSendBody(body =>
                {
                    body.Name.Should().Be("Workstation-Alpha 2");
                    body.VramMb.Should().Be(24000);
                    body.CudaCores.Should().Be(16000);
                    body.PricePerHourUsdCents.Should().Be(550);
                    body.DriverVersion.Should().Be("535.105");
                });
        }

        [Fact]
        public async Task UpdateDeviceAsync_Should_Return_Updated_Device()
        {
            _mockHttp.When(HttpMethod.Patch, "https://localhost:5001/api/devices/123")
                .Respond(HttpStatusCode.OK);

            var device = await _sut.UpdateDeviceAsync(123, _devices[0], _updateDeviceRequest);

            device.Name.Should().Be("Workstation-Alpha 2");
            device.VramMb.Should().Be(24000);
            device.CudaCores.Should().Be(16000);
            device.PricePerHourUsdCents.Should().Be(550);
            device.DriverVersion.Should().Be("535.105");
        }

        [Fact]
        public async Task UpdateDeviceAsync_Should_Throw_On_400()
        {
            _mockHttp.When(HttpMethod.Patch, "https://localhost:5001/api/devices/123")
                .Respond(HttpStatusCode.BadRequest);

            var act = async () => await _sut.UpdateDeviceAsync(123, _devices[0], _updateDeviceRequest);

            var exception = await act.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // =====================================================
        // REMOVE DEVICE
        // =====================================================

        [Fact]
        public async Task RemoveDeviceAsync_Should_Call_Delete_Endpoint()
        {
            _mockHttp.When(HttpMethod.Delete, "https://localhost:5001/api/devices/123")
                .Respond(HttpStatusCode.OK);

            var act = async () => await _sut.DeleteDeviceAsync(123);

            _mockHttp.VerifyNoOutstandingExpectation();
            _mockHttp.VerifyNoOutstandingRequest();
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task RemoveDeviceAsync_Should_Throw_On_404()
        {
            _mockHttp.When(HttpMethod.Delete, "https://localhost:5001/api/devices/123")
                .Respond(HttpStatusCode.NotFound);

            var act = async () => await _sut.DeleteDeviceAsync(123);

            var exception = await act.Should().ThrowAsync<ApiException>();
            exception.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
