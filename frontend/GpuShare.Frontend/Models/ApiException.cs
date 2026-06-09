namespace GpuShare.Frontend.Models;
using System.Net;

/// <summary>
/// Custom exception for API errors, captures HTTP status code and message.
/// Example usage:
/// if (response.StatusCode == HttpStatusCode.Conflict)
/// {
///     throw new ApiException("Device is already reserved.", HttpStatusCode.Conflict);
/// }
/// </summary>
public class ApiException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string? ServerMessage { get; } = message;
}