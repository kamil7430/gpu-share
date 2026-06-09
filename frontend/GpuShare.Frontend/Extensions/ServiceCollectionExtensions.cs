namespace GpuShare.Frontend.Extensions;
using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiInfrastructure(this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<BackendSettings>(configuration.GetSection("Backend"));

        services.AddScoped<ApiClientHandler>();

        var backendAddress = new Uri("http://10.5.0.2:2137");

        // A plain http client used by the refresh handler to call auth endpoints without
        // going through the ApiClient pipeline (avoids circular DI dependencies).
        services.AddHttpClient("auth", (sp, client) =>
        {
            client.BaseAddress = backendAddress;
        });

        services.AddScoped<RefreshTokenHandler>();

        services.AddHttpClient<IApiClient, ApiClient>(
            (sp, client) =>
            {
                client.BaseAddress = backendAddress;
            })
            .AddHttpMessageHandler<ApiClientHandler>()
            .AddHttpMessageHandler<RefreshTokenHandler>()
            .AddPolicyHandler(HttpPolicies.GetRetryPolicy());

        return services;
    }
}
