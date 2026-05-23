namespace GpuShare.Frontend.Http;
using GpuShare.Frontend.Services;
using GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.State;
using System.Net.Http.Headers;

public class ApiClientHandler : DelegatingHandler
{
    private readonly AuthState _authState;

    public ApiClientHandler(AuthState authState)
    {
        _authState = authState;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_authState.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}