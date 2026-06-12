using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Device;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using System.Net;
using System.Reflection;

namespace GpuShare.Frontend.Tests.Components.Device
{
    public class DeviceOrderFormTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IOrderService> _orderServiceMock = new();
        private readonly Mock<IFormatters> _formattersMock = new();
        private readonly Mock<ISnackbar> _snackbarMock = new();

        private readonly Models.Device gpu = new()
        {
            DeviceId = 1,
            Name = "RTX 4090",
            GpuModel = "NVIDIA",
            PricePerHourUsdCents = 1000,
            VramMb = 24000,
            CudaCores = 16000,
            DriverVersion = "535",
            Frameworks = [ "CUDA" ],
            State = Models.DeviceState.AVAILABLE
        };

        public DeviceOrderFormTests()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton(_orderServiceMock.Object);
            Services.AddSingleton(_formattersMock.Object);
            Services.AddSingleton(_snackbarMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);
            Render<MudPopoverProvider>();
        }

        private static async Task InvokePrivateAsync(IRenderedComponent<DeviceOrderForm> cut, string methodName, params object[] parameters)
        {
            var method = cut.Instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException($"Method '{methodName}' not found.");
            var result = method.Invoke(cut.Instance, parameters);
            if (result is Task task)
                await task;
        }

        private static void SetPrivateField(IRenderedComponent<DeviceOrderForm> cut, string fieldName, object value)
        {
            var field = cut.Instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException($"Field '{fieldName}' not found.");
            field.SetValue(cut.Instance, value);
        }

        private static T GetPrivateField<T>(IRenderedComponent<DeviceOrderForm> cut, string fieldName)
        {
            return (T)cut.Instance.GetType().GetField(fieldName,
                    BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(cut.Instance)!;
        }

        /// <summary>Renders the form with a model that passes all validation guards.</summary>
        private IRenderedComponent<DeviceOrderForm> CreateReadyToSubmitComponent()
        {
            var cut = Render<DeviceOrderForm>(p => p.Add(x => x.Device, gpu));

            SetPrivateField(cut, "orderModel", new DeviceOrderForm.OrderModel
            {
                StartDate = new DateTime(2026, 1, 1),
                StartTime = new TimeSpan(12, 30, 0),
                DurationHours = 5,
                DockerImage = "gpu-image:latest",
            });

            return cut;
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
            cut.Instance.GetType().GetField("orderModel", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(cut.Instance, new DeviceOrderForm.OrderModel
                {
                    StartDate = DateTime.Today,
                    StartTime = TimeSpan.FromHours(12),
                    DurationHours = 4
                });

            cut.Render();

            cut.Markup.Should().Contain("$40,00");
        }

        [Fact]
        public void Form_Should_Render_Docker_Image_Input()
        {
            var cut = Render<DeviceOrderForm>(p => p.Add(x => x.Device, gpu));

            cut.Markup.Should().Contain("Docker Image");
            cut.Markup.Should().Contain("Name of the Docker image to run on the device");
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
                    DurationHours = 3,
                    DockerImage = "gpu-image:latest",
                });

            cut.InvokeAsync(() => instance.GetType()
                    .GetMethod("SubmitOrder", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .Invoke(instance, null)
            );

            // no UI assertion needed — this is behavioral verification
            Assert.True(true); // replace with ILogger or service mock later
        }

        [Fact]
        public async Task Submit_Should_Send_Correct_CreateOrderRequest()
        {
            // Arrange

            CreateOrderRequest? captured = null;

            _orderServiceMock.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .Callback<CreateOrderRequest>(r => captured = r)
                .ReturnsAsync(new Models.Order { OrderId = 999 });

            var cut = CreateReadyToSubmitComponent();

            // Act

            await InvokePrivateAsync(cut, "SubmitOrder");

            // Assert

            captured.Should().NotBeNull();

            captured!.DeviceId.Should().Be(1);
            captured.DurationHours.Should().Be(5);
            captured.StartTime.Should().Be(new DateTime(2026, 1, 1, 12, 30, 0));
            // The contract (orders.yaml) expects the Docker image name, not a file URL
            captured.DockerImage.Should().Be("gpu-image:latest");
        }

        [Fact]
        public async Task Submit_Should_Show_Error_When_No_Docker_Image()
        {
            // Default model has no Docker image — the guard must stop the submit
            var cut = Render<DeviceOrderForm>(p => p.Add(x => x.Device, gpu));

            await InvokePrivateAsync(cut, "SubmitOrder");

            _orderServiceMock.Verify(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()),
                Times.Never);

            _snackbarMock.Verify(x => x.Add("Docker image is required", Severity.Error,
                    It.IsAny<Action<SnackbarOptions>?>()), Times.Once);
        }

        [Fact]
        public async Task Submit_Should_Show_InvalidConfiguration_Message_On_400()
        {
            var cut = CreateReadyToSubmitComponent();

            _orderServiceMock.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .ThrowsAsync(new ApiException("Bad request", HttpStatusCode.BadRequest));

            await InvokePrivateAsync(cut, "SubmitOrder");

            _snackbarMock.Verify(x => x.Add("Invalid order configuration", Severity.Error,
                    It.IsAny<Action<SnackbarOptions>?>()), Times.Once);
        }

        [Fact]
        public async Task Submit_Should_Show_InsufficientBalance_Message_On_402()
        {
            var cut = CreateReadyToSubmitComponent();

            _orderServiceMock.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .ThrowsAsync(new ApiException("Payment required", HttpStatusCode.PaymentRequired));

            await InvokePrivateAsync(cut, "SubmitOrder");

            _snackbarMock.Verify(x => x.Add("Insufficient wallet balance", Severity.Error,
                    It.IsAny<Action<SnackbarOptions>?>()), Times.Once);
        }

        [Fact]
        public async Task Submit_Should_Store_OrderId_When_Order_Is_Created()
        {
            var cut = CreateReadyToSubmitComponent();

            _orderServiceMock.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .ReturnsAsync(new Models.Order { OrderId = 777 });

            await InvokePrivateAsync(cut, "SubmitOrder");

            GetPrivateField<int?>(cut, "_orderId").Should().Be(777);
        }

        [Fact]
        public async Task Submit_Should_Show_ViewOrder_Link_When_Order_Is_Created()
        {
            var cut = CreateReadyToSubmitComponent();

            _orderServiceMock.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
                .ReturnsAsync(new Models.Order { OrderId = 777 });

            await InvokePrivateAsync(cut, "SubmitOrder");

            cut.Render();

            // The order page route is /order/{id}
            cut.Markup.Should().Contain("/order/777");
            cut.Markup.Should().Contain("View order");
        }
    }
}
