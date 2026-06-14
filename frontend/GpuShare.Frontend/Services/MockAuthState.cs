using GpuShare.Frontend.Auth;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.State;

namespace GpuShare.Frontend.Services;

/// <summary>
/// AuthState that survives full page reloads. The real app rehydrates auth from a
/// persisted JWT; the mocks simulate that by restoring the session recorded in
/// MockStore. Every reload starts a new Blazor circuit with fresh scoped services,
/// so without this each GotoAsync in a test would silently log the user out.
/// </summary>
public class MockAuthState : AuthState
{
    public MockAuthState(IJwtHelper jwtHelper) : base(jwtHelper)
    {
        if (MockStore.AuthenticatedUser != null)
        {
            SetAuth(new AuthResponse
            {
                User = MockStore.AuthenticatedUser,
                Token = "JWT",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
            });
        }
    }
}
