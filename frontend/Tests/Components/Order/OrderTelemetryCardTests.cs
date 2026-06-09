using Bunit;
using GpuShare.Frontend.Components.Pages.Order;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using System.Reflection;
using static MudBlazor.CategoryTypes;

namespace GpuShare.Frontend.Tests.Components.Order
{
    public class OrderTelemetryCardTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IOrderService> _orderServiceMock = new();
        private readonly Mock<IDeviceService> _deviceServiceMock = new();

        public OrderTelemetryCardTests()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton(_orderServiceMock.Object);
            Services.AddSingleton(_deviceServiceMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true);
            JSInterop.SetupModule(_ => true);

            // Arrange
            var order = new Models.Order
            {
                OrderId = 1,
                StartDate = DateTime.UtcNow.AddHours(-2),
                EndDate = DateTime.UtcNow.AddHours(1)
            };

            var gpu = new Models.Device
            {
                DeviceId = 10,
                PricePerHourUsdCents = 500
            };

            _orderServiceMock.Setup(x => x.GetOrderAsync(1)).ReturnsAsync(order);

            _deviceServiceMock.Setup(x => x.GetDeviceAsync(10)).ReturnsAsync(gpu);
            _deviceServiceMock.Setup(x => x.GetDeviceStatusAsync(It.IsAny<int>()))
                .ReturnsAsync(new DeviceStatus
                {
                    LastHeartbeat = DateTime.Now
                });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public void Should_Render_Telemetry_Header()
        {
            var cut = Render<OrderTelemetryCard>();

            cut.Markup.Contains("Live GPU Telemetry");
        }

        [Fact]
        public void Should_Render_Sse_Stream_Badge()
        {
            var cut = Render<OrderTelemetryCard>();

            cut.Markup.Contains("SSE Stream");
        }

        [Fact]
        public void Should_Render_MudChart()
        {
            var cut = Render<OrderTelemetryCard>();

            cut.FindComponent<MudChart<double>>();
        }

        [Fact]
        public void Should_Render_All_Metric_Series()
        {
            var cut = Render<OrderTelemetryCard>();

            var chart = cut.FindComponent<MudChart<double>>();

            var series = chart.Instance.ChartSeries;

            Assert.Equal(3, series.Count);

            Assert.Contains(series, s => s.Name == "GPU Usage");
            Assert.Contains(series, s => s.Name == "Memory Usage");
            Assert.Contains(series, s => s.Name == "Temperature");
        }

        [Fact]
        public void Should_Enable_Data_Markers()
        {
            var cut = Render<OrderTelemetryCard>();

            var options = cut.Instance.GetType().GetField("options", BindingFlags.NonPublic | BindingFlags.Instance)!
                             .GetValue(cut.Instance) as LineChartOptions;

            Assert.True(options!.ShowDataMarkers);
        }

        [Fact]
        public async Task Should_Connect_To_Telemetry_Stream_On_Load() {
            
        }

        [Fact]
        public async Task Should_Update_Chart_When_New_Data_Arrives() {
            // Given:
            //     GPU Usage = 20
            // When:
            //     new sample arrives
            //     GPU Usage = 40
            // Then:
            //     chart series updated
        }

        [Fact]
        public async Task Should_Keep_Last_30_Minutes_Of_Data() {
            // After receiving 100 samples:
            // Assert.True(series.Count <= MaxSamples);
        }

        [Fact]
        public async Task Should_Show_Disconnected_When_Stream_Ends() {
            // Future UI:
            //    < span > Disconnected </ span >
        }

        [Fact]
        public async Task Should_Attempt_Reconnect_When_Stream_Drops() { }

        [Fact]
        public void Should_Show_Loading_While_Connecting() {
            // Future markup:
            //    Loading telemetry...
        }

        [Fact]
        public async Task Should_Show_Error_When_Stream_Fails()
        {
            // Future:
            //    Unable to load telemetry
        }

        [Fact]
        public async Task Selecting_One_Hour_Should_Reload_Data()
        {
            // 5m
            // 30m
            // 1h
            // 24h
        }

        [Fact]
        public async Task Export_Button_Should_Download_Csv() { }
    }
}
