namespace GpuShare.Frontend.Models.Dtos;

public class ResolveDisputeRequest
{
    public string Decision { get; set; } = string.Empty;
    // CustomerFavor / OwnerFavor

    public string Justification { get; set; } = string.Empty;

    public decimal? RefundAmount { get; set; }
}