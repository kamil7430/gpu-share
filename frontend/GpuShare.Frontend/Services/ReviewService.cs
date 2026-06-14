using GpuShare.Frontend.Infrastructure.Http;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services
{
    public class ReviewService(IApiClient api, ILogger<ReviewService> logger) : IReviewService
    {
        private readonly IApiClient _api = api;
        private readonly ILogger<ReviewService> _logger = logger;

        public async Task<Review> CreateReviewAsync(int orderId, CreateReviewRequest cmd)
        {
            var response = await _api.PostAsync<CreateReviewRequest, CreateReviewResponse>($"/api/orders/{orderId}/review", cmd);
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Created review for order {orderId} from user {username}. Got ID {id} for it.",
                    cmd.OrderId, response!.AuthorUsername, response!.ReviewId);

            return new Review
            {
                ReviewId = response!.ReviewId,
                OrderId = cmd.OrderId,
                AuthorUsername = response!.AuthorUsername,
                Comment = cmd.Comment,
                Rating = cmd.Rating,
                CreatedAt = response.CreatedAt ?? DateTime.UtcNow
            };
        }

        public async Task<PagedResult<Review>> GetDeviceReviewsAsync(int deviceId, int page = 1, int count = 10)
        {
            var reviews = await _api.GetAsync<List<Review>>($"/api/devices/{deviceId}/reviews", 
                new LimitQuery { Limit = page * count });
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Got reviews for device {deviceId}.", deviceId);

            return new PagedResult<Review>
            {
                Items = reviews!,
                TotalCount = reviews!.Count,
                Page = 1,
                PageSize = page * count
            };
        }

        public async Task<UserRatingDto> GetUserRatingAsync(string username)
        {
            var rating = await _api.GetAsync<UserRatingDto>($"/api/users/{username}/rating");
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Got rating for user {username}.", username);
            return rating!;
        }

        public async Task<PagedResult<Review>> GetUserReviewsAsync(string username, int page = 1, int count = 10)
        {
            var reviews = await _api.GetAsync<List<Review>>($"/api/users/{username}/reviews",
                new LimitQuery { Limit = page * count });
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Got reviews for user {username}.", username);

            return new PagedResult<Review>
            {
                Items = reviews!,
                TotalCount = reviews!.Count,
                Page = 1,
                PageSize = page * count
            };
        }
    }
}
