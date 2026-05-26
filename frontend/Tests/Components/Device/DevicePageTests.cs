using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Device;
using GpuShare.Frontend.Components.Shared;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Polly;
using System.Globalization;
using System.Reflection.PortableExecutable;

namespace GpuShare.Frontend.Tests.Components.Device
{
    public class DevicePageTests : BunitContext, Xunit.IAsyncLifetime
    {
        private Mock<IAuthState> _authStateMock;
        private Mock<IDeviceService> _deviceServiceMock;

        public DevicePageTests()
        {
            _authStateMock = new Mock<IAuthState>();
            _deviceServiceMock = new Mock<IDeviceService>();
            Services.AddAuthorizationCore();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddSingleton(_deviceServiceMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            // important note: the JSInterop calls in the Device component are not being properly mocked, which causes the tests to fail. The following setup is an attempt to mock those calls, but it may need to be adjusted based on the actual JSInterop calls being made in the Device component.
            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);
            //JSInterop.SetupVoid("mudElementRef.removeOnBlurEvent", _ => true).SetVoidResult();

            // Stub heavy components
            ComponentFactories.AddStub<TelemetryCard>("TELEMETRY_CARD");
            ComponentFactories.AddStub<ReservationCalendar>("CALENDAR");
            ComponentFactories.AddStub<OpinionsList>("OPINIONS");
            ComponentFactories.AddStub<DeviceOrderForm>("ORDER_FORM");

            _deviceServiceMock.Setup(s => s.GetDeviceAsync(It.IsAny<int>()))
                .ReturnsAsync(new Models.Device
                {
                    Id = 123,
                    Name = "Workstation-Alpha",
                    OwnerUsername = "julie",
                    IsAvailable = true
                });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
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
            var cut = Render<DevicePage>(p => p.Add(x => x.ModeString, "view").Add(x => x.Id, 1));

            // Assert
            cut.Markup.Should().Contain("DEVICE_INFO");
            cut.Markup.Should().Contain("TELEMETRY_CARD");
            cut.Markup.Should().Contain("CALENDAR");

            cut.Markup.Should().NotContain("ORDER_FORM");
            cut.Markup.Should().NotContain("OPINIONS");
        }

        [Fact]
        public void View_Mode_Unauthorized_Should_Show_Opinions_And_Order_Form()
        {
            // Arrange
            ComponentFactories.AddStub<DeviceInfoCard>("DEVICE_INFO");
            var auth = AddAuthorization();
            auth.SetNotAuthorized();

            // Act
            var cut = Render<DevicePage>(p => p.Add(x => x.ModeString, "view").Add(x => x.Id, 1));

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
            var cut = Render<DevicePage>(p => p.Add(x => x.ModeString, "edit").Add(x => x.Id, 1));

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
            var cut = Render<DevicePage>(p => p.Add(x => x.ModeString, "edit").Add(x => x.Id, 1));

            // Assert
            cut.Markup.Should().Contain("DEVICE_INFO");
            cut.Markup.Should().NotContain("EDIT_FORM");
        }

        // =====================================================
        // ADD MODE
        // =====================================================

        [Fact]
        public void Add_Mode_Authorized_Should_Show_Only_Edit_Form()
        {
            // Arrange
            ComponentFactories.AddStub<DeviceInfoCard>("DEVICE_INFO");
            ComponentFactories.AddStub<EditDeviceForm>("EDIT_FORM");
            var auth = AddAuthorization();
            auth.SetAuthorized("john");

            // Act
            var cut = Render<DevicePage>(p => p.Add(x => x.ModeString, "add"));

            // Assert
            cut.Markup.Should().Contain("EDIT_FORM");
            cut.Markup.Should().NotContain("DEVICE_INFO");
            cut.Markup.Should().NotContain("TELEMETRY_CARD");
            cut.Markup.Should().NotContain("CALENDAR");
            cut.Markup.Should().NotContain("ORDER_FORM");
            cut.Markup.Should().NotContain("OPINIONS");
        }

        [Fact]
        public void Add_Mode_Unauthorized_Should_Show_Nothing()
        {
            // Arrange
            ComponentFactories.AddStub<DeviceInfoCard>("DEVICE_INFO");
            ComponentFactories.AddStub<EditDeviceForm>("EDIT_FORM");
            var auth = AddAuthorization();
            auth.SetNotAuthorized();

            // Act
            var cut = Render<DevicePage>(p => p.Add(x => x.ModeString, "add"));

            // Assert
            cut.Markup.Should().NotContain("EDIT_FORM");
            cut.Markup.Should().NotContain("DEVICE_INFO");
            cut.Markup.Should().NotContain("TELEMETRY_CARD");
            cut.Markup.Should().NotContain("CALENDAR");
            cut.Markup.Should().NotContain("ORDER_FORM");
            cut.Markup.Should().NotContain("OPINIONS");
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
            var cut = Render<DevicePage>(p => p.Add(x => x.ModeString, "view").Add(x => x.Id, 123));

            // Assert
            cut.Markup.Should().Contain("DEVICE_INFO");
        }

        [Fact]
        public void Add_Mode_Should_Not_Render_Device_Info()
        {
            // Arrange
            ComponentFactories.AddStub<DeviceInfoCard>("DEVICE_INFO");
            var auth = AddAuthorization();
            auth.SetAuthorized("john");

            // Act
            var cut = Render<DevicePage>(p => p.Add(x => x.ModeString, "add"));

            // Assert
            cut.Markup.Should().NotContain("DEVICE_INFO");
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
            info.Instance.gpu.Name.Should().Be("Workstation-Alpha");
        }

        [Fact]
        public void Edit_Form_Should_Receive_Mode()
        {
            // Arrange
            var auth = AddAuthorization();
            auth.SetAuthorized("john");

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
            info.Instance.gpu.Name.Should().Be("Workstation-Alpha");
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
            // Arrange
            var auth = AddAuthorization();
            auth.SetAuthorized("john");

            // Act
            var cut = Render<DevicePage>();

            // Assert
            cut.Markup.Should().Contain("There is no GPU");
        }
    }
}
