using GpuShare.Frontend.Auth;
using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.State;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services;

public class AuthService(IApiClient api, IAuthState authState, IJwtHelper jwtHelper, ILogger<AuthService> logger) : IAuthService
{
    private readonly IApiClient _api = api;
    private readonly IAuthState _authState = authState;
    private readonly IJwtHelper _jwtHelper = jwtHelper;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task LoginAsync(AuthRequest payload)
    {
        var token = await _api.PostAsync<AuthRequest, string>("/users/login", payload);
        if (token != null)
        {
            var response = new AuthResponse
            {
                User = new User { Username = payload.Username, Admin = false },
                Token = token,
                ExpiresAt = _jwtHelper.GetExpiration(token)
            };
            _authState.SetAuth(response);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("User {Username} logged in successfully", payload.Username);
            }
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("Login failed for user {Username}", payload.Username);
            }
            throw new InvalidOperationException("Failed to login");
        }
    }

    public async Task RefreshTokenAsync()
    {
        var token = await _api.PostAsync<string>("/users/refresh");
        if (token != null)
        {
            var response = new AuthResponse
            {
                User = _authState.User!,
                Token = token,
                ExpiresAt = _jwtHelper.GetExpiration(token)
            };
            _authState.SetAuth(response);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Token refreshed successfully for user {Username}", response.User.Username);
            }
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError("Failed to refresh token for user {Username}", _authState.User?.Username);
            }
            throw new InvalidOperationException("Failed to refresh token");
        }
    }

    public async Task RegisterAsync(AuthRequest payload)
    {
        await _api.PostAsync("/users/register", payload);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("User {Username} registered successfully", payload.Username);
        }
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest payload)
    {
        await _api.PostAsync("/users/changePassword", payload);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("User {Username} changed password successfully", payload.Username);
        }
    }

    public async Task LogoutAsync()
    {
        // await _api.PostAsync("/users/logout", new { });
        _authState.Logout();
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("User logged out successfully");
        }
    }

    public async Task<User> GetMeAsync()
    {
        // return await _api.GetAsync<User>("/users/me") ?? throw new Exception("Invalid response");
        return _authState.User ?? throw new InvalidOperationException("User is not authenticated");
    }
}