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
        private Mock<IAuthState> _authStateMock;
        private Mock<IOrderService> _orderServiceMock;

        public OrderTableTests()
        {
            _authStateMock = new Mock<IAuthState>();
            _orderServiceMock = new Mock<IOrderService>();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddSingleton(_orderServiceMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);

            _orderServiceMock.Setup(x => x.ListOrdersAsync(It.IsAny<OrderQueryParams>())).ReturnsAsync(new PagedResult<Models.Order>
            {
                Items =
                [
                    new Models.Order { Id = 1001, DeviceId = 1, OwnerUsername = "user1", Status = "Active", 
                        StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), Cost = 10.00m
                    },
                    new Models.Order { Id = 1002, DeviceId = 2, OwnerUsername = "user2", Status = "Completed", 
                        StartDate = DateTime.UtcNow.AddDays(-2), EndDate = DateTime.UtcNow.AddDays(-1), Cost = 20.00m 
                    },
                    new Models.Order { Id = 1003, DeviceId = 3, OwnerUsername = "user3", Status = "Cancelled", 
                        StartDate = DateTime.UtcNow.AddDays(-3), EndDate = DateTime.UtcNow.AddDays(-2), Cost = 30.00m 
                    },
                    new Models.Order { Id = 1004, DeviceId = 4, OwnerUsername = "user4", Status = "Dispute", 
                        StartDate = DateTime.UtcNow.AddDays(-4), EndDate = DateTime.UtcNow.AddDays(-3), Cost = 40.00m 
                    }
                ],
                TotalCount = 4,
                PageSize = 10,
                Page = 1
            });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
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
            cut.Markup.Should().Contain("Active");
            cut.Markup.Should().Contain("Completed");
            cut.Markup.Should().Contain("Cancelled");
            cut.Markup.Should().Contain("Dispute");
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
        public void Active_Filter_Should_Show_Only_Active_Orders()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            cut.FindAll("button").First(x => x.TextContent.Contains("Active")).Click();
            var table = cut.Find(".custom-table");

            // Assert
            table.InnerHtml.Should().Contain("Active");
            table.InnerHtml.Should().NotContain("Completed");
            table.InnerHtml.Should().NotContain("Cancelled");
            table.InnerHtml.Should().NotContain("Dispute");
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
            table.InnerHtml.Should().NotContain("Active");
            table.InnerHtml.Should().NotContain("Cancelled");
            table.InnerHtml.Should().NotContain("Dispute");
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
            table.InnerHtml.Should().Contain("Active");
            table.InnerHtml.Should().Contain("Completed");
            table.InnerHtml.Should().Contain("Cancelled");
            table.InnerHtml.Should().Contain("Dispute");
        }

        [Fact]
        public void Review_Button_Should_Only_Render_For_Completed_Orders()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            var reviewButtons = cut.FindAll(".review-btn");

            // Assert

            // Only one completed order exists in mock data
            reviewButtons.Should().HaveCount(1);

            // Verify button text
            reviewButtons[0].TextContent.Should().Contain("Leave Review");

            // Verify it belongs to completed order row
            var row = reviewButtons[0].Closest("tr");

            row!.TextContent.Should().Contain("Completed");
            row!.TextContent.Should().Contain("#1001");
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
                    new Models.Order { Id = 1001, DeviceId = 1, OwnerUsername = "user1", Status = "Active",
                        StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), Cost = 10.00m
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
                    new Models.Order { Id = 1001, DeviceId = 1, OwnerUsername = "user1", Status = "Active",
                        StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1), Cost = 10.00m
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
        public void Active_Order_Should_Have_Active_Status_Class()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            var a = cut.Find(".status-active");

            // Assert
            a.Should().NotBeNull();
            a.OuterHtml.Should().Contain("Active");
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
        public void Cancelled_Order_Should_Have_Cancelled_Status_Class()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            var badge = cut.Find(".status-cancelled");

            // Assert
            badge.Should().NotBeNull();
            badge.OuterHtml.Should().Contain("Cancelled");
        }

        [Fact]
        public void Dispute_Order_Should_Have_Dispute_Status_Class()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            var badge = cut.Find(".status-dispute");

            // Assert
            badge.Should().NotBeNull();
            badge.OuterHtml.Should().Contain("Dispute");
        }

        [Fact]
        public void Selected_Filter_Should_Have_Active_Class()
        {
            // Arrange
            var cut = Render<OrderTable>();

            // Act
            var btn = cut.FindAll(".filter-btn").First();
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
        public async Task Load_More_Should_Request_Next_Page()
        {

        }
    }
}
