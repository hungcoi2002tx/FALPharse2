using FAL.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Amazon.DynamoDBv2.DataModel;
using FAL.Models;
using Amazon.DynamoDBv2;

namespace FAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DynamoDBContext _dbContext;
        public AuthController(IConfiguration configuration, IAmazonDynamoDB dynamoDbClient)
        {
            _configuration = configuration;
            _dbContext = new DynamoDBContext(dynamoDbClient);

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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            // Kiểm tra tính hợp lệ của thông tin đăng nhập
            if (loginModel == null || string.IsNullOrEmpty(loginModel.Username) || string.IsNullOrEmpty(loginModel.Password))
            {
                return BadRequest("Thông tin đăng nhập không hợp lệ.");
            }

            // Tìm người dùng trong DynamoDB
            var user = await _dbContext.LoadAsync<User>(loginModel.Username);
            if (user == null)
            {
                return Unauthorized("Tên người dùng không tồn tại.");
            }

            // Kiểm tra mật khẩu
            if (!BCrypt.Net.BCrypt.Verify(loginModel.Password, user.Password))
            {
                return Unauthorized("Mật khẩu không chính xác.");
            }

            // Tạo JWT token
            var tokenString = GenerateJwtToken(user.Username, user.RoleId);
            return Ok(new { Token = tokenString, UserRole = user.RoleId });
        }

        // Phương thức tạo JWT token
        private string GenerateJwtToken(string username, int roleId)
        {
            // Cấu hình thông tin cho token
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim("roleId", roleId.ToString()), // Thêm RoleId vào claims
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])); // Thay YOUR_SECRET_KEY bằng khóa bí mật của bạn
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "yourdomain.com", // Thay bằng domain của bạn
                audience: "yourdomain.com", // Thay bằng domain của bạn
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDTO userDto)
        {
            if (userDto == null)
            {
                return BadRequest("Thông tin không hợp lệ.");
            }

            // Kiểm tra xem người dùng đã tồn tại chưa
            var existingUser = await _dbContext.LoadAsync<User>(userDto.Username);
            if (existingUser != null)
            {
                return Conflict("Tên người dùng đã tồn tại.");
            }

            // Mã hóa mật khẩu
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            // Tạo một đối tượng User
            var user = new User
            {
                UserId = Guid.NewGuid().ToString(), // Tự sinh UserId
                Username = userDto.Username,
                Password = hashedPassword,
                Email = userDto.Email,
                RoleId = 2, // Mặc định RoleId là 2 cho staff
                SystemName = userDto.SystemName,
                WebhookUrl = userDto.WebhookUrl,
                WebhookSecretKey = userDto.WebhookSecretKey,
                Status = "Active" // Trạng thái mặc định
            };

            // Lưu user vào DynamoDB
            await _dbContext.SaveAsync(user);

            return Ok("Đăng ký thành công!");
        }
    }

}
