using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Device;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;

namespace GpuShare.Frontend.Tests.Components.Device
{
    public class ReservationCalendarTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IOrderService> _orderServiceMock = new();
        public ReservationCalendarTests()
        {
            Services.AddSingleton(_orderServiceMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);

            var now = DateTime.Today.AddHours(10);
            _orderServiceMock.Setup(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>()))
                .ReturnsAsync(new PagedResult<Models.Order>
                {
                    Items = [
                        new Models.Order
                        {
                            Id = 1,
                            OwnerUsername = "john",
                            StartDate = now,
                            EndDate = now.AddHours(2),
                            Status = Models.OrderStatus.WaitingForStart
                        }
                    ]
                });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public void Should_Load_Orders_On_Initialize()
        {
            // Act
            Render<ReservationCalendar>();

            // Assert
            _orderServiceMock.Verify(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>()),Times.Once);
        }

        [Fact]
        public void Orders_Should_Render()
        {
            // Arrange
            var now = DateTime.Today.AddHours(10);

            // Act
            var cut = Render<ReservationCalendar>();
            var reservation = cut.Find(".reservation-block");

            // Assert
            reservation.Children.First().TextContent.Should().Contain("john");
            reservation.Children.Last().TextContent.Should().Contain("10:00");
            reservation.Children.Last().TextContent.Should().Contain("12:00");
        }

        [Fact]
        public async Task Next_Week_Should_Load_New_Data()
        {
            // Arrange
            var cut = Render<ReservationCalendar>();

            // initial load
            _orderServiceMock.Invocations.Clear();

            // Act
            var nextButton = cut.FindAll("button").First(x => x.TextContent.Contains("Next"));

            await nextButton.ClickAsync(new MouseEventArgs());

            // Assert
            _orderServiceMock.Verify(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>()), Times.Once);
        }

        [Fact]
        public async Task Week_Label_Should_Update_After_Navigatio()
        {
            var cut = Render<ReservationCalendar>();
            var initialText = cut.Markup;

            // Act
            var nextButton = cut.FindAll("button").First(x => x.TextContent.Contains("Next"));

            await nextButton.ClickAsync(new MouseEventArgs());

            // Assert
            cut.Markup.Should().NotBe(initialText);
        }

        [Fact]
        public async Task Previous_Week_Should_Load_New_Data()
        {
            // Arrange
            var cut = Render<ReservationCalendar>();

            _orderServiceMock.Invocations.Clear();

            // Act
            var previousButton = cut.FindAll("button").First(x => x.TextContent.Contains("Previous"));

            await previousButton.ClickAsync(new MouseEventArgs());

            // Assert
            _orderServiceMock.Verify(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>()), Times.Once);
        }

        [Fact]
        public void Empty_Orders_Should_Not_Render_Reservations()
        {
            // Arrange
            _orderServiceMock
                .Setup(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>()))
                .ReturnsAsync(new PagedResult<Models.Order>
                {
                    Items = []
                });

            // Act
            var cut = Render<ReservationCalendar>();

            // Assert
            cut.FindAll(".reservation-block").Should().BeEmpty();
        }

        [Fact]
        public void Reserved_Order_Should_Have_Reserved_Color()
        {
            // Arrange
            var now = DateTime.Today.AddHours(8);

            _orderServiceMock
                .Setup(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>()))
                .ReturnsAsync(new PagedResult<Models.Order>
                {
                    Items =
                    [
                        new Models.Order
                    {
                        Id = 1,
                        OwnerUsername = "alice",
                        StartDate = now,
                        EndDate = now.AddHours(1),
                        Status = Models.OrderStatus.WaitingForStart
                    }
                    ]
                });
            var cut = Render<ReservationCalendar>();

            // Act
            var reservation = cut.Find(".reservation-block");

            // Assert
            reservation.GetAttribute("style").Should().Contain("#ff9800");
        }

        [Fact]
        public void Should_Render_Seven_Days()
        {
            // Act
            var cut = Render<ReservationCalendar>();

            // Assert
            cut.FindAll(".calendar-header-cell").Should().HaveCount(7);
        }

        [Fact]
        public void Should_Render_24_Hours()
        {
            // Act
            var cut = Render<ReservationCalendar>();

            cut.FindAll(".time-label").Should().HaveCount(24);

            // Assert
            cut.Markup.Should().Contain("00:00");
            cut.Markup.Should().Contain("23:00");
        }

        [Fact]
        public void Should_Render_Legend()
        {
            // Act
            var cut = Render<ReservationCalendar>();

            // Assert
            cut.Markup.Should().Contain("Reserved");
            cut.Markup.Should().Contain("In Use");
            cut.Markup.Should().Contain("Completed");
            cut.Markup.Should().Contain("Available");
        }

        [Fact]
        public void Multiple_Orders_Should_Render()
        {
            // Arrange
            var now = DateTime.Today.AddHours(9);

            _orderServiceMock.Setup(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>()))
                .ReturnsAsync(new PagedResult<Models.Order>
                {
                    Items =
                    [
                        new Models.Order
                        {
                            Id = 1,
                            OwnerUsername = "john",
                            StartDate = now,
                            EndDate = now.AddHours(1),
                            Status = Models.OrderStatus.WaitingForStart
                        },
                        new Models.Order
                        {
                            Id = 2,
                            OwnerUsername = "alice",
                            StartDate = now.AddHours(2),
                            EndDate = now.AddHours(3),
                            Status = Models.OrderStatus.Completed
                        }
                    ]
                });

            // Act
            var cut = Render<ReservationCalendar>();

            // Assert
            cut.FindAll(".reservation-block").Should().HaveCount(2);

            cut.Markup.Should().Contain("john");
            cut.Markup.Should().Contain("alice");
        }

        [Fact]
        public void Completed_Order_Should_Have_Completed_Color()
        {
            // Arrange
            var now = DateTime.Today.AddHours(14);

            _orderServiceMock.Setup(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>()))
                .ReturnsAsync(new PagedResult<Models.Order>
                {
                    Items =
                    [
                        new Models.Order
                {
                    Id = 1,
                    OwnerUsername = "bob",
                    StartDate = now,
                    EndDate = now.AddHours(1),
                    Status = Models.OrderStatus.Completed
                }
                    ]
                });

            // Act
            var cut = Render<ReservationCalendar>();

            // Assert
            var reservation = cut.Find(".reservation-block");

            reservation.GetAttribute("style").Should().Contain("#2196f3");
        }

        [Fact]
        public void Orders_Should_Render_In_Correct_Hour()
        {
            // Arrange
            var now = DateTime.Today.AddHours(15);

            _orderServiceMock
                .Setup(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>()))
                .ReturnsAsync(new PagedResult<Models.Order>
                {
                    Items =
                    [
                        new Models.Order
                        {
                            Id = 1,
                            OwnerUsername = "correct-slot-user",
                            StartDate = now,
                            EndDate = now.AddHours(1),
                            Status = Models.OrderStatus.WaitingForStart
                        }
                    ]
                });

            // Act
            var cut = Render<ReservationCalendar>();

            // Assert
            cut.Markup.Should().Contain("15:00");
            cut.Markup.Should().Contain("correct-slot-user");

            cut.FindAll(".reservation-block").Should().ContainSingle();
        }

        [Fact]
        public void Should_Request_Correct_Week_Range()
        {
            // Arrange
            OrderQueryParams? capturedQuery = null;

            _orderServiceMock.Setup(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>()))
                .Callback<OrderQueryParams>(q => capturedQuery = q)
                .ReturnsAsync(new PagedResult<Models.Order>
                {
                    Items = []
                });

            // Act
            Render<ReservationCalendar>();

            // Assert
            capturedQuery.Should().NotBeNull();

            capturedQuery!.StartDate.Should().NotBeNull();
            capturedQuery.EndDate.Should().NotBeNull();

            (capturedQuery.EndDate - capturedQuery.StartDate)
                .Should()
                .Be(TimeSpan.FromDays(7));
        }
    }
}
