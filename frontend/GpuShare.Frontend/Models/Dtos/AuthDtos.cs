namespace GpuShare.Frontend.Models.Dtos;

public class AuthRequest
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public User User { get; set; } = new User();

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}