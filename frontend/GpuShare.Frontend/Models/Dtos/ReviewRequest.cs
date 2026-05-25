namespace GpuShare.Frontend.Models.Dtos
{
    public class ReviewRequest
    {
        public int OrderId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; } = "";
    }
}
