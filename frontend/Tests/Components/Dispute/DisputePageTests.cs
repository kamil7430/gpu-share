using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Dispute;
using GpuShare.Frontend.State;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;

namespace GpuShare.Frontend.Tests.Components.Dispute
{
    public class DisputePageTests : BunitContext, Xunit.IAsyncLifetime
    {
        private Mock<IAuthState> _authStateMock;

        public DisputePageTests()
        {
            _authStateMock = new Mock<IAuthState>();
            Services.AddAuthorizationCore();
            Services.AddSingleton(_authStateMock.Object);
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
        public async Task Authorized_User_Should_See_Order_Page()
        {
            // Arrange
            var authContext = AddAuthorization();
            authContext.SetAuthorized("john");

            // Act
            var cut = Render<DisputePage>();

            // Assert
            cut.Markup.Contains("Report a Dispute");
        }

        [Fact]
        public void Unauthorized_User_Should_Not_See_Order_Page()
        {
            // Arrange
            var authContext = AddAuthorization();
            //authContext.SetAuthorized("TEST USER", AuthorizationState.Unauthorized);
            authContext.SetNotAuthorized();

            // Act
            var cut = Render<DisputePage>();

            // Assert
            cut.Markup.Should().NotContain("Report a Dispute");
        }
    }
}
