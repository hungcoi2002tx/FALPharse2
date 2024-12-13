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
    public class UsersController : ControllerBase
    {
        private readonly IDynamoDBContext _dbContext;

        public UsersController(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// API for users to update their information
        /// </summary>
        /// <param name="userUpdate"></param>
        /// <returns></returns>
        // PUT: api/users/{username}/update
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UserUpdateRequest userUpdate)
        {
            // Identify the current user from the token
            var currentUsername = User.Identity?.Name;

            if (currentUsername == null)
                return Unauthorized("Unable to identify the current user!");

            // Load user information from DynamoDB
            var existingUser = await _dbContext.LoadAsync<Account>(currentUsername);
            if (existingUser == null)
                return NotFound("User does not exist!");

            // Update user information
            existingUser.Email = userUpdate.Email ?? existingUser.Email;
            existingUser.WebhookSecretKey = userUpdate.WebhookSecretKey ?? existingUser.WebhookSecretKey;

            // Save changes
            await _dbContext.SaveAsync(existingUser);

            return Ok(new
            {
                Message = "User information updated successfully!",
                existingUser.Username,
                existingUser.Email,
                existingUser.SystemName,
                existingUser.WebhookUrl,
                existingUser.WebhookSecretKey
            });
        }

        /// <summary>
        /// API to retrieve the current user's information
        /// </summary>
        /// <returns></returns>
        // GET: api/users
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetUserInfo()
        {
            // Identify the current user from the token
            var currentUsername = User.Identity?.Name;

            if (currentUsername == null)
                return Unauthorized("Unable to identify the current user!");

            // Load user information from DynamoDB
            var existingUser = await _dbContext.LoadAsync<Account>(currentUsername);
            if (existingUser == null)
                return NotFound("User does not exist!");

            return Ok(new
            {
                existingUser.Username,
                existingUser.Email,
                existingUser.SystemName,
                existingUser.WebhookUrl,
                existingUser.WebhookSecretKey
            });
        }

        /// <summary>
        /// API for users to update webhook information
        /// </summary>
        /// <param name="webhookUpdate"></param>
        /// <returns></returns>
        // PUT: api/users/{username}/webhook
        [Authorize]
        [HttpPut("webhook")]
        public async Task<IActionResult> UpdateWebhookInfo([FromBody] WebhookUpdateRequest webhookUpdate)
        {
            // Identify the current user from the token
            var currentUsername = User.Identity?.Name;

            if (currentUsername == null)
                return Unauthorized("Unable to identify the current user!");

            // Load user information from DynamoDB
            var existingUser = await _dbContext.LoadAsync<Account>(currentUsername);
            if (existingUser == null)
                return NotFound("User does not exist!");

            // Check if the current user has the right to update (optional)
            // if (currentUsername != username)
            // {
            //     return StatusCode(StatusCodes.Status403Forbidden, "Permission denied to update another user's webhook information!");
            // }

            // Update WebhookUrl and WebhookSecretKey
            existingUser.WebhookUrl = webhookUpdate.WebhookUrl;
            existingUser.WebhookSecretKey = webhookUpdate.WebhookSecretKey;

            // Save changes
            await _dbContext.SaveAsync(existingUser);

            return Ok(new
            {
                Message = "Webhook information updated successfully!",
                existingUser.WebhookUrl
            });
        }

        /// <summary>
        /// API for users to change their password
        /// </summary>
        /// <param name="changePasswordRequest"></param>
        /// <returns></returns>
        // PUT: api/users/change-password
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            // Identify the current user from the token
            var currentUsername = User.Identity?.Name;

            if (currentUsername == null)
                return Unauthorized("Unable to identify the current user!");

            // Load user information from DynamoDB
            var existingUser = await _dbContext.LoadAsync<Account>(currentUsername);
            if (existingUser == null)
                return NotFound("User does not exist!");

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(changePasswordRequest.CurrentPassword, existingUser.Password))
                return BadRequest("Current password is incorrect!");

            // Update password
            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordRequest.NewPassword);

            // Save changes
            await _dbContext.SaveAsync(existingUser);

            return Ok(new
            {
                Message = "Password has been changed successfully!"
            });
        }
    }
}
