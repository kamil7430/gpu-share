using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Modals;
using GpuShare.Frontend.Components.Pages.Profile;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Xunit;

namespace GpuShare.Frontend.Tests.Components.Profile
{
    public class ProfileCardTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IAuthState> _authStateMock;
        private readonly Mock<IReviewService> _reviewServiceMock;

        public ProfileCardTests()
        {
            _authStateMock = new Mock<IAuthState>();
            _reviewServiceMock = new Mock<IReviewService>();

            Services.AddSingleton(_authStateMock.Object);
            Services.AddSingleton(_reviewServiceMock.Object);

            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);

            //ComponentFactories.AddStub<ChangePasswordModal>("CHANGE_PASSWORD_MODAL");
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        // =====================================================
        // USERNAME
        // =====================================================

        [Fact]
        public void Should_Render_Username_In_Header()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated)
                .Returns(false);

            // Act
            var cut = Render<ProfileCard>(p => p
                .Add(x => x.Username, "john"));

            // Assert
            cut.Find("h3").TextContent.Should().Contain("john");
        }

        // =====================================================
        // RATING
        // =====================================================

        [Fact]
        public void Should_Render_Rating_From_Rating_Service()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated)
                .Returns(false);

            _reviewServiceMock
                .Setup(x => x.GetUserRatingAsync("john"))
                .ReturnsAsync(new UserRatingDto
                {
                    AverageRating = 4.5m,
                    ReviewCount = 21
                });

            // Act
            var cut = Render<ProfileCard>(p => p
                .Add(x => x.Username, "john"));

            // Assert
            cut.Markup.Should().Contain("4.5/5");
            cut.Markup.Should().Contain("(21)");
        }

        // =====================================================
        // CHANGE PASSWORD BUTTON
        // =====================================================

        [Fact]
        public void Authenticated_User_Should_See_Change_Password_Button_On_Own_Profile()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated)
                .Returns(true);

            _authStateMock.SetupGet(x => x.User)
                .Returns(new User
                {
                    Username = "john"
                });

            // Act
            var cut = Render<ProfileCard>(p => p
                .Add(x => x.Username, "john"));

            // Assert
            cut.Markup.Should().Contain("Change Password");
        }

        [Fact]
        public void Authenticated_User_Should_Not_See_Change_Password_Button_On_Other_Profile()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated)
                .Returns(true);

            _authStateMock.SetupGet(x => x.User)
                .Returns(new User
                {
                    Username = "alice"
                });

            // Act
            var cut = Render<ProfileCard>(p => p
                .Add(x => x.Username, "john"));

            // Assert
            cut.Markup.Should().NotContain("Change Password");
        }

        [Fact]
        public void Unauthenticated_User_Should_Not_See_Change_Password_Button()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated)
                .Returns(false);

            // Act
            var cut = Render<ProfileCard>(p => p
                .Add(x => x.Username, "john"));

            // Assert
            cut.Markup.Should().NotContain("Change Password");
        }

        // =====================================================
        // CHANGE PASSWORD MODAL
        // =====================================================

        [Fact]
        public void Clicking_Change_Password_Should_Show_Modal()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(true);

            _authStateMock.SetupGet(x => x.User).Returns(new User{Username = "john"});

            var cut = Render<ProfileCard>(p => p.Add(x => x.Username, "john"));

            // Act
            cut.Find("button").Click();

            // Assert
            cut.Markup.Should().Contain("modal-backdrop");
        }
    }
}
