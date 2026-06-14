namespace GpuShare.Frontend.Infrastructure.Http;

using GpuShare.Frontend.Models;
using GpuShare.Frontend.State;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ApiClientHandler(IAuthState authState, ILogger<ApiClientHandler> logger) : DelegatingHandler
{
    private readonly IAuthState _authState = authState;
    private readonly ILogger<ApiClientHandler> _logger = logger;

    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new JsonStringEnumConverter() }
    };

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

            _logger.LogInformation("Api request content: {content}", JsonSerializer.Serialize(request));
            _logger.LogInformation("Token: {token}", _authState.AccessToken ?? "");

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