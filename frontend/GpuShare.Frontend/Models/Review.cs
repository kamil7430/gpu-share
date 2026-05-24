namespace GpuShare.Frontend.Models;

public class Review
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int DeviceId { get; set; }

    public string AuthorUsername { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}