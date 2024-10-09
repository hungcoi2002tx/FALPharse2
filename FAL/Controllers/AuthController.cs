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
using System.ComponentModel.DataAnnotations;
using FAL.Utils;

namespace FAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DynamoDBContext _dbContext;
        private readonly JwtTokenGenerator _jwtTokenGenerator;

        public AuthController(IConfiguration configuration, IAmazonDynamoDB dynamoDbClient)
        {
            _configuration = configuration;
            _dbContext = new DynamoDBContext(dynamoDbClient);
            _jwtTokenGenerator = new JwtTokenGenerator(configuration);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
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
            var tokenString = _jwtTokenGenerator.GenerateJwtToken(user.Username, user.RoleId, user.SystemName);
            return Ok(new { Token = tokenString, UserRole = user.RoleId });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO userDto)
        {
            if (userDto == null)
            {
                return BadRequest("Thông tin đăng ký không hợp lệ.");
            }

            // Validation các trường dữ liệu
            if (!ValidateRegisterDto(userDto, out string validationMessage))
            {
                return BadRequest(validationMessage);
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
                UserId = Guid.NewGuid().ToString(),
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

        // Phương thức validate dữ liệu từ UserRegisterDTO
        private bool ValidateRegisterDto(UserRegisterDTO userDto, out string errorMessage)
        {
            if (string.IsNullOrEmpty(userDto.Username) || userDto.Username.Length < 3)
            {
                errorMessage = "Tên người dùng phải có ít nhất 3 ký tự.";
                return false;
            }

            if (string.IsNullOrEmpty(userDto.Password) || userDto.Password.Length < 6)
            {
                errorMessage = "Mật khẩu phải có ít nhất 6 ký tự.";
                return false;
            }

            if (!new EmailAddressAttribute().IsValid(userDto.Email))
            {
                errorMessage = "Email không hợp lệ.";
                return false;
            }

            if (string.IsNullOrEmpty(userDto.SystemName))
            {
                errorMessage = "System Name không được để trống.";
                return false;
            }

            if (string.IsNullOrEmpty(userDto.WebhookUrl))
            {
                errorMessage = "Webhook URL không được để trống.";
                return false;
            }

            if (string.IsNullOrEmpty(userDto.WebhookSecretKey))
            {
                errorMessage = "Webhook Secret Key không được để trống.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
