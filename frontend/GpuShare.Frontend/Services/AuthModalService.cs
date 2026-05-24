using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services;

public class AuthModalService : IAuthModalService
{
    public bool IsOpen { get; private set; }

    public event Action? OnChange;

    public void Open()
    {
        IsOpen = true;
        NotifyStateChanged();
    }

    public void Close()
    {
        IsOpen = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}