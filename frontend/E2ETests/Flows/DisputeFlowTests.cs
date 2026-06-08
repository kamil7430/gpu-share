using E2ETests.Infrastructure;
using FluentAssertions;
using GpuShare.Frontend.Services;
using Microsoft.Playwright;

namespace E2ETests.Flows
{
    public class DisputeFlowTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
    {
        [Fact]
        public async Task Open_Dispute_Appears_In_Disputes_List()
        {
            await LoginAsAsync("alice");
            await Page.GotoAsync("/order/2");            // completed order

            await Page.GetByRole(AriaRole.Button, new() { Name = "Open dispute" }).ClickAsync();
            await Page.GetByLabel("Reason").FillAsync("Connection dropped");
            await Page.GetByLabel("Description").FillAsync(
                "The session was terminated unexpectedly after 30 minutes of a planned 4-hour job.");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();

            await Page.WaitForURLAsync("**/dispute/**");

            MockStore.Disputes.Should().Contain(d => d.Reason == "Connection dropped");
        }
    }
}
