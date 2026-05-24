namespace GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.Models;


public class AuthResponse
{
    public User User { get; set; } = new User();

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}