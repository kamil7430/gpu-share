using E2ETests.Infrastructure;
using FluentAssertions;
using GpuShare.Frontend.Services;
using Microsoft.Playwright;

namespace E2ETests.Flows
{
    public class ReviewFlowTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
    {
        [Fact]
        public async Task Leave_Review_On_Completed_Order_Is_Saved()
        {
            await LoginAsAsync("alice");
            await Page.GotoAsync("/profile/alice");
            await WaitForBlazorInteractiveAsync();

            // Of alice's seeded orders only #2 is COMPLETED, so exactly one review button
            await Page.GetByRole(AriaRole.Button, new() { Name = "Leave Review" }).ClickAsync();

            // Rating defaults to 5; pick 4 stars via the MudRating control
            await Page.Locator(".mud-rating-item").Nth(3).ClickAsync();
            await Page.GetByPlaceholder("Describe your experience").FillAsync(
                "Great experience, the GPU ran my training job without any hiccups.");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Submit Review" }).ClickAsync();

            // Success snackbar appears only after the server processed the review
            await Page.GetByText("Review created successfully").WaitForAsync();

            MockStore.Reviews.Should().Contain(r =>
                r.OrderId == 2 &&
                r.AuthorUsername == "alice" &&
                r.Rating == 4 &&
                r.Comment.Contains("Great experience"));
        }
    }
}
