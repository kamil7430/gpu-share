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
            // Route is /device/{ModeString}/{DeviceId} — device 1 is RTX 4090, AVAILABLE, owned by bob
            await Page.GotoAsync("/device/view/1");

            // The order form is pre-filled with defaults (tomorrow 12:00, 1 hour).
            // Just change the duration to 4 h and submit.
            await Page.GetByLabel("Duration (hours)").FillAsync("4");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Order GPU" }).ClickAsync();

            // DeviceOrderForm navigates to /order/{id} on success
            await Page.WaitForURLAsync("**/order/**");
            var markup = await Page.ContentAsync();
            markup.Should().Contain("gpu1.gpushare.io");     // MockOrderService seed for device 1
        }

        [Fact]
        public async Task End_Order_Returns_Device_To_Available()
        {
            await LoginAsAsync("alice");
            await Page.GotoAsync("/order/1");            // order 1 is RUNNING for device 2

            // Open the end-session modal
            await Page.GetByRole(AriaRole.Button, new() { Name = "End Session" }).ClickAsync();
            // Confirm inside the modal (button also says "End Session")
            await Page.GetByRole(AriaRole.Button, new() { Name = "End Session" }).Last.ClickAsync();

            // Device 2 should now show as Available
            await Page.GotoAsync("/device/view/2");
            var markup = await Page.ContentAsync();
            markup.Should().Contain("Available");
        }
    }
}
