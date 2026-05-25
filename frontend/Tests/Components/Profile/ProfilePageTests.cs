using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Profile;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GpuShare.Frontend.Tests.Components.Profile
{
    public class ProfilePageTests : BunitContext, Xunit.IAsyncLifetime
    {
        private Mock<IAuthState> _authStateMock;

        public ProfilePageTests()
        {
            _authStateMock = new Mock<IAuthState>();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true).SetVoidResult();
            JSInterop.SetupModule(_ => true);

            ComponentFactories.AddStub<WalletCard>("WALLET_STUB");
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public void Authorized_User_Should_See_Orders()
        {
            // Arrange
            _authStateMock.SetupGet(a => a.IsAuthenticated).Returns(true);
            _authStateMock.SetupGet(a => a.User).Returns(new User("john"));

            // Act
            var popoverProvider = Render<MudPopoverProvider>();
            var cut = Render<ProfilePage>(p => p.Add(x => x.Username, "john"));

            // Assert
            cut.Markup.Should().Contain("My Orders");
        }

        [Fact]
        public void Unauthorized_User_Should_Not_See_Orders()
        {
            // Arrange
            _authStateMock.SetupGet(a => a.IsAuthenticated).Returns(false);
            _authStateMock.SetupGet(a => a.User).Returns((User?)null);

            // Act
            var popoverProvider = Render<MudPopoverProvider>();
            var cut = Render<ProfilePage>(p => p.Add(x => x.Username, "john"));

            // Assert
            cut.Markup.Should().NotContain("My Orders");
        }

        [Fact]
        public void Authorized_User_Should_See_Wallet()
        {
            // Arrange
            _authStateMock.SetupGet(a => a.IsAuthenticated).Returns(true);
            _authStateMock.SetupGet(a => a.User).Returns(new User("john"));

            // Act
            var popoverProvider = Render<MudPopoverProvider>();
            var cut = Render<ProfilePage>(p => p.Add(x => x.Username, "john"));

            // Assert
            cut.Markup.Should().Contain("WALLET_STUB");
        }

        [Fact]
        public void Unauthorized_User_Should_Not_See_Wallet()
        {
            // Arrange
            _authStateMock.SetupGet(a => a.IsAuthenticated).Returns(false);
            _authStateMock.SetupGet(a => a.User).Returns((User?)null);

            // Act
            var popoverProvider = Render<MudPopoverProvider>();
            var cut = Render<ProfilePage>(p => p.Add(x => x.Username, "john"));

            // Assert
            cut.Markup.Should().NotContain("WALLET_STUB");
        }

        [Fact]
        public void Authorized_User_Should_See_Add_Gpu_Button()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(true);
            _authStateMock.SetupGet(a => a.User).Returns(new User("john"));

            // Act
            var popoverProvider = Render<MudPopoverProvider>();
            var cut = Render<ProfilePage>(p => p.Add(x => x.Username, "john"));

            // Assert
            cut.Markup.Should().Contain("Add GPU");
        }

        [Fact]
        public void Unauthorized_User_Should_Not_See_Add_Gpu_Button()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(false);
            _authStateMock.SetupGet(a => a.User).Returns((User?)null);

            // Act
            var popoverProvider = Render<MudPopoverProvider>();
            var cut = Render<ProfilePage>(p => p.Add(x => x.Username, "john"));

            // Assert
            cut.Markup.Should().NotContain("Add GPU");
        }

        [Fact]
        public void Authorized_User_Should_See_My_Gpus_Header()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(true);
            _authStateMock.SetupGet(a => a.User).Returns(new User("john"));

            // Act
            var popoverProvider = Render<MudPopoverProvider>();
            var cut = Render<ProfilePage>(p => p.Add(x => x.Username, "john"));

            // Assert
            cut.Markup.Should().Contain("My GPUs");
        }

        [Fact]
        public void Unauthorized_User_Should_See_Username_Gpus_Header()
        {
            // Arrange
            _authStateMock.SetupGet(x => x.IsAuthenticated).Returns(false);
            _authStateMock.SetupGet(a => a.User).Returns((User?)null);

            // Act
            var popoverProvider = Render<MudPopoverProvider>();
            var cut = Render<ProfilePage>(p => p.Add(x => x.Username, "john"));

            // Assert
            cut.Markup.Should().Contain("john's GPUs");
        }
    }
}
