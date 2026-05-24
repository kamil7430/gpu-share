using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using GpuShare.Frontend.Components.Pages.Order;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Routing;
using Moq;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GpuShare.Frontend.Tests.Components
{
    //TO-DO: Wrap with <AuthorizeView> instead of attribute
    public class OrderPageTests : BunitContext, Xunit.IAsyncLifetime
    {
        private Mock<IAuthState> _authStateMock;

        public OrderPageTests()
        {
            _authStateMock = new Mock<IAuthState>();
            Services.AddAuthorizationCore();
            Services.AddSingleton(_authStateMock.Object);
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;

            JSInterop.SetupVoid(_ => true);
            JSInterop.SetupModule(_ => true);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await base.DisposeAsync();
        }

        [Fact]
        public async Task Authorized_User_Should_See_Order_Page()
        {
            // Arrange
            var authContext = AddAuthorization();
            authContext.SetAuthorized("john");
            //ComponentFactories.AddStub<OrderDeviceStats>("ORDER_DEVICE_STATS_STUB");
            //ComponentFactories.AddStub<OrderTelemetry>("ORDER_TELEMETRY_STUB");

            // Act
            var cut = Render<Order>();

            // Assert
            cut.Markup.Contains("Order #");
        }

        [Fact]
        public void Unauthorized_User_Should_Not_See_Order_Page()
        {
            // Arrange
            var authContext = AddAuthorization();
            //authContext.SetAuthorized("TEST USER", AuthorizationState.Unauthorized);
            authContext.SetNotAuthorized();
            //authContext.SetAuthorized("john");

            // Act
            var cut = Render<Order>();
            //var nav = Services.GetRequiredService<NavigationManager>(); nav.NavigateTo("/order/1"); // set the route you want to test
            
            // Assert
            cut.Markup.Should().NotContain("Order #");
        }
    }
}
