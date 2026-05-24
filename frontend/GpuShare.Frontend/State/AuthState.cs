using GpuShare.Frontend.Auth;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

namespace GpuShare.Frontend.State;

public class AuthState
{
    public User? User { get; private set; }
    public string? AccessToken { get; private set; }
    public DateTime? AccessTokenExpiresAt { get; set; }
    public bool Admin => User?.Admin ?? false;
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken) && AccessTokenExpiresAt > DateTime.UtcNow;
    public event Action? OnChange;

    public void SetAuth(User user, string token)
    {
        User = user;
        AccessToken = token;
        AccessTokenExpiresAt = JwtHelper.GetExpiration(token);
        NotifyStateChanged();
    }

    public void SetAuth(AuthResponse authResponse)
    {
        User = authResponse.User;
        AccessToken = authResponse.Token;
        AccessTokenExpiresAt = authResponse.ExpiresAt;
        NotifyStateChanged();
    }

    public void Logout()
    {
        User = null;
        AccessToken = null;
        AccessTokenExpiresAt = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}