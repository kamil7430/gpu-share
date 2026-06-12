using E2ETests.Infrastructure;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Services;
using Microsoft.Playwright;

namespace E2ETests.Flows
{
    public class AdminDashboardTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
    {
        /// <summary>The value rendered in the stat box whose label matches.</summary>
        private ILocator StatValue(string label) =>
            Page.Locator(".spec-box", new() { HasText = label }).Locator(".value");

        [Fact]
        public async Task Admin_Sees_Platform_Stats()
        {
            await LoginAsAsync("diana");

            // The Admin nav entry only renders for admin users
            await Page.GetByRole(AriaRole.Link, new() { Name = "Admin" }).ClickAsync();
            await WaitForBlazorInteractiveAsync();

            // Expected values computed from live MockStore state, mirroring
            // MockAdminService — with the default seed: 1 running order, 4 users,
            // 5 devices, 2 open/under-review disputes, $0 settled revenue.
            var activeSessions = MockStore.Orders.Count(o => o.Status == OrderStatus.RUNNING);
            var openDisputes = MockStore.Disputes.Count(
                d => d.Status is DisputeStatus.OPEN or DisputeStatus.UNDER_REVIEW);
            var revenue = MockStore.Transactions
                .Where(t => t.Type == TransactionType.SETTLEMENT && t.Status == TransactionStatus.COMPLETED)
                .Sum(t => (decimal)t.AmountUsdCents) / 100m;

            // Expect() auto-waits, covering the async stats load after navigation
            await Assertions.Expect(StatValue("Active Sessions")).ToHaveTextAsync(activeSessions.ToString());
            await Assertions.Expect(StatValue("Registered Users")).ToHaveTextAsync(MockStore.Users.Count.ToString());
            await Assertions.Expect(StatValue("Devices in Catalog")).ToHaveTextAsync(MockStore.Devices.Count.ToString());
            await Assertions.Expect(StatValue("Open Disputes")).ToHaveTextAsync(openDisputes.ToString());
            await Assertions.Expect(StatValue("Total Revenue")).ToHaveTextAsync($"${revenue:0.00}");
        }

        [Fact]
        public async Task NonAdmin_Gets_No_Nav_Entry_And_Is_Denied()
        {
            await LoginAsAsync("alice");

            // Regular users get no Admin entry in the nav...
            await Assertions.Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Admin" })).ToHaveCountAsync(0);

            // ...and navigating to the page directly shows the unauthorized message
            await Page.GotoAsync("/admin");
            await WaitForBlazorInteractiveAsync();

            await Page.GetByText("not authorized").WaitForAsync();
            await Assertions.Expect(Page.Locator(".spec-box")).ToHaveCountAsync(0);
        }
    }
}
