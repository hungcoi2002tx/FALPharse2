using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.WebSockets;
using FAL.Dtos;
using Share.Model;

namespace FAL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IDynamoDBContext _dbContext;

        public AccountsController(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var accounts = await _dbContext.ScanAsync<Account>(new List<ScanCondition>()).GetRemainingAsync();
            var accountDtos = accounts.Select(account => new AccountViewDto
            {
                Username = account.Username,
                Password = account.Password,
                Email = account.Email,
                RoleId = account.RoleId,
                SystemName = account.SystemName,
                WebhookUrl = account.WebhookUrl,
                WebhookSecretKey = account.WebhookSecretKey,
                Status = account.Status
            }).ToList();

            return Ok(accountDtos);
        }

        // GET: api/users/{username}
        [Authorize]
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserById(string username)
        {
            var account = await _dbContext.LoadAsync<Account>(username);
            if (account == null)
                return NotFound("User không tìm thấy!");

            var accountDto = new AccountViewDto
            {
                Username = account.Username,
                Password = account.Password,
                Email = account.Email,
                RoleId = account.RoleId,
                SystemName = account.SystemName,
                WebhookUrl = account.WebhookUrl,
                WebhookSecretKey = account.WebhookSecretKey,
                Status = account.Status
            };

            return Ok(accountDto);
        }


        // POST: api/users
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] Account user)
        {
            var existingUser = await _dbContext.LoadAsync<Account>(user.Username);
            if (existingUser != null)
                return BadRequest("User đã tồn tại!");

            await _dbContext.SaveAsync(user);
            return Ok(user); // Tạo mới một user
        }

        // PUT: api/users/{username}
        [Authorize]
        [HttpPut("{username}")]
        public async Task<IActionResult> UpdateUser(string username, [FromBody] Account updatedUser)
        {
            var existingUser = await _dbContext.LoadAsync<Account>(username);
            if (existingUser == null)
                return NotFound("User không tìm thấy để update!");

            // Cập nhật thông tin user
            existingUser.Username = updatedUser.Username;

            // Kiểm tra nếu mật khẩu mới khác với mật khẩu cũ, thì mã hóa mật khẩu
            if (!BCrypt.Net.BCrypt.Verify(updatedUser.Password, existingUser.Password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);  // Mã hóa mật khẩu
            }

            existingUser.Email = updatedUser.Email;
            existingUser.RoleId = updatedUser.RoleId; // Cập nhật RoleId duy nhất, kiểu int
            existingUser.SystemName = updatedUser.SystemName;
            existingUser.WebhookUrl = updatedUser.WebhookUrl;
            existingUser.WebhookSecretKey = updatedUser.WebhookSecretKey;
            existingUser.Status = updatedUser.Status;

            await _dbContext.SaveAsync(existingUser);
            return Ok(existingUser);
        }


        // DELETE: api/users/{username}
        [Authorize]
        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var user = await _dbContext.LoadAsync<Account>(username);
            if (user == null)
                return NotFound("User không tìm thấy để xóa!");

            await _dbContext.DeleteAsync(user);
            return Ok("Đã xóa user!");
        }
    }
}