namespace GpuShare.Frontend.Models.Dtos
{
    public class CreateReviewRequest
    {
        public int OrderId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; } = "";
    }

    public class UserRatingDto
    {
        public decimal AverageRating { get; set; } = 0.0m;
        public int RatingCount { get; set; } = 0;
    }
}
