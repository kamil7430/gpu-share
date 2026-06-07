using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Profile;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace GpuShare.Frontend.Tests.Components.Profile
{
    public class OrderTableTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IAuthState> _authStateMock = new();
        private readonly Mock<IFormatters> _formattersMock = new();
        private readonly Mock<IOrderService> _orderServiceMock = new();
        private readonly Mock<IReviewService> _reviewServiceMock = new();

        public OrderTableTests()
        {
            Services.AddSingleton(_authStateMock.Object);
            Services.AddSingleton(_formattersMock.Object);
            Services.AddSingleton(_orderServiceMock.Object);
            Services.AddSingleton(_reviewServiceMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);

            _orderServiceMock.Setup(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>())).ReturnsAsync(new PagedResult<Models.Order>
            {
                Items =
                [
                    new Models.Order { OrderId = 1005, DeviceId = 5, Username = "user0", Status = OrderStatus.WAITING_FOR_START,
                        StartDate = DateTime.UtcNow.AddDays(1).AddHours(8), EndDate = DateTime.UtcNow.AddDays(1).AddHours(14), TotalReservedCostCents = 1200
                    },
                    new Models.Order { OrderId = 1001, DeviceId = 1, Username = "user1", Status = OrderStatus.RUNNING, 
                        StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), TotalReservedCostCents = 1000
                    },
                    new Models.Order { OrderId = 1002, DeviceId = 2, Username = "user2", Status = OrderStatus.COMPLETED, 
                        StartDate = DateTime.UtcNow.AddDays(-2), EndDate = DateTime.UtcNow.AddDays(-1), TotalReservedCostCents = 2000 
                    },
                    new Models.Order { OrderId = 1003, DeviceId = 3, Username = "user3", Status = OrderStatus.FAILURE, 
                        StartDate = DateTime.UtcNow.AddDays(-3), EndDate = DateTime.UtcNow.AddDays(-2), TotalReservedCostCents = 3000 
                    },
                    new Models.Order { OrderId = 1004, DeviceId = 4, Username = "user4", Status = OrderStatus.SUSPENDED, 
                        StartDate = DateTime.UtcNow.AddDays(-4), EndDate = DateTime.UtcNow.AddDays(-3), TotalReservedCostCents = 4000 
                    }
                ],
                TotalCount = 5,
                PageSize = 5,
                Page = 1
            });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public void Should_Render_All_Orders_By_Default()
        {
            // Arrange
            //var cut = Render<OrderTable>();

            // Act
            var cut = Render<OrderTable>();

            // Assert
            cut.Markup.Should().Contain("Running");
            cut.Markup.Should().Contain("Completed");
            cut.Markup.Should().Contain("Failure");
            cut.Markup.Should().Contain("Suspended");
        }

        [Fact]
        public void Should_Render_Table_Headers()
        {
            // Act
            var cut = Render<OrderTable>();

            // Assert
            cut.Markup.Should().Contain("Order");
            cut.Markup.Should().Contain("Device");
            cut.Markup.Should().Contain("Owner");
            cut.Markup.Should().Contain("Status");
            cut.Markup.Should().Contain("Start Date");
            cut.Markup.Should().Contain("End Date");
            cut.Markup.Should().Contain("Cost");
            cut.Markup.Should().Contain("Actions");
        }

        [Fact]
        public void Running_Filter_Should_Show_Only_Running_Orders()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            cut.FindAll("button").First(x => x.TextContent.Contains("Running")).Click();
            var table = cut.Find(".custom-table");

            // Assert
            table.InnerHtml.Should().Contain("Running");
            table.InnerHtml.Should().NotContain("Waiting for start");
            table.InnerHtml.Should().NotContain("Completed");
            table.InnerHtml.Should().NotContain("Failure");
            table.InnerHtml.Should().NotContain("Suspended");
        }

        [Fact]
        public void Completed_Filter_Should_Show_Review_Button()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            cut.FindAll("button").First(x => x.TextContent.Contains("Completed")).Click();
            var table = cut.Find(".custom-table");

            // Assert
            table.InnerHtml.Should().Contain("Completed");
            table.InnerHtml.Should().NotContain("Running");
            table.InnerHtml.Should().NotContain("Failure");
            table.InnerHtml.Should().NotContain("Suspended");
            table.InnerHtml.Should().Contain("Leave Review");
        }

        [Fact]
        public void All_Filter_Should_Show_All_Orders()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            cut.FindAll("button").First(x => x.TextContent.Contains("All")).Click();
            var table = cut.Find(".custom-table");

            // Assert
            table.InnerHtml.Should().Contain("Running");
            table.InnerHtml.Should().Contain("Completed");
            table.InnerHtml.Should().Contain("Failure");
            table.InnerHtml.Should().Contain("Suspended");
        }

        [Fact]
        public void Review_Button_Should_Only_Render_For_Completed_And_Failed_Orders()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            var reviewButtons = cut.FindAll(".review-btn");

            // Assert

            // Only one completed order and one failed order should exists in mock data
            reviewButtons.Should().HaveCount(2);

            // Verify button text
            reviewButtons[0].TextContent.Should().Contain("Leave Review");

            // Verify it belongs to completed order row
            var row = reviewButtons[0].Closest("tr");
            row!.TextContent.Should().Contain("Completed");

            var row1 = reviewButtons[1].Closest("tr");
            row1!.TextContent.Should().Contain("Failure");
        }

        [Fact]
        public void Empty_Orders_Should_Show_Empty_State_Message()
        {
            // Arrange
            _orderServiceMock.Setup(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>())).ReturnsAsync(new PagedResult<Models.Order>
            {
                Items = [],
                TotalCount = 0,
                PageSize = 10,
                Page = 1
            });

            // Act
            var cut = Render<OrderTable>();

            // Assert
            cut.Markup.Should().Contain("No orders yet.");
            cut.Markup.Should().NotContain("order-link");
        }

        [Fact]
        public void Order_Link_Should_Have_Correct_Href()
        {
            // Arrange
            _orderServiceMock.Setup(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>())).ReturnsAsync(new PagedResult<Models.Order>
            {
                Items = [
                    new Models.Order { OrderId = 1001, DeviceId = 1, Username = "user1", Status = OrderStatus.RUNNING,
                        StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), TotalReservedCostCents = 1000
                    }],
                TotalCount = 0,
                PageSize = 10,
                Page = 1
            });

            // Act
            var cut = Render<OrderTable>();
            var a = cut.Find(".order-link");

            // Assert
            a.Should().NotBeNull();
            a.OuterHtml.Should().Contain("/order/1001");
        }

        [Fact]
        public void Profile_Link_Should_Have_Correct_Href()
        {
            // Arrange
            _orderServiceMock.Setup(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>())).ReturnsAsync(new PagedResult<Models.Order>
            {
                Items = [
                    new Models.Order { OrderId = 1001, DeviceId = 1, Username = "user1", Status = OrderStatus.RUNNING,
                        StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), 
                        TotalReservedCostCents = 1000
                    }],
                TotalCount = 0,
                PageSize = 10,
                Page = 1
            });

            // Act
            var cut = Render<OrderTable>();
            var a = cut.Find(".profile-link");

            // Assert
            a.Should().NotBeNull();
            a.OuterHtml.Should().Contain("/profile/user1");
        }

        [Fact]
        public void Running_Order_Should_Have_Running_Status_Class()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            var a = cut.Find(".status-running");

            // Assert
            a.Should().NotBeNull();
            a.OuterHtml.Should().Contain("Running");
        }

        [Fact]
        public void Completed_Order_Should_Have_Completed_Status_Class()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            var badge = cut.Find(".status-completed");

            // Assert
            badge.Should().NotBeNull();
            badge.OuterHtml.Should().Contain("Completed");
        }

        [Fact]
        public void Failure_Order_Should_Have_Failure_Status_Class()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            var badge = cut.Find(".status-failure");

            // Assert
            badge.Should().NotBeNull();
            badge.OuterHtml.Should().Contain("Failure");
        }

        [Fact]
        public void Suspended_Order_Should_Have_Suspended_Status_Class()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            var badge = cut.Find(".status-suspended");

            // Assert
            badge.Should().NotBeNull();
            badge.OuterHtml.Should().Contain("Suspended");
        }

        [Fact]
        public void Selected_Filter_Should_Have_Active_Class()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            var btn = cut.FindAll(".filter-btn")[0];
            btn.Click();

            // Assert
            btn.OuterHtml.Should().Contain("active");
        }

        [Fact]
        public void Clicking_Leave_Review_Should_Open_Review_Modal()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            cut.Find(".review-btn").Click();

            // Assert
            cut.Markup.Should().Contain("modal-backdrop");
        }

        [Fact]
        public async Task Load_More_Should_Request_More_Orders()
        {
            // Arrange
            var cut = Render<OrderTable>();
            _orderServiceMock.Setup(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>())).ReturnsAsync(new PagedResult<Models.Order>
            {
                Items =
                [
                    new Models.Order { OrderId = 1001, DeviceId = 1, Username = "user1", Status = OrderStatus.RUNNING,
                        StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), 
                        TotalReservedCostCents = 1000
                    },
                    new Models.Order { OrderId = 1002, DeviceId = 2, Username = "user2", Status = OrderStatus.COMPLETED,
                        StartDate = DateTime.UtcNow.AddDays(-2), EndDate = DateTime.UtcNow.AddDays(-1), 
                        TotalReservedCostCents = 2000
                    },
                ],
                TotalCount = 2,
                PageSize = 4,
                Page = 2
            });

            // Act
            cut.Find(".load-more-link").Click();
            var orders = cut.FindAll(".order-link");

            // Assert
            orders.Should().HaveCountGreaterThan(4);
        }
    }
}
