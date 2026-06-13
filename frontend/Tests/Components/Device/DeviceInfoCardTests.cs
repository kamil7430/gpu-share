using Bunit;
using GpuShare.Frontend.Components.Pages.Device;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using FluentAssertions;
using MudBlazor;
using GpuShare.Frontend.Models.Dtos;

namespace GpuShare.Frontend.Tests.Components.Device
{
    public class DeviceInfoCardTests : BunitContext, Xunit.IAsyncLifetime
    {
        private readonly Mock<IAuthState> _authStateMock = new();
        private readonly Mock<IFormatters> _formattersMock = new();
        private readonly Mock<IDeviceService> _deviceServiceMock = new();

        public DeviceInfoCardTests()
        {
            Services.AddAuthorizationCore();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddSingleton(_formattersMock.Object);
            Services.AddSingleton(_deviceServiceMock.Object);
            Services.AddSingleton(new Mock<IAppNotifier>().Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);

            Render<MudPopoverProvider>();

            _deviceServiceMock.Setup(x => x.GetAgentInstallInfoAsync(123))
                .ReturnsAsync(new DeviceAgentInfo
                {
                    InstallScriptUrl = "https://gpu-share.io/install.sh",
                    AgentToken = "abc123"
                });
        }

        private void SetupAuthenticatedUser(string username)
        {
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(true);
            _authStateMock.SetupGet(x => x.User).Returns(new User { Username = username });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        private static Models.Device CreateGpu(bool isAvailable = true)
        {
            return new Models.Device
            {
                DeviceId = 123,
                Name = "Workstation-Alpha",
                OwnerUsername = "julie",
                State = isAvailable ? DeviceState.AVAILABLE : DeviceState.UNAVAILABLE,
                GpuModel = "RTX 4090",
                VramMb = 24576,
                CudaCores = 16384,
                DriverVersion = "535.xx",
                PricePerHourUsdCents = 450,
            };
        }

            [Fact]
        public void Should_Render_Gpu_Info()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(false);
            _formattersMock.Setup(x => x.FormatUsd(450)).Returns("4,50");

            var gpu = CreateGpu();

            // Act
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.Device, gpu).Add(x => x.Username, "julie"));

            // Assert
            cut.Markup.Should().Contain("Workstation-Alpha");
            cut.Markup.Should().Contain("RTX 4090");

            cut.Markup.Should().Contain("24576");
            cut.Markup.Should().Contain("16384");

            cut.Markup.Should().Contain("535.xx");

            cut.Markup.Should().Contain("4,50");

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
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.Device, gpu));

            // Assert
            var banner = cut.Find(".status-banner");

            banner.TextContent.Should().Contain("Available");

            banner.ClassList.Should().Contain("online");
        }

        [Fact]
        public void Unavailable_Gpu_Should_Show_Offline_Banner()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(false);

            var gpu = CreateGpu(false);

            // Act
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.Device, gpu));

            // Assert
            var banner = cut.Find(".status-banner");

            banner.TextContent.Should().Contain("Unavailable");

            banner.ClassList.Should().Contain("offline");
        }

        [Fact]
        public void Authenticated_User_Should_See_Edit_Button()
        {
            // Arrange
            SetupAuthenticatedUser("julie");
            var gpu = CreateGpu();

            // Act
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.Device, gpu));

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
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.Device, gpu));

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
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.Device, gpu).Add(x => x.Username, "julie"));

            // Assert
            var profileLink = cut.Find(".profile-link");

            profileLink.TextContent.Should().Contain("julie");

            profileLink.GetAttribute("href").Should().Be("/profile/julie");
        }

        [Fact]
        public void Authenticated_User_Should_Not_See_Profile_Link()
        {
            // Arrange
            SetupAuthenticatedUser("julie");
            var gpu = CreateGpu();

            // Act
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.Device, gpu).Add(x => x.Username, "julie"));

            // Assert
            cut.FindAll(".profile-link").Should().BeEmpty();
        }

        [Fact]
        public void Clicking_Edit_Button_Should_Change_Mode_To_Edit()
        {
            // Arrange
            SetupAuthenticatedUser("julie");
            var gpu = CreateGpu();

            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.Device, gpu).Add(x => x.Mode, DevicePageMode.View));

            // Act
            cut.Find(".btn-edit").Click();

            // Assert
            cut.Instance.Mode.Should().Be(DevicePageMode.Edit);
        }

        [Fact]
        public void Copy_Button_Should_Invoke_Clipboard_JS()
        {
            JSInterop.SetupVoid("navigator.clipboard.writeText");
            SetupAuthenticatedUser("julie");
            var gpu = CreateGpu();
            
            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.Device, gpu)
            .Add(x => x.Mode, DevicePageMode.View));

            cut.Find(".agent-command-container button").Click();

            JSInterop.VerifyInvoke("navigator.clipboard.writeText").Arguments[0]!.ToString()
                .Should().Contain("https://gpu-share.io/install.sh");
        }

        [Fact]
        public void Should_Not_Show_Agent_Section_When_User_Is_Not_Owner()
        {
            // Arrange
            SetupAuthenticatedUser("other-user");

            var gpu = CreateGpu();

            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.Device, gpu)
                .Add(x => x.Mode, DevicePageMode.View));

            // Assert
            cut.Markup.Should().NotContain("Node Agent Installation");
        }

        [Fact]
        public void Should_Show_Agent_Section_When_User_Is_Owner()
        {
            // Arrange
            SetupAuthenticatedUser("julie");

            var gpu = CreateGpu();

            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.Device, gpu)
                .Add(x => x.Mode, DevicePageMode.View));

            // Assert
            cut.Markup.Should().Contain("Node Agent Installation");
        }

        [Fact]
        public async Task Should_Call_GetAgentInstallInfo_On_Render()
        {
            // Arrange
            SetupAuthenticatedUser("julie");

            var gpu = CreateGpu();

            _deviceServiceMock.Setup(x => x.GetAgentInstallInfoAsync(123))
                .ReturnsAsync(new DeviceAgentInfo
                {
                    InstallScriptUrl = "https://gpu-share.io/install.sh",
                    AgentToken = "abc123"
                });

            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.Device, gpu)
                .Add(x => x.Mode, DevicePageMode.View));

            // Allow async lifecycle to complete
            await cut.InvokeAsync(() => Task.CompletedTask);

            _deviceServiceMock.Verify(x => x.GetAgentInstallInfoAsync(123), Times.Once);
        }

        [Fact]
        public void Should_Render_Agent_Token()
        {
            SetupAuthenticatedUser("julie");

            var gpu = CreateGpu();

            _deviceServiceMock.Setup(x => x.GetAgentInstallInfoAsync(123))
                .ReturnsAsync(new DeviceAgentInfo
                {
                    InstallScriptUrl = "https://gpu-share.io/install.sh",
                    AgentToken = "gpu_test_token_123"
                });

            var cut = Render<DeviceInfoCard>(p => p.Add(x => x.Device, gpu)
                .Add(x => x.Mode, DevicePageMode.View));

            cut.Markup.Should().Contain("gpu_test_token_123");
        }
    }
}
