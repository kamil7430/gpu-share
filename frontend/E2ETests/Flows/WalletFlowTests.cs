using E2ETests.Infrastructure;
using FluentAssertions;
using GpuShare.Frontend.Services;
using Microsoft.Playwright;

namespace E2ETests.Flows
{
    public class WalletFlowTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
    {
        [Fact]
        public async Task TopUp_Increases_Balance()
        {
            await LoginAsAsync("alice");
            await Page.GotoAsync("/profile/alice");

            var before = MockStore.Wallet.TotalUsdCents;

            await Page.GetByRole(AriaRole.Button, new() { Name = "Top up" }).ClickAsync();
            await Page.GetByLabel("Amount (USD)").FillAsync("50");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Confirm" }).ClickAsync();

            MockStore.Wallet.TotalUsdCents.Should().Be(before + 5000);
        }

        [Fact]
        public async Task Withdraw_Below_Available_Succeeds()
        {
            await LoginAsAsync("alice");
            await Page.GotoAsync("/profile/alice");

            // Available = 14000 - 9600 = 4400 cents
            await Page.GetByRole(AriaRole.Button, new() { Name = "Withdraw" }).ClickAsync();
            await Page.GetByLabel("Amount (USD)").FillAsync("10");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Confirm" }).ClickAsync();

            var markup = await Page.ContentAsync();
            markup.Should().NotContain("Insufficient");
        }
    }
}
