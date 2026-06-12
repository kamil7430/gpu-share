using E2ETests.Infrastructure;
using FluentAssertions;
using GpuShare.Frontend.Services;
using Microsoft.Playwright;

namespace E2ETests.Flows
{
    public class DeviceRegistrationTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
    {
        [Fact]
        public async Task Register_Device_Shows_Agent_Token()
        {
            await LoginAsAsync("alice");
            await Page.GotoAsync("/device/new");
            await WaitForBlazorInteractiveAsync();

            // EditDeviceForm labels aren't for-associated with their inputs, so target
            // the inputs directly. Device has no validation attributes — name + model is enough.
            await Page.GetByPlaceholder("e.g. My Beast Rig").FillAsync("Alice Test Rig");
            await Page.Locator("input[list='models']").FillAsync("NVIDIA RTX 5090");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Save Configuration" }).ClickAsync();

            // NewDevicePage navigates to the device view on success
            await Page.WaitForURLAsync("**/device/view/**");

            // The owner sees the node-agent install command with the per-device token
            await Assertions.Expect(Page.Locator(".agent-command")).ToContainTextAsync("mck_agt_");
            await Assertions.Expect(Page.Locator(".agent-command")).ToContainTextAsync("install.gpushare.io");

            MockStore.Devices.Should().Contain(d =>
                d.Name == "Alice Test Rig" && d.OwnerUsername == "alice");
        }
    }
}
