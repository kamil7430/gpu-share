using GpuShare.Frontend.Services;
using GpuShare.Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace E2ETests.Infrastructure
{
    public class MockAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                // Remove all real API-touching services
                services.RemoveAll<IAuthService>();
                services.RemoveAll<IDeviceService>();
                services.RemoveAll<IOrderService>();
                services.RemoveAll<IReviewService>();
                services.RemoveAll<IPaymentService>();
                services.RemoveAll<IDisputeService>();
                services.RemoveAll<IFileService>();
                services.RemoveAll<IAdminService>();

                // Register mocks (all backed by MockStore)
                services.AddScoped<IAuthService, MockAuthService>();
                services.AddScoped<IDeviceService, MockDeviceService>();
                services.AddScoped<IOrderService, MockOrderService>();
                services.AddScoped<IReviewService, MockReviewService>();
                services.AddScoped<IPaymentService, MockPaymentService>();
                services.AddScoped<IDisputeService, MockDisputeService>();
                services.AddScoped<IFileService, MockFileService>();
                services.AddScoped<IAdminService, MockAdminService>();
            });
        }
    }
}
