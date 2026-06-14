using Bunit;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Components.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Xunit;
using FluentAssertions;
using AngleSharp.Dom;
using GpuShare.Frontend.State;

namespace GpuShare.Frontend.Tests.Components.Shared
{
    public class GpuCardTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IAuthState> _authStateMock = new();
        private readonly Mock<IFormatters> _formattersMock = new();
        private readonly Mock<IDeviceService> _deviceServiceMock = new();

        private readonly Models.Device gpu = new()
        {
            DeviceId = 1,
            Name = "RTX 4090",
            GpuModel = "NVIDIA",
            OwnerUsername = "john",
            PricePerHourUsdCents = 1000,
            VramMb = 24000,
            CudaCores = 16000,
            DriverVersion = "535",
            State = DeviceState.AVAILABLE
        };

        public GpuCardTests()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddSingleton(_formattersMock.Object);
            Services.AddSingleton(_deviceServiceMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true);
            JSInterop.SetupModule(_ => true);

            _authStateMock.Setup(x => x.IsAuthenticated).Returns(true);
            _authStateMock.Setup(x => x.User).Returns(new User() { Username = "john"});
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public void Should_Show_Loading_When_Gpu_Is_Null()
        {
            // Act
            var cut = Render<DeviceCard>(parameters => parameters.Add(p => p.Device, null!)
            );

            // Assert
            cut.Markup.Should().Contain("loading-spinner");
        }

        [Fact]
        public void Unauthorized_User_Should_See_Order_Button()
        {
            _authStateMock.Setup(x => x.IsAuthenticated).Returns(false);
            var cut = Render<DeviceCard>(p => p.Add(x => x.Device, gpu));

            cut.Markup.Should().Contain("Order");
        }

        [Fact]
        public void Authorized_User_Should_See_Edit_And_Remove_Buttons()
        {
            var cut = Render<DeviceCard>(p => p.Add(x => x.Device, gpu));

            cut.Markup.Should().Contain("Edit");
            cut.Markup.Should().Contain("Remove");
        }

        [Fact]
        public void Edit_Button_Should_Navigate_To_Edit_Page()
        {
            var nav = Services.GetRequiredService<NavigationManager>();

            var gpu = new Models.Device() { DeviceId = 5, Name = "Test GPU", OwnerUsername = "john" };

            var cut = Render<DeviceCard>(p => p.Add(x => x.Device, gpu));

            cut.Find(".edit-btn").Click();

            nav.Uri.Should().Contain("/device/edit/5");
        }

        [Fact]
        public void Remove_Button_Should_Open_Modal()
        {
            var gpu = new Models.Device { DeviceId = 1, Name = "RTX", OwnerUsername = "john" };

            var cut = Render<DeviceCard>(p => p.Add(x => x.Device, gpu));

            cut.Find("button.btn-danger").Click();

            // modal should appear in DOM
            cut.Markup.Should().Contain("Remove Device");
        }

        [Fact]
        public void Order_Button_Should_Be_Disabled_When_Gpu_Unavailable() 
        {
            _authStateMock.Setup(x => x.IsAuthenticated).Returns(false);
            var gpu = new Models.Device { DeviceId = 1, Name = "RTX", State = DeviceState.UNAVAILABLE, };

            var cut = Render<DeviceCard>(p => p.Add(x => x.Device, gpu));

            var removeBtn = cut.Find(".order-btn");

            removeBtn.IsDisabled().Should().BeTrue();
        }

        [Fact]
        public void Should_Show_Available_Badge_When_Gpu_Is_Available() 
        {
            var gpu = new Models.Device { DeviceId = 1, Name = "RTX", State = DeviceState.AVAILABLE, };

            var cut = Render<DeviceCard>(p => p.Add(x => x.Device, gpu));

            var badge = cut.Find(".badge");
            badge.ClassList.Should().Contain("available");
        }

        [Fact]
        public void Should_Navigate_To_Device_Page_On_Title_Click() 
        {
            // Arrange
            var gpu = new Models.Device
            {
                DeviceId = 42,
                Name = "RTX 4090"
            };

            _authStateMock.Setup(x => x.IsAuthenticated).Returns(false);

            var cut = Render<DeviceCard>(p => p.Add(x => x.Device, gpu));

            // Act
            var navLink = cut.Find(".device-name");

            // Assert
            navLink.GetAttribute("href").Should().Be($"/device/view/{gpu.DeviceId}");
        }
    }
}
