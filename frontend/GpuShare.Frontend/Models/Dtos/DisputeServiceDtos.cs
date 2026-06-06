namespace GpuShare.Frontend.Models.Dtos
{
    public class OpenDisputeRequest
    {
        public int OrderId { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<DisputeAttachmentDto> Attachments { get; set; } = [];
    }

    public class OpenDisputeResponse
    {
        public int DisputeId { get; set; }

        public DisputeStatus Status { get; set; } = DisputeStatus.OPEN;

        public string Description { get; set; } = string.Empty;

        public string OwnerUsername { get; set; } = string.Empty;

        public string CustomerUsername { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }
    }

    public class DisputeQueryParams
    {
        public DisputeStatus? Status { get; set; }

        public string? Search { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }

    public class SubmitClarificationRequest
    {
        public string Message { get; set; } = string.Empty;

        public List<DisputeAttachmentDto> Attachments { get; set; } = [];
    }

    public class ResolveDisputeRequest
    {
        public string Decision { get; set; } = string.Empty;
        // CustomerFavor / OwnerFavor

        public string Justification { get; set; } = string.Empty;

        public int? RefundAmountUsdCents { get; set; }
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

        public DateTime SentAt { get; set; }
    }
}
