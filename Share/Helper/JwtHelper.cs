using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
namespace FAL.Utils;

public class JwtHelper
{
    public static string GetSystemNameFromCurrentToken(HttpContext httpContext)
    {
        // Lấy token từ header Authorization
        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            return GetSystemNameFromJwt(token);
        }

        return null; 
    }

    private static string GetSystemNameFromJwt(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Trích xuất claim "systemName"
        var systemNameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "systemName");

        return systemNameClaim?.Value;
    }
}

