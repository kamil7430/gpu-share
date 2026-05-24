namespace GpuShare.Frontend.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GpuShare.Frontend.Models;
using System.Net;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        return await ExecuteRequest(async () =>
        {   
            var response = await _http.GetAsync(url);

            await EnsureSuccess(response);

            return await response.Content.ReadFromJsonAsync<T>();
        });
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        return await ExecuteRequest(async () =>
        {   
            var response = await _http.PostAsJsonAsync(url, data);

            await EnsureSuccess(response);

            return await response.Content.ReadFromJsonAsync<TResponse>();
        });
    }

    public async Task PostAsync<TRequest>(string url, TRequest data)
    {
        await ExecuteRequest(async () =>
        {   
            var response = await _http.PostAsJsonAsync(url, data);

            await EnsureSuccess(response);
        });
    }

    public async Task<TResponse?> PostAsync<TResponse>(string url)
    {
        return await ExecuteRequest(async () =>
        {   
            var response = await _http.PostAsync(url, null);

            await EnsureSuccess(response);

            return await response.Content.ReadFromJsonAsync<TResponse>();
        });
    }

    public async Task PatchAsync<TRequest>(string url, TRequest data)
    {
        await ExecuteRequest(async () =>
        {   
            var response = await _http.PatchAsJsonAsync(url, data);

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