namespace GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.State;
using System.Net.Http.Headers;

public class ApiClientHandler(AuthState authState) : DelegatingHandler
{
    private readonly AuthState _authState = authState;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_authState.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}