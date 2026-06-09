using Bunit;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using GpuShare.Frontend.Components.Pages.Device;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using System.Net;
using FluentAssertions;

namespace GpuShare.Frontend.Tests.Components.Device
{
    public class NewDevicePageTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IDeviceService> _deviceServiceMock = new();
        private readonly Mock<IAuthState> _authStateMock = new();
        private readonly Mock<IAppNotifier> _notifierMock = new();

        public NewDevicePageTests()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton(_notifierMock.Object);
            Services.AddSingleton(_authStateMock.Object);
            Services.AddSingleton(_deviceServiceMock.Object);
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
                    State = DeviceState.AVAILABLE
                });

            _authStateMock.Setup(x => x.User)
               .Returns(new User
               {
                   Username = "john"
               });

            var authContext = AddAuthorization();
            authContext.SetAuthorized("john");

            Render<MudPopoverProvider>();
        }

        private readonly Models.Device _device = new()
        {
            Name = "RTX 4090",
            GpuModel = "NVIDIA RTX",
            VramMb = 24576,
            CudaCores = 16384,
            DriverVersion = "535.104",
            PricePerHourUsdCents = 450
        };

        public Task InitializeAsync() => Task.CompletedTask;

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public void Should_Render_Profile_Back_Link()
        {
            var cut = Render<NewDevicePage>();

            var link = cut.Find("a.back-link");

            link.GetAttribute("href").Should().Be("/profile/john");
        }

        [Fact]
        public void Should_Render_EditDeviceForm_When_Authorized()
        {
            var cut = Render<NewDevicePage>();

            cut.FindComponent<EditDeviceForm>().Should().NotBeNull();
        }

        [Fact]
        public async Task Should_Call_RegisterDevice_With_Mapped_Request()
        {
            _deviceServiceMock.Setup(x => x.RegisterDeviceAsync(It.IsAny<RegisterDeviceRequest>()))
                .ReturnsAsync(new Models.Device { DeviceId = 123 });

            var cut = Render<NewDevicePage>();

            var form = cut.FindComponent<EditDeviceForm>();

            await cut.InvokeAsync(() =>
                form.Instance.OnSave.InvokeAsync(_device));

            _deviceServiceMock.Verify(x => x.RegisterDeviceAsync(It.Is<RegisterDeviceRequest>(r =>
                        r.Name == _device.Name &&
                        r.GpuModel == _device.GpuModel &&
                        r.VramMb == _device.VramMb &&
                        r.CudaCores == _device.CudaCores &&
                        r.DriverVersion == _device.DriverVersion &&
                        r.PricePerHourUsdCents == _device.PricePerHourUsdCents)),
                Times.Once);
        }

        [Fact]
        public async Task Should_Navigate_On_Success()
        {
            var nav = Services.GetRequiredService<Bunit.TestDoubles.BunitNavigationManager>();

            _deviceServiceMock.Setup(x => x.RegisterDeviceAsync(It.IsAny<RegisterDeviceRequest>()))
                .ReturnsAsync(new Models.Device { DeviceId = 999 });

            var cut = Render<NewDevicePage>();

            var form = cut.FindComponent<EditDeviceForm>();

            await cut.InvokeAsync(() => form.Instance.OnSave.InvokeAsync(new Models.Device { Name = "RTX" }));

            nav.Uri.Should().Contain("/device/view/999");
        }

        [Fact]
        public async Task Should_Show_Snackbar_On_BadRequest()
        {
            _deviceServiceMock.Setup(x => x.RegisterDeviceAsync(It.IsAny<RegisterDeviceRequest>()))
                .ThrowsAsync(new ApiException("bad", HttpStatusCode.BadRequest));

            var cut = Render<NewDevicePage>();

            var form = cut.FindComponent<EditDeviceForm>();

            await cut.InvokeAsync(() => form.Instance.OnSave.InvokeAsync(new Models.Device()));

            _notifierMock.Verify(x => x.ShowError(
                    "Creating device failed because of wrong input data. Please check your input and try again."), Times.Once);
        }

        [Fact]
        public async Task Should_Show_Snackbar_On_Unauthorized()
        {
            _deviceServiceMock.Setup(x => x.RegisterDeviceAsync(It.IsAny<RegisterDeviceRequest>()))
                .ThrowsAsync(new ApiException("unauthorized", HttpStatusCode.Unauthorized));

            var cut = Render<NewDevicePage>();

            var form = cut.FindComponent<EditDeviceForm>();

            await cut.InvokeAsync(() => form.Instance.OnSave.InvokeAsync(new Models.Device()));

            _notifierMock.Verify(x => x.ShowError(
                    "You are not authorized to create a device. Please log in and try again."), Times.Once);
        }

        [Fact]
        public async Task Should_Show_Generic_Error_On_Unexpected_Exception()
        {
            _deviceServiceMock.Setup(x => x.RegisterDeviceAsync(It.IsAny<RegisterDeviceRequest>()))
                .ThrowsAsync(new Exception("boom"));

            var cut = Render<NewDevicePage>();

            var form = cut.FindComponent<EditDeviceForm>();

            await cut.InvokeAsync(() => form.Instance.OnSave.InvokeAsync(new Models.Device()));

            _notifierMock.Verify(x => x.ShowError(
                    "An unexpected error occured while creating the device. Please try again."), Times.Once);
        }
    }
}
