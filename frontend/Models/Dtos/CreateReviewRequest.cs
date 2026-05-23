namespace GpuShare.Frontend.Models.Dtos;

public class CreateReviewRequest
{
    public int Rating { get; set; }
    // 1-5

    public string Comment { get; set; } = string.Empty;
}