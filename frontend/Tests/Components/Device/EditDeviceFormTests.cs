using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Device;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;

namespace GpuShare.Frontend.Tests.Components.Device
{
    public class EditDeviceFormTests : BunitContext, Xunit.IAsyncLifetime
    {
        private Mock<IAuthState> _authStateMock;

        public EditDeviceFormTests()
        {
            _authStateMock = new Mock<IAuthState>();
            Services.AddAuthorizationCore();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        private Models.Device CreateGpu(bool isAvailable = true)
        {
            return new Models.Device
            {
                Id = 123,
                Name = "Workstation-Alpha",
                OwnerUsername = "julie",
                IsAvailable = isAvailable,
                Model = "RTX 4090",
                VramMb = 24576,
                CudaCores = 16384,
                DriverVersion = "535.xx",
                PricePerHour = 4.5m,
                Frameworks = ["CUDA", "PyTorch"]
            };
        }

        // ------------------------------------------------------------
        // 1. MODE RENDERING
        // ------------------------------------------------------------

        [Fact]
        public void Add_Mode_Should_Show_Register_Title()
        {
            var cut = Render<EditDeviceForm>(p => p
                .Add(x => x.Mode, DevicePageMode.Add)
                .Add(x => x.gpu, CreateGpu()));

            cut.Markup.Should().Contain("Register New Device");
        }

        [Fact]
        public void Edit_Mode_Should_Show_Edit_Title()
        {
            var cut = Render<EditDeviceForm>(p => p
                .Add(x => x.Mode, DevicePageMode.Edit)
                .Add(x => x.gpu, CreateGpu()));

            cut.Markup.Should().Contain("Edit Device Specs");
        }

        // ------------------------------------------------------------
        // 2. BINDING (CRITICAL)
        // ------------------------------------------------------------

        [Fact]
        public void Name_Input_Should_Update_Model()
        {
            var gpu = CreateGpu();

            var cut = Render<EditDeviceForm>(p => p
                .Add(x => x.gpu, gpu));

            cut.Find("input").Change("New GPU Name");

            gpu.Name.Should().Be("New GPU Name");
        }

        // ------------------------------------------------------------
        // 3. AVAILABILITY UI
        // ------------------------------------------------------------

        [Fact]
        public void Available_Device_Should_Show_Available_Text()
        {
            var cut = Render<EditDeviceForm>(p => p
                .Add(x => x.gpu, CreateGpu()));

            cut.Markup.Should().Contain("Users can reserve and run workloads");
        }

        [Fact]
        public void Disabled_Device_Should_Show_Hidden_Text()
        {
            var gpu = CreateGpu();
            gpu.IsAvailable = false;

            var cut = Render<EditDeviceForm>(p => p
                .Add(x => x.gpu, gpu));

            cut.Markup.Should().Contain("Device is hidden");
        }

        // ------------------------------------------------------------
        // 4. VALIDATION RENDERING
        // ------------------------------------------------------------

        [Fact]
        public void Validation_Errors_Should_Render_When_Present()
        {
            var cut = Render<EditDeviceForm>(p => p
                .Add(x => x.gpu, CreateGpu()));

            cut.Markup.Should().Contain("Please fix the following issues");
            cut.Markup.Should().Contain("Error1");
        }

        // ------------------------------------------------------------
        // 5. JS INTEROP (COPY TOKEN)
        // ------------------------------------------------------------

        [Fact]
        public void Copy_Button_Should_Invoke_Clipboard_JS()
        {
            JSInterop.SetupVoid("navigator.clipboard.writeText");

            var cut = Render<EditDeviceForm>(p => p
                .Add(x => x.gpu, CreateGpu()));

            cut.Find(".agent-command-container button").Click();

            JSInterop.VerifyInvoke("navigator.clipboard.writeText");
        }

        // ------------------------------------------------------------
        // 6. SAVE BEHAVIOR
        // ------------------------------------------------------------

        [Fact]
        public void Valid_Submit_Should_Switch_Mode_To_View()
        {
            var gpu = CreateGpu();

            bool invoked = false;

            var cut = Render<EditDeviceForm>(p => p
                .Add(x => x.OnSave,
                    EventCallback.Factory.Create<Models.Device>(
                        this,
                        _ => invoked = true)));

            cut.Find("form").Submit();

            invoked.Should().BeTrue();
        }
    }
}
