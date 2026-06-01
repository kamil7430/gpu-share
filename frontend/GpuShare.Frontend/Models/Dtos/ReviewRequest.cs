namespace GpuShare.Frontend.Models.Dtos
{
    public class CreateReviewRequest
    {
        public int OrderId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; } = "";
    }
}
