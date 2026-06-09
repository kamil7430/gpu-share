namespace GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

public class ApiClient(HttpClient http, ILogger<ApiClient> logger) : IApiClient
{
    private readonly HttpClient _http = http;
    private readonly ILogger<ApiClient> _logger = logger;
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<T?> GetAsync<T>(string url)
    {
        return await ExecuteRequest(async () =>
        {   
            var response = await _http.GetAsync(url);

            await EnsureSuccess(response);

            return await response.Content.ReadFromJsonAsync<T>(_options);
        });
    }

    public async Task<T?> GetAsync<T>(string url, object query)
    {
        return await ExecuteRequest(async () =>
        {
            var response = await _http.GetAsync(QueryStringBuilder.Build(url, query));

            await EnsureSuccess(response);

            return await response.Content.ReadFromJsonAsync<T>(_options);
        });
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        return await ExecuteRequest(async () =>
        {   
            var response = await _http.PostAsJsonAsync(url, data, _options);

            await EnsureSuccess(response);

            return await response.Content.ReadFromJsonAsync<TResponse>(_options);
        });
    }

    public async Task PostAsync<TRequest>(string url, TRequest data)
    {
        await ExecuteRequest(async () =>
        {   
            var response = await _http.PostAsJsonAsync(url, data, _options);

            await EnsureSuccess(response);
        });
    }

    public async Task<TResponse?> PostAsync<TResponse>(string url)
    {
        return await ExecuteRequest(async () =>
        {   
            var response = await _http.PostAsync(url, null);

            await EnsureSuccess(response);

            return await response.Content.ReadFromJsonAsync<TResponse>(_options);
        });
    }

    public async Task PatchAsync<TRequest>(string url, TRequest data)
    {
        await ExecuteRequest(async () =>
        {   
            var response = await _http.PatchAsJsonAsync(url, data, _options);

            await EnsureSuccess(response);
        });
    }

    public async Task DeleteAsync(string url)
    {
        await ExecuteRequest(async () =>
        {   
            var response = await _http.DeleteAsync(url);

            await EnsureSuccess(response);
        });
    }

    private async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();

        _logger.LogError("API call did not succeed. Error message: {message}. Http code: {code}.", 
            content, response.StatusCode);
        throw new ApiException(content, response.StatusCode);
    }

    private async Task<T> ExecuteRequest<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Request timed out. Http code: {code}.", HttpStatusCode.RequestTimeout);
            throw new ApiException("Request timed out. ", HttpStatusCode.RequestTimeout);
        }
        catch (HttpRequestException)
        {
            _logger.LogError("Cannot connect to server. Http code: {code}.", HttpStatusCode.ServiceUnavailable);
            throw new ApiException("Cannot connect to server.", HttpStatusCode.ServiceUnavailable);
        }
    }

    private async Task ExecuteRequest(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (ApiException ex)
        {
            throw new ApiException(ex.Message, ex.StatusCode);
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Request timed out. Http code: {code}.", HttpStatusCode.RequestTimeout);
            throw new ApiException("Request timed out.", HttpStatusCode.RequestTimeout);
        }
        catch (HttpRequestException)
        {
            _logger.LogError("Cannot connect to server. Http code: {code}.", HttpStatusCode.ServiceUnavailable);
            throw new ApiException("Cannot connect to server.", HttpStatusCode.ServiceUnavailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            throw new ApiException("Unexpected error", HttpStatusCode.InternalServerError);
        }
    }
}