using Bunit;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Devices;
using GpuShare.Frontend.Components.Shared;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Xunit;

namespace GpuShare.Frontend.Tests.Components.Devices
{
    public class DevicesPageTests : BunitContext, Xunit.IAsyncLifetime
    {
        private Mock<IAuthState> _authStateMock;
        private Mock<IDeviceService> _deviceServiceMock;

        public DevicesPageTests()
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
        public void Should_Render_SearchBar()
        {
            // Act
            var cut = Render<DevicesPage>();

            // Assert
            cut.FindComponent<SearchBar>();
        }

        [Fact]
        public void Should_Render_GpuList()
        {
            // Act
            var cut = Render<DevicesPage>();

            // Assert
            cut.FindComponent<GpuList>();
        }

        [Fact]
        public void ApplySearch_Should_Invoke_OnSearch()
        {
            // Arrange
            SearchFilter? receivedFilter = null;

            // Act
            var cut = Render<DevicesPage>(p => p.Add(x => x.OnSearch, (SearchFilter f) => receivedFilter = f));

            cut.InvokeAsync(() => cut.Instance.ApplySearch());

            // Assert
            receivedFilter.Should().NotBeNull();
        }

        [Fact]
        public void ApplySearch_Should_Pass_Current_Filter()
        {
            // Arrange
            SearchFilter? received = null;

            var cut = Render<DevicesPage>(p => p.Add(x => x.OnSearch, (SearchFilter f) => received = f));

            var filter = new SearchFilter
            {
                Term = "RTX 4090",
                AvailableOnly = true
            };

            // Act
            cut.InvokeAsync(() =>
            {
                cut.Instance.Filter = filter;
                return cut.Instance.ApplySearch();
            });

            // Assert
            received.Should().NotBeNull();
            received!.Term.Should().Be("RTX 4090");
            received.AvailableOnly.Should().BeTrue();
        }

        [Fact]
        public void OnFiltersApplied_Should_Update_Filter()
        {
            // Arrange
            var cut = Render<DevicesPage>();

            var updated = new SearchFilter
            {
                Term = "CUDA",
                AvailableOnly = true
            };

            // Act
            cut.InvokeAsync(() =>
            {
                cut.Instance.OnFiltersApplied(updated);
                return Task.CompletedTask;
            });

            // Assert
            cut.Instance.Filter.Term.Should().Be("CUDA");
            cut.Instance.Filter.AvailableOnly.Should().BeTrue();
        }
        
        [Fact]
        public async Task Search_Should_Load_Filtered_Devices()
        {

        }

        [Fact]
        public async Task Search_Should_Call_Device_Service()
        {

        }

        [Fact]
        public async Task Failed_Search_Should_Show_Error()
        {

        }
    }
}
