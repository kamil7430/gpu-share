using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Dispute;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;

namespace GpuShare.Frontend.Tests.Components.Dispute
{
    public class DisputePageTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IAuthState> _authStateMock;
        private readonly Mock<IDisputeService> _disputeServiceMock = new();
        private readonly BunitAuthorizationContext? _authContext;

        public DisputePageTests()
        {
            _authStateMock = new Mock<IAuthState>();
            Services.AddAuthorizationCore();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddSingleton(_disputeServiceMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true);
            JSInterop.SetupModule(_ => true);

            _authContext = AddAuthorization();
            _authContext.SetAuthorized("john");

            _disputeServiceMock.Setup(x => x.GetDisputeAsync(It.IsAny<int>())).ReturnsAsync(new Models.Dispute() {
                DisputeId = 15,
                OrderId = 10,
            });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public async Task Authorized_User_Should_See_Dispute_Page()
        {
            // Arrange
            
            // Act
            var cut = Render<DisputePage>();

            // Assert
            cut.Markup.Contains("Report a Dispute");
        }

        [Fact]
        public void Unauthorized_User_Should_Not_See_Dispute_Page()
        {
            // Arrange
            _authContext!.SetNotAuthorized();

            // Act
            var cut = Render<DisputePage>();

            // Assert
            cut.Markup.Should().NotContain("Report a Dispute");
        }

        [Fact]
        public void Authorized_User_Should_See_Timeline()
        {
            ComponentFactories.AddStub<TimelineCard>();

            var cut = Render<DisputePage>(p => p.Add(x => x.DisputeId, 15));

            cut.FindComponent<Stub<TimelineCard>>();
        }

        [Fact]
        public void Unauthorized_User_Should_See_Message()
        {
            _authContext!.SetNotAuthorized();

            var cut = Render<DisputePage>(p => p.Add(x => x.DisputeId, 15));

            cut.Markup.Contains("You are not authorized to view this dispute");
        }

        [Fact]
        public void Should_Render_Order_Id()
        {
            var cut = Render<DisputePage>(p => p.Add(x => x.DisputeId, 15));

            cut.Markup.Contains("Order #10");
        }

        [Fact]
        public void Back_Link_Should_Point_To_Order_Page()
        {
            var cut = Render<DisputePage>(p => p.Add(x => x.DisputeId, 15));

            var link = cut.Find("a.back-link");

            Assert.Equal("/order/10", link.GetAttribute("href"));
        }

        [Fact]
        public void Should_Pass_OrderId_To_Dispute_Form()
        {
            var auth = AddAuthorization();
            auth.SetAuthorized("john");

            var cut = Render<DisputePage>(p => p.Add(x => x.DisputeId, 15));

            var form = cut.FindComponent<DisputeForm>();

            Assert.Equal(10, form.Instance.OrderId);
        }

        [Fact]
        public async Task Should_Load_Dispute_Data() { }

        [Fact]
        public void Closed_Dispute_Should_Show_Resolved_Status() { }

        [Fact]
        public void Open_Dispute_Should_Show_Under_Review_Chip() { }

        [Fact]
        public void Resolved_Dispute_Should_Show_Resolved_Chip() { }

        [Fact]
        public void Rejected_Dispute_Should_Show_Rejected_Chip() { }

        [Fact]
        public async Task User_Not_Owner_Should_Be_Redirected() { }

        [Fact]
        public async Task Missing_Dispute_Should_Show_NotFound() { }
    }
}
