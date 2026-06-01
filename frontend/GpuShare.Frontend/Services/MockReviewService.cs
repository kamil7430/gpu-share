using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services
{
    public class MockReviewService : IReviewService
    {
        public Task<Review> CreateReviewAsync(int orderId, CreateReviewRequest cmd)
        {
            return Task.FromResult(new Review
            {
                Id = 1,
                OrderId = orderId,
                Rating = cmd.Rating,
                Comment = cmd.Comment,
                CreatedAt = DateTime.Now,
                AuthorUsername = "mockuser"
            });
        }

        public Task<PagedResult<Review>> GetDeviceReviewsAsync(int deviceId, int page = 1, int count = 10)
        {
            return Task.FromResult(new PagedResult<Review>
            {
                Items = new List<Review> {
                    new Review
                    {
                        Id = 1,
                        OrderId = deviceId,
                        Rating = 4,
                        Comment = "This is a mock review.",
                        CreatedAt = DateTime.Now,
                        AuthorUsername = "mockuser"
                    } 
                }
            });
        }

        public Task<PagedResult<Review>> GetUserReviewsAsync(string username, int page = 1, int count = 10)
        {
            return Task.FromResult(new PagedResult<Review>
            {
                Items = [new Review
                {
                    Id = 1,
                    OrderId = 4,
                    Rating = 4,
                    Comment = "This is a mock review.",
                    CreatedAt = DateTime.Now,
                    AuthorUsername = "mockuser"
                }]
            });
        }

        public Task<UserRatingDto> GetUserRatingAsync(string username)
        {
            return Task.FromResult(new UserRatingDto
            {
                AverageRating = 4.5m,
                RatingCount = 21
            });
        }
    }
}
