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

var builder = WebApplication.CreateBuilder(args);

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
//builder.Services.AddScoped<IAuthState, MockAuthState>(); // for testing purposes, replace with real implementation later
//if (builder.Environment.IsDevelopment()) { builder.Services.AddScoped<IApiClient, MockApiClient>(); }
//else { builder.Services.AddApiInfrastructure(builder.Configuration); }
builder.Services.AddApiInfrastructure(builder.Configuration);

//builder.Services.AddScoped<IAuthService, AuthService>();
//builder.Services.AddScoped<IFileService, FileService>();
//builder.Services.AddScoped<IDeviceService, DeviceService>();
//builder.Services.AddScoped<IReviewService, ReviewService>();
//builder.Services.AddScoped<IOrderService, OrderService>();
//builder.Services.AddScoped<IDisputeService, DisputeService>();
//builder.Services.AddScoped<IPaymentService, PaymentService>();

// Mock services — swap individual lines above with these to run without a backend:
builder.Services.AddScoped<IAuthService, MockAuthService>();
builder.Services.AddScoped<IFileService, MockFileService>();
builder.Services.AddScoped<IDeviceService, MockDeviceService>();
builder.Services.AddScoped<IReviewService, MockReviewService>();
builder.Services.AddScoped<IOrderService, MockOrderService>();
builder.Services.AddScoped<IDisputeService, MockDisputeService>();
builder.Services.AddScoped<IPaymentService, MockPaymentService>();
builder.Services.AddScoped<IAdminService, MockAdminService>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(
        new DirectoryInfo("/keys"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// Disable HTTPS redirect for Docker container
// app.UseHttpsRedirection();

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseStaticFiles();
app.UseRouting();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
