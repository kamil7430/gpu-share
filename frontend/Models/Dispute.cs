namespace GpuShare.Frontend.Models;

using System.ComponentModel.DataAnnotations;
using GpuShare.Frontend.Models.Dtos;

public class Dispute
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string CustomerUsername { get; set; } = string.Empty;

    public string OwnerUsername { get; set; } = string.Empty;

    public string Reason { get; set; } = "";

    public string Status { get; set; } = string.Empty;

    [MinLength(50)]
    public string Details { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; }

    public List<DisputeMessageDto> Messages { get; set; } = new();

    public List<DisputeAttachmentDto> Attachments { get; set; } = new();
}