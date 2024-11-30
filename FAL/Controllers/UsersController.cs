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
        /// Api cho user update thông tin webhook
        /// </summary>
        /// <param name="username"></param>
        /// <param name="webhookUpdate"></param>
        /// <returns></returns>
        // PUT: api/users/{username}/webhook
        [Authorize]
        [HttpPut("{username}/webhook")]
        public async Task<IActionResult> UpdateWebhookInfo(string username, [FromBody] WebhookUpdateRequest webhookUpdate)
        {
            // Xác định user hiện tại từ token (cần triển khai nếu chưa có)
            var currentUsername = User.Identity?.Name;

            if (currentUsername == null)
                return Unauthorized("Không xác định được người dùng hiện tại!");

            // Load thông tin user từ DynamoDB
            var existingUser = await _dbContext.LoadAsync<Account>(username);
            if (existingUser == null)
                return NotFound("User không tồn tại!");

            // Kiểm tra xem user hiện tại có quyền cập nhật không
            if (currentUsername != username)
            {
                // Trả về 403 Forbidden với thông báo chi tiết
                return StatusCode(StatusCodes.Status403Forbidden, "Không có quyền cập nhật thông tin webhook của user khác!");
            }

            // Cập nhật WebhookUrl và WebhookSecretKey
            existingUser.WebhookUrl = webhookUpdate.WebhookUrl;
            existingUser.WebhookSecretKey = webhookUpdate.WebhookSecretKey;

            // Lưu thay đổi
            await _dbContext.SaveAsync(existingUser);

            return Ok(new
            {
                Message = "Thông tin webhook đã được cập nhật thành công!",
                existingUser.WebhookUrl
            });
        }
    }
}