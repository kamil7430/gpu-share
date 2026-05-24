namespace GpuShare.Frontend.Models.Dtos;

public class OpenDisputeRequest
{
    public Guid OrderId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}