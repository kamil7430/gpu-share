using E2ETests.Infrastructure;
using FluentAssertions;
using GpuShare.Frontend.Services;
using Microsoft.Playwright;

namespace E2ETests.Flows
{
    public class OrderFlowTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
    {
        [Fact]
        public async Task Create_Order_Updates_Device_State_And_Shows_Connection()
        {
            await LoginAsAsync("alice");

            // Browse the catalog (login leaves us on "/") and open the first device —
            // device 1, RTX 4090, AVAILABLE, owned by bob — via its card link.
            await Page.Locator(".gpu-card .device-name").First.ClickAsync();
            await Page.WaitForURLAsync("**/device/view/1");

            // The order form is pre-filled with defaults (tomorrow 12:00, 1 hour).
            // Change the duration to 4 h, set the required Docker image and submit.
            await Page.GetByLabel("Duration (hours)").FillAsync("4");
            await Page.GetByLabel("Docker Image").FillAsync("pytorch/pytorch:2.0-cuda11.7");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Order GPU" }).ClickAsync();

            // DeviceOrderForm navigates to /order/{id} on success. SPA navigation updates
            // the URL before the new page renders, so use an auto-waiting assertion
            // instead of snapshotting Page.ContentAsync() at the moment of URL change.
            await Page.WaitForURLAsync("**/order/**");
            await Assertions.Expect(Page.Locator(".connection-command"))
                .ToContainTextAsync("gpu1.gpushare.io");     // MockOrderService host for device 1
        }

        [Fact]
        public async Task Order_Without_Docker_Image_Is_Blocked()
        {
            await LoginAsAsync("alice");
            await Page.GotoAsync("/device/view/1");
            await WaitForBlazorInteractiveAsync();

            var ordersBefore = MockStore.Orders.Count;

            // dockerImage is required by the API contract (orders.yaml) — submitting
            // without it must be stopped by form validation, not reach the service.
            await Page.GetByRole(AriaRole.Button, new() { Name = "Order GPU" }).ClickAsync();

            // Message appears in the ValidationSummary and under the field
            await Page.GetByText("Docker image is required").First.WaitForAsync();

            MockStore.Orders.Count.Should().Be(ordersBefore);
        }

        [Fact]
        public async Task End_Order_Returns_Device_To_Available()
        {
            await LoginAsAsync("alice");
            await Page.GotoAsync("/order/1");            // order 1 is RUNNING for device 2
            await WaitForBlazorInteractiveAsync();

            // Open the end-session modal
            await Page.GetByRole(AriaRole.Button, new() { Name = "End Session" }).ClickAsync();
            // Confirm inside the modal (button also says "End Session")
            await Page.GetByRole(AriaRole.Button, new() { Name = "End Session" }).Last.ClickAsync();

            // Modal closes only after EndOrderAsync completed server-side — wait before
            // navigating away, otherwise the next page can render stale device state.
            await Assertions.Expect(Page.GetByRole(AriaRole.Button, new() { Name = "End Session" }))
                .ToHaveCountAsync(1);

            // Device 2 should now show as Available
            await Page.GotoAsync("/device/view/2");
            var markup = await Page.ContentAsync();
            markup.Should().Contain("Available");
        }
    }
}
