using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GpuShare.Frontend.Http;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services;
using GpuShare.Frontend.Services.Interfaces;
using RichardSzalay.MockHttp;
using Xunit;

namespace GpuShare.Frontend.Tests.Services;

public class AuthServiceTests
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly HttpClient _httpClient;
    private readonly IApiClient _apiClient;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = _mockHttp.ToHttpClient();
        _httpClient.BaseAddress = new Uri("https://localhost:5001");
        _apiClient = new ApiClient(_httpClient);
        _sut = new AuthService(_apiClient);
    }

    [Fact]
    public async Task LoginAsync_Should_Return_AuthResponse_When_Credentials_Are_Valid()
    {
        // Arrange
        var expected = new AuthResponse
        {
            AccessToken = "jwt-token",
            RefreshToken = "refresh-token",
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

        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/auth/login")
            .Respond("application/json", JsonSerializer.Serialize(expected));

        // Act
        var result = await _sut.LoginAsync(payload);

        // Assert
        result.Should().NotBeNull();

        result.AccessToken.Should().Be("jwt-token");

        result.RefreshToken.Should().Be("refresh-token");

        result.User.Username.Should().Be("john");
    }

    [Fact]
    public async Task LoginAsync_Should_Throw_When_Response_Is_Unauthorized()
    {
        // Arrange
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/auth/login")
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
        exception.Which.StatusCode.Should().Be(401);
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

        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/auth/register")
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
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/auth/register")
            .Respond(HttpStatusCode.Conflict);

        var payload = new RegisterRequest
        {
            Email = "taken@example.com",
            Username = "john",
            Password = "password123"
        };

        // Act
        var action = async () => await _sut.RegisterAsync(payload);

        // Assert
        var exception = await action.Should().ThrowAsync<ApiException>();

        exception.Which.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_New_Tokens()
    {
        // Arrange
        var expected = new AuthResponse
        {
            AccessToken = "new-jwt",
            RefreshToken = "new-refresh"
        };

        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/auth/refresh")
            .Respond("application/json", JsonSerializer.Serialize(expected));

        // Act
        var result = await _sut.RefreshTokenAsync();

        // Assert
        result.AccessToken.Should().Be("new-jwt");
        result.RefreshToken.Should().Be("new-refresh");
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Throw_When_Refresh_Fails()
    {
        // Arrange
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/auth/refresh")
            .Respond(HttpStatusCode.Unauthorized);

        // Act
        var action = async () => await _sut.RefreshTokenAsync();

        // Assert
        var exception = await action.Should().ThrowAsync<ApiException>();

        exception.Which.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task GetMeAsync_Should_Return_Current_User()
    {
        // Arrange
        var expected = new User
        {
            Username = "john"
        };

        _mockHttp.When(HttpMethod.Get, "https://localhost:5001/auth/me")
            .Respond("application/json", JsonSerializer.Serialize(expected));

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
        _mockHttp
            .When(HttpMethod.Get,
                "https://localhost:5001/auth/me")
            .Respond(HttpStatusCode.Unauthorized);

        // Act
        var action = async () =>
            await _sut.GetMeAsync();

        // Assert
        var exception = await action
            .Should()
            .ThrowAsync<ApiException>();

        exception.Which.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task LogoutAsync_Should_Complete_Without_Exception()
    {
        // Arrange
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/auth/logout")
            .Respond(HttpStatusCode.OK);

        // Act
        var action = async () => await _sut.LogoutAsync();

        // Assert
        await action.Should().NotThrowAsync();
    }
}