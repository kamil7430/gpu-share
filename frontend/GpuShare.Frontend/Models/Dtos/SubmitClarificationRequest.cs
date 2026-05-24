namespace GpuShare.Frontend.Models.Dtos;

public class SubmitClarificationRequest
{
    public string Message { get; set; } = string.Empty;

    public List<string> AttachmentUrls { get; set; } = new();
}