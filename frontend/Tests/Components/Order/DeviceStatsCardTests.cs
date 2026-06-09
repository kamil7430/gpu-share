using Bunit;
using GpuShare.Frontend.Components.Pages.Order;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using FluentAssertions;

namespace GpuShare.Frontend.Tests.Components.Order
{
    public class DeviceStatsCardTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IFormatters> _formattersMock = new();
        private readonly Mock<IOrderService> _orderServiceMock = new();
        private readonly Mock<IDeviceService> _deviceServiceMock = new();

        private readonly Models.Order _order = new()
        {
            OrderId = 1,
            StartDate = DateTime.UtcNow.AddHours(-2),
            EndDate = DateTime.UtcNow.AddHours(1)
        };

        private readonly Models.Device _gpu = new()
        {
            DeviceId = 10,
            PricePerHourUsdCents = 500
        };

        public DeviceStatsCardTests()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton(_formattersMock.Object);
            Services.AddSingleton(_orderServiceMock.Object);
            Services.AddSingleton(_deviceServiceMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true);
            JSInterop.SetupModule(_ => true);

            // Arrange
            
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
        public void Should_Get_Device_And_Order_Data()
        {
            // Act
            var cut = Render<DeviceStatsCard>(p => p.Add(x => x.Device, _gpu).Add(x => x.Order, _order));

            // Assert
            cut.Instance.Order.Should().BeEquivalentTo(_order);
            cut.Instance.Device.Should().BeEquivalentTo(_gpu);
        }

        [Fact]
        public void Shows_Connected_When_DeviceStatus_Exists()
        {
            // Act
            var cut = Render<DeviceStatsCard>(p => p.Add(x => x.Device, _gpu).Add(x => x.Order, _order));

            // Assert
            cut.Markup.Contains("Connected");
        }

        [Fact]
        public void Shows_Disconnected_When_Status_Request_Fails()
        {
            // Arrange
            _deviceServiceMock.Setup(x => x.GetDeviceStatusAsync(It.IsAny<int>())).ThrowsAsync(new Exception());

            // Act
            var cut = Render<DeviceStatsCard>(p => p.Add(x => x.Device, _gpu).Add(x => x.Order, _order));

            // Assert
            cut.Markup.Contains("Disconnected");
        }

        [Fact]
        public void Connected_Status_Should_Show_Green_Dot()
        {
            // Act
            var cut = Render<DeviceStatsCard>(p => p.Add(x => x.Device, _gpu).Add(x => x.Order, _order));

            // Assert
            cut.Find(".heartbeat-dot-green");
        }

        [Fact]
        public void Disconnected_Status_Should_Show_Red_Dot()
        {
            // Arrange
            _deviceServiceMock.Setup(x => x.GetDeviceStatusAsync(It.IsAny<int>())).ThrowsAsync(new Exception());

            // Act
            var cut = Render<DeviceStatsCard>(p => p.Add(x => x.Device, _gpu).Add(x => x.Order, _order));

            // Assert
            cut.Find(".heartbeat-dot-red");
        }

        [Fact]
        public void Calculates_Current_Cost()
        {
            var cut = Render<DeviceStatsCard>(p => p.Add(x => x.Order, _order).Add(x => x.Device, _gpu));

            cut.WaitForAssertion(() =>
            {
                cut.Markup.Contains("$10");
            });
        }

        [Fact]
        public void Expired_Order_Should_Show_Zero_Time()
        {
            _orderServiceMock.Setup(x => x.GetOrderAsync(1)).ReturnsAsync(new Models.Order
                {
                    StartDate = DateTime.UtcNow.AddHours(-3),
                    EndDate = DateTime.UtcNow.AddHours(-1)
                });

            var cut = Render<DeviceStatsCard>(p => p.Add(x => x.Order, _order).Add(x => x.Device, _gpu));

            cut.WaitForAssertion(() =>
            {
                cut.Markup.Contains("00:00:00");
            });
        }
    }
}
