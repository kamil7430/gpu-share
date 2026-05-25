namespace GpuShare.Frontend.Http
{
    public class MockApiClient : IApiClient
    {
        public Task DeleteAsync(string url)
        {
            return Task.CompletedTask;
        }

        public Task<T?> GetAsync<T>(string url)
        {
            return Task.FromResult<T?>(default);
        }

        public Task PatchAsync<TRequest>(string url, TRequest data)
        {
            return Task.CompletedTask;
        }

        public Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
        {
            return Task.FromResult<TResponse?>(default);
        }

        public Task PostAsync<TRequest>(string url, TRequest data)
        {
            return Task.CompletedTask;
        }

        public Task<TResponse?> PostAsync<TResponse>(string url)
        {
            return Task.FromResult<TResponse?>(default);
        }
    }
}
