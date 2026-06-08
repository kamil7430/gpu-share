using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using System.Net;

namespace GpuShare.Frontend.Services;

public class MockAuthService(IAuthState authState) : IAuthService
{
    private readonly IAuthState _authState = authState;

    public Task LoginAsync(AuthRequest payload)
    {
        var user = MockStore.Users.FirstOrDefault(u => u.Username == payload.Username)
            ?? new User { Id = MockStore.NextId(), Username = payload.Username };

        _authState.SetAuth(new AuthResponse
        {
            User = new User { Username = payload.Username },
            Token = "JWT",
            ExpiresAt = DateTime.Now.AddHours(1)
        });

        MockStore.CurrentUser = user;
        return Task.CompletedTask;
    }

    public Task RegisterAsync(AuthRequest payload)
    {
        if (MockStore.Users.Any(u => u.Username == payload.Username))
            throw new ApiException("Username already taken.", HttpStatusCode.Conflict);

        var user = new User { Id = MockStore.NextId(), Username = payload.Username };
        MockStore.Users.Add(user);
        MockStore.CurrentUser = user;
        return Task.CompletedTask;
    }

    public Task RefreshTokenAsync() => Task.CompletedTask;

    public Task LogoutAsync()
    {
        _authState.Logout();
        MockStore.CurrentUser = new User { Id = 0, Username = "" };
        return Task.CompletedTask;
    }

    public Task<User> GetMeAsync() => Task.FromResult(MockStore.CurrentUser);

    public Task ChangePasswordAsync(ChangePasswordRequest payload) => Task.CompletedTask;
}
