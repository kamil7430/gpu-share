using GpuShare.Frontend.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace GpuShare.Frontend.Tests
{
    internal class MockJwtHelper : IJwtHelper
    {
        public DateTime GetExpiration(string token)
        {
            return DateTime.UtcNow.AddHours(1);
        }
    }
}
