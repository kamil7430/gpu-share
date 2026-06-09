namespace GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

public interface IReviewService
{
    /// <summary>
    /// POST /api/orders/{orderId}/review
    /// Creates a one-time review after completed session.
    /// </summary>
    /// <param name="cmd">The review creation request containing rating and comment.</param>
    /// <param name="orderId">The ID of the order.</param>
    /// <returns>The created <see cref="Review"/> object.</returns>
    Task<Review> CreateReviewAsync(int orderId, CreateReviewRequest cmd);

    /// <summary>
    /// GET /api/devices/{deviceId}/reviews
    /// Returns reviews for device details page.
    /// </summary>
    /// <param name="deviceId">The ID of the device.</param>
    /// <param name="page">The number of page to fetch (default to 1).</param>
    /// <param name="count">The number of items on the page (default to 10).</param>
    /// <returns>A <see cref="PagedResult{T}"/> object of <see cref="Review"/> objects for the specified device.</returns>
    Task<PagedResult<Review>> GetDeviceReviewsAsync(int deviceId, int page = 1, int count = 10);

    /// <summary>
    /// GET /api/users/{username}/reviews
    /// Returns reviews for user profile page.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="page">The number of page to fetch (default to 1).</param>
    /// <param name="count">The number of items on the page (default to 10).</param>
    /// <returns>A <see cref="PagedResult{T}"/> object of <see cref="Review"/> objects for the specified user.</returns>
    Task<PagedResult<Review>> GetUserReviewsAsync(string username, int page = 1, int count = 10);

    /// <summary>
    /// GET /api/users/{username}/rating
    /// Returns the average rating and review count for a user, used in the profile page.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <returns>A <see cref="UserRatingDto"/> containing the average rating and review count.</returns>
    Task<UserRatingDto> GetUserRatingAsync(string username);
}