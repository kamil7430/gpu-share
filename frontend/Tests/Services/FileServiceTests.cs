using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace GpuShare.Frontend.Tests.Services
{
    public class FileServiceTests
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly HttpClient _http;
        private readonly ApiClient _apiClient;
        private readonly ILogger<FileService> _logger;
        private readonly FileService _sut;

        public FileServiceTests()
        {
            _mockHttp = new MockHttpMessageHandler();
            _http = _mockHttp.ToHttpClient();
            _http.BaseAddress = new Uri("https://localhost:5001");
            _logger = NullLogger<FileService>.Instance;

            _apiClient = new ApiClient(_http, NullLogger<ApiClient>.Instance);
            _sut = new FileService(_apiClient, _logger);
        }

        private readonly FakeBrowserFile _file = new(
            "test.zip", "application/zip", Encoding.UTF8.GetBytes("fake-content"));

        private readonly string _uploadResultJson = """
            {
                "url": "https://cdn/file.zip",
                "fileName": "test.zip",
                "size": 123
            }
            """;

        [Fact]
        public async Task UploadAsync_Should_Send_Multipart_Request_And_Return_Result()
        {
            await ApiContract<MultipartFormDataContent, FileUploadResult>
                .Post(_mockHttp, () => _sut.UploadAsync(_file))
                .To("/api/files/upload")
                .Returns(_uploadResultJson)
                .ShouldSendBody(async body =>
                {
                    body.Headers.ContentType!.MediaType.Should().Contain("multipart/form-data");
                    var content = await body.ReadAsStringAsync();
                    content.Should().Contain("test.zip");
                });
        }

        [Fact]
        public async Task UploadAsync_Should_Throw_On_Http_Error()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/api/files/upload")
                .Respond(HttpStatusCode.BadRequest);

            await ApiErrorAssertions.ShouldFailWith(() => _sut.UploadAsync(_file),
                HttpStatusCode.BadRequest);
        }

        public class FakeBrowserFile(string name, string contentType, byte[] data) : IBrowserFile
        {
            private readonly byte[] _data = data;

            public string Name { get; } = name;
            public string ContentType { get; } = contentType;
            public long Size { get; } = data.Length;

            public DateTimeOffset LastModified => DateTimeOffset.Now.AddMinutes(-1);

            public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
            {
                return new MemoryStream(_data);
            }
        }
    }
}

