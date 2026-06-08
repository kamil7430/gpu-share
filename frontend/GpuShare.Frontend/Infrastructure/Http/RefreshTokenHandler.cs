namespace GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.State;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GpuShare.Frontend.Models;
using Microsoft.Extensions.Http;

public class RefreshTokenHandler(IAuthState authState, IHttpClientFactory httpFactory) : DelegatingHandler
{
    private readonly IAuthState _authState = authState;
    private readonly IHttpClientFactory _httpFactory = httpFactory;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
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

                    if (string.IsNullOrEmpty(newToken))
                    {
                        throw new InvalidOperationException("Failed to refresh token: empty response");
                    }
                    // Try to get current user info
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
                    throw new ApiException("Authentication failed", HttpStatusCode.Unauthorized);
                }
            }

            return response;
        }
        catch (ApiException)
        {
            throw;
        }
        catch (HttpRequestException ex) {
            
            _authState.Logout();
            var code = ex.StatusCode ?? HttpStatusCode.InternalServerError;
            throw new ApiException($"Error ocurred while refreshing token: {ex.Message}", code);
        }
        catch (Exception ex)
        {
            _authState.Logout();
            throw new ApiException($"Error ocurred while refreshing token: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
