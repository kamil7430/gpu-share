namespace GpuShare.Frontend.Http;
using GpuShare.Frontend.Services;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using System.Net;
using System.Net.Http.Headers;

public class RefreshTokenHandler : DelegatingHandler
{
    private readonly AuthState _authState;
    private readonly IAuthService _authService;

    public RefreshTokenHandler(AuthState authState, IAuthService authService)
    {
        _authState = authState;
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            try
            {
                var auth = await _authService.RefreshTokenAsync();
                var user = await _authService.GetMeAsync();
                _authState.SetAuth(user, auth.AccessToken);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

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