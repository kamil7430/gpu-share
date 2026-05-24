namespace GpuShare.Frontend.Models.Dtos;

public class ConnectionDetailsDto
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string Protocol { get; set; } = "WSS";

    public string AccessToken { get; set; } = string.Empty;
}