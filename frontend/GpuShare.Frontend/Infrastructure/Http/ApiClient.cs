namespace GpuShare.Frontend.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GpuShare.Frontend.Models;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        var response = await _http.GetAsync(url);

        await EnsureSuccess(response);

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        var response = await _http.PostAsJsonAsync(url, data);

        await EnsureSuccess(response);

        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task PostAsync<TRequest>(string url, TRequest data)
    {
        var response = await _http.PostAsJsonAsync(url, data);

        await EnsureSuccess(response);
    }

    public async Task<TResponse?> PostAsync<TResponse>(string url)
    {
        var response = await _http.PostAsync(url, null);

        await EnsureSuccess(response);

        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task PatchAsync<TRequest>(string url, TRequest data)
    {
        var response = await _http.PatchAsJsonAsync(url, data);

        await EnsureSuccess(response);
    }

    public async Task DeleteAsync(string url)
    {
        var response = await _http.DeleteAsync(url);

        await EnsureSuccess(response);
    }

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();

        throw new ApiException(content, (int)response.StatusCode);
    }
}