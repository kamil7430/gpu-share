using Bunit;
using GpuShare.Frontend.Components.Pages.Device;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using FluentAssertions;

namespace GpuShare.Frontend.Tests.Components.Device
{
    public class DeviceInfoCardTests : BunitContext, Xunit.IAsyncLifetime
    {
        private Mock<IAuthState> _authStateMock;

        public DeviceInfoCardTests()
        {
            _authStateMock = new Mock<IAuthState>();
            Services.AddAuthorizationCore();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            // important note: the JSInterop calls in the Device component are not being properly mocked, which causes the tests to fail. The following setup is an attempt to mock those calls, but it may need to be adjusted based on the actual JSInterop calls being made in the Device component.
            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        private Models.Device CreateGpu(bool isAvailable = true)
        {
            return new Models.Device
            {
                Id = 123,
                Name = "Workstation-Alpha",
                OwnerUsername = "julie",
                IsAvailable = isAvailable,
                Model = "RTX 4090",
                VramMb = 24576,
                CudaCores = 16384,
                DriverVersion = "535.xx",
                PricePerHour = 4.5m,
                Frameworks = ["CUDA", "PyTorch"]
            };
        }

            [Fact]
        public void Should_Render_Gpu_Info()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(false);

            var gpu = CreateGpu();

            // Act
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.gpu, gpu).Add(x => x.Username, "julie"));

            // Assert
            cut.Markup.Should().Contain("Workstation-Alpha");
            cut.Markup.Should().Contain("RTX 4090");

            cut.Markup.Should().Contain("24576");
            cut.Markup.Should().Contain("16384");

            cut.Markup.Should().Contain("535.xx");

            cut.Markup.Should().Contain("4,5");

            cut.Markup.Should().Contain("CUDA");
            cut.Markup.Should().Contain("PyTorch");
        }

        [Fact]
        public void Available_Gpu_Should_Show_Online_Banner()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(false);

            var gpu = CreateGpu(true);

            // Act
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.gpu, gpu));

            // Assert
            var banner = cut.Find(".status-banner");

            banner.TextContent.Should().Contain("Available for Rent");

            banner.ClassList.Should().Contain("online");
        }

        [Fact]
        public void Unavailable_Gpu_Should_Show_Offline_Banner()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(false);

            var gpu = CreateGpu(false);

            // Act
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.gpu, gpu));

            // Assert
            var banner = cut.Find(".status-banner");

            banner.TextContent.Should().Contain("Currently Disabled");

            banner.ClassList.Should().Contain("offline");
        }

        [Fact]
        public void Authenticated_User_Should_See_Edit_Button()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(true);

            var gpu = CreateGpu();

            // Act
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.gpu, gpu));

            // Assert
            cut.Markup.Should().Contain("Edit Device");
        }

        [Fact]
        public void Unauthorized_User_Should_Not_See_Edit_Button()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(false);

            var gpu = CreateGpu();

            // Act
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.gpu, gpu));

            // Assert
            cut.Markup.Should().NotContain("Edit Device");
        }

        [Fact]
        public void Unauthorized_User_Should_See_Profile_Link()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(false);

            var gpu = CreateGpu();

            // Act
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.gpu, gpu).Add(x => x.Username, "julie"));

            // Assert
            var profileLink = cut.Find(".profile-link");

            profileLink.TextContent.Should().Contain("julie");

            profileLink.GetAttribute("href").Should().Be("/profile/julie");
        }

        [Fact]
        public void Authenticated_User_Should_Not_See_Profile_Link()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(true);

            var gpu = CreateGpu();

            // Act
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.gpu, gpu).Add(x => x.Username, "julie"));

            // Assert
            cut.FindAll(".profile-link").Should().BeEmpty();
        }

        [Fact]
        public void Clicking_Edit_Button_Should_Change_Mode_To_Edit()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(true);

            var gpu = CreateGpu();

            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.gpu, gpu).Add(x => x.Mode, DevicePageMode.View));

            // Act
            cut.Find(".btn-edit").Click();

            // Assert
            cut.Instance.Mode.Should().Be(DevicePageMode.Edit);
        }
    }
}
