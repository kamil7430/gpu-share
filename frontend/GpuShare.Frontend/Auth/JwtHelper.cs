namespace GpuShare.Frontend.Auth;
using System.IdentityModel.Tokens.Jwt;

public class JwtHelper : IJwtHelper
{
    public DateTime GetExpiration(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        var expClaim = token.Claims.First(x => x.Type == "exp");
        var exp = long.Parse(expClaim.Value);

        return DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
    }
}