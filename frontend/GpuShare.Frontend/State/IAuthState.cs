using GpuShare.Frontend.Auth;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

namespace GpuShare.Frontend.State;

public interface IAuthState
{
    public User? User { get; }
    public string? AccessToken { get; }
    public DateTime? AccessTokenExpiresAt { get; set; }
    public bool Admin { get; }
    public bool IsAuthenticated { get; }
    public event Action? OnChange;
    
    void SetAuth(User user, string token);

    public void SetAuth(AuthResponse authResponse);

    public void Logout();
}