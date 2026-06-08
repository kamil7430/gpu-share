namespace GpuShare.Frontend.Services.Interfaces
{
    public interface IAppNotifier
    {
        void ShowError(string message);
        void ShowInfo(string message);
    }
}
