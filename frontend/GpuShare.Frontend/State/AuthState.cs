namespace GpuShare.Frontend.State;
using GpuShare.Frontend.Models;

public class AuthState
{
    public User? User { get; private set; }

    public string? AccessToken { get; private set; }

    public bool Admin => User?.Admin ?? false;

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);

    public event Action? OnChange;

    public void SetAuth(User user, string token)
    {
        User = user;
        AccessToken = token;

        NotifyStateChanged();
    }

    public void Logout()
    {
        User = null;
        AccessToken = null;

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}