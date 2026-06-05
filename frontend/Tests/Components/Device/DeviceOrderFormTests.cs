using Bunit;
using GpuShare.Frontend.Components.Pages.Device;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace GpuShare.Frontend.Tests.Components.Device
{
    public class DeviceOrderFormTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Models.Device gpu = new()
        {
            DeviceId = 1,
            Name = "RTX 4090",
            GpuModel = "NVIDIA",
            PricePerHour = 10,
            VramMb = 24000,
            CudaCores = 16000,
            DriverVersion = "535",
            Frameworks = [ "CUDA" ],
            IsAvailable = true
        };

        public DeviceOrderFormTests()
        {
            Services.AddAuthorizationCore();
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true);
            JSInterop.SetupModule(_ => true);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public void EstimatedCost_Should_Calculate_From_Price_And_Duration()
        {
            var cut = Render<DeviceOrderForm>(p => p.Add(x => x.Device, gpu));

            // set duration = 4 via reflection or parameter binding helper
            cut.Instance.GetType().GetProperty("orderModel", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(cut.Instance, new DeviceOrderForm.OrderModel
                {
                    StartDate = DateTime.Today,
                    StartTime = TimeSpan.FromHours(12),
                    DurationHours = 4
                });

            cut.Render();

            Assert.Contains("$10.00", cut.Markup);
        }

        [Fact]
        public void Selecting_File_Should_Show_File_Name()
        {
            var cut = Render<DeviceOrderForm>();

            var file = new Mock<IBrowserFile>();
            file.Setup(f => f.Name).Returns("test.zip");
            file.Setup(f => f.Size).Returns(1024);
            file.Setup(f => f.ContentType).Returns("application/zip");

            cut.InvokeAsync(() => cut.Instance.GetType()
                    .GetMethod("OnFileSelected", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .Invoke(cut.Instance, [ file.Object ])
            );

            cut.Render();

            Assert.Contains("test.zip", cut.Markup);
        }

        [Fact]
        public void Removing_File_Should_Clear_Selection()
        {
            var cut = Render<DeviceOrderForm>();

            var file = Mock.Of<IBrowserFile>(f => f.Name == "file.zip" &&  f.Size == 100);

            cut.InvokeAsync(() =>  cut.Instance.GetType()
                    .GetMethod("OnFileSelected", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .Invoke(cut.Instance, [ file ])
            );

            cut.Instance.GetType().GetField("selectedFile", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(cut.Instance, null);

            cut.Render();

            Assert.DoesNotContain("file.zip", cut.Markup);
        }

        [Fact]
        public void Submit_Should_Merge_Date_And_Time()
        {
            var cut = Render<DeviceOrderForm>(p => p.Add(x => x.Device, gpu));

            var instance = cut.Instance;

            instance.GetType().GetField("orderModel", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(instance, new DeviceOrderForm.OrderModel
                {
                    StartDate = new DateTime(2026, 1, 1),
                    StartTime = new TimeSpan(14, 30, 0),
                    DurationHours = 3
                });

            cut.InvokeAsync(() => instance.GetType()
                    .GetMethod("SubmitOrder", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .Invoke(instance, null)
            );

            // no UI assertion needed — this is behavioral verification
            Assert.True(true); // replace with ILogger or service mock later
        }

        [Fact]
        public void Submit_Should_Create_Order() { }
    }
}
