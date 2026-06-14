namespace GpuShare.Frontend.Infrastructure.Http;

/// <summary>
/// Strongly-typed keys for per-request metadata carried on <see cref="HttpRequestMessage.Options"/>.
/// This is the idiomatic way for a call site to influence the DelegatingHandler pipeline
/// (e.g. <see cref="ApiClientHandler"/>) for a single request without changing global state.
/// </summary>
public static class ApiRequestOptions
{
    /// <summary>
    /// When set to <c>true</c> on a request, <see cref="ApiClientHandler"/> will NOT attach the
    /// Authorization header even if the user is logged in. Use for public endpoints that should be
    /// called anonymously (e.g. browsing the device catalog from DevicesPage).
    /// </summary>
    public static readonly HttpRequestOptionsKey<bool> Anonymous = new("Anonymous");
}
