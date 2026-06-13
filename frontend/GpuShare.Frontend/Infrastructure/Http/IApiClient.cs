namespace GpuShare.Frontend.Infrastructure.Http;

/// <summary>
/// Interface for API client, defines methods for making HTTP requests to the backend API.
/// Provides methods for GET, POST, PATCH, and DELETE operations, with support for JSON serialization and error handling via ApiException.
/// Example usage:
/// private readonly IApiClient _api;
/// 
/// public OrderService(IApiClient api)
/// {
///     _api = api;
/// }
/// 
/// public async Task<Order> GetOrderAsync(Guid id)
/// {
///     return await _api.GetAsync<Order>($"/api/orders/{id}");
/// }
/// </summary>
public interface IApiClient
{
    Task<T?> GetAsync<T>(string url);

    Task<T?> GetAsync<T>(string url, object query);

    /// <summary>
    /// GET with a query object. When <paramref name="anonymous"/> is true the request is sent
    /// without an Authorization header even if the user is logged in (for public endpoints).
    /// </summary>
    Task<T?> GetAsync<T>(string url, object query, bool anonymous);

    Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data);

    Task PostAsync<TRequest>(string url, TRequest data);

    Task<TResponse?> PostAsync<TResponse>(string url);

    Task PatchAsync<TRequest>(string url, TRequest data);

    //Task<TResponse?> PatchAsync<TRequest, TResponse>(string url, TRequest data);

    Task DeleteAsync(string url);
}