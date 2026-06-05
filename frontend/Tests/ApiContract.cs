using FluentAssertions;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Collections.Specialized;

namespace GpuShare.Frontend.Tests
{
    public class ApiContract<TRequest, TResult>
    {
        private readonly MockHttpMessageHandler _mock;
        private readonly Func<Task<TResult>> _action;

        private HttpMethod _method = HttpMethod.Get;
        private string _url = "*";

        private string _responseJson = "";

        private HttpStatusCode _expectedStatus = HttpStatusCode.OK;

        private readonly Dictionary<string, string> _expectedHeaders = [];
        private readonly Dictionary<string, string> _expectedQuery = [];

        private HttpRequestMessage? _captured;
        private TRequest? _expectedBody;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        private ApiContract(MockHttpMessageHandler mock, Func<Task<TResult>> action)
        {
            _mock = mock;
            _action = action;
        }

        public static ApiContract<TRequest, TResult> Post(MockHttpMessageHandler mock,
            Func<Task<TResult>> action)
        {
            return new ApiContract<TRequest, TResult>(mock, action)
            {
                _method = HttpMethod.Post
            };
        }

        public static ApiContract<TRequest, TResult> Get(MockHttpMessageHandler mock,
            Func<Task<TResult>> action)
        {
            return new ApiContract<TRequest, TResult>(mock, action)
            {
                _method = HttpMethod.Get
            };
        }

        public static ApiContract<TRequest, TResult> Patch(MockHttpMessageHandler mock,
            Func<Task<TResult>> action)
        {
            return new ApiContract<TRequest, TResult>(mock, action)
            {
                _method = HttpMethod.Patch
            };
        }

        public ApiContract<TRequest, TResult> Returns(string json)
        {
            _responseJson = json;

            _mock.When(_method, _url)
                .Respond(req =>
                {
                    _captured = req;

                    return new HttpResponseMessage(_expectedStatus)
                    {
                        Content = new StringContent(_responseJson)
                    };
                });

            return this;
        }

        public ApiContract<TRequest, TResult> WithBody(TRequest body)
        {
            _expectedBody = body;
            return this;
        }   

        public async Task ShouldSendBody(Action<TRequest> bodyAssert)
        {
            await _action();

            _captured.Should().NotBeNull();

            // ======================
            // METHOD CHECK
            // ======================
            _captured!.Method.Should().Be(_method);

            // ======================
            // URL + QUERY CHECK
            // ======================
            var uri = _captured.RequestUri!.ToString();

            uri.Should().Contain(_url);

            var query = System.Web.HttpUtility.ParseQueryString(_captured.RequestUri!.Query);

            foreach (var expected in _expectedQuery)
            {
                query[expected.Key].Should().Be(expected.Value);
            }

            // ======================
            // HEADER CHECK
            // ======================
            foreach (var expectedHeader in _expectedHeaders)
            {
                _captured.Headers.TryGetValues(expectedHeader.Key, out var values)
                    .Should().BeTrue($"Header {expectedHeader.Key} should exist");

                values!.First().Should().Be(expectedHeader.Value);
            }

            // ======================
            // BODY CHECK
            // ======================
            if (_expectedBody is not null)
            {
                var json = await _captured.Content!.ReadAsStringAsync();

                var actual = JsonSerializer.Deserialize<TRequest>(json, _jsonOptions);

                actual.Should().NotBeNull();
                bodyAssert(actual!);
            }
        }

        public ApiContract<TRequest, TResult> ExpectStatus(HttpStatusCode status)
        {
            _expectedStatus = status;
            return this;
        }

        public ApiContract<TRequest, TResult> ExpectQuery(Action<NameValueCollection> assert)
        {
            _mock.When(_method, _url)
                .Respond(req =>
                {
                    _captured = req;

                    var query = System.Web.HttpUtility.ParseQueryString(req.RequestUri!.Query);

                    assert(query);

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(_responseJson)
                    };
                });

            return this;
        }

        public ApiContract<TRequest, TResult> WithHeader(string key, string value)
        {
            _expectedHeaders[key] = value;
            return this;
        }

        public ApiContract<TRequest, TResult> WithQuery(string key, string value)
        {
            _expectedQuery[key] = value;
            return this;
        }

        public ApiContract<TRequest, TResult> To(string url)
        {
            _url = url;
            return this;
        }

        public async Task ThenResponse(Action<TResult> assert)
        {
            var result = await _action();

            assert(result);
        }

        public async Task ShouldMapTo(TResult expected)
        {
            var actual = await _action();

            actual.Should().BeEquivalentTo(expected, options =>
                options
                    .Using<DateTime>(ctx =>
                        ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromDays(30)))
                    .WhenTypeIs<DateTime>()
            );
        }
    }
}
