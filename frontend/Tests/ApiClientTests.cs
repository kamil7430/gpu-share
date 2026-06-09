using FluentAssertions;
using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Text.Json;

namespace GpuShare.Frontend.Tests
{
    public class ApiClientTests
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<ApiClient>> _logger;
        private readonly ApiClient _sut;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new JsonStringEnumConverter() }
        };

        public ApiClientTests()
        {
            _mockHttp = new MockHttpMessageHandler();

            _httpClient = _mockHttp.ToHttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:5001");

            _logger = new Mock<ILogger<ApiClient>>();

            _sut = new ApiClient(_httpClient, _logger.Object);
        }

        private class TestDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        [Fact]
        public async Task GetAsync_Should_Send_Get_Request()
        {
            HttpRequestMessage? captured = null;

            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/test")
                .Respond(req =>
                {
                    captured = req;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new { value = "ok" })
                    };
                });

            await _sut.GetAsync<object>("/test");

            captured.Should().NotBeNull();
            captured!.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public async Task GetAsync_Should_Deserialize_Response()
        {
            var expected = new TestDto
            {
                Id = 1,
                Name = "RTX"
            };

            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/test")
                .Respond("application/json", JsonSerializer.Serialize(expected));

            var result = await _sut.GetAsync<TestDto>("/test");

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetAsync_WithQuery_Should_Append_Query_String()
        {
            HttpRequestMessage? captured = null;

            _mockHttp.When(HttpMethod.Get, "*")
                .Respond(req =>
                {
                    captured = req;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new { })
                    };
                });

            await _sut.GetAsync<object>("/devices",
                new
                {
                    page = 2,
                    limit = 10
                });

            captured.Should().NotBeNull();

            var query = System.Web.HttpUtility.ParseQueryString(captured!.RequestUri!.Query);

            query["page"].Should().Be("2");
            query["limit"].Should().Be("10");
        }

        [Fact]
        public async Task PostAsync_Should_Send_Request_Body()
        {
            HttpRequestMessage? captured = null;

            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/test")
                .Respond(req =>
                {
                    captured = req;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new TestDto())
                    };
                });

            var request = new TestDto
            {
                Id = 123,
                Name = "RTX"
            };

            await _sut.PostAsync<TestDto, TestDto>("/test", request);

            var body =
                await captured!.Content!.ReadAsStringAsync();

            var sent = JsonSerializer.Deserialize<TestDto>(body, _jsonOptions);

            sent.Should().BeEquivalentTo(request);
        }

        [Fact]
        public async Task PostAsync_Should_Deserialize_Response()
        {
            var expected = new TestDto
            {
                Id = 10,
                Name = "Created"
            };

            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/test")
                .Respond("application/json", JsonSerializer.Serialize(expected));

            var result = await _sut.PostAsync<TestDto, TestDto>("/test", new TestDto());

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task PostAsync_Void_Should_Succeed()
        {
            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/test")
                .Respond(HttpStatusCode.NoContent);

            var act =
                () => _sut.PostAsync("/test", new TestDto());

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task PostAsync_NoRequest_Should_Send_Empty_Body()
        {
            HttpRequestMessage? captured = null;

            _mockHttp.When(HttpMethod.Post, "https://localhost:5001/test")
                .Respond(req =>
                {
                    captured = req;

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new TestDto())
                    };
                });

            await _sut.PostAsync<TestDto>("/test");

            captured.Should().NotBeNull();
        }

        [Fact]
        public async Task PatchAsync_Should_Send_Patch_Request()
        {
            HttpRequestMessage? captured = null;

            _mockHttp.When(HttpMethod.Patch, "https://localhost:5001/test")
                .Respond(req =>
                {
                    captured = req;

                    return new HttpResponseMessage(HttpStatusCode.NoContent);
                });

            await _sut.PatchAsync("/test", new TestDto());

            captured!.Method.Should().Be(HttpMethod.Patch);
        }

        [Fact]
        public async Task DeleteAsync_Should_Send_Delete_Request()
        {
            HttpRequestMessage? captured = null;

            _mockHttp.When(HttpMethod.Delete, "https://localhost:5001/test")
                .Respond(req =>
                {
                    captured = req;

                    return new HttpResponseMessage(HttpStatusCode.NoContent);
                });

            await _sut.DeleteAsync("/test");

            captured!.Method.Should().Be(HttpMethod.Delete);
        }

        [Fact]
        public async Task GetAsync_Should_Throw_ApiException_On_BadRequest()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/test")
                .Respond(HttpStatusCode.BadRequest);

            var act = () => _sut.GetAsync<object>("/test");

            var ex = await act.Should().ThrowAsync<ApiException>();

            ex.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetAsync_Should_Include_Error_Message()
        {
            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/test")
                .Respond(HttpStatusCode.BadRequest, "text/plain", "validation failed");

            var act = () => _sut.GetAsync<object>("/test");

            var ex = await act.Should().ThrowAsync<ApiException>();

            ex.Which.Message.Should().Contain("validation failed");
        }

        [Fact]
        public async Task GetAsync_Should_Read_Int_From_String()
        {
            const string json = """
            {
                "id":"123"
            }
            """;

            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/test")
                .Respond("application/json", json);

            var result = await _sut.GetAsync<TestDto>("/test");

            result!.Id.Should().Be(123);
        }

        [Fact]
        public async Task GetAsync_Should_Read_Enum_From_String()
        {
            const string json = """
            {
                "state":"Available"
            }
            """;

            _mockHttp.When(HttpMethod.Get, "https://localhost:5001/test")
                .Respond("application/json", json);

            var result = await _sut.GetAsync<Device>("/test");

            result!.State.Should().Be(DeviceState.AVAILABLE);
        }
    }
}
