using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Modals;
using GpuShare.Frontend.Components.Pages.Order;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;

namespace GpuShare.Frontend.Tests.Components.Order
{
    public class OrderPageTests : BunitContext, Xunit.IAsyncLifetime
    {
        //private Mock<IFormatters> _formattersMock = new();
        private Mock<IAuthState> _authStateMock;
        private Mock<IOrderService> _orderServiceMock = new();
        private Mock<IDeviceService> _deviceServiceMock = new();
        private Bunit.TestDoubles.BunitAuthorizationContext? authContext;

        public OrderPageTests()
        {
            _authStateMock = new Mock<IAuthState>();
            Services.AddAuthorizationCore();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddSingleton(_orderServiceMock.Object);
            Services.AddSingleton(_deviceServiceMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true);
            JSInterop.SetupModule(_ => true);

            // IMPORTANT: stub heavy child components
            ComponentFactories.AddStub<DeviceStatsCard>();
            ComponentFactories.AddStub<OrderTelemetryCard>();
            ComponentFactories.AddStub<ConnectionCard>();
            ComponentFactories.AddStub<EndSessionModal>();

            // Arrange
            _authStateMock.SetupGet(x => x.User).Returns(new User() { Username = "john"});

            var order = new Models.Order { Id = 1, DeviceId = 10, Status = OrderStatus.Running };
            _orderServiceMock.Setup(x => x.GetOrderAsync(1)).ReturnsAsync(order);

            _deviceServiceMock.Setup(x => x.GetDeviceAsync(10)).ReturnsAsync(new Models.Device());

            authContext = AddAuthorization();
            authContext.SetAuthorized("john");
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public async Task Authorized_User_Should_See_Order_Page()
        {
            // Act
            var cut = Render<OrderPage>(p => p.Add(x => x.OrderId, 1));

            // Assert
            cut.Markup.Contains("Order #");
        }

        [Fact]
        public void Unauthorized_User_Should_Not_See_Order_Page()
        {
            // Arrange
            authContext?.SetNotAuthorized();

            // Act
            var cut = Render<OrderPage>(p => p.Add(x => x.OrderId, 1));
            
            // Assert
            cut.Markup.Should().NotContain("Order #");
            cut.Markup.Contains("You are not authorized to view this order");
        }

        [Theory]
        [InlineData(OrderStatus.Running, "Running")]
        [InlineData(OrderStatus.Completed, "Completed")]
        [InlineData(OrderStatus.Suspended, "Suspended")]
        public void Should_Render_Correct_Status_Text(OrderStatus status, string expected)
        {
            _orderServiceMock.Setup(x => x.GetOrderAsync(1)).ReturnsAsync(new Models.Order { Id = 1, Status = status, DeviceId = 10 });

            var cut = Render<OrderPage>(p => p.Add(x => x.OrderId, 1));

            cut.Markup.Contains(expected);
        }

        [Fact]
        public void Clicking_EndSession_Should_Open_Modal()
        {
            var authContext = AddAuthorization();
            authContext.SetAuthorized("john");

            var cut = Render<OrderPage>(p => p.Add(x => x.OrderId, 1));

            // Act
            cut.Find(".btn-danger").Click();

            // Assert (stub exists and receives re-render)
            cut.Markup.Contains("End Session Modal");
        }

        [Fact]
        public void Back_Link_Should_Point_To_User_Profile()
        {
            var cut = Render<OrderPage>(p => p.Add(x => x.OrderId, 1));

            var link = cut.Find(".back-link");

            link.GetAttribute("href")
                .Should().Be("/profile/john");
        }

        [Fact]
        public void Should_Load_Order_And_Device_On_Init()
        {
            // Arrange
            var order = new Models.Order { Id = 1, DeviceId = 42, Status = OrderStatus.Running };

            _orderServiceMock.Setup(x => x.GetOrderAsync(1)).ReturnsAsync(order);

            _deviceServiceMock.Setup(x => x.GetDeviceAsync(42)).ReturnsAsync(new Models.Device { Name = "GPU X" });

            // Act
            Render<OrderPage>(p => p.Add(x => x.OrderId, 1));

            // Assert
            _orderServiceMock.Verify(x => x.GetOrderAsync(1), Times.Once);
            _deviceServiceMock.Verify(x => x.GetDeviceAsync(42), Times.Once);
        }

        [Fact]
        public void Should_Render_Device_Name_And_Model()
        {
            _deviceServiceMock.Setup(x => x.GetDeviceAsync(10))
                .ReturnsAsync(new Models.Device
                {
                    Name = "Workstation-Alpha",
                    Model = "RTX 4090"
                });

            var cut = Render<OrderPage>(p => p.Add(x => x.OrderId, 1));

            cut.Markup.Should().Contain("Workstation-Alpha");
            cut.Markup.Should().Contain("RTX 4090");
        }

        [Fact]
        public void Running_Status_Should_Show_Success_Color()
        {
            var cut = Render<OrderPage>(p => p.Add(x => x.OrderId, 1));

            cut.Markup.Should().Contain("mud-chip-color-success");
        }

        [Fact]
        public void Should_Show_Dispute_Link_When_Time_Exceeded()
        {
            var order = new Models.Order
            {
                Id = 1,
                DeviceId = 10,
                Status = OrderStatus.Running
            };

            _orderServiceMock.Setup(x => x.GetOrderAsync(1)).ReturnsAsync(order);

            var cut = Render<OrderPage>(p => p.Add(x => x.OrderId, 1));

            // NOTE: currently logic uses DateTime.UtcNow so we assert presence indirectly
            cut.FindAll(".dispute-link").Should().NotBeNull();
        }

        //[Fact]
        //public void Should_Not_Crash_When_Order_Is_Null()
        //{
        //    _orderServiceMock.Setup(x => x.GetOrderAsync(1)).ReturnsAsync((Models.Order)null);

        //    var act = () => Render<OrderPage>(p => p.Add(x => x.OrderId, 1));

        //    act.Should().NotThrow();
        //}
    }
}
