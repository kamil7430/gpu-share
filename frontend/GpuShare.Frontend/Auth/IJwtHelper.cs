namespace GpuShare.Frontend.Auth;

public interface IJwtHelper
{
    DateTime GetExpiration(string jwt);
}
