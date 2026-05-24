namespace GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

public interface IAuthService
{
    /// <summary>
    /// POST /auth/login
    /// Returns JWT access token and refresh token.
    /// </summary>
    Task<AuthResponse> LoginAsync(AuthRequest payload);

    /// <summary>
    /// POST /auth/register
    /// Creates a new account and sends verification email.
    /// </summary>
    Task RegisterAsync(RegisterRequest payload);

    /// <summary>
    /// Silently refreshes access token before expiration.
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync();

    /// <summary>
    /// Invalidates tokens and clears local auth state.
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// GET /auth/me
    /// Returns currently authenticated user profile.
    /// </summary>
    Task<User> GetMeAsync();
}
