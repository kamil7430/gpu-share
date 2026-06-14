using Castle.Core.Logging;
using GpuShare.Frontend.Auth;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

namespace GpuShare.Frontend.State;

public class AuthState(IJwtHelper jwtHelper, ILogger<AuthState> logger) : IAuthState
{
    public User? User { get; private set; }
    public string? AccessToken { get; private set; }
    public DateTime? AccessTokenExpiresAt { get; set; }
    public bool Admin => User?.Admin ?? false;
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken) && AccessTokenExpiresAt > DateTime.UtcNow;
    public event Action? OnChange;
    
    private readonly IJwtHelper _jwtHelper = jwtHelper;
    private readonly ILogger<AuthState> _logger = logger;

    public void SetAuth(User user, string token)
    {
        User = user;
        AccessToken = token;
        AccessTokenExpiresAt = _jwtHelper.GetExpiration(token);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("AuthState set to username: {Username}, token: {token}, expiration: {exp}",
                User.Username, AccessToken, AccessTokenExpiresAt.ToString());
        }
        NotifyStateChanged();
    }

    public void SetAuth(AuthResponse authResponse)
    {
        User = authResponse.User;
        AccessToken = authResponse.Token;
        AccessTokenExpiresAt = authResponse.ExpiresAt;
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("AuthState set to username: {Username}, token: {token}, expiration: {exp}", 
                User.Username, AccessToken, AccessTokenExpiresAt.ToString());
        }
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