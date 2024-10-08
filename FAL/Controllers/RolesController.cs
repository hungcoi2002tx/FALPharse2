using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using FAL.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FAL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly DynamoDBContext _dbContext;

        public RolesController(IAmazonDynamoDB dynamoDbClient)
        {
            _dbContext = new DynamoDBContext(dynamoDbClient);
        }

        // GET: api/roles
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var scanConditions = new List<ScanCondition>();
            var roles = await _dbContext.ScanAsync<Role>(scanConditions).GetRemainingAsync();
            return Ok(roles);
        }

        // GET: api/roles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id) // Thay đổi thành int id
        {
            var role = await _dbContext.LoadAsync<Role>(id);
            if (role == null)
                return NotFound("Role không tìm thấy!");

            return Ok(role);
        }

        // POST: api/roles
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] Role role)
        {
            var existingRole = await _dbContext.LoadAsync<Role>(role.RoleId);
            if (existingRole != null)
                return BadRequest("Role đã tồn tại!");

            await _dbContext.SaveAsync(role);
            return Ok(role); // Tạo mới một role
        }

        // PUT: api/roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role updatedRole) // Thay đổi thành int id
        {
            var existingRole = await _dbContext.LoadAsync<Role>(id);
            if (existingRole == null)
                return NotFound("Role không tìm thấy để update!");

            // Cập nhật role
            existingRole.RoleName = updatedRole.RoleName;
            existingRole.Permissions = updatedRole.Permissions;

            await _dbContext.SaveAsync(existingRole);
            return Ok(existingRole);
        }

        // DELETE: api/roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id) // Thay đổi thành int id
        {
            var role = await _dbContext.LoadAsync<Role>(id);
            if (role == null)
                return NotFound("Role không tìm thấy để xóa!");

            await _dbContext.DeleteAsync(role);
            return Ok("Đã xóa role!");
        }
    }
}
