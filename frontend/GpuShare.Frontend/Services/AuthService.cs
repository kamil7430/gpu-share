using GpuShare.Frontend.Http;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services;

public class AuthService : IAuthService
{
    private readonly IApiClient _api;

    public AuthService(IApiClient api)
    {
        _api = api;
    }

    public async Task<AuthResponse?> LoginAsync(AuthRequest payload)
    {
        return await _api.PostAsync<AuthRequest, AuthResponse>("/auth/login", payload);
    }

    public async Task<AuthResponse?> RefreshTokenAsync()
    {
        return await _api.PostAsync<AuthResponse>("/auth/refresh");
    }

    public async Task RegisterAsync(RegisterRequest payload)
    {
        await _api.PostAsync("/auth/register", payload);
    }

    public async Task LogoutAsync()
    {
        await _api.PostAsync("/auth/logout", new { });
    }

    public async Task<User?> GetMeAsync()
    {
        return await _api.GetAsync<User>("/auth/me") ?? throw new Exception("Invalid response");
    }
}