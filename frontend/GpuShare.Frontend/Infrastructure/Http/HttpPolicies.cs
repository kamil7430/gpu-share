namespace GpuShare.Frontend.Http;
using Polly;
using Polly.Extensions.Http;

public static class HttpPolicies
{
    public static IAsyncPolicy<HttpResponseMessage>
        GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                3,
                retryAttempt =>
                    TimeSpan.FromSeconds(
                        Math.Pow(2, retryAttempt)));
    }
}