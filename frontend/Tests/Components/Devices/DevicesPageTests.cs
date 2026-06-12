using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Devices;
using GpuShare.Frontend.Components.Shared;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using System.Net;
using Xunit;

namespace GpuShare.Frontend.Tests.Components.Devices
{
    public class DevicesPageTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IAuthState> _authStateMock = new();
        private readonly Mock<IFormatters> _formattersMock = new();
        private readonly Mock<IDeviceService> _deviceServiceMock = new();
        private readonly Mock<IAppNotifier> _notifierMock = new();

        public DevicesPageTests()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddSingleton(_formattersMock.Object);
            Services.AddSingleton(_deviceServiceMock.Object);
            Services.AddSingleton(_notifierMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);

            _deviceServiceMock.Setup(s => s.GetDeviceAsync(It.IsAny<int>()))
                .ReturnsAsync(new Models.Device
                {
                    DeviceId = 123,
                    Name = "Workstation-Alpha",
                    OwnerUsername = "julie",
                    State = DeviceState.AVAILABLE,
                });

            _deviceServiceMock.Setup(s => s.SearchDevicesAsync(It.IsAny<DeviceSearchFilters>()))
                .ReturnsAsync(new PagedResult<Models.Device>
                {
                    Items = [
                    new Models.Device
                    {
                        DeviceId = 123,
                        Name = "Workstation-Alpha",
                        OwnerUsername = "julie",
                        State = DeviceState.AVAILABLE,
                    },
                    new Models.Device
                    {
                        DeviceId = 456,
                        Name = "RenderNode-01",
                        OwnerUsername = "mark",
                        State = DeviceState.UNAVAILABLE
                    }],
                    TotalCount = 2,
                    Page = 1,
                    PageSize = 10
                });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public void Should_Render_SearchBar()
        {
            // Act
            var cut = Render<DevicesPage>();

            // Assert
            cut.FindComponent<SearchBar>();
        }

        [Fact]
        public void Should_Render_GpuList()
        {
            // Act
            var cut = Render<DevicesPage>();

            // Assert
            cut.FindComponent<DevicesList>();
        }

        [Fact]
        public async Task ApplySearch_Should_Pass_Current_Filter()
        {
            // Arrange
            var cut = Render<DevicesPage>();

            var filter = new SearchFilter
            {
                Term = "RTX 4090",
                AvailableOnly = true
            };

            // Act
            await cut.InvokeAsync(async () =>
            {
                await cut.Instance.ApplySearchAsync(filter);
            });

            // Assert
            cut.Instance.Filter.Should().NotBeNull();
            cut.Instance.Filter.Term.Should().Be("RTX 4090");
            cut.Instance.Filter.AvailableOnly.Should().BeTrue();
        }

        [Fact]
        public async Task Search_Should_Load_Filtered_Devices()
        {
            // Arrange
            var devices = new List<Models.Device>
            {
                new()
                {
                    DeviceId = 1,
                    Name = "RTX 4090 Rig",
                    GpuModel = "RTX 4090"
                }
            };

            _deviceServiceMock.Setup(x => x.SearchDevicesAsync(It.IsAny<DeviceSearchFilters>()))
                .ReturnsAsync(new PagedResult<Models.Device>
                {
                    Items = devices,
                    TotalCount = 1
                });

            var cut = Render<DevicesPage>();

            // Act
            await cut.InvokeAsync(async () =>
                await cut.Instance.ApplySearchAsync(new SearchFilter { Term = "RTX" }));
            cut.Render();

            // Assert
            cut.Markup.Should().Contain("RTX 4090 Rig");
        }

        [Fact]
        public async Task Search_Should_Call_Device_Service()
        {
            // Arrange
            var cut = Render<DevicesPage>();

            var filter = new SearchFilter
            {
                Term = "RTX",
                AvailableOnly = true
            };

            _deviceServiceMock.Setup(x => x.SearchDevicesAsync(It.IsAny<DeviceSearchFilters>()))
                .ReturnsAsync(new PagedResult<Models.Device>());

            // Act
            await cut.InvokeAsync(async () =>
                await cut.Instance.ApplySearchAsync(filter));

            // Assert
            _deviceServiceMock.Verify(x => x.SearchDevicesAsync(
                    It.Is<DeviceSearchFilters>(f =>
                        f.Name == "RTX" &&
                        f.GpuModel == "RTX" &&
                        f.AvailableOnly == true)),
                Times.Once);
        }

        [Fact]
        public async Task Failed_Search_Should_Show_Error()
        {
            // Arrange
            _deviceServiceMock
                .Setup(x => x.SearchDevicesAsync(It.IsAny<DeviceSearchFilters>()))
                .ThrowsAsync(new Exception("boom"));

            var cut = Render<DevicesPage>();
            _deviceServiceMock.Invocations.Clear();
            _notifierMock.Invocations.Clear();

            // Act
            await cut.InvokeAsync(async () => await cut.Instance.ApplySearchAsync(new SearchFilter()));

            // Assert
            _notifierMock.Verify(x => x.ShowError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Unauthorized_Search_Should_Show_Unauthorized_Message()
        {
            // Arrange
            _deviceServiceMock.Setup(x => x.SearchDevicesAsync(It.IsAny<DeviceSearchFilters>()))
                .ThrowsAsync(new ApiException("Unauthorized", HttpStatusCode.Unauthorized));

            var cut = Render<DevicesPage>();
            _deviceServiceMock.Invocations.Clear();
            _notifierMock.Invocations.Clear();

            // Act
            await cut.InvokeAsync(async () => await cut.Instance.ApplySearchAsync(new SearchFilter()));

            // Assert
            _notifierMock.Verify(x => x.ShowError("Unauthorized access to device search."), Times.Once);
        }

        [Fact]
        public async Task BadRequest_Search_Should_Show_Invalid_Request_Message()
        {
            // Arrange
            _deviceServiceMock.Setup(x => x.SearchDevicesAsync(It.IsAny<DeviceSearchFilters>()))
                .ThrowsAsync(new ApiException("Bad Request", HttpStatusCode.BadRequest));

            var cut = Render<DevicesPage>();

            _deviceServiceMock.Invocations.Clear();
            _notifierMock.Invocations.Clear();

            // Act
            await cut.InvokeAsync(async () => await cut.Instance.ApplySearchAsync(new SearchFilter()));

            // Assert
            _notifierMock.Verify(x => x.ShowError("Invalid request for device search."), Times.Once);
        }

        [Fact]
        public async Task NotFound_Search_Should_Clear_Devices()
        {
            // Arrange

            var firstResponse = new PagedResult<Models.Device>
            {
                Items =
                [
                    new()
                    {
                        DeviceId = 1,
                        Name = "RTX 4090"
                    }
                ],
                TotalCount = 1
            };

            _deviceServiceMock.SetupSequence(x => x.SearchDevicesAsync(It.IsAny<DeviceSearchFilters>()))
                .ReturnsAsync(firstResponse)
                .ReturnsAsync(firstResponse)
                .ThrowsAsync(new ApiException("Not found", HttpStatusCode.NotFound));

            var cut = Render<DevicesPage>();

            await cut.InvokeAsync(async () => await cut.Instance.ApplySearchAsync(new SearchFilter()));

            cut.Markup.Should().Contain("RTX 4090");

            // Act

            await cut.InvokeAsync(async () =>
                await cut.Instance.ApplySearchAsync(new SearchFilter()));

            // Assert
            cut.Markup.Should().NotContain("RTX 4090");
        }

        [Fact]
        public async Task Search_Should_Render_Devices_From_Result()
        {
            _deviceServiceMock.Setup(x => x.SearchDevicesAsync(It.IsAny<DeviceSearchFilters>()))
                .ReturnsAsync(new PagedResult<Models.Device>
                {
                    Items =
                    [
                        new() { DeviceId = 1, Name = "RTX 4090" },
                        new() { DeviceId = 2, Name = "A100" }
                    ]
                });

            var cut = Render<DevicesPage>();

            await cut.InvokeAsync(async () =>
                await cut.Instance.ApplySearchAsync(new SearchFilter()));
            cut.Render();

            cut.Markup.Should().Contain("RTX 4090");
            cut.Markup.Should().Contain("A100");
        }
    }
}
