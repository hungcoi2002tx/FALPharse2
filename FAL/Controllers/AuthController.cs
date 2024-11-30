using FAL.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using System.ComponentModel.DataAnnotations;
using FAL.Utils;
using Share.Model;

namespace FAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBContext _dbContext;
        private readonly JwtTokenGenerator _jwtTokenGenerator;

        public AuthController(IConfiguration configuration, IDynamoDBContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _jwtTokenGenerator = new JwtTokenGenerator(configuration);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            // Kiểm tra tính hợp lệ của thông tin đăng nhập
            if (loginModel == null || string.IsNullOrEmpty(loginModel.Username) || string.IsNullOrEmpty(loginModel.Password))
            {
                return BadRequest(new { status = false, message = "Thông tin đăng nhập không hợp lệ." });
            }

            // Tìm người dùng trong DynamoDB
            var user = await _dbContext.LoadAsync<Account>(loginModel.Username);
            if (user == null)
            {
                return Unauthorized(new { status = false, message = "Tên người dùng không tồn tại." });
            }

            // Kiểm tra mật khẩu
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginModel.Password, user.Password);
            if (!isPasswordValid)
            {
                return Unauthorized(new { status = false, message = "Mật khẩu không chính xác." });
            }

            // Tạo JWT token và lấy thời gian hết hạn của token
            // Gọi GenerateJwtToken
            var jwtToken = _jwtTokenGenerator.GenerateJwtToken(user.Username, user.RoleId.ToString(), user.SystemName);

            // Lấy chuỗi token
            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            // Tính toán expires_in
            var expiresIn = (int)(jwtToken.ValidTo - DateTime.UtcNow).TotalSeconds;

            // Trả về thông tin người dùng và token với status true
            return Ok(new
            {
                status = true,
                token = tokenString,
                userRole = user.RoleId,
                systemName = user.SystemName,
                expiresIn
            });
        }



        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AccountRegisterDTO userDto)
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
            var existingUser = await _dbContext.LoadAsync<Account>(userDto.Username);
            if (existingUser != null)
            {
                return Conflict("Tên người dùng đã tồn tại.");
            }

            // Mã hóa mật khẩu
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            // Tạo một đối tượng User
            var user = new Account
            {
                Username = userDto.Username,
                Password = hashedPassword,
                Email = userDto.Email,
                RoleId = 2, // Mặc định RoleId là 2 cho staff
                SystemName = userDto.SystemName,
                WebhookUrl = userDto.WebhookUrl,
                WebhookSecretKey = userDto.WebhookSecretKey,
                Status = "Deactive" // Trạng thái mặc định
            };

            // Lưu user vào DynamoDB
            await _dbContext.SaveAsync(user);

            return Ok("Đăng ký thành công!");
        }

        // Phương thức validate dữ liệu từ UserRegisterDTO
        private bool ValidateRegisterDto(AccountRegisterDTO userDto, out string errorMessage)
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
