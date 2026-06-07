namespace GpuShare.Frontend.Models;

using System.ComponentModel.DataAnnotations;
using GpuShare.Frontend.Models.Dtos;

public class Dispute
{
    public int DisputeId { get; set; }

    public int OrderId { get; set; }

    public string CustomerUsername { get; set; } = string.Empty;

    public string OwnerUsername { get; set; } = string.Empty;

    public string Reason { get; set; } = "";

    public DisputeStatus Status { get; set; } = DisputeStatus.OPEN;

    [MinLength(50)]
    public string Description { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    //public List<DisputeMessageDto> Messages { get; set; } = [];

    public List<DisputeAttachmentDto> Attachments { get; set; } = [];
}

public enum DisputeStatus
{
    [Display(Name = "Open")]
    OPEN,
    [Display(Name = "Under Review")]
    UNDER_REVIEW,
    [Display(Name = "Resolved")]
    RESOLVED,
    [Display(Name = "Rejected")]
    REJECTED
}