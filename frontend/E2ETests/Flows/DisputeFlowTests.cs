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
            // Order 2 is COMPLETED — alice can open a dispute.
            // CanOpenDispute = (now - _startTime) > 5 min, where _startTime = now - 12 min → true.
            await Page.GotoAsync("/order/2");

            // Link text was changed to "Open dispute" in OrderPage; href is /order/2/dispute
            await Page.GetByRole(AriaRole.Link, new() { Name = "Open dispute" }).ClickAsync();

            // We're now on the dispute creation page (/order/2/dispute)
            await Page.WaitForURLAsync("**/dispute*");

            // Reason is a MudRadioGroup — select one of the available options
            await Page.GetByRole(AriaRole.Radio, new() { Name = "Other" }).CheckAsync();

            // Description field has Label="Description" in MudTextField
            await Page.GetByLabel("Description").FillAsync(
                "The session was terminated unexpectedly after 30 minutes of a planned 4-hour job.");

            await Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();

            // SubmitDispute navigates to /dispute/{newId}
            await Page.WaitForURLAsync("**/dispute/**");

            MockStore.Disputes.Should().Contain(d => d.Reason == "Other");
        }
    }
}
