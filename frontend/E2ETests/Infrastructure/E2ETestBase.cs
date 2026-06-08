using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services;
using Microsoft.Playwright;

namespace E2ETests.Infrastructure
{
    /// <summary>
    /// Resets MockStore before every test so mutations don't bleed across tests.
    /// </summary>
    [Collection("E2E")]          // serialises tests; Playwright + Blazor Server don't like parallelism
    public abstract class E2ETestBase : IAsyncLifetime
    {
        protected readonly PlaywrightFixture Fixture;
        protected IPage Page = null!;

        protected E2ETestBase(PlaywrightFixture fixture)
        {
            Fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            MockStore.Reset();           // see §5 — add Reset() to MockStore
            Page = await Fixture.NewPageAsync();
        }

        public async Task DisposeAsync()
        {
            await Page.Context.CloseAsync();
        }

        // ── helpers ──────────────────────────────────────────────────────
        protected async Task LoginAsAsync(string username)
        {
            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
            await Page.GetByLabel("Username").FillAsync(username);
            await Page.GetByLabel("Password").FillAsync("anypassword");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();
            await Page.WaitForURLAsync("**/profile/**");
        }
    }

    [CollectionDefinition("E2E")]
    public class E2ECollection : ICollectionFixture<PlaywrightFixture> { }
}
