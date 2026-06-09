using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services;

public class MockReviewService : IReviewService
{
    public Task<Review> CreateReviewAsync(int orderId, CreateReviewRequest cmd)
    {
        var review = new Review
        {
            ReviewId = MockStore.NextId(),
            OrderId = orderId,
            Rating = cmd.Rating,
            Comment = cmd.Comment,
            CreatedAt = DateTime.UtcNow,
            AuthorUsername = MockStore.CurrentUser.Username,
        };
        MockStore.Reviews.Add(review);
        return Task.FromResult(review);
    }

    public Task<PagedResult<Review>> GetDeviceReviewsAsync(int deviceId, int page = 1, int count = 10)
    {
        var deviceOrderIds = MockStore.Orders
            .Where(o => o.DeviceId == deviceId)
            .Select(o => o.OrderId)
            .ToHashSet();

        var items = MockStore.Reviews
            .Where(r => deviceOrderIds.Contains(r.OrderId))
            .Skip((page - 1) * count)
            .Take(count)
            .ToList();

        return Task.FromResult(new PagedResult<Review>
        {
            Items = items,
            TotalCount = items.Count,
            Page = page,
            PageSize = count,
        });
    }

    public Task<PagedResult<Review>> GetUserReviewsAsync(string username, int page = 1, int count = 10)
    {
        var items = MockStore.Reviews
            .Where(r => r.AuthorUsername == username)
            .Skip((page - 1) * count)
            .Take(count)
            .ToList();

        return Task.FromResult(new PagedResult<Review>
        {
            Items = items,
            TotalCount = items.Count,
            Page = page,
            PageSize = count,
        });
    }

    public Task<UserRatingDto> GetUserRatingAsync(string username)
    {
        var deviceIds = MockStore.Devices
            .Where(d => d.OwnerUsername == username)
            .Select(d => d.DeviceId)
            .ToHashSet();

        var orderIds = MockStore.Orders
            .Where(o => deviceIds.Contains(o.DeviceId))
            .Select(o => o.OrderId)
            .ToHashSet();

        var reviews = MockStore.Reviews.Where(r => orderIds.Contains(r.OrderId)).ToList();

        return Task.FromResult(new UserRatingDto
        {
            AverageRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0,
            RatingCount = reviews.Count,
        });
    }
}
