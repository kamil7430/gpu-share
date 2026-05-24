using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Modals;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace GpuShare.Frontend.Tests.Components
{
    public class LoginModalTests : BunitContext
    {
        private Mock<IAuthModalService> _authModalServiceMock;
        private Mock<IAuthState> _authStateMock;
        private Mock<IAuthService> _authServiceMock;

        public LoginModalTests()
        {
            _authModalServiceMock = new Mock<IAuthModalService>();
            _authStateMock = new Mock<IAuthState>();
            _authServiceMock = new Mock<IAuthService>();
            Services.AddSingleton(_authModalServiceMock.Object);
            Services.AddSingleton(_authStateMock.Object);
            Services.AddSingleton(_authServiceMock.Object);
        }

        [Fact]
        public void Submit_Login_Form_Should_Call_AuthService()
        {
            // Arrange
            
            var cut = Render<LoginModal>(parameters => parameters.Add(x => x.IsVisible, true));

            // Act
            cut.Find("input[name='username']").Change("john");
            cut.Find("input[name='password']").Change("password123");
            cut.Find("button[type='submit']").Click();

            // Assert

            _authServiceMock.Verify(x => x.LoginAsync(It.Is<AuthRequest>(r => r.Username == "john" && r.Password == "password123")), Times.Once);
        }

        [Fact]
        public void Empty_Form_Should_Show_Validation_Errors()
        {
            // Arrange
            var cut = Render<LoginModal>(parameters => parameters.Add(x => x.IsVisible, true));

            // Act
            cut.Find("button[type='submit']").Click();

            // Assert
            cut.Markup.Should().Contain("required");

            _authServiceMock.Verify(x => x.LoginAsync(It.IsAny<AuthRequest>()), Times.Never);
        }

        [Fact]
        public void Invalid_Login_Should_Show_Error_Message()
        {
            // Arrange
            _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<AuthRequest>())).ThrowsAsync(
                    new ApiException("Invalid credentials", HttpStatusCode.Unauthorized));

            var cut = Render<LoginModal>(parameters => parameters.Add(x => x.IsVisible, true));

            // Act
            cut.Find("input[name='username']").Change("john");
            cut.Find("input[name='password']").Change("wrong");
            cut.Find("button[type='submit']").Click();

            // Assert
            cut.Markup.Should().Contain("Login failed");
        }

        [Fact]
        public void Successful_Login_Should_Close_Modal()
        {
            // Arrange
            _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<AuthRequest>())).Returns(Task.CompletedTask);

            var closed = false;

            var cut = Render<LoginModal>(parameters => parameters.Add(x => x.IsVisible, true)
                    .Add(x => x.OnClose, () => closed = true));

            // Act
            cut.Find("input[name='username']").Change("john");
            cut.Find("input[name='password']").Change("password");
            cut.Find("button[type='submit']").Click();

            // Assert
            closed.Should().BeTrue();
        }

        [Fact]
        public void Register_Form_Should_Call_Register_Service()
        {
            // Arrange
            _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>())).Returns(Task.CompletedTask);

            var cut = Render<LoginModal>(parameters => parameters.Add(x => x.IsVisible, true));

            // Switch to register mode
            cut.FindAll("a").First(x => x.TextContent.Contains("Register")).Click();

            // Fill form
            cut.Find("input[name='username']").Change("john");
            cut.Find("input[name='password']").Change("Password123");
            cut.Find("input[name='confirmPassword']").Change("Password123");
            cut.Find("input[type='checkbox']").Change(true);

            // Submit
            cut.Find("button[type='submit']").Click();

            // Assert
            _authServiceMock.Verify(x => x.RegisterAsync(It.Is<RegisterRequest>(r =>
                        r.Username == "john" && r.Password == "Password123")), Times.Once);
        }

        [Fact]
        public void Register_Form_Should_Show_Validation_Error_When_Passwords_Do_Not_Match()
        {
            // Arrange
            var cut = Render<LoginModal>(parameters => parameters.Add(x => x.IsVisible, true));

            // Switch to register mode
            cut.FindAll("a").First(x => x.TextContent.Contains("Register")).Click();

            // Fill invalid form
            cut.Find("input[name='username']").Change("john");
            cut.Find("input[name='password']").Change("password123");
            cut.Find("input[name='confirmPassword']").Change("different-password");
            cut.Find("input[type='checkbox']").Change(true);

            // Submit
            cut.Find("button[type='submit']").Click();

            // Assert
            cut.Markup.Should().Contain("Passwords do not match");

            _authServiceMock.Verify(x => x.RegisterAsync(It.IsAny<RegisterRequest>()), Times.Never);
        }

        [Fact]
        public void Register_Form_Should_Show_Error_When_Terms_Not_Accepted()
        {
            // Arrange
            var cut = Render<LoginModal>(parameters => parameters.Add(x => x.IsVisible, true));

            // Switch to register mode
            cut.FindAll("a").First(x => x.TextContent.Contains("Register")).Click();

            // Fill form without accepting terms
            cut.Find("input[name='username']").Change("john");
            cut.Find("input[name='password']").Change("password123");
            cut.Find("input[name='confirmPassword']").Change("password123");

            // Submit
            cut.Find("button[type='submit']").Click();

            // Assert
            cut.Markup.Should().Contain("You must accept the Terms and Conditions");

            _authServiceMock.Verify(x => x.RegisterAsync(It.IsAny<RegisterRequest>()), Times.Never);
        }

        [Fact]
        public void Register_Should_Show_Error_Banner_When_Service_Throws()
        {
            // Arrange
            _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>()))
                .ThrowsAsync(new Exception("Registration failed"));

            var cut = Render<LoginModal>(parameters => parameters.Add(x => x.IsVisible, true));

            // Switch to register mode
            cut.FindAll("a").First(x => x.TextContent.Contains("Register")).Click();

            // Fill form
            cut.Find("input[name='username']").Change("john");
            cut.Find("input[name='password']").Change("Password123");
            cut.Find("input[name='confirmPassword']").Change("Password123");
            cut.Find("input[type='checkbox']").Change(true);

            // Submit
            cut.Find("button[type='submit']").Click();

            // Assert
            cut.Markup.Should().Contain("Registration failed");
        }

        [Fact]
        public void Successful_Register_Should_Close_Modal()
        {
            // Arrange
            _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>())).Returns(Task.CompletedTask);

            var closed = false;

            var cut = Render<LoginModal>(parameters => parameters.Add(x => x.IsVisible, true)
                    .Add(x => x.OnClose, () => closed = true));

            // Switch to register mode
            cut.FindAll("a").First(x => x.TextContent.Contains("Register")).Click();

            // Fill form
            cut.Find("input[name='username']").Change("john");
            cut.Find("input[name='password']").Change("Password123");
            cut.Find("input[name='confirmPassword']").Change("Password123");
            cut.Find("input[type='checkbox']").Change(true);

            // Submit
            cut.Find("button[type='submit']").Click();

            // Assert
            closed.Should().BeTrue();
        }
    }
}
