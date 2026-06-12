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
            await WaitForBlazorInteractiveAsync();

            var before = MockStore.Wallet.TotalUsdCents;

            await Page.GetByRole(AriaRole.Button, new() { Name = "Top up" }).ClickAsync();
            await Page.GetByLabel("Amount (USD)").FillAsync("50");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Confirm" }).ClickAsync();

            // The form closes only after the server processed the top-up; the click alone
            // returns before the SignalR round-trip, so wait before asserting MockStore.
            await Assertions.Expect(Page.GetByLabel("Amount (USD)")).ToHaveCountAsync(0);

            MockStore.Wallet.TotalUsdCents.Should().Be(before + 5000);
        }

        [Fact]
        public async Task Withdraw_Below_Available_Succeeds()
        {
            await LoginAsAsync("alice");
            await Page.GotoAsync("/profile/alice");
            await WaitForBlazorInteractiveAsync();

            // Available = 14000 - 9600 = 4400 cents
            await Page.GetByRole(AriaRole.Button, new() { Name = "Withdraw" }).ClickAsync();
            await Page.GetByLabel("Amount (USD)").FillAsync("10");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Confirm" }).ClickAsync();

            // Form closes only on success — wait for the server round-trip to finish
            await Assertions.Expect(Page.GetByLabel("Amount (USD)")).ToHaveCountAsync(0);

            var markup = await Page.ContentAsync();
            markup.Should().NotContain("Insufficient");
        }

        [Fact]
        public async Task Withdraw_Above_Available_Fails_With_Validation()
        {
            await LoginAsAsync("alice");
            await Page.GotoAsync("/profile/alice");
            await WaitForBlazorInteractiveAsync();

            var before = MockStore.Wallet.TotalUsdCents;

            // Available = 14000 - 9600 = 4400 cents; $100 exceeds it
            await Page.GetByRole(AriaRole.Button, new() { Name = "Withdraw" }).ClickAsync();
            await Page.GetByLabel("Amount (USD)").FillAsync("100");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Confirm" }).ClickAsync();

            // WalletCard surfaces the ApiException message in an error snackbar
            await Page.GetByText("Insufficient available balance").WaitForAsync();

            // Balance untouched and the form stays open — nothing was processed
            MockStore.Wallet.TotalUsdCents.Should().Be(before);
            await Assertions.Expect(Page.GetByLabel("Amount (USD)")).ToHaveCountAsync(1);
        }
    }
}
