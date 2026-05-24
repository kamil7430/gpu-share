namespace GpuShare.Frontend.Http;
using GpuShare.Frontend.State;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GpuShare.Frontend.Models;
using Microsoft.Extensions.Http;

public class RefreshTokenHandler : DelegatingHandler
{
    private readonly AuthState _authState;
    private readonly IHttpClientFactory _httpFactory;

    public RefreshTokenHandler(AuthState authState, IHttpClientFactory httpFactory)
    {
        _authState = authState;
        _httpFactory = httpFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            try
            {
                // Use a plain client (named "auth") that does not include this handler to
                // call the refresh endpoint and obtain new tokens.
                var client = _httpFactory.CreateClient("auth");

                var tokenResponse = await client.PostAsJsonAsync<object>("/users/refresh", new { }, cancellationToken);
                tokenResponse.EnsureSuccessStatusCode();

                var newToken = await tokenResponse.Content.ReadFromJsonAsync<string>(cancellationToken: cancellationToken);

                // Try to get current user info
                //var userResponse = await client.GetAsync("/users/me", cancellationToken);
                //userResponse.EnsureSuccessStatusCode();

                //var user = await userResponse.Content.ReadFromJsonAsync<User?>(cancellationToken: cancellationToken);
                var user = _authState.User;

                if (user != null)
                {
                    _authState.SetAuth(user, newToken);
                }

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                response = await base.SendAsync(request, cancellationToken);
            }
            catch
            {
                _authState.Logout();
            }
        }

        return response;
    }
}
