using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.WebSockets;
using FAL.Dtos;
using Share.Model;
using Share.DTO;

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

        // GET: api/accounts/{username}
        [Authorize]
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserById(string username)
        {
            var account = await _dbContext.LoadAsync<Account>(username);
            if (account == null)
                return NotFound("User not found!");

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

        // POST: api/accounts
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] Account user)
        {
            if (user == null)
                return BadRequest("Invalid user data.");

            // Validate model state
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if the user already exists
            var existingUser = await _dbContext.LoadAsync<Account>(user.Username);
            if (existingUser != null)
                return Conflict("A user with this username already exists.");

            // Hash the password using BCrypt
            try
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password); // Ensure BCrypt library is installed
            }
            catch (Exception)
            {
                // Log the error (using your logging framework)
                return StatusCode(500, "An error occurred while processing the password.");
            }

            // Save the user to the database
            try
            {
                await _dbContext.SaveAsync(user);
            }
            catch (Exception)
            {
                // Log the error (using your logging framework)
                return StatusCode(500, "An error occurred while saving the user.");
            }

            return Ok("User created successfully."); // Do not return user object for security reasons
        }


        // PUT: api/accounts/{username}
        [Authorize]
        [HttpPut("{username}")]
        public async Task<IActionResult> UpdateUser(string username, [FromBody] UpdateAccountDto updatedUser)
        {
            // Find the current user in the database
            var existingUser = await _dbContext.LoadAsync<Account>(username);
            if (existingUser == null)
                return NotFound("User not found for update!");

            // Update user information only if the parameter has a value
            if (!string.IsNullOrEmpty(updatedUser.Password) &&
                !BCrypt.Net.BCrypt.Verify(updatedUser.Password, existingUser.Password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);
            }

            if (!string.IsNullOrEmpty(updatedUser.Email))
                existingUser.Email = updatedUser.Email;

            if (updatedUser.RoleId.HasValue) // Check if RoleId is not null
                existingUser.RoleId = updatedUser.RoleId.Value;

            if (!string.IsNullOrEmpty(updatedUser.SystemName))
                existingUser.SystemName = updatedUser.SystemName;

            if (!string.IsNullOrEmpty(updatedUser.WebhookUrl))
                existingUser.WebhookUrl = updatedUser.WebhookUrl;

            if (!string.IsNullOrEmpty(updatedUser.WebhookSecretKey))
                existingUser.WebhookSecretKey = updatedUser.WebhookSecretKey;

            if (!string.IsNullOrEmpty(updatedUser.Status))
                existingUser.Status = updatedUser.Status;

            // Save the updated user
            await _dbContext.SaveAsync(existingUser);
            return Ok(existingUser);
        }

        // DELETE: api/accounts/{username}
        [Authorize]
        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var user = await _dbContext.LoadAsync<Account>(username);
            if (user == null)
                return NotFound("User not found for deletion!");

            await _dbContext.DeleteAsync(user);
            return Ok("User deleted!");
        }
    }
}
