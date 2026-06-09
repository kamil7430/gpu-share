namespace GpuShare.Frontend.Auth;

using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using GpuShare.Frontend.State;
using GpuShare.Frontend.Models;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IAuthState _authState;

    public JwtAuthenticationStateProvider(IAuthState authState)
    {
        _authState = authState;

        _authState.OnChange += NotifyAuthenticationStateChanged;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        ClaimsIdentity identity;

        if (_authState.User is null)
        {
            identity = new ClaimsIdentity();
        }
        else
        {
            identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, _authState.User.Id.ToString()), 
                new Claim(ClaimTypes.Name, _authState.User.Username),
                new Claim(ClaimTypes.Role, _authState.User.Admin ? "Admin" : "User")
            ], "jwt");
        }

        var user = new ClaimsPrincipal(identity);

        return Task.FromResult(new AuthenticationState(user));
    }

    private void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}