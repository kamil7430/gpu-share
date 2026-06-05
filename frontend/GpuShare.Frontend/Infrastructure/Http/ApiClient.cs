namespace GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public ApiClient(HttpClient http)
    {
        _http = http;
        _options.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        return await ExecuteRequest(async () =>
        {   
            var response = await _http.GetAsync(url);

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

    //public async Task<TResponse?> PatchAsync<TRequest, TResponse>(string url, TRequest data)
    //{
    //    return await ExecuteRequest(async () =>
    //    {
    //        var response = await _http.PatchAsJsonAsync(url, data, _options);

    //        await EnsureSuccess(response);

    //        await response.Content.ReadFromJsonAsync<TResponse>();
    //    });
    //}

    public async Task DeleteAsync(string url)
    {
        await ExecuteRequest(async () =>
        {   
            var response = await _http.DeleteAsync(url);

            await EnsureSuccess(response);
        });
    }

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();

        throw new ApiException(content, response.StatusCode);
    }

    private static async Task<T> ExecuteRequest<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (TaskCanceledException)
        {
            throw new ApiException("Request timed out.", HttpStatusCode.RequestTimeout);
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Cannot connect to server.", HttpStatusCode.ServiceUnavailable);
        }
    }

    private static async Task ExecuteRequest(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (TaskCanceledException)
        {
            throw new ApiException("Request timed out.", HttpStatusCode.RequestTimeout);
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Cannot connect to server.", HttpStatusCode.ServiceUnavailable);
        }
    }
}