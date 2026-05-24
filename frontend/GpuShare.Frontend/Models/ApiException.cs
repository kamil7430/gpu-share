namespace GpuShare.Frontend.Models;

/// <summary>
/// Custom exception for API errors, captures HTTP status code and message.
/// Example usage:
/// if (response.StatusCode == HttpStatusCode.Conflict)
/// {
///     throw new ApiException("Device is already reserved.", 409);
/// }
/// </summary>
public class ApiException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}