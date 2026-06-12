using Microsoft.Playwright;

namespace E2ETests.Infrastructure;

/// <summary>
/// Starts the real Kestrel server once and reuses a single Playwright browser
/// across the entire test session.
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    public MockAppFactory Factory { get; } = new();
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public string BaseUrl { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Start a real Kestrel server with all services swapped to mocks
        await Factory.StartAsync();
        BaseUrl = Factory.ServerUrl;

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,  // set to false to watch tests run in a real browser
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.CloseAsync();
        Playwright.Dispose();
        await Factory.DisposeAsync();
    }

    /// <summary>Creates a fresh isolated browser context (own cookies/session) per test.</summary>
    public async Task<IPage> NewPageAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
        });
        return await context.NewPageAsync();
    }
}
