namespace GpuShare.Frontend.Models.Dtos
{
    public class CreateReviewRequest
    {
        public int OrderId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; } = "";
    }

    public class CreateReviewResponse
    {
        public int ReviewId { get; set; }

        public DateTime? CreatedAt { get; set; } = null;

        public string AuthorUsername { get; set; } = "";
    }

    public class UserRatingDto
    {
        public double AverageRating { get; set; } = 0.0;
        public int RatingCount { get; set; } = 0;
    }
}
