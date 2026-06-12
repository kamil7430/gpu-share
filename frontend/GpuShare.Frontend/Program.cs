using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using GpuShare.Frontend.Components;
using MudBlazor.Services;
using GpuShare.Frontend.Services;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.State;
using GpuShare.Frontend.Auth;
using GpuShare.Frontend.Extensions;
using GpuShare.Frontend.Infrastructure.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using MudBlazor;

var app = Program.CreateApp(args);
app.Run();

public partial class Program
{
    /// <summary>
    /// Builds a fully-configured <see cref="WebApplication"/> from the standard setup.
    /// Pass <paramref name="configure"/> to override services or settings (e.g. swap in mocks
    /// for tests, or call <c>builder.WebHost.UseUrls(...)</c> for a custom port).
    /// </summary>
    public static WebApplication CreateApp(
        string[] args,
        Action<WebApplicationBuilder>? configure = null,
        string? contentRootPath = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = contentRootPath, // null → default (current directory)
        });

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services
            .AddBlazorise(options =>
            {
                options.Immediate = true;
            })
            .AddBootstrapProviders()
            .AddFontAwesomeIcons();

        builder.Services.AddMudServices();

        builder.Services.AddAuthorizationCore();

        builder.Services.AddScoped<IJwtHelper, JwtHelper>();
        builder.Services.AddScoped<IFormatters, Formatters>();
        builder.Services.AddScoped<IAuthState, AuthState>();
        builder.Services.AddScoped<IAppNotifier, SnackbarNotifier>();
        builder.Services.AddScoped<IAuthModalService, AuthModalService>();
        builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

        builder.Services.AddApiInfrastructure(builder.Configuration);

        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IDeviceService, DeviceService>();
        builder.Services.AddScoped<IOrderService, OrderService>();

        // Services that currently only have mock implementations
        builder.Services.AddScoped<IFileService, MockFileService>();
        builder.Services.AddScoped<IReviewService, MockReviewService>();
        builder.Services.AddScoped<IDisputeService, MockDisputeService>();
        builder.Services.AddScoped<IPaymentService, MockPaymentService>();
        builder.Services.AddScoped<IAdminService, MockAdminService>();

        // Allow callers (tests) to override any of the above registrations
        configure?.Invoke(builder);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Test"))
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        // Disable HTTPS redirect for Docker container
        // app.UseHttpsRedirection();

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAntiforgery();

        // MapStaticAssets() reads a manifest keyed on the entry-assembly name.
        // Under `dotnet test` the entry assembly is `testhost`, so the manifest
        // is never found.  UseStaticFiles() already covers serving — skip the
        // optimised-assets middleware when running in the Test environment.
        if (!app.Environment.IsEnvironment("Test"))
            app.MapStaticAssets();

        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        return app;
    }
}
