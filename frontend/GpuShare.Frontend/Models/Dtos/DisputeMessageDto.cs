namespace GpuShare.Frontend.Models.Dtos;

public class DisputeMessageDto
{
    public string AuthorUsername { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime SentAtUtc { get; set; }
}