namespace GpuShare.Frontend.Models.Dtos;

public class CreateOrderResponse
{
    public int OrderId { get; set; }

    public ConnectionDetailsDto ConnectionDetails { get; set; } = new();
}