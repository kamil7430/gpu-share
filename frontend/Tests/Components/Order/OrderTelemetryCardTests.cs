using Bunit;
using GpuShare.Frontend.Components.Pages.Order;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;

namespace GpuShare.Frontend.Tests.Components.Order
{
    public class OrderTelemetryCardTests : BunitContext, Xunit.IAsyncLifetime
    {
        private Mock<IOrderService> _orderServiceMock = new();
        private Mock<IDeviceService> _deviceServiceMock = new();

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
    }
}
