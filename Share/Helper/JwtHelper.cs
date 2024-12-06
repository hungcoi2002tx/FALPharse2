using Microsoft.AspNetCore.Http;
using Share.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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

    public static Account DecodeJwt(string jwtToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(jwtToken) as JwtSecurityToken;

        return new Account
        {
            Username = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value ?? string.Empty,
            RoleId = int.TryParse(jsonToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value, out var roleId) ? roleId : 0,
        };
    }
}

