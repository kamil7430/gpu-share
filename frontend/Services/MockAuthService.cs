namespace GpuShare.Frontend.Services;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

public class MockAuthService : IAuthService
{
    public Task<AuthResponse> LoginAsync(string username, string password)
    {
        // Simulate successful login with dummy tokens
        var response = new AuthResponse
        {
            AccessToken = "mock_access_token",
            RefreshToken = "mock_refresh_token",
            ExpiresAt = DateTime.UtcNow.AddSeconds(3600)
        };
        return Task.FromResult(response);
    }

    public Task RegisterAsync(RegisterRequest payload)
    {
        // Simulate successful registration
        return Task.CompletedTask;
    }

    public Task<AuthResponse> RefreshTokenAsync()
    {
        // Simulate token refresh with new dummy tokens
        var response = new AuthResponse
        {
            AccessToken = "new_mock_access_token",
            RefreshToken = "new_mock_refresh_token",
            ExpiresAt = DateTime.UtcNow.AddSeconds(3600)
        };
        return Task.FromResult(response);
    }

    public Task LogoutAsync()
    {
        // Simulate logout by doing nothing
        return Task.CompletedTask;
    }

    public Task<User> GetMeAsync()
    {
        // Simulate fetching user profile with dummy data
        var user = new User
        {
            Id = int.Parse(Guid.NewGuid().ToString().Replace("-", "")),
            Username = "mockuser",
        };
        return Task.FromResult(user);
    }
}
