namespace GpuShare.Frontend.Models.Dtos
{
    public class OpenDisputeRequest
    {
        public int OrderId { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    public class DisputeQueryParams
    {
        public string? Status { get; set; }

        public string? Search { get; set; }

        public DateTime? FromUtc { get; set; }

        public DateTime? ToUtc { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }

    public class SubmitClarificationRequest
    {
        public string Message { get; set; } = string.Empty;

        public List<string> AttachmentUrls { get; set; } = [];
    }

    public class ResolveDisputeRequest
    {
        public string Decision { get; set; } = string.Empty;
        // CustomerFavor / OwnerFavor

        public string Justification { get; set; } = string.Empty;

        public decimal? RefundAmount { get; set; }
    }

    public class DisputeAttachmentDto
    {
        public string FileName { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
    }

    public class DisputeMessageDto
    {
        public string AuthorUsername { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime SentAtUtc { get; set; }
    }
}
