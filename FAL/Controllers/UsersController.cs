using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Mvc;
using FAL.Models;

namespace FAL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly DynamoDBContext _dbContext;

        public UsersController(IAmazonDynamoDB dynamoDbClient)
        {
            _dynamoDbClient = dynamoDbClient;
            _dbContext = new DynamoDBContext(dynamoDbClient);
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _dbContext.ScanAsync<User>(new List<ScanCondition>()).GetRemainingAsync();
            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _dbContext.LoadAsync<User>(id);
            if (user == null)
                return NotFound("User không tìm thấy!");

            return Ok(user);
        }

        // POST: api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            var existingUser = await _dbContext.LoadAsync<User>(user.UserId);
            if (existingUser != null)
                return BadRequest("User đã tồn tại!");

            await _dbContext.SaveAsync(user);
            return Ok(user); // Tạo mới một user
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User updatedUser)
        {
            var existingUser = await _dbContext.LoadAsync<User>(id);
            if (existingUser == null)
                return NotFound("User không tìm thấy để update!");

            // Cập nhật user
            existingUser.Username = updatedUser.Username;
            existingUser.Password = updatedUser.Password;
            existingUser.Email = updatedUser.Email;
            existingUser.RoleId = updatedUser.RoleId; // Cập nhật RoleId duy nhất, kiểu int
            existingUser.SystemName = updatedUser.SystemName;
            existingUser.WebhookUrl = updatedUser.WebhookUrl;
            existingUser.WebhookSecretKey = updatedUser.WebhookSecretKey;
            existingUser.Status = updatedUser.Status;

            await _dbContext.SaveAsync(existingUser);
            return Ok(existingUser);
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _dbContext.LoadAsync<User>(id);
            if (user == null)
                return NotFound("User không tìm thấy để xóa!");

            await _dbContext.DeleteAsync(user);
            return Ok("Đã xóa user!");
        }
    }
}