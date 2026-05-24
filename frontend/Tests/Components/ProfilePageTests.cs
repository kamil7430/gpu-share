using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Profile;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GpuShare.Frontend.Tests.Components
{
    public class ProfilePageTests : BunitContext
    {
        private Mock<IAuthState> _authStateMock;

        public ProfilePageTests()
        {
            _authStateMock = new Mock<IAuthState>();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true);
            JSInterop.SetupModule(_ => true);
        }

        [Fact]
        public void Authorized_User_Should_See_Orders()
        {
            // Arrange
            //var authContext = AddAuthorization();
            //authContext.SetAuthorized("john");
            _authStateMock.SetupGet(a => a.IsAuthenticated).Returns(true);
            ComponentFactories.AddStub<WalletCard>("WALLET_STUB");

            // Act
            var cut = Render<Profile>();

            // Assert
            cut.Markup.Contains("My Orders");
        }

        [Fact]
        public void Unauthorized_User_Should_Not_See_Orders()
        {
            // Arrange
            _authStateMock.SetupGet(a => a.IsAuthenticated).Returns(false);
            ComponentFactories.AddStub<WalletCard>("WALLET_STUB");

            // Act
            var cut = Render<Profile>();

            // Assert
            cut.Markup.Should().NotContain("My Orders");
        }
    }
}
