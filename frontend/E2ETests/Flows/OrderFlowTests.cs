using E2ETests.Infrastructure;
using FluentAssertions;
using Microsoft.Playwright;

namespace E2ETests.Flows
{
    public class OrderFlowTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
    {
        [Fact]
        public async Task Create_Order_Updates_Device_State_And_Shows_Connection()
        {
            await LoginAsAsync("alice");
            await Page.GotoAsync("/device/1/view");      // RTX 4090 — AVAILABLE

            // Fill out the order form
            await Page.GetByLabel("Start time").FillAsync(
                DateTime.UtcNow.AddMinutes(5).ToString("yyyy-MM-ddTHH:mm"));
            await Page.GetByLabel("Duration (hours)").FillAsync("4");
            await Page.GetByLabel("Docker image").FillAsync("pytorch/pytorch:latest");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Reserve" }).ClickAsync();

            // Redirected to order page with connection info
            await Page.WaitForURLAsync("**/order/**");
            var markup = await Page.ContentAsync();
            markup.Should().Contain("gpu1.gpushare.io");     // from MockOrderService
        }

        [Fact]
        public async Task End_Order_Returns_Device_To_Available()
        {
            await LoginAsAsync("alice");
            await Page.GotoAsync("/order/1");            // running order in seed data

            await Page.GetByRole(AriaRole.Button, new() { Name = "End session" }).ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Confirm" }).ClickAsync();

            // Device should show as Available again
            await Page.GotoAsync("/device/2/view");
            var markup = await Page.ContentAsync();
            markup.Should().Contain("Available");
        }
    }
}
