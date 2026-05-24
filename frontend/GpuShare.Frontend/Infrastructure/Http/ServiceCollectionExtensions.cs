namespace GpuShare.Frontend.Http;
using Polly;
using Polly.Extensions.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using GpuShare.Frontend.Services;
using GpuShare.Frontend.Services.Interfaces;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ApiClientHandler>();

        // A plain http client used by the refresh handler to call auth endpoints without
        // going through the ApiClient pipeline (avoids circular DI dependencies).
        services.AddHttpClient("auth", client =>
        {
            client.BaseAddress = new Uri("https://localhost:5001");
        });

        services.AddScoped<RefreshTokenHandler>();

        services.AddHttpClient<IApiClient, ApiClient>(
            client =>
            {
                client.BaseAddress =
                    new Uri("https://localhost:5001");
            })
            .AddHttpMessageHandler<ApiClientHandler>()
            .AddHttpMessageHandler<RefreshTokenHandler>()
            .AddPolicyHandler(HttpPolicies.GetRetryPolicy());

        return services;
    }
}