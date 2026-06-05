using GpuShare.Frontend.Auth;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using GpuShare.Frontend.State;
using System;
using System.Collections.Generic;
using System.Text;

namespace GpuShare.Frontend.Tests
{
    internal class TestAuthState : IAuthState
    {
        public User? User { get; private set; } = null;

        public string? AccessToken { get; private set; } = null;

        public DateTime? AccessTokenExpiresAt { get; set; }

        public bool Admin => User?.Admin ?? false;

        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken) && AccessTokenExpiresAt > DateTime.UtcNow;

        public event Action? OnChange;

        private readonly MockJwtHelper _jwtHelper;

        public TestAuthState()
        {
            _jwtHelper = new MockJwtHelper();
        }

        public void Logout()
        {
            User = null;
            AccessToken = null;
            AccessTokenExpiresAt = null;
            OnChange?.Invoke();
        }

        public void SetAuth(User user, string token)
        {
            User = user;
            AccessToken = token;
            AccessTokenExpiresAt = _jwtHelper.GetExpiration(token);
            OnChange?.Invoke();
        }

        public void SetAuth(AuthResponse authResponse)
        {
            User = authResponse.User;
            AccessToken = authResponse.Token;
            AccessTokenExpiresAt = authResponse.ExpiresAt;
            OnChange?.Invoke();
        }
    }
}
