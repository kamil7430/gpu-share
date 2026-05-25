using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Shared;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Services;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.State;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GpuShare.Frontend.Tests.Components.Shared;

public class NavBarTests : BunitContext
{
    private Mock<IAuthModalService> _authModalServiceMock;
    private Mock<IAuthState> _authStateMock;
    private Mock<IAuthService> _authServiceMock;

    public NavBarTests()
    {
        _authModalServiceMock = new Mock<IAuthModalService>();
        _authStateMock = new Mock<IAuthState>();
        _authServiceMock = new Mock<IAuthService>();
        Services.AddSingleton(_authModalServiceMock.Object);
        Services.AddSingleton(_authStateMock.Object);
        Services.AddSingleton(_authServiceMock.Object);
    }

    [Fact]
    public void Should_Show_Login_When_Not_Authenticated()
    {
        // Arrange
        _authStateMock.Setup(x => x.IsAuthenticated).Returns(false);
        var cut = Render<TopNav>(); // your component

        // Assert
        cut.FindAll("button").Should().Contain(b => b.TextContent.Contains("Login"));
    }

    [Fact]
    public void Should_Show_User_And_Logout_When_Authenticated()
    {
        // Arrange
        _authStateMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authStateMock.Setup(x => x.User).Returns(new User { Username = "john" });
        var cut = Render<TopNav>();

        // Assert
        cut.Markup.Should().Contain("john");
        cut.Markup.Should().Contain("Logout");
        cut.Markup.Should().NotContain("Login");
    }

    [Fact]
    public void Clicking_Login_Should_Open_LoginModal()
    {
        // Arrange
        _authStateMock.Setup(x => x.IsAuthenticated).Returns(false);
        //_authModalServiceMock.Setup(x => x.IsOpen).Returns(false);

        var cut = Render<TopNav>();

        // Act
        cut.Find("button.login-btn").Click();

        // Assert
        _authModalServiceMock.Verify(x => x.Open(), Times.Once);
    }

    [Fact]
    public void Clicking_Logout_Should_Call_Logout()
    {
        // Arrange
        _authStateMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authStateMock.Setup(x => x.User).Returns(new User { Username = "john" });

        var cut = Render<TopNav>();

        // Act
        cut.Find("button.login-btn").Click();

        // Assert
        _authServiceMock.Verify(x => x.LogoutAsync(), Times.Once);
    }
}