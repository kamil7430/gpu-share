namespace GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

public interface IReviewService
{
    /// <summary>
    /// POST /api/orders/{id}/review
    /// Creates a one-time review after completed session.
    /// </summary>
    /// <param name="cmd">The review creation request containing rating and comment.</param>
    /// <param name="orderId">The ID of the order.</param>
    /// <returns>The created <see cref="Review"/> object.</returns>
    Task<Review> CreateReviewAsync(int orderId, CreateReviewRequest cmd);

    /// <summary>
    /// GET /api/devices/{id}/reviews
    /// Returns reviews for device details page.
    /// </summary>
    /// <param name="deviceId">The ID of the device.</param>
    /// <returns>A list of <see cref="Review"/> objects for the specified device.</returns>
    Task<List<Review>> GetDeviceReviewsAsync(int deviceId);

    /// <summary>
    /// Returns the average rating and review count for a user, used in the profile page.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <returns>A <see cref="UserRatingDto"/> containing the average rating and review count.</returns>
    Task<UserRatingDto> GetUserRatingAsync(string username);
}