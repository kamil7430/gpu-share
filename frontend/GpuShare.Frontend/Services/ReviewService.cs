using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Services.Interfaces;

namespace GpuShare.Frontend.Services
{
    public class ReviewService : IReviewService
    {
        public Task<Review> CreateReviewAsync(int orderId, CreateReviewRequest cmd)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<Review>> GetDeviceReviewsAsync(int deviceId, int page = 1, int count = 10)
        {
            throw new NotImplementedException();
        }

        public Task<UserRatingDto> GetUserRatingAsync(string username)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<Review>> GetUserReviewsAsync(string username, int page = 1, int count = 10)
        {
            throw new NotImplementedException();
        }
    }
}
