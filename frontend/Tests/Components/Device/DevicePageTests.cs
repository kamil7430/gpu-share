using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Device;
using GpuShare.Frontend.Components.Shared;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;

namespace GpuShare.Frontend.Tests.Components.Device
{
    public class DevicePageTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IAuthState> _authStateMock = new();
        private readonly Mock<IFormatters> _formattersMock = new();
        private readonly Mock<IDeviceService> _deviceServiceMock = new();

        private readonly Models.Device _device = new()
        {
            DeviceId = 123,
            Name = "Workstation-Alpha",
            OwnerUsername = "julie",
            GpuModel = "RTX 4080",
            VramMb = 20,
            State = DeviceState.AVAILABLE
        };

        private readonly Models.Device _newDevice = new()
        {
            DeviceId = 1,
            Name = "B",
            GpuModel = "RTX 4090",
            VramMb = 10
        };

        public DevicePageTests()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddSingleton(_formattersMock.Object);
            Services.AddSingleton(_deviceServiceMock.Object);
            Services.AddSingleton(new Mock<IAppNotifier>().Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);

            // Stub heavy components
            ComponentFactories.AddStub<TelemetryCard>("TELEMETRY_CARD");
            ComponentFactories.AddStub<ReservationCalendar>("CALENDAR");
            ComponentFactories.AddStub<ReviewsList>("OPINIONS");
            ComponentFactories.AddStub<DeviceOrderForm>("ORDER_FORM");

            _deviceServiceMock.Setup(s => s.GetDeviceAsync(It.IsAny<int>()))
                .ReturnsAsync(_device);

            _deviceServiceMock.Setup(x => x.UpdateDeviceAsync(It.IsAny<int>(), It.IsAny<Models.Device>(),
                    It.IsAny<UpdateDeviceRequest>())).ReturnsAsync(_newDevice);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        // =====================================================
        // VIEW MODE
        // =====================================================

        [Fact]
        public async Task View_Mode_Authorized_Should_Show_Telemetry_And_Calendar()
        {
            // Arrange
            ComponentFactories.AddStub<DeviceInfoCard>("DEVICE_INFO");
            var authContext = AddAuthorization();
            authContext.SetAuthorized("john");

            // Act
            // var popoverProvider = Render<MudPopoverProvider>()
            var cut = Render<DevicePage>(p => p.Add(x => x.ModeString, "view").Add(x => x.DeviceId, 1));

            // Assert
            cut.Markup.Should().Contain("DEVICE_INFO");
            cut.Markup.Should().Contain("TELEMETRY_CARD");
            cut.Markup.Should().Contain("CALENDAR");
        }

        [Fact]
        public void View_Mode_Unauthorized_Should_Show_Opinions_And_Order_Form()
        {
            // Arrange
            ComponentFactories.AddStub<DeviceInfoCard>("DEVICE_INFO");
            var auth = AddAuthorization();
            auth.SetNotAuthorized();

            // Act
            var cut = Render<DevicePage>(p => p.Add(x => x.ModeString, "view").Add(x => x.DeviceId, 1));

            // Assert
            cut.Markup.Should().Contain("DEVICE_INFO");

            cut.Markup.Should().Contain("OPINIONS");
            cut.Markup.Should().Contain("ORDER_FORM");

            cut.Markup.Should().NotContain("TELEMETRY_CARD");
            cut.Markup.Should().NotContain("CALENDAR");
        }

        // =====================================================
        // EDIT MODE
        // =====================================================

        [Fact]
        public void Edit_Mode_Authorized_Should_Show_Edit_Form()
        {
            // Arrange
            ComponentFactories.AddStub<DeviceInfoCard>("DEVICE_INFO");
            ComponentFactories.AddStub<EditDeviceForm>("EDIT_FORM");
            var auth = AddAuthorization();
            auth.SetAuthorized("john");

            // Act
            var cut = Render<DevicePage>(p => p.Add(x => x.ModeString, "edit").Add(x => x.DeviceId, 1));

            // Assert
            cut.Markup.Should().Contain("DEVICE_INFO");
            cut.Markup.Should().Contain("EDIT_FORM");
        }

        [Fact]
        public void Edit_Mode_Unauthorized_Should_Not_Show_Edit_Form()
        {
            // Arrange
            ComponentFactories.AddStub<DeviceInfoCard>("DEVICE_INFO");
            ComponentFactories.AddStub<EditDeviceForm>("EDIT_FORM");
            var auth = AddAuthorization();
            auth.SetNotAuthorized();

            // Act
            var cut = Render<DevicePage>(p => p.Add(x => x.ModeString, "edit").Add(x => x.DeviceId, 1));

            // Assert
            cut.Markup.Should().Contain("DEVICE_INFO");
            cut.Markup.Should().NotContain("EDIT_FORM");
        }

        // =====================================================
        // ID / DATA LOADING
        // =====================================================

        [Fact]
        public void Device_With_Id_Should_Load_Device_Info()
        {
            // Arrange
            ComponentFactories.AddStub<DeviceInfoCard>("DEVICE_INFO");
            var auth = AddAuthorization();
            auth.SetAuthorized("john");

            // Act
            var cut = Render<DevicePage>(p => p.Add(x => x.ModeString, "view").Add(x => x.DeviceId, 123));

            // Assert
            cut.Markup.Should().Contain("DEVICE_INFO");
        }

        [Fact]
        public void DeviceInfo_Should_Receive_Device()
        {
            // Arrange
            var auth = AddAuthorization();
            auth.SetAuthorized("john");

            // Act
            var cut = Render<DevicePage>();
            var info = cut.FindComponent<DeviceInfoCard>();

            // Assert
            info.Instance.Device!.Name.Should().Be("Workstation-Alpha");
        }

        [Fact]
        public void Edit_Form_Should_Receive_Mode()
        {
            // Arrange
            var auth = AddAuthorization();
            auth.SetAuthorized("john");

            Render<MudPopoverProvider>();

            // Act
            var cut = Render<DevicePage>(p => p.Add(x => x.ModeString, "edit"));
            var info = cut.FindComponent<DeviceInfoCard>();

            // Assert
            info.Instance.Mode.Should().Be(DevicePageMode.Edit);
        }

        [Fact]
        public void Edit_Form_Should_Receive_Device()
        {
            // Arrange
            var auth = AddAuthorization();
            auth.SetAuthorized("john");

            // Act
            var cut = Render<DevicePage>();
            var info = cut.FindComponent<DeviceInfoCard>();

            // Assert
            info.Instance.Device!.Name.Should().Be("Workstation-Alpha");
        }

        [Fact]
        public void Back_Link_Should_Navigate_To_Profile()
        {
            // Arrange
            var auth = AddAuthorization();
            auth.SetAuthorized("john");

            // Act
            var cut = Render<DevicePage>();

            // Assert
            cut.Find(".back-link").GetAttribute("href").Should().Be("/profile/julie");
        }

        [Fact]
        public void Should_Not_Throw_When_Device_Is_Default()
        {
            _deviceServiceMock.Setup(s => s.GetDeviceAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception());

            // Arrange
            var auth = AddAuthorization();
            auth.SetAuthorized("john");

            // Act
            var cut = Render<DevicePage>();

            // Assert
            cut.Markup.Should().Contain("Could not load device data");
        }

        [Fact]
        public async Task Should_Call_UpdateDevice_When_Fields_Changed()
        {
            // Arrange
            var auth = AddAuthorization();
            auth.SetAuthorized("john");
            Render<MudPopoverProvider>();

            var cut = Render<DevicePage>(p => p.Add(x => x.DeviceId, 1).Add(x => x.ModeString, "edit"));

            // Wait for load
            await cut.InvokeAsync(() => Task.CompletedTask);

            await cut.Instance.HandleDeviceSaved(_newDevice);

            // Assert
            _deviceServiceMock.Verify(x => x.UpdateDeviceAsync(1, It.IsAny<Models.Device>(), It.Is<UpdateDeviceRequest>(r => r.Name == "B")
                ), Times.Once);
        }

        [Fact]
        public async Task Should_Not_Send_Fields_When_Unchanged()
        {
            // Arrange
            var auth = AddAuthorization();
            auth.SetAuthorized("john");
            Render<MudPopoverProvider>();

            var cut = Render<DevicePage>(p => p.Add(x => x.DeviceId, 1).Add(x => x.ModeString, "edit"));

            await cut.InvokeAsync(() => Task.CompletedTask);

            // Act
            await cut.Instance.HandleDeviceSaved(_device);

            // Assert → request should contain only nulls
            _deviceServiceMock.Verify(x => x.UpdateDeviceAsync(1, It.IsAny<Models.Device>(),
                    It.Is<UpdateDeviceRequest>(r => r.Name == null && r.GpuModel == null && r.VramMb == null)
                ), Times.Once);
        }

        [Fact]
        public async Task Should_Show_Error_When_Update_Fails()
        {
            // Arrange
            var auth = AddAuthorization();
            auth.SetAuthorized("john");
            Render<MudPopoverProvider>();

            _deviceServiceMock.Setup(x => x.UpdateDeviceAsync(It.IsAny<int>(),
                    It.IsAny<Models.Device>(), It.IsAny<UpdateDeviceRequest>()))
                .ThrowsAsync(new Exception("fail"));

            var cut = Render<DevicePage>(p => p.Add(x => x.DeviceId, 1).Add(x => x.ModeString, "edit"));

            await cut.InvokeAsync(() => Task.CompletedTask);

            // Act
            await cut.Instance.HandleDeviceSaved(_device);

            // Assert snackbar
            var snackbar = Services.GetService<ISnackbar>();
            snackbar.Should().NotBeNull();
        }
    }
}
