using E2ETests.Infrastructure;
using FluentAssertions;
using GpuShare.Frontend.Services;
using Microsoft.Playwright;

namespace E2ETests.Flows
{
    public class RegisterFlowTests(PlaywrightFixture fixture) : E2ETestBase(fixture)
    {
        private async Task OpenRegisterFormAsync()
        {
            await Page.GotoAsync("/");
            await WaitForBlazorInteractiveAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
            // The modal opens in Login mode; switch to Register
            await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        }

        private async Task FillRegisterFormAsync(string username)
        {
            await Page.GetByPlaceholder("Username").FillAsync(username);
            // "Password" placeholder is a substring of "Confirm Password" — match exactly
            await Page.GetByPlaceholder("Password", new() { Exact = true }).FillAsync("Password1");
            await Page.GetByPlaceholder("Confirm Password").FillAsync("Password1");
            // The native input is visually hidden behind the styled checkmark span;
            // clicking the span toggles the wrapped checkbox like a real user would.
            await Page.Locator(".custom-checkbox .checkmark").ClickAsync();
        }

        [Fact]
        public async Task Register_New_Account_Succeeds()
        {
            await OpenRegisterFormAsync();
            // Username must be >= 4 chars (LoginModal validation) and not already seeded
            await FillRegisterFormAsync("evelyn");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();

            // Success path closes the modal and shows an info snackbar
            await Page.GetByText("Successfully registered").WaitForAsync();

            MockStore.Users.Should().Contain(u => u.Username == "evelyn");
        }

        [Fact]
        public async Task Register_Duplicate_Username_Shows_Error()
        {
            var usersBefore = MockStore.Users.Count;

            await OpenRegisterFormAsync();
            // "charlie" is seeded in MockStore and satisfies the >= 4 char username rule
            await FillRegisterFormAsync("charlie");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Create Account" }).ClickAsync();

            // MockAuthService throws Conflict; the UI surfaces an error snackbar
            await Page.GetByText("Registration failed").WaitForAsync();

            MockStore.Users.Count.Should().Be(usersBefore);
            MockStore.Users.Count(u => u.Username == "charlie").Should().Be(1);
        }
    }
}
