namespace GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

public interface IAuthService
{
    /// <summary>
    /// POST /users/login
    /// Returns JWT access token and refresh token.
    /// </summary>
    Task LoginAsync(AuthRequest payload);

    /// <summary>
    /// POST /users/register
    /// Creates a new account and sends verification email.
    /// </summary>
    Task RegisterAsync(AuthRequest payload);

    /// <summary>
    /// POST /users/changePassword
    /// Creates a new account and sends verification email.
    /// </summary>
    Task ChangePasswordAsync(ChangePasswordRequest payload);

    /// <summary>
    /// POST /users/refresh
    /// Silently refreshes access token before expiration.
    /// </summary>
    Task RefreshTokenAsync();

    /// <summary>
    /// Invalidates tokens and clears local auth state.
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Returns currently authenticated user profile.
    /// </summary>
    Task<User> GetMeAsync();
}
