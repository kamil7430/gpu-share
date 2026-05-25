using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;

namespace GpuShare.Frontend.State
{
    public class MockAuthState : IAuthState
    {
        public User? User { get; set; } = new User("user1");

        public string? AccessToken { get; private set; } = "mock-token";

        public DateTime? AccessTokenExpiresAt { get; set; } = DateTime.UtcNow.AddHours(1);

        public bool Admin => false;

        public bool IsAuthenticated => true;
        public event Action? OnChange;

        public void Logout()
        {
            
        }

        public void SetAuth(User user, string token)
        {
            User = user;
            AccessToken = token;
        }

        public void SetAuth(AuthResponse authResponse)
        {
            User = authResponse.User;
            AccessToken = authResponse.Token;
        }
    }
}
