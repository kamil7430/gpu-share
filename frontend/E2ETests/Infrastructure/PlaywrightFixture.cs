using Microsoft.Playwright;

/// <summary>
/// Starts the app once and reuses a single Playwright browser across the test session.
/// </summary>
namespace E2ETests.Infrastructure
{
    public class PlaywrightFixture : IAsyncLifetime
    {
        public MockAppFactory Factory { get; } = new();
        public IPlaywright Playwright { get; private set; } = null!;
        public IBrowser Browser { get; private set; } = null!;
        public string BaseUrl { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            // Start the in-process Blazor Server
            var client = Factory.CreateClient();
            BaseUrl = client.BaseAddress!.ToString().TrimEnd('/');

            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync(new()
            {
                Headless = true,                    // set false to watch tests run
            });
        }

        public async Task DisposeAsync()
        {
            await Browser.CloseAsync();
            Playwright.Dispose();
            await Factory.DisposeAsync();
        }

        /// <summary>Creates a fresh browser context (= isolated cookies/session) per test.</summary>
        public async Task<IPage> NewPageAsync()
        {
            var context = await Browser.NewContextAsync(new()
            {
                BaseURL = BaseUrl,
            });
            return await context.NewPageAsync();
        }

    }
}
