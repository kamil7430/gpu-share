using Microsoft.AspNetCore.Components.Forms;
using GpuShare.Frontend.Models.Dtos;

namespace GpuShare.Frontend.Services.Interfaces
{
    public interface IFileService
    {
        Task<FileUploadResult> UploadAsync(IBrowserFile file, CancellationToken cancellationToken = default);
    }
}
