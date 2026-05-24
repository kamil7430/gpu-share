using GpuShare.Frontend.Auth;
using GpuShare.Frontend.Http;
using GpuShare.Frontend.State;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services;

public class AuthService : IAuthService
{
    private readonly IApiClient _api;
    private readonly AuthState _authState;
    private readonly IJwtHelper _jwtHelper;

    public AuthService(IApiClient api, AuthState authState, IJwtHelper jwtHelper)
    {
        _api = api;
        _authState = authState;
        _jwtHelper = jwtHelper;
    }

    public async Task<AuthResponse> LoginAsync(AuthRequest payload)
    {
        var token = await _api.PostAsync<AuthRequest, string>("/users/login", payload);
        if (token != null) {
            var response = new AuthResponse { 
                User = new User { Username = payload.Username, Admin = false }, 
                Token = token,
                ExpiresAt = _jwtHelper.GetExpiration(token)
            };  
            _authState.SetAuth(response);
            return response;
        }
        throw new InvalidOperationException("Failed to login");
    }

    public async Task<AuthResponse> RefreshTokenAsync()
    {
        var token = await _api.PostAsync<string>("/users/refresh");
        if (token != null) {
            var response = new AuthResponse { 
                User = _authState.User!, 
                Token = token,
                ExpiresAt = _jwtHelper.GetExpiration(token)
            };
            _authState.SetAuth(response);
            return response;
        }
        throw new InvalidOperationException("Failed to refresh token");
    }

    public async Task RegisterAsync(RegisterRequest payload)
    {
        await _api.PostAsync("/users/register", payload);
    }

    public async Task LogoutAsync()
    {
        // await _api.PostAsync("/users/logout", new { });
        _authState.Logout();
    }

    public async Task<User> GetMeAsync()
    {
        // return await _api.GetAsync<User>("/users/me") ?? throw new Exception("Invalid response");
        return _authState.User ?? throw new InvalidOperationException("User is not authenticated");
    }
}