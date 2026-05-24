namespace GpuShare.Frontend.Services.Interfaces;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

public interface IReviewService
{
    /// <summary>
    /// POST /api/orders/{id}/review
    /// Creates a one-time review after completed session.
    /// </summary>
    Task<Review> CreateReviewAsync(int orderId, CreateReviewRequest cmd);

    /// <summary>
    /// GET /api/devices/{id}/reviews
    /// Returns reviews for device details page.
    /// </summary>
    Task<List<Review>> GetDeviceReviewsAsync(int deviceId);
}