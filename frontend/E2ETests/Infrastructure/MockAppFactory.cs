using GpuShare.Frontend.Services;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Sockets;

namespace E2ETests.Infrastructure;

/// <summary>
/// Starts the Blazor Server app on a real Kestrel TCP socket so Playwright can connect.
/// Swaps all API-backed services for MockStore-backed mocks.
/// </summary>
public sealed class MockAppFactory : IAsyncDisposable
{
    private readonly int _port = GetFreePort();
    private WebApplication? _app;

    /// <summary>Actual base URL, e.g. http://127.0.0.1:54321 — available after StartAsync().</summary>
    public string ServerUrl => $"http://127.0.0.1:{_port}";

    public async Task StartAsync()
    {
        // AppContext.BaseDirectory is  …/E2ETests/bin/Debug/net10.0/
        // The frontend project sits four levels up.
        var contentRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "GpuShare.Frontend"));

        _app = Program.CreateApp([], contentRootPath: contentRoot, configure: builder =>
        {
            // Tell Kestrel which port to use
            builder.WebHost.UseUrls($"http://127.0.0.1:{_port}");

            // Force Test environment (skips error-handler / HSTS middleware)
            builder.Environment.EnvironmentName = "Test";

            // Replace real API services with MockStore-backed in-process implementations
            builder.Services.RemoveAll<IAuthService>();
            builder.Services.RemoveAll<IDeviceService>();
            builder.Services.RemoveAll<IOrderService>();
            builder.Services.RemoveAll<IReviewService>();
            builder.Services.RemoveAll<IPaymentService>();
            builder.Services.RemoveAll<IDisputeService>();
            builder.Services.RemoveAll<IFileService>();
            builder.Services.RemoveAll<IAdminService>();

            builder.Services.AddScoped<IAuthService,    MockAuthService>();
            builder.Services.AddScoped<IDeviceService,  MockDeviceService>();
            builder.Services.AddScoped<IOrderService,   MockOrderService>();
            builder.Services.AddScoped<IReviewService,  MockReviewService>();
            builder.Services.AddScoped<IPaymentService, MockPaymentService>();
            builder.Services.AddScoped<IDisputeService, MockDisputeService>();
            builder.Services.AddScoped<IFileService,    MockFileService>();
            builder.Services.AddScoped<IAdminService,   MockAdminService>();
        });

        await _app.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }

    /// <summary>Asks the OS for a free TCP port then immediately releases it.</summary>
    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
