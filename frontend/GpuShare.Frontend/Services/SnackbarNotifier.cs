using GpuShare.Frontend.Services.Interfaces;
using MudBlazor;

namespace GpuShare.Frontend.Services
{
    public class SnackbarNotifier(ISnackbar snackbar) : IAppNotifier
    {
        private readonly ISnackbar _snackbar = snackbar;

        public void ShowError(string message)
        {
            _snackbar.Add(message, Severity.Error);
        }

        public void ShowInfo(string message)
        {
            _snackbar.Add(message, Severity.Info);
        }
    }
}
