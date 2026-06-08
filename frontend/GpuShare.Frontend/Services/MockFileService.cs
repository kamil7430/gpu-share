using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;

namespace GpuShare.Frontend.Services;

public class MockFileService : IFileService
{
    public Task<FileUploadResult> UploadAsync(IBrowserFile file, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new FileUploadResult
        {
            FileName = file.Name,
            Size = file.Size,
            ContentType = file.ContentType,
            Url = $"https://cdn.mock.gpushare.io/uploads/{Guid.NewGuid():N}/{file.Name}",
            StorageKey = $"mock/{Guid.NewGuid():N}/{file.Name}",
        });
    }
}
