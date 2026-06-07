using Bunit;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using GpuShare.Frontend.Components.Modals;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using FluentAssertions;
using System.Net;

namespace GpuShare.Frontend.Tests.Modals
{
    public class ChangePasswordModalTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IAuthService> _authServiceMock = new();
        private readonly Mock<IAuthState> _authStateMock = new();

        public ChangePasswordModalTests()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton(_authServiceMock.Object);
            Services.AddSingleton(_authStateMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true);
            JSInterop.SetupModule(_ => true);

            _authStateMock.Setup(x => x.IsAuthenticated).Returns(true);
            _authStateMock.Setup(x => x.User).Returns(new User() { Username = "john" });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public void Should_Render_Form_Fields()
        {
            var cut = Render<ChangePasswordModal>(p => p.Add(x => x.IsVisible, true));

            cut.Find("input[name=oldPassword]");
            cut.Find("input[name=newPassword]");
            cut.Find("input[name=confirmPassword]");
            cut.Find("button[type=submit]");
        }

        [Fact]
        public void Should_Show_Validation_Errors_When_Fields_Are_Empty()
        {
            var cut = Render<ChangePasswordModal>(p => p.Add(x => x.IsVisible, true));

            cut.Find("form").Submit();

            cut.Markup.Should().Contain("The OldPassword field is required");
            cut.Markup.Should().Contain("The NewPassword field is required");
            cut.Markup.Should().Contain("The ConfirmPassword field is required");
        }

        [Fact]
        public void Should_Show_Error_When_Passwords_Do_Not_Match()
        {
            var cut = Render<ChangePasswordModal>(p =>
                p.Add(x => x.IsVisible, true));

            cut.Find("input[name=oldPassword]").Change("OldPassword123");
            cut.Find("input[name=newPassword]").Change("NewPassword123");
            cut.Find("input[name=confirmPassword]").Change("DifferentPassword");

            cut.Find("form").Submit();

            cut.Markup.Should().Contain("Passwords do not match");
        }

        [Fact]
        public void Should_Call_ChangePassword_Service()
        {
            _authServiceMock.Setup(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>()))
                .Returns(Task.CompletedTask);

            var cut = Render<ChangePasswordModal>(p => p.Add(x => x.IsVisible, true));

            cut.Find("input[name=oldPassword]").Change("OldPassword123");
            cut.Find("input[name=newPassword]").Change("NewPassword123");
            cut.Find("input[name=confirmPassword]").Change("NewPassword123");

            cut.Find("form").Submit();

            _authServiceMock.Verify(x => x.ChangePasswordAsync(It.Is<ChangePasswordRequest>(r =>
                        r.Username == "john" && r.OldPassword == "OldPassword123" &&
                        r.NewPassword == "NewPassword123")), Times.Once);
        }

        [Fact]
        public void Should_Close_With_Ok_When_Change_Succeeds()
        {
            ModalResult<string>? result = null;

            _authServiceMock.Setup(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>()))
                .Returns(Task.CompletedTask);

            var cut = Render<ChangePasswordModal>(p => p.Add(x => x.IsVisible, true)
                .Add(x => x.OnClose, r => result = r));

            cut.Find("input[name=oldPassword]").Change("OldPassword123");
            cut.Find("input[name=newPassword]").Change("NewPassword123");
            cut.Find("input[name=confirmPassword]").Change("NewPassword123");

            cut.Find("form").Submit();

            result.Should().NotBeNull();
            result!.Status.Should().Be(ModalResultStatus.Success);
        }

        [Fact]
        public void Should_Show_Error_When_Old_Password_Is_Incorrect()
        {
            _authServiceMock.Setup(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>()))
                .ThrowsAsync(new ApiException( "Unauthorized", HttpStatusCode.Unauthorized));

            var cut = Render<ChangePasswordModal>(p => p.Add(x => x.IsVisible, true));

            cut.Find("input[name=oldPassword]").Change("WrongPassword");
            cut.Find("input[name=newPassword]").Change("NewPassword123");
            cut.Find("input[name=confirmPassword]").Change("NewPassword123");

            cut.Find("form").Submit();

            cut.WaitForAssertion(() =>
            {
                cut.Markup.Should().Contain("Incorrect old password");
            });
        }

        [Fact]
        public void Should_Not_Close_When_Old_Password_Is_Wrong()
        {
            var callbackCalled = false;

            _authServiceMock.Setup(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>()))
                .ThrowsAsync(new ApiException("Unauthorized", HttpStatusCode.Unauthorized));

            var cut = Render<ChangePasswordModal>(p => p.Add(x => x.IsVisible, true)
                .Add(x => x.OnClose, _ => callbackCalled = true));

            cut.Find("input[name=oldPassword]").Change("WrongPassword");
            cut.Find("input[name=newPassword]").Change("NewPassword123");
            cut.Find("input[name=confirmPassword]").Change("NewPassword123");

            cut.Find("form").Submit();

            callbackCalled.Should().BeFalse();
        }

        [Fact]
        public void Should_Close_With_Fail_On_Server_Error()
        {
            ModalResult<string>? result = null;

            _authServiceMock.Setup(x => x.ChangePasswordAsync(It.IsAny<ChangePasswordRequest>()))
                .ThrowsAsync(new ApiException("Server error", HttpStatusCode.InternalServerError));

            var cut = Render<ChangePasswordModal>(p => p.Add(x => x.IsVisible, true)
                .Add(x => x.OnClose, r => result = r));

            cut.Find("input[name=oldPassword]").Change("OldPassword123");
            cut.Find("input[name=newPassword]").Change("NewPassword123");
            cut.Find("input[name=confirmPassword]").Change("NewPassword123");

            cut.Find("form").Submit();

            result.Should().NotBeNull();
            result!.Status.Should().Be(ModalResultStatus.Failed);
        }

        [Fact]
        public void Should_Return_Cancel_When_Closed()
        {
            ModalResult<string>? result = null;

            var cut = Render<ChangePasswordModal>(p => p.Add(x => x.IsVisible, true)
                .Add(x => x.OnClose, r => result = r));

            cut.Find("button.btn-close").Click();

            result.Should().NotBeNull();
            result!.Status.Should().Be(ModalResultStatus.Cancelled);
        }
    }
}
