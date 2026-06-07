using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;

namespace GpuShare.Frontend.Services
{
    public class FileService(IApiClient api, ILogger<FileService> logger) : IFileService
    {
        private readonly IApiClient _api = api;
        private readonly ILogger<FileService> _logger = logger;

        public async Task<FileUploadResult> UploadAsync(IBrowserFile file, CancellationToken cancellationToken = default)
        {
            using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024, cancellationToken);

            using var form = new MultipartFormDataContent();

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            form.Add(fileContent, "file", file.Name);

            var result = await _api.PostAsync<MultipartFormDataContent, FileUploadResult>("/files/upload", form);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("File uploaded to url {url} with name {name} and size {size}", 
                    result!.Url, result.FileName, result.Size);

            return result ?? throw new InvalidOperationException("Empty upload response");
        }
    }
}
