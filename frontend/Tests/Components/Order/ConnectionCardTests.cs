using Blazorise;
using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Order;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using System.Reflection;

namespace GpuShare.Frontend.Tests.Components.Order
{
    public class ConnectionCardTests : BunitContext, Xunit.IAsyncLifetime
    {
        private Mock<IOrderService> _orderServiceMock = new();
        private Mock<IDeviceService> _deviceServiceMock = new();

        public ConnectionCardTests()
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

        private ConnectionDetailsDto connectionDetails = new ConnectionDetailsDto()
        {
            Host = "gpu-12.gpushare.io",
            Port = 443,
            Protocol = "WSS",
            ConnectionString = "wss://gpu-12.gpushare.io/connect/session_x82A"
        };

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public void Should_Render_Connection_Details_Header()
        {
            var cut = Render<ConnectionCard>(p => p.Add(x => x.ConnectionDetails, connectionDetails));

            cut.Markup.Should().Contain("Connection Details");
        }

        [Fact]
        public void Should_Render_Connection_Information()
        {
            var cut = Render<ConnectionCard>(p => p.Add(x => x.ConnectionDetails, connectionDetails));

            cut.Markup.Should().Contain("gpu-12.gpushare.io");
            cut.Markup.Should().Contain("443");
            cut.Markup.Should().Contain("WSS");
        }

        [Fact]
        public void Should_Render_Connection_String()
        {
            var cut = Render<ConnectionCard>(p => p.Add(x => x.ConnectionDetails, connectionDetails));

            cut.Markup.Should().Contain("wss://gpu-12.gpushare.io/connect/session_x82A");
        }

        [Fact]
        public void Copy_Button_Should_Copy_Connection_String()
        {
            JSInterop.SetupVoid("navigator.clipboard.writeText", "wss://gpu-12.gpushare.io/connect/session_x82A");

            var cut = Render<ConnectionCard>(p => p.Add(x => x.ConnectionDetails, connectionDetails));

            cut.Find("button").Click();

            JSInterop.VerifyInvoke("navigator.clipboard.writeText");
        }

        [Fact]
        public void Copy_Button_Should_Be_Disabled_When_No_Connection_String() { } // If session hasn't started yet

        [Fact]
        public void Should_Show_Loading_State() {
            // When connection info is still loading
        }

        [Fact]
        public void Should_Show_Session_Expired_Message() { } // When order is completed
    }
}
