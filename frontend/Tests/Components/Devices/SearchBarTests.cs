using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Devices;
using GpuShare.Frontend.Components.Modals;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Xunit;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

namespace GpuShare.Frontend.Tests.Components.Devices
{
    public class SearchBarTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IAuthState> _authStateMock;
        private readonly Mock<IDeviceService> _deviceServiceMock;

        public SearchBarTests()
        {
            _authStateMock = new Mock<IAuthState>();
            _deviceServiceMock = new Mock<IDeviceService>();
            Services.AddAuthorizationCore();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddSingleton(_deviceServiceMock.Object);
            Services.AddMudServices();
            Services.AddBlazorise(options => { })
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);

            // Stub heavy components
            //ComponentFactories.AddStub<SearchBar>("SEARCH_BAR");

            _deviceServiceMock.Setup(s => s.GetDeviceAsync(It.IsAny<int>()))
                .ReturnsAsync(new Models.Device
                {
                    DeviceId = 123,
                    Name = "Workstation-Alpha",
                    OwnerUsername = "julie",
                    State = DeviceState.AVAILABLE
                });

            _deviceServiceMock.Setup(s => s.SearchDevicesAsync(It.IsAny<DeviceSearchFilters>()))
                .ReturnsAsync(new PagedResult<Models.Device>
                {
                    Items = [
                    new Models.Device
                    {
                        DeviceId = 123,
                        Name = "Workstation-Alpha",
                        OwnerUsername = "julie",
                        State = DeviceState.AVAILABLE
                    },
                    new Models.Device
                    {
                        DeviceId = 456,
                        Name = "RenderNode-01",
                        OwnerUsername = "mark",
                        State = DeviceState.UNAVAILABLE
                    }],
                    TotalCount = 2,
                    Page = 1,
                    PageSize = 10
                });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public void Should_Render_Search_Input_And_Buttons()
        {
            // Act
            var cut = Render<SearchBar>();

            // Assert
            cut.Find("input");

            cut.FindAll("button").Should().HaveCount(2);

            cut.Markup.Should().Contain("Filters");
            cut.Markup.Should().Contain("Search");
        }

        [Fact]
        public async Task Search_Should_Pass_Filter_Term()
        {
            // Arrange
            SearchFilter? received = null;

            var cut = Render<SearchBar>(p => p.Add(x => x.OnSearch, filter => received = filter));

            // Act
            cut.Find("input").Input("RTX 4090");

            cut.Find(".btn-search").Click();

            // Assert
            received.Should().NotBeNull();
            received!.Term.Should().Be("RTX 4090");
        }

        [Fact]
        public void Search_Button_Should_Invoke_OnSearch()
        {
            // Arrange
            var invoked = false;

            var cut = Render<SearchBar>(p => p.Add(x => x.OnSearch, _ => invoked = true));

            // Act
            cut.Find(".btn-search").Click();

            // Assert
            invoked.Should().BeTrue();
        }

        [Fact]
        public async Task Applying_Filters_Should_Render_Badges()
        {
            // Arrange
            var cut = Render<SearchBar>();

            var filter = new SearchFilter
            {
                VRAM_MB = ["24"],
                PricePerHour = new (1, 5),
                AvailableOnly = true,
            };

            // Act
            await cut.InvokeAsync(async () =>
            {
                await cut.Instance.ApplySearch(filter);
                return Task.CompletedTask;
            });

            cut.Render();

            // Assert
            cut.Markup.Should().Contain("VRAM:");
            cut.Markup.Should().Contain("24 GB");

            cut.Markup.Should().Contain("Price:");
            cut.Markup.Should().Contain("1 - 5");
        }

        [Fact]
        public async Task Empty_Filter_Should_Not_Render_Badges()
        {
            // Arrange
            var cut = Render<SearchBar>();

            var filter = new SearchFilter();

            // Act
            await cut.InvokeAsync(async () =>
            {
                await cut.Instance.ApplySearch(filter);
                return Task.CompletedTask;
            });

            cut.Render();

            // Assert
            cut.FindAll(".filter-badge").Should().BeEmpty();
        }

        [Fact]
        public async Task FilterModal_Should_Receive_Current_Filter()
        {
            // Arrange
            var cut = Render<SearchBar>();

            var filter = new SearchFilter
            {
                Term = "CUDA"
            };

            // Act
            await cut.InvokeAsync(async () =>
            {
                await cut.Instance.ApplySearch(filter);
                return Task.CompletedTask;
            });

            cut.Render();

            // Assert
            var modal = cut.FindComponent<FilterModal>();

            modal.Instance.CurrentFilter.Term.Should().Be("CUDA");
        }

        [Fact]
        public async Task AvailableOnly_Should_Render_Badge()
        {
            // Arrange
            var cut = Render<SearchBar>();

            var filter = new SearchFilter
            {
                AvailableOnly = true
            };

            // Act
            await cut.InvokeAsync(async () =>
            {
                await cut.Instance.ApplySearch(filter);
                return Task.CompletedTask;
            });

            cut.Render();

            // Assert
            cut.Markup.Should().Contain("Available Only");
        }

        [Fact]
        public async Task Sort_None_Should_Not_Render_Badge()
        {
            // Arrange
            var cut = Render<SearchBar>();

            var filter = new SearchFilter
            {
                SortBy = SortOption.None
            };

            // Act
            await cut.InvokeAsync(async () =>
            {
                await cut.Instance.ApplySearch(filter);
                return Task.CompletedTask;
            });

            cut.Render();

            // Assert
            cut.Markup.Should().NotContain("Sort:");
        }
    }
}
