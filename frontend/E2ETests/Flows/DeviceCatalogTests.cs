using E2ETests.Infrastructure;
using FluentAssertions;
using Microsoft.Playwright;
using System.Reflection.PortableExecutable;

namespace E2ETests.Flows
{
    public class DeviceCatalogTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
    {
        [Fact]
        public async Task Browse_Catalog_Shows_Seeded_Devices()
        {
            await Page.GotoAsync("/");

            await Page.WaitForSelectorAsync(".gpu-card");
            var cards = await Page.QuerySelectorAllAsync(".gpu-card");

            cards.Should().HaveCountGreaterThanOrEqualTo(4);   // matches MockStore seed
        }

        [Fact]
        public async Task Search_By_Name_Filters_Results()
        {
            await Page.GotoAsync("/");
            // Wait for the Blazor circuit to connect before interacting — SSR prerender shows cards
            // immediately but @onclick/@bind are inert until the SignalR circuit is established.
            await WaitForBlazorInteractiveAsync();

            await Page.GetByPlaceholder("Search...").FillAsync("A100");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();

            // Wait until the list contracts to the single matching card
            await Assertions.Expect(Page.Locator(".gpu-card")).ToHaveCountAsync(1, new() { Timeout = 10000 });
            var cards = await Page.WaitForSelectorAsync(".results-container");
            (await cards!.InnerHTMLAsync()).Should().Contain("A100 Server Node");
            (await cards!.InnerHTMLAsync()).Should().NotContain("RTX 4090");
        }

        [Fact]
        public async Task Click_Device_Card_Navigates_To_Device_Page()
        {
            await Page.GotoAsync("/");
            await Page.WaitForSelectorAsync(".gpu-card");

            // .gpu-card is a plain <div>; navigation lives on the .device-name NavLink inside it
            await Page.Locator(".gpu-card .device-name").First.ClickAsync();
            await Page.WaitForURLAsync("**/device/**");

            Page.Url.Should().Contain("/device/");

            var header = await Page.WaitForSelectorAsync(".gpu-header");
            // var markup = await Page.ContentAsync();
            (await header!.InnerHTMLAsync()).Should().Contain("RTX 4090 Workstation");
            // markup.Should().NotContain("RTX 4090");
        }
    }
}
