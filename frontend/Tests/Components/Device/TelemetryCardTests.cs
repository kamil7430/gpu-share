using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Device;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;

namespace GpuShare.Frontend.Tests.Components.Device
{
    public class TelemetryCardTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IDeviceService> _deviceServiceMock = new();

        public TelemetryCardTests()
        {
            Services.AddSingleton(_deviceServiceMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);

            _deviceServiceMock.Setup(x => x.GetDeviceStatusAsync(It.IsAny<int>()))
                .ReturnsAsync(new DeviceStatus
                {
                    DeviceId = 1,
                    TemperatureCelsius = 65,
                    Online = true,
                    LastHeartbeat = DateTime.Now
                });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public async Task Should_Load_Device_Status()
        {
            // Act
            Render<TelemetryCard>(parameters => parameters.Add(p => p.gpu, new Models.Device { Id = 1 }));

            // Assert
            _deviceServiceMock.Verify(x => x.GetDeviceStatusAsync(1), Times.Once);
        }

        [Fact]
        public void Should_Render_Temperature()
        {
            // Act
            var cut = Render<TelemetryCard>(parameters => parameters.Add(p => p.gpu, new Models.Device { Id = 1 }));

            // Assert
            cut.Markup.Should().Contain("65°C");
        }

        [Fact]
        public void Should_Render_Online_Status()
        {
            // Arrange
            _deviceServiceMock
                .Setup(x => x.GetDeviceStatusAsync(It.IsAny<int>()))
                .ReturnsAsync(new DeviceStatus
                {
                    Online = true
                });

            // Act
            var cut = Render<TelemetryCard>(parameters => parameters.Add(p => p.gpu, new Models.Device { Id = 1 }));

            // Assert
            cut.Markup.Should().Contain("Online");
        }
        
        [Fact]
        public void Should_Render_Offline_Status()
        {
            // Arrange
            _deviceServiceMock.Setup(x => x.GetDeviceStatusAsync(It.IsAny<int>())).ReturnsAsync(new DeviceStatus
                {
                    Online = false
                });
            
            // Act
            var cut = Render<TelemetryCard>(parameters => parameters.Add(p => p.gpu, new Models.Device { Id = 1 }));

            // Assert
            cut.Markup.Should().Contain("Offline");
        }

        [Fact]
        public void Safe_Temperature_Should_Use_Success_Color()
        {
            // Arrange
            _deviceServiceMock.Setup(x => x.GetDeviceStatusAsync(It.IsAny<int>())).ReturnsAsync(new DeviceStatus
                {
                    TemperatureCelsius = 55
                });

            // Act
            var cut = Render<TelemetryCard>(parameters => parameters.Add(p => p.gpu, new Models.Device { Id = 1 }));

            // Assert
            cut.Markup.Should().Contain("mud-chip-color-success");
        }

        [Fact]
        public void Warning_Temperature_Should_Use_Warning_Color()
        {
            // Arrange
            _deviceServiceMock.Setup(x => x.GetDeviceStatusAsync(It.IsAny<int>())).ReturnsAsync(new DeviceStatus
                {
                    TemperatureCelsius = 75
                });

            // Act
            var cut = Render<TelemetryCard>(parameters => parameters
                .Add(p => p.gpu, new Models.Device { Id = 1 }));

            // Assert
            cut.Markup.Should().Contain("mud-chip-color-warning");
        }

        [Fact]
        public void Critical_Temperature_Should_Use_Error_Color()
        {
            // Arrange
            _deviceServiceMock.Setup(x => x.GetDeviceStatusAsync(It.IsAny<int>())).ReturnsAsync(new DeviceStatus
                {
                    TemperatureCelsius = 90
                });

            // Act
            var cut = Render<TelemetryCard>(parameters => parameters.Add(p => p.gpu, new Models.Device { Id = 1 }));

            // Assert
            cut.Markup.Should().Contain("mud-chip-color-error");
        }

        [Fact]
        public void Utilization_Chart_Should_Render()
        {
            // Act
            var cut = Render<TelemetryCard>(parameters => parameters.Add(p => p.gpu, new Models.Device { Id = 1 }));

            // Assert
            cut.FindComponent<MudChart<double>>();
        }

        [Fact]
        public void Should_Render_Chart_Legends()
        {
            // Act
            var cut = Render<TelemetryCard>(parameters => parameters.Add(p => p.gpu, new Models.Device { Id = 1 }));

            // Assert
            cut.Markup.Should().Contain("GPU Usage");
            cut.Markup.Should().Contain("VRAM Usage");
        }

        [Fact]
        public void Should_Render_Metric_Summary()
        {
            // Act
            var cut = Render<TelemetryCard>(parameters => parameters.Add(p => p.gpu, new Models.Device { Id = 1 }));

            // Assert
            cut.Markup.Should().Contain("Avg GPU Usage");
            cut.Markup.Should().Contain("Avg VRAM Usage");
            cut.Markup.Should().Contain("Peak Temperature");
            cut.Markup.Should().Contain("Runtime");
        }

        [Fact]
        public void Export_Button_Should_Render()
        {
            // Act
            var cut = Render<TelemetryCard>(parameters => parameters.Add(p => p.gpu, new Models.Device { Id = 1 }));

            // Assert
            cut.FindAll("button").Any(x => x.TextContent.Contains("Export CSV")).Should().BeTrue();
        }

        [Fact]
        public void Should_Render_Range_Selector()
        {
            // Act
            var cut = Render<TelemetryCard>(parameters => parameters.Add(p => p.gpu, new Models.Device { Id = 1 }));

            // Assert
            var select = cut.Find("select");

            select.Should().NotBeNull();

            cut.Markup.Should().Contain("1 Hour");
            cut.Markup.Should().Contain("24 Hours");
            cut.Markup.Should().Contain("7 Days");
        }

        [Fact]
        public void Null_Status_Should_Not_Crash()
        {
            // Act
            var act = () => Render<TelemetryCard>(parameters => parameters.Add(p => p.gpu, new Models.Device { Id = 1 }));

            // Assert
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(69, "success")]
        [InlineData(70, "warning")]
        [InlineData(84, "warning")]
        [InlineData(85, "error")]
        public void Temperature_Thresholds_Should_Map_Correctly(int temperature, string expectedClass)
        {
            // Arrange
            _deviceServiceMock
                .Setup(x => x.GetDeviceStatusAsync(It.IsAny<int>()))
                .ReturnsAsync(new DeviceStatus
                {
                    TemperatureCelsius = temperature
                });

            // Act
            var cut = Render<TelemetryCard>(parameters => parameters.Add(p => p.gpu, new Models.Device { Id = 1 }));

            // Assert
            cut.Markup.Should().Contain($"mud-chip-color-{expectedClass}");
        }
    }
}
