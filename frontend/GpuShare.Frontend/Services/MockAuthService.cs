namespace GpuShare.Frontend.Services;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

public class MockAuthService : IAuthService
{
    public Task LoginAsync(AuthRequest payload)
    {
        // Simulate successful login with dummy tokens
        var response = new AuthResponse
        {
            Token = "mock_access_token",
            ExpiresAt = DateTime.UtcNow.AddSeconds(3600),
            User = new User
            {
                Id = int.Parse(Guid.NewGuid().ToString().Replace("-", "")),
                Username = payload.Username,
            }
        };
        return Task.FromResult(response);
    }

    public Task RegisterAsync(RegisterRequest payload)
    {
        // Simulate successful registration
        return Task.CompletedTask;
    }

    public Task RefreshTokenAsync()
    {
        // Simulate token refresh with new dummy tokens
        var response = new AuthResponse
        {
            Token = "new_mock_access_token",
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
