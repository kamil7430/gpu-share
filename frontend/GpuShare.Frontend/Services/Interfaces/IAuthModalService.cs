namespace GpuShare.Frontend.Services.Interfaces
{
    public interface IAuthModalService
    {
        public bool IsOpen { get; }

        public event Action? OnChange;

        public void Open();

        public void Close();
    }
}
