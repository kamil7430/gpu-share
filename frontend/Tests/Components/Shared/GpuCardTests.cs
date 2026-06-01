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

namespace GpuShare.Frontend.Tests.Components.Shared
{
    public class GpuCardTests : BunitContext, Xunit.IAsyncLifetime
    {
        private Models.Device gpu = new()
        {
            Id = 1,
            Name = "RTX 4090",
            Model = "NVIDIA",
            PricePerHour = 10,
            VramMb = 24000,
            CudaCores = 16000,
            DriverVersion = "535",
            Frameworks = new() { "CUDA" },
            IsAvailable = true
        };

        public GpuCardTests()
        {
            Services.AddAuthorizationCore();
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true);
            JSInterop.SetupModule(_ => true);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
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
            var cut = Render<DeviceCard>(p => p.Add(x => x.Device, gpu).Add(x => x.Authorized, false));

            cut.Markup.Should().Contain("Order");
        }

        [Fact]
        public void Authorized_User_Should_See_Edit_And_Remove_Buttons()
        {
            var cut = Render<DeviceCard>(p => p.Add(x => x.Device, gpu).Add(x => x.Authorized, true));

            cut.Markup.Should().Contain("Edit");
            cut.Markup.Should().Contain("Remove");
        }

        [Fact]
        public void Edit_Button_Should_Navigate_To_Edit_Page()
        {
            var nav = Services.GetRequiredService<NavigationManager>();

            var gpu = new Models.Device() { Id = 5, Name = "Test GPU" };

            var cut = Render<DeviceCard>(p => p.Add(x => x.Device, gpu).Add(x => x.Authorized, true));

            cut.Find("button.btn-primary").Click();

            nav.Uri.Should().Contain("/device/edit/5");
        }

        [Fact]
        public void Remove_Button_Should_Open_Modal()
        {
            var gpu = new Models.Device { Id = 1, Name = "RTX" };

            var cut = Render<DeviceCard>(p => p.Add(x => x.Device, gpu).Add(x => x.Authorized, true));

            cut.Find("button.btn-danger").Click();

            // modal should appear in DOM
            cut.Markup.Should().Contain("Remove Device");
        }

        [Fact]
        public void Order_Button_Should_Be_Disabled_When_Gpu_Unavailable() { }

        [Fact]
        public void Should_Render_All_Frameworks() { }

        [Fact]
        public void Should_Show_Available_Badge_When_Gpu_Is_Available() { }

        [Fact]
        public void Should_Navigate_To_Device_Page_On_Title_Click() { }
    }
}
