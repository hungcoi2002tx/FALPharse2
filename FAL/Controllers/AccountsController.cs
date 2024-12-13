using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.WebSockets;
using FAL.Dtos;
using Share.Model;
using Share.DTO;
using Amazon.DynamoDBv2.DocumentModel;
using FAL.Services.IServices;

namespace FAL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IDynamoDBContext _dbContext;
        private readonly ICollectionService _collectionService;

        public AccountsController(IDynamoDBContext dbContext, ICollectionService collectionService)
        {
            _dbContext = dbContext;
            _collectionService = collectionService;
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
        public async Task<IActionResult> CreateUser([FromBody] Account newUser)
        {
            if (newUser == null)
                return BadRequest("Invalid user data.");

            // Validate model state
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if the user already exists by username
            var existingUser = await _dbContext.LoadAsync<Account>(newUser.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists.");
            }

            // Check if the SystemName already exists (using ScanAsync)
            var scanResult = await _dbContext.ScanAsync<Account>(
                           [
                               new("SystemName", ScanOperator.Equal, newUser.SystemName)
                           ]).GetRemainingAsync();

            if (scanResult.Count != 0)
            {
                return Conflict("SystemName already exists.");
            }

            // Check if collection already exists with the same SystemName
            var collectionExists = await _collectionService.IsCollectionExistAsync(newUser.SystemName);
            if (collectionExists)
            {
                return Conflict("Collection already exists for this SystemName.");
            }

            // Create a new collection for the user
            bool collectionCreated = await _collectionService.CreateCollectionAsync(newUser.SystemName);
            if (!collectionCreated)
            {
                return StatusCode(500, "Failed to create collection.");
            }

            try
            {
                // Hash the password
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newUser.Password);

                // Create a new User object
                var user = new Account
                {
                    Username = newUser.Username,
                    Password = hashedPassword,
                    Email = newUser.Email,
                    RoleId = newUser.RoleId > 0 ? newUser.RoleId : 2, // Default RoleId is 2 for staff
                    SystemName = newUser.SystemName,
                    WebhookUrl = newUser.WebhookUrl,
                    WebhookSecretKey = newUser.WebhookSecretKey,
                    Status = newUser.Status ?? "Deactive" // Default status
                };

                // Save the user to DynamoDB
                await _dbContext.SaveAsync(user);

                return Ok("User creation and collection setup successful!");
            }
            catch (Exception ex)
            {
                // Rollback the collection creation in case of user creation failure
                bool collectionDeleted = await _collectionService.DeleteCollectionAsync(newUser.SystemName);
                if (!collectionDeleted)
                {
                    return StatusCode(500, "User creation failed, and collection rollback unsuccessful.");
                }

                return StatusCode(500, $"User creation failed: {ex.Message}");
            }
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
