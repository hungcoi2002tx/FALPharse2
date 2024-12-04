using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FAL.Utils
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public JwtSecurityToken GenerateJwtToken(string username, string roleId, string systemName)
        {
            // Lấy khóa bí mật từ cấu hình
            var secretKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("Jwt:Key is missing in configuration.");
            }

            // Tạo khóa bảo mật
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Thêm các claims cho token, bao gồm cả systemName
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // Thời điểm phát hành token
        new Claim(ClaimTypes.Role, roleId), // Thêm RoleId vào claims
        new Claim("systemName", systemName), // Thêm SystemName vào claims
        new Claim(ClaimTypes.Name, username) // Đặt username như tên người dùng
    };

            // Lấy thời gian hết hạn từ cấu hình (nếu có)
            var tokenExpiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out int expiry) ? expiry : 30;

            // Tạo và trả về đối tượng JwtSecurityToken
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(tokenExpiryMinutes),
                signingCredentials: creds
            );

            return token;
        }



    }
}
