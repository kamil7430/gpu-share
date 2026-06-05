using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.State;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services;
using GpuShare.Frontend.Services.Interfaces;
using RichardSzalay.MockHttp;
using Xunit;
using GpuShare.Frontend.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GpuShare.Frontend.Tests.Services
{
    public class DeviceServiceTests
    {
        private static readonly MockHttpMessageHandler _mockHttp = new();
        private static readonly HttpClient _httpClient = _mockHttp.ToHttpClient();
        private static readonly IApiClient _apiClient = new ApiClient(_httpClient);
        private static readonly TestAuthState _authState = new();
        private readonly ILogger<DeviceService> _logger;
        private readonly DeviceService _sut;

        public DeviceServiceTests()
        {
            _httpClient.BaseAddress = new Uri("https://localhost:5001");
            _logger = NullLogger<DeviceService>.Instance;
            //_sut = new DeviceService(_apiClient, _authState, new MockJwtHelper(), _logger);
            _sut = new DeviceService();
        }

        private readonly List<Device> _devices = [
            new Device(){
                DeviceId = 1
            },
            new Device(){
                DeviceId = 2
            },
        ];

        // =====================================================
        // SEARCH DEVICES
        // =====================================================

        [Fact]
        public async Task SearchDevicesAsync_Should_Call_Devices_Endpoint()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/devices")
                .Respond("application/json", JsonSerializer.Serialize(_devices));
        }

        [Fact]
        public async Task SearchDevicesAsync_Should_Convert_Filters_To_Query_Parameters()
        {

        }

        [Fact]
        public async Task SearchDevicesAsync_Should_Map_Response_To_Devices()
        {

        }

        [Fact]
        public async Task SearchDevicesAsync_Should_Throw_On_400()
        {

        }

        [Fact]
        public async Task SearchDevicesAsync_Should_Throw_When_No_Devices_Found()
        {

        }

        // =====================================================
        // GET DEVICE
        // =====================================================

        [Fact]
        public async Task GetDeviceAsync_Should_Call_Correct_Endpoint()
        {

        }

        [Fact]
        public async Task GetDeviceAsync_Should_Return_Device()
        {

        }

        [Fact]
        public async Task GetDeviceAsync_Should_Throw_On_404()
        {

        }

        // =====================================================
        // GET DEVICE STATUS
        // =====================================================

        [Fact]
        public async Task GetDeviceStatusAsync_Should_Call_Status_Endpoint()
        {

        }

        [Fact]
        public async Task GetDeviceStatusAsync_Should_Map_Response()
        {

        }

        [Fact]
        public async Task GetDeviceStatusAsync_Should_Throw_On_404()
        {

        }

        // =====================================================
        // REGISTER DEVICE
        // =====================================================

        [Fact]
        public async Task RegisterDeviceAsync_Should_Send_Correct_Request()
        {

        }

        [Fact]
        public async Task RegisterDeviceAsync_Should_Return_Created_Device()
        {

        }

        [Fact]
        public async Task RegisterDeviceAsync_Should_Throw_On_400()
        {

        }

        [Fact]
        public async Task RegisterDeviceAsync_Should_Throw_On_401()
        {

        }

        // =====================================================
        // UPDATE DEVICE
        // =====================================================

        [Fact]
        public async Task UpdateDeviceAsync_Should_Call_Correct_Endpoint() 
        {
            
        }

        [Fact]
        public async Task UpdateDeviceAsync_Should_Send_Correct_Payload()
        {

        }

        [Fact]
        public async Task UpdateDeviceAsync_Should_Return_Updated_Device()
        {

        }

        [Fact]
        public async Task UpdateDeviceAsync_Should_Throw_On_400()
        {

        }

        // =====================================================
        // REMOVE DEVICE
        // =====================================================

        [Fact]
        public async Task RemoveDeviceAsync_Should_Call_Delete_Endpoint()
        {

        }

        [Fact]
        public async Task RemoveDeviceAsync_Should_Not_Throw_On_Success()
        {

        }

        [Fact]
        public async Task RemoveDeviceAsync_Should_Throw_On_404()
        {

        }
    }
}
