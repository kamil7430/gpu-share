using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GpuShare.Frontend.Http;
using GpuShare.Frontend.State;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services;
using GpuShare.Frontend.Services.Interfaces;
using RichardSzalay.MockHttp;
using Xunit;
using GpuShare.Frontend.Auth;

namespace GpuShare.Frontend.Tests.Services;

public class AuthServiceTests
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly HttpClient _httpClient;
    private readonly IApiClient _apiClient;
    private readonly AuthState _authState;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = _mockHttp.ToHttpClient();
        _httpClient.BaseAddress = new Uri("https://localhost:5001");
        _apiClient = new ApiClient(_httpClient);
        _authState = new AuthState(new MockJwtHelper());
        _sut = new AuthService(_apiClient, _authState, new MockJwtHelper());
    }

    [Fact]
    public async Task LoginAsync_Should_Return_AuthResponse_When_Credentials_Are_Valid()
    {
        // Arrange
        var expected = new AuthResponse
        {
            Token = "jwt-token",
            User = new User
            {
                Username = "john"
            }
        };
        var payload = new AuthRequest
        {
            Username = "john",
            Password = "password123"
        };

        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/login")
            .Respond("application/json", JsonSerializer.Serialize(expected.Token));

        // Act
        await _sut.LoginAsync(payload);

        // Assert
        _authState.User.Should().NotBeNull();
        _authState.AccessToken.Should().Be("jwt-token");
        _authState.User.Username.Should().Be("john");
    }

    [Fact]
    public async Task LoginAsync_Should_Throw_When_Response_Is_Unauthorized()
    {
        // Arrange
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/login")
            .Respond(HttpStatusCode.Unauthorized);

        // Act
        var payload = new AuthRequest
        {
            Username = "john",
            Password = "wrong-password"
        };
        var action = async () => await _sut.LoginAsync(payload);

        // Assert
        var exception = await action.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterAsync_Should_Send_Register_Request()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "newuser",
            Password = "password123"
        };

        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/register")
            .Respond(HttpStatusCode.Created);

        // Act
        var action = async () => await _sut.RegisterAsync(request);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RegisterAsync_Should_Throw_When_Email_Is_Already_Taken()
    {
        // Arrange
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/register")
            .Respond(HttpStatusCode.Conflict);

        var payload = new RegisterRequest
        {
            Username = "john",
            Password = "password123"
        };

        // Act
        var action = async () => await _sut.RegisterAsync(payload);

        // Assert
        var exception = await action.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_New_Tokens()
    {
        // Arrange
        var expected = new AuthResponse
        {
            Token = "new-jwt",
            User = new User
            {
                Username = "john"
            }
        };
        _authState.SetAuth(expected);

        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/refresh")
            .Respond("application/json", JsonSerializer.Serialize(expected.Token));

        // Act
        await _sut.RefreshTokenAsync();

        // Assert
        _authState.AccessToken.Should().Be("new-jwt");
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Throw_When_Refresh_Fails()
    {
        // Arrange
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/refresh")
            .Respond(HttpStatusCode.Unauthorized);

        // Act
        var action = async () => await _sut.RefreshTokenAsync();

        // Assert
        var exception = await action.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMeAsync_Should_Return_Current_User()
    {
        // Arrange
        var expected = new User
        {
            Username = "john"
        };
        _authState.SetAuth(new AuthResponse { User = expected, Token = "jwt-token", ExpiresAt = DateTime.UtcNow.AddHours(1) });

        // Act
        var result = await _sut.GetMeAsync();

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("john");
    }

    [Fact]
    public async Task GetMeAsync_Should_Throw_When_User_Is_Logged_Out()
    {
        // Arrange
        _authState.Logout();

        // Act
        var action = async () => await _sut.GetMeAsync();

        // Assert
        var exception = await action.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Be("User is not authenticated");
    }

    [Fact]
    public async Task LogoutAsync_Should_Complete_Without_Exception()
    {
        // Arrange

        // Act
        var action = async () => await _sut.LogoutAsync();

        // Assert
        await action.Should().NotThrowAsync();
        _authState.User.Should().BeNull();
        _authState.AccessToken.Should().BeNull();
        _authState.AccessTokenExpiresAt.Should().BeNull();
    }

    private class MockJwtHelper : IJwtHelper
    {
        public DateTime GetExpiration(string token)
        {
            return DateTime.UtcNow.AddHours(1);
        }
    }
}