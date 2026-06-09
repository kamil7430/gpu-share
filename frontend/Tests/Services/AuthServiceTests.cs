using System.Net;
using System.Text.Json;
using FluentAssertions;
using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services;
using RichardSzalay.MockHttp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GpuShare.Frontend.Tests.Services;

public class AuthServiceTests
{
    private readonly MockHttpMessageHandler _mockHttp = new();
    private readonly HttpClient _httpClient;
    private readonly IApiClient _apiClient;
    private static readonly TestAuthState _authState = new();
    private readonly ILogger<AuthService> _logger;
    private readonly AuthService _sut;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ChangePasswordRequest _changePasswordRequest = new()
    {
        Username = "testuser",
        OldPassword = "Old123!",
        NewPassword = "New123!"
    };

    private readonly AuthRequest _loginRequest = new()
    {
        Username = "john",
        Password = "password123"
    };

    private readonly AuthRequest _registerRequest = new()
    {
        Username = "newuser",
        Password = "password123"
    };

    private readonly AuthResponse _expectedLogin = new()
    {
        Token = "jwt-token",
        User = new User
        {
            Username = "john"
        }
    };

    public AuthServiceTests()
    {
        _httpClient = _mockHttp.ToHttpClient();
        _httpClient.BaseAddress = new Uri("https://localhost:5001");
        _apiClient = new ApiClient(_httpClient, NullLogger<ApiClient>.Instance);
        _logger = NullLogger<AuthService>.Instance;
        _sut = new AuthService(_apiClient, _authState, new MockJwtHelper(), _logger);
    }

    // =====================================================
    // LOGIN
    // =====================================================

    [Fact]
    public async Task LoginAsync_Should_Send_Post_To_Correct_Endpoint()
    {
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/login")
            .Respond("application/json", JsonSerializer.Serialize(_expectedLogin.Token));

        var act = async () => await _sut.LoginAsync(_loginRequest);

        _mockHttp.VerifyNoOutstandingExpectation();
        _mockHttp.VerifyNoOutstandingRequest();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task LoginAsync_Should_Return_AuthResponse_When_Credentials_Are_Valid()
    {
        // Arrange
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/login")
            .Respond("application/json", JsonSerializer.Serialize(_expectedLogin.Token));

        // Act
        await _sut.LoginAsync(_loginRequest);

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

        var payload = new AuthRequest
        {
            Username = "john",
            Password = "wrong-password"
        };

        // Act
        var action = async () => await _sut.LoginAsync(payload);

        // Assert
        var exception = await action.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // =====================================================
    // REGISTER
    // =====================================================

    [Fact]
    public async Task RegisterAsync_Should_Send_Post_To_Correct_Endpoint()
    {
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/register")
            .Respond(HttpStatusCode.Created);

        var act = async () => await _sut.RegisterAsync(_registerRequest);

        _mockHttp.VerifyNoOutstandingExpectation();
        _mockHttp.VerifyNoOutstandingRequest();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RegisterAsync_Should_Throw_When_Username_Is_Already_Taken()
    {
        // Arrange
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/register")
            .Respond(HttpStatusCode.Conflict);

        // Act
        var action = async () => await _sut.RegisterAsync(_registerRequest);

        // Assert
        var exception = await action.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // =====================================================
    // REFRESH TOKEN
    // =====================================================

    [Fact]
    public async Task RefreshTokenAsync_Should_Send_Post_To_Correct_Endpoint()
    {
        // Arrange
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/refresh")
            .Respond("application/json", JsonSerializer.Serialize(_expectedLogin.Token));
        _authState.SetAuth(_expectedLogin);

        // Act
        var act = async () => await _sut.RefreshTokenAsync();

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
        _mockHttp.VerifyNoOutstandingRequest();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_Return_New_Tokens()
    {
        // Arrange
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/refresh")
            .Respond("application/json", JsonSerializer.Serialize("new-jwt"));
        _authState.SetAuth(_expectedLogin);

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

    // =====================================================
    // GET ME
    // =====================================================

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

    // =====================================================
    // LOGOUT
    // =====================================================

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

    // =====================================================
    // CHANGE PASSWORD
    // =====================================================

    [Fact]
    public async Task ChangePasswordAsync_Should_Send_Post_To_Correct_Endpoint()
    {
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/changePassword")
            .Respond(HttpStatusCode.OK);

        var act = async () => await _sut.ChangePasswordAsync(_changePasswordRequest);

        _mockHttp.VerifyNoOutstandingExpectation();
        _mockHttp.VerifyNoOutstandingRequest();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ChangePasswordAsync_Should_Send_Correct_Payload()
    {
        ChangePasswordRequest? receivedPayload = null;

        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/changePassword")
            .Respond(async req =>
            {
                var json = await req.Content!.ReadAsStringAsync();

                receivedPayload = JsonSerializer.Deserialize<ChangePasswordRequest>(
                    json, _jsonOptions);

                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        await _sut.ChangePasswordAsync(_changePasswordRequest);

        receivedPayload.Should().NotBeNull();
        receivedPayload!.Username.Should().Be("testuser");
        receivedPayload.OldPassword.Should().Be("Old123!");
        receivedPayload.NewPassword.Should().Be("New123!");
    }

    [Fact]
    public async Task ChangePasswordAsync_Should_Throw_When_Old_Password_Is_Invalid()
    {
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/changePassword")
            .Respond(HttpStatusCode.Unauthorized);

        var request = new ChangePasswordRequest
        {
            Username = "testuser",
            OldPassword = "wrong",
            NewPassword = "New123!"
        };

        var act = () => _sut.ChangePasswordAsync(request);

        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePasswordAsync_Should_Throw_When_New_Password_Is_Invalid()
    {
        _mockHttp.When(HttpMethod.Post, "https://localhost:5001/users/changePassword")
            .Respond(HttpStatusCode.BadRequest);

        var request = new ChangePasswordRequest
        {
            Username = "testuser",
            OldPassword = "Old123!",
            NewPassword = "123"
        };

        var act = () => _sut.ChangePasswordAsync(request);

        await act.Should().ThrowAsync<ApiException>().Where(e => e.StatusCode == HttpStatusCode.BadRequest);
    }
}