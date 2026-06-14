using E2ETests.Infrastructure;
using FluentAssertions;
using GpuShare.Frontend.Services;
using Microsoft.Playwright;

namespace E2ETests.Flows
{
    public class DeviceRegistrationTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
    {
        [Fact]
        public async Task Register_Device_Shows_Agent_Install_Section()
        {
            await LoginAsAsync("alice");
            await Page.GotoAsync("/device/new");
            await WaitForBlazorInteractiveAsync();

            // EditDeviceForm labels aren't for-associated with their inputs, so target
            // the inputs directly. Device has no validation attributes — name + model is enough.
            await Page.GetByPlaceholder("e.g. My Beast Rig").FillAsync("Alice Test Rig");
            await Page.Locator("input[list='models']").FillAsync("NVIDIA RTX 5090");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Save Configuration" }).ClickAsync();

            // NewDevicePage navigates to /device/view/{id} via Blazor's client-side NavigateTo.
            // Wait on the destination content (auto-waiting) rather than WaitForURLAsync, which
            // doesn't reliably observe this SignalR-driven navigation even after the URL updates.
            // As the owner, the node-agent install section is shown with the command and copy button.
            await Assertions.Expect(Page.GetByText("Node Agent Installation")).ToBeVisibleAsync(new() { Timeout = 15000 });
            await Assertions.Expect(Page.Locator(".agent-command")).ToBeVisibleAsync();
            await Assertions.Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Copy" })).ToBeVisibleAsync();

            Page.Url.Should().Contain("/device/view/");

            MockStore.Devices.Should().Contain(d =>
                d.Name == "Alice Test Rig" && d.OwnerUsername == "alice");
        }
    }
}
