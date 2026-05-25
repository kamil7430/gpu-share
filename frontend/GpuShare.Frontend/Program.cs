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
using GpuShare.Frontend.Http;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

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
//builder.Services.AddScoped<IAuthState, AuthState>();
builder.Services.AddScoped<IAuthState, MockAuthState>(); // for testing purposes, replace with real implementation later
if (builder.Environment.IsDevelopment()) { builder.Services.AddScoped<IApiClient, MockApiClient>(); }
else { builder.Services.AddApiInfrastructure(); }
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthModalService, AuthModalService>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

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
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
