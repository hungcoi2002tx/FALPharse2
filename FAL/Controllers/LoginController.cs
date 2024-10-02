using FAL.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            // Kiểm tra tính hợp lệ của thông tin đăng nhập
            if (IsValidUser(loginModel, out string userRole))
            {
                var tokenString = GenerateJwtToken(loginModel.Username, userRole);
                return Ok(tokenString);
            }
            else
            {
                return Unauthorized("Thông tin đăng nhập không hợp lệ.");
            }
        }

        // Phương thức giả lập kiểm tra thông tin đăng nhập và trả về vai trò người dùng
        private bool IsValidUser(LoginModel loginModel, out string role)
        {
            // Giả định các user với role khác nhau
            if (loginModel.Username == "admin" && loginModel.Password == "admin")
            {
                role = "Admin";
                return true;
            }
            else if (loginModel.Username == "user" && loginModel.Password == "user")
            {
                role = "User";
                return true;
            }
            role = string.Empty;
            return false;
        }

        // Tạo JWT token với vai trò người dùng (role)
        private string GenerateJwtToken(string username, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role) // Thêm role vào claim
        };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
