using GpuShare.Frontend.Services;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
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

        // The build copies the frontend's static-assets manifest into the test output.
        // MapStaticAssets() can't find it on its own (it looks for "testhost.*" under
        // dotnet test), so pass the path explicitly — it serves _framework/blazor.web.js
        // and _content/* package assets, without which the Blazor circuit never starts.
        var staticAssetsManifest = Path.Combine(
            AppContext.BaseDirectory, "GpuShare.Frontend.staticwebassets.endpoints.json");

        _app = Program.CreateApp([],
            contentRootPath: contentRoot,
            staticAssetsManifestPath: staticAssetsManifest,
            configure: builder =>
        {
            // Tell Kestrel which port to use
            builder.WebHost.UseUrls($"http://127.0.0.1:{_port}");

            // Force Test environment (skips error-handler / HSTS middleware)
            builder.Environment.EnvironmentName = "Test";

            // Wire the dev-style static web assets file provider. The endpoints manifest
            // (above) only maps ROUTES; the physical files for _content/* packages,
            // _framework/blazor.web.js and the scoped-CSS bundle live in NuGet/obj folders
            // that this runtime manifest maps into the web root. CreateBuilder does this
            // automatically only in the Development environment with a matching app name,
            // so under dotnet test we must do it ourselves — without it every mapped asset
            // endpoint 500s and the Blazor circuit never starts.
            builder.Configuration[WebHostDefaults.StaticWebAssetsKey] = Path.Combine(
                AppContext.BaseDirectory, "GpuShare.Frontend.staticwebassets.runtime.json");
            StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

            // Replace real API services with MockStore-backed in-process implementations
            builder.Services.RemoveAll<GpuShare.Frontend.State.IAuthState>();
            builder.Services.AddScoped<GpuShare.Frontend.State.IAuthState, MockAuthState>();

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
