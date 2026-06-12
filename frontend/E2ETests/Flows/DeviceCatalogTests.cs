using E2ETests.Infrastructure;
using FluentAssertions;
using Microsoft.Playwright;

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
            await Page.GetByPlaceholder("Search").FillAsync("A100");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();

            await Page.WaitForSelectorAsync(".gpu-card");
            var markup = await Page.ContentAsync();
            markup.Should().Contain("A100 Server Node");
            markup.Should().NotContain("RTX 4090");
        }

        [Fact]
        public async Task Click_Device_Card_Navigates_To_Device_Page()
        {
            await Page.GotoAsync("/");
            await Page.WaitForSelectorAsync(".gpu-card");

            await Page.Locator(".gpu-card").First.ClickAsync();
            await Page.WaitForURLAsync("**/device/**");

            (await Page.TitleAsync()).Should().NotBeNullOrEmpty();
        }
    }
}
