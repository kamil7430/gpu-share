using FluentAssertions;
using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.State;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace GpuShare.Frontend.Tests
{
    public class HandlersTests
    {
        [Fact]
        public async Task Should_Add_Bearer_Token_When_User_Is_Authenticated()
        {
            var authState = new TestAuthState();
            authState.SetAuth(new Models.User(), "token123");

            var innerHandler = new TestHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK));

            var handler = new ApiClientHandler(authState)
            {
                InnerHandler = innerHandler
            };

            var client = new HttpClient(handler);

            await client.GetAsync("https://localhost/test");

            innerHandler.LastRequest!
                .Headers.Authorization!
                .Scheme.Should().Be("Bearer");

            innerHandler.LastRequest!
                .Headers.Authorization!
                .Parameter.Should().Be("token123");
        }

        [Fact]
        public async Task Should_Not_Add_Header_When_Not_Authenticated()
        {
            var authState = new AuthState(new MockJwtHelper());

            var innerHandler = new TestHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK));

            var handler = new ApiClientHandler(authState)
            {
                InnerHandler = innerHandler
            };

            var client = new HttpClient(handler);

            await client.GetAsync("https://localhost/test");

            innerHandler.LastRequest!
                .Headers.Authorization
                .Should().BeNull();
        }

        [Fact]
        public async Task RetryPolicy_Should_Retry_Three_Times()
        {
            var attempts = 0;

            var policy = HttpPolicies.GetRetryPolicy();

            await policy.ExecuteAsync(() =>
            {
                attempts++;

                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.InternalServerError));
            });

            attempts.Should().Be(4);
        }

        [Fact]
        public async Task RetryPolicy_Should_Not_Retry_On_Success()
        {
            var attempts = 0;

            var policy = HttpPolicies.GetRetryPolicy();

            await policy.ExecuteAsync(() =>
            {
                attempts++;

                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.OK));
            });

            attempts.Should().Be(1);
        }

        [Fact]
        public async Task Should_Return_Response_When_Not_Unauthorized()
        {
            var authState = new AuthState(new MockJwtHelper());

            var innerHandler = new TestHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK));

            var factory = new Mock<IHttpClientFactory>();

            var handler = new RefreshTokenHandler(authState, factory.Object)
            {
                InnerHandler = innerHandler
            };

            var client = new HttpClient(handler);

            var response = await client.GetAsync("https://localhost/test");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            factory.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Should_Refresh_Token_And_Retry_Request()
        {
            var authState = new AuthState(new MockJwtHelper());

            authState.SetAuth(
                new User { Username = "john" },
                "old-token");

            var calls = 0;

            var apiHandler = new TestHandler(req =>
            {
                calls++;

                return calls == 1
                    ? new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    : new HttpResponseMessage(HttpStatusCode.OK);
            });

            var refreshHandler = new TestHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create("new-token")
                });

            var refreshClient = new HttpClient(refreshHandler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            var factory = new Mock<IHttpClientFactory>();

            factory.Setup(x => x.CreateClient("auth"))
                .Returns(refreshClient);

            var handler = new RefreshTokenHandler(authState, factory.Object)
            {
                InnerHandler = apiHandler
            };

            var client = new HttpClient(handler);

            var response = await client.GetAsync("https://localhost/test");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            authState.AccessToken.Should().Be("new-token");

            calls.Should().Be(2);
        }

        [Fact]
        public async Task Should_Logout_When_Refresh_Fails()
        {
            var authState = new AuthState(new MockJwtHelper());

            authState.SetAuth(
                new User { Username = "john" },
                "old-token");

            var apiHandler = new TestHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.Unauthorized));

            var refreshHandler = new TestHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.Unauthorized));

            var refreshClient = new HttpClient(refreshHandler);

            var factory = new Mock<IHttpClientFactory>();

            factory.Setup(x => x.CreateClient("auth"))
                .Returns(refreshClient);

            var handler = new RefreshTokenHandler(authState, factory.Object)
            {
                InnerHandler = apiHandler
            };

            var client = new HttpClient(handler);

            await client.GetAsync("https://localhost/test");

            authState.AccessToken.Should().BeNull();
            authState.User.Should().BeNull();
        }

        [Fact]
        public async Task Should_Logout_When_Refresh_Returns_Empty_Token()
        {
            var authState = new AuthState(new MockJwtHelper());

            authState.SetAuth(
                new User { Username = "john" },
                "old-token");

            var apiHandler = new TestHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.Unauthorized));

            var refreshHandler = new TestHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create("")
                });

            var refreshClient = new HttpClient(refreshHandler);

            var factory = new Mock<IHttpClientFactory>();

            factory.Setup(x => x.CreateClient("auth"))
                .Returns(refreshClient);

            var handler = new RefreshTokenHandler(authState, factory.Object)
            {
                InnerHandler = apiHandler
            };

            var client = new HttpClient(handler);

            await client.GetAsync("https://localhost/test");

            authState.AccessToken.Should().BeNull();
            authState.User.Should().BeNull();
        }

        [Fact]
        public async Task Should_Use_New_Token_On_Retry()
        {
            // Arrange
            var authState = new AuthState(new MockJwtHelper());

            authState.SetAuth(
                new User { Username = "john" },
                "old-token");

            var authorizationHeaders = new List<string?>();

            var apiHandler = new TestHandler(req =>
            {
                authorizationHeaders.Add(
                    req.Headers.Authorization?.Parameter);

                return authorizationHeaders.Count == 1
                    ? new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    : new HttpResponseMessage(HttpStatusCode.OK);
            });

            var refreshHandler = new TestHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create("new-token")
                });

            var refreshClient = new HttpClient(refreshHandler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            var factory = new Mock<IHttpClientFactory>();

            factory.Setup(x => x.CreateClient("auth"))
                .Returns(refreshClient);

            var refreshTokenHandler = new RefreshTokenHandler(
                authState,
                factory.Object)
            {
                InnerHandler = apiHandler
            };

            var authHandler = new ApiClientHandler(authState)
            {
                InnerHandler = refreshTokenHandler
            };

            var client = new HttpClient(authHandler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            // Act
            var response = await client.GetAsync("/devices");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            authorizationHeaders.Should().HaveCount(2);

            authorizationHeaders[0].Should().Be("old-token");
            authorizationHeaders[1].Should().Be("new-token");

            authState.AccessToken.Should().Be("new-token");
        }
    }

    public class TestHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public TestHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_responseFactory(request));
        }
    }
}
