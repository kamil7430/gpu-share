using Bunit;
using GpuShare.Frontend.Components.Pages.Order;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;

namespace GpuShare.Frontend.Tests.Components.Order
{
    public class DeviceStatsCardTests : BunitContext, Xunit.IAsyncLifetime
    {
        private Mock<IOrderService> _orderServiceMock = new();
        private Mock<IDeviceService> _deviceServiceMock = new();

        public DeviceStatsCardTests()
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
                Id = 1,
                StartDate = DateTime.UtcNow.AddHours(-2),
                EndDate = DateTime.UtcNow.AddHours(1)
            };

            var gpu = new Models.Device
            {
                Id = 10,
                PricePerHour = 5
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

        public async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public void Loads_Device_And_Order_Data()
        {
            // Act
            var cut = Render<DeviceStatsCard>(p => p.Add(x => x.DeviceId, 10).Add(x => x.OrderId, 1));

            // Assert
            _deviceServiceMock.Verify(x => x.GetDeviceAsync(10), Times.Once);
            _orderServiceMock.Verify(x => x.GetOrderAsync(1), Times.Once);
        }

        [Fact]
        public void Shows_Connected_When_DeviceStatus_Exists()
        {
            // Act
            var cut = Render<DeviceStatsCard>(p => p.Add(x => x.DeviceId, 10).Add(x => x.OrderId, 1));

            // Assert
            cut.Markup.Contains("Connected");
        }

        [Fact]
        public void Shows_Disconnected_When_Status_Request_Fails()
        {
            // Arrange
            _deviceServiceMock.Setup(x => x.GetDeviceStatusAsync(It.IsAny<int>())).ThrowsAsync(new Exception());

            // Act
            var cut = Render<DeviceStatsCard>(p => p.Add(x => x.DeviceId, 10).Add(x => x.OrderId, 1));

            // Assert
            cut.Markup.Contains("Disconnected");
        }

        [Fact]
        public void Connected_Status_Should_Show_Green_Dot()
        {
            // Act
            var cut = Render<DeviceStatsCard>(p => p.Add(x => x.DeviceId, 10).Add(x => x.OrderId, 1));

            // Assert
            cut.Find(".heartbeat-dot-green");
        }

        [Fact]
        public void Disconnected_Status_Should_Show_Red_Dot()
        {
            // Arrange
            _deviceServiceMock.Setup(x => x.GetDeviceStatusAsync(It.IsAny<int>())).ThrowsAsync(new Exception());

            // Act
            var cut = Render<DeviceStatsCard>(p => p.Add(x => x.DeviceId, 10).Add(x => x.OrderId, 1));

            // Assert
            cut.Find(".heartbeat-dot-red");
        }

        [Fact]
        public void Calculates_Current_Cost()
        {
            var cut = Render<DeviceStatsCard>(p => p.Add(x => x.OrderId, 1).Add(x => x.DeviceId, 10));

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

            var cut = Render<DeviceStatsCard>(p => p.Add(x => x.OrderId, 1).Add(x => x.DeviceId, 10));

            cut.WaitForAssertion(() =>
            {
                cut.Markup.Contains("00:00:00");
            });
        }
    }
}
