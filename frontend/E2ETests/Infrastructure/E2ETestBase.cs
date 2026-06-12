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

        /// <summary>Browser console errors, page crashes and failed/4xx+ requests collected during the test.</summary>
        protected readonly List<string> BrowserLogs = new();

        protected E2ETestBase(PlaywrightFixture fixture)
        {
            Fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            MockStore.Reset();           // see §5 — add Reset() to MockStore
            Page = await Fixture.NewPageAsync();

            Page.Console += (_, msg) =>
            {
                if (msg.Type == "error")
                    BrowserLogs.Add($"[console.{msg.Type}] {msg.Text}");
            };
            Page.PageError += (_, err) => BrowserLogs.Add($"[pageerror] {err}");
            Page.RequestFailed += (_, req) => BrowserLogs.Add($"[requestfailed] {req.Method} {req.Url} — {req.Failure}");
            Page.Response += (_, resp) =>
            {
                if (resp.Status >= 400)
                    BrowserLogs.Add($"[http {resp.Status}] {resp.Url}");
            };
        }

        public async Task DisposeAsync()
        {
            await Page.Context.CloseAsync();
        }

        // ── helpers ──────────────────────────────────────────────────────

        /// <summary>
        /// Waits for the Blazor Server circuit to connect and complete its first interactive render.
        /// OnAfterRenderAsync never fires during SSR prerender — the sentinel only appears once the
        /// circuit is live and event handlers are wired up.
        /// </summary>
        protected async Task WaitForBlazorInteractiveAsync()
        {
            try
            {
                // State.Attached = element exists in DOM regardless of visibility.
                // The sentinel has display:none so Visible (the default) would always time out.
                await Page.WaitForSelectorAsync("#blazor-ready",
                    new PageWaitForSelectorOptions
                    {
                        State = WaitForSelectorState.Attached,
                        Timeout = 15000
                    });
            }
            catch (TimeoutException ex)
            {
                var logs = BrowserLogs.Count > 0
                    ? string.Join(Environment.NewLine, BrowserLogs)
                    : "(no browser errors captured)";
                throw new TimeoutException(
                    $"Blazor circuit never became interactive (#blazor-ready not found).{Environment.NewLine}" +
                    $"Browser logs:{Environment.NewLine}{logs}", ex);
            }
        }

        protected async Task LoginAsAsync(string username)
        {
            await Page.GotoAsync("/");
            // The Login button is inert until the circuit connects — clicking earlier is a no-op.
            await WaitForBlazorInteractiveAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
            // The modal uses placeholders, not <label> elements
            await Page.GetByPlaceholder("Username").FillAsync(username);
            await Page.GetByPlaceholder("Password").FillAsync("anypassword");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
            // TopNav swaps "Login" → "Logout" after auth; no page navigation occurs
            await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).WaitForAsync();
        }
    }

    [CollectionDefinition("E2E")]
    public class E2ECollection : ICollectionFixture<PlaywrightFixture> { }
}
