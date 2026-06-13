namespace GpuShare.Frontend.Infrastructure.Http;

using GpuShare.Frontend.Models;
using GpuShare.Frontend.State;
using System.Net;
using System.Net.Http.Headers;

public class ApiClientHandler(IAuthState authState) : DelegatingHandler
{
    private readonly IAuthState _authState = authState;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            // A call site can opt a single request out of auth (e.g. the public device catalog)
            // by setting ApiRequestOptions.Anonymous via HttpRequestMessage.Options.
            var anonymous = request.Options.TryGetValue(ApiRequestOptions.Anonymous, out var skip) && skip;

            if (!anonymous && !string.IsNullOrWhiteSpace(_authState.AccessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            var code = ex.StatusCode ?? HttpStatusCode.InternalServerError;
            throw new ApiException($"Error ocurred while adding authentication header: {ex.Message}", code);
        }
        catch (Exception ex)
        {
            throw new ApiException($"Error ocurred while adding authentication header: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}