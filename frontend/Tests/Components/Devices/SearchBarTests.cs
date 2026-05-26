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

namespace GpuShare.Frontend.Tests.Components.Devices
{
    public class SearchBarTests : BunitContext, Xunit.IAsyncLifetime
    {
        private Mock<IAuthState> _authStateMock;
        private Mock<IDeviceService> _deviceServiceMock;

        public SearchBarTests()
        {
            _authStateMock = new Mock<IAuthState>();
            _deviceServiceMock = new Mock<IDeviceService>();
            Services.AddAuthorizationCore();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddSingleton(_deviceServiceMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);

            // Stub heavy components
            //ComponentFactories.AddStub<SearchBar>("SEARCH_BAR");

            _deviceServiceMock.Setup(s => s.GetDeviceAsync(It.IsAny<int>()))
                .ReturnsAsync(new Models.Device
                {
                    Id = 123,
                    Name = "Workstation-Alpha",
                    OwnerUsername = "julie",
                    IsAvailable = true
                });

            _deviceServiceMock.Setup(s => s.SearchDevicesAsync(It.IsAny<DeviceSearchFilters>()))
                .ReturnsAsync(new PagedResult<Models.Device>
                {
                    Items = [
                    new Models.Device
                    {
                        Id = 123,
                        Name = "Workstation-Alpha",
                        OwnerUsername = "julie",
                        IsAvailable = true
                    },
                    new Models.Device
                    {
                        Id = 456,
                        Name = "RenderNode-01",
                        OwnerUsername = "mark",
                        IsAvailable = false
                    }],
                    TotalCount = 2,
                    Page = 1,
                    PageSize = 10
                });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
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

            cut.FindAll("button")
                .Should()
                .HaveCount(2);

            cut.Markup.Should().Contain("Filters");
            cut.Markup.Should().Contain("Search");
        }

        [Fact]
        public void Search_Should_Pass_Filter_Term()
        {
            // Arrange
            SearchFilter? received = null;

            var cut = Render<SearchBar>(p => p
                .Add(x => x.OnSearch,
                    (SearchFilter filter) => received = filter));

            // Act
            cut.Find("input")
                .Input("RTX 4090");

            cut.Find(".btn-search")
                .Click();

            // Assert
            received.Should().NotBeNull();
            received!.Term.Should().Be("RTX 4090");
        }

        [Fact]
        public void Search_Button_Should_Invoke_OnSearch()
        {
            // Arrange
            var invoked = false;

            var cut = Render<SearchBar>(p => p
                .Add(x => x.OnSearch,
                    (SearchFilter _) => invoked = true));

            // Act
            cut.Find(".btn-search")
                .Click();

            // Assert
            invoked.Should().BeTrue();
        }

        [Fact]
        public void Applying_Filters_Should_Render_Badges()
        {
            // Arrange
            var cut = Render<SearchBar>();

            var filter = new SearchFilter
            {
                VRAM_MB = ["24"],
                PricePerHour = new (1, 5),
                AvailableOnly = true,
                SupportedFrameworks = ["CUDA", "PyTorch"]
            };

            // Act
            cut.InvokeAsync(() =>
            {
                cut.Instance.OnFiltersApplied(filter);
                return Task.CompletedTask;
            });

            // Assert
            cut.Markup.Should().Contain("VRAM:");
            cut.Markup.Should().Contain("24 GB");

            cut.Markup.Should().Contain("Price:");
            cut.Markup.Should().Contain("1 - 5");

            cut.Markup.Should().Contain("Available Only");

            cut.Markup.Should().Contain("Frameworks:");
            cut.Markup.Should().Contain("CUDA");
            cut.Markup.Should().Contain("PyTorch");
        }

        [Fact]
        public void Empty_Filter_Should_Not_Render_Badges()
        {
            // Arrange
            var cut = Render<SearchBar>();

            // Assert
            cut.FindAll(".filter-badge")
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void FilterModal_Should_Receive_Current_Filter()
        {
            // Arrange
            var cut = Render<SearchBar>();

            var filter = new SearchFilter
            {
                Term = "CUDA"
            };

            // Act
            cut.InvokeAsync(() =>
            {
                cut.Instance.OnFiltersApplied(filter);
                return Task.CompletedTask;
            });

            // Assert
            var modal = cut.FindComponent<FilterModal>();

            modal.Instance.CurrentFilter.Term
                .Should()
                .Be("CUDA");
        }

        [Fact]
        public void AvailableOnly_Should_Render_Badge()
        {
            // Arrange
            var cut = Render<SearchBar>();

            var filter = new SearchFilter
            {
                AvailableOnly = true
            };

            // Act
            cut.InvokeAsync(() =>
            {
                cut.Instance.OnFiltersApplied(filter);
                return Task.CompletedTask;
            });

            // Assert
            cut.Markup.Should().Contain("Available Only");
        }

        [Fact]
        public void Sort_None_Should_Not_Render_Badge()
        {
            // Arrange
            var cut = Render<SearchBar>();

            var filter = new SearchFilter
            {
                SortBy = SortOption.None
            };

            // Act
            cut.InvokeAsync(() =>
            {
                cut.Instance.OnFiltersApplied(filter);
                return Task.CompletedTask;
            });

            // Assert
            cut.Markup.Should().NotContain("Sort:");
        }
    }
}
