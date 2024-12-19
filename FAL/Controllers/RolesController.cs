using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Share.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FAL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IDynamoDBContext _dbContext;

        public RolesController(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/roles
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var scanConditions = new List<ScanCondition>();
            var roles = await _dbContext.ScanAsync<Role>(scanConditions).GetRemainingAsync();
            return Ok(roles);
        }

        // GET: api/roles/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id) // Changed to int id
        {
            var role = await _dbContext.LoadAsync<Role>(id);
            if (role == null)
                return NotFound("Role not found!");

            return Ok(role);
        }

        // POST: api/roles
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] Role role)
        {
            if (role == null)
            {
                return BadRequest("Role cannot be null.");
            }
            var existingRole = await _dbContext.LoadAsync<Role>(role.RoleId);
            if (existingRole != null)
                return BadRequest("Role already exists!");

            await _dbContext.SaveAsync(role);
            return Ok(role); // Create a new role
        }

        // PUT: api/roles/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role updatedRole) // Changed to int id
        {
            var existingRole = await _dbContext.LoadAsync<Role>(id);
            if (existingRole == null)
                return NotFound("Role not found for update!");

            // Update role
            existingRole.RoleName = updatedRole.RoleName;
            existingRole.Permissions = updatedRole.Permissions;

            await _dbContext.SaveAsync(existingRole);
            return Ok(existingRole);
        }

        // DELETE: api/roles/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id) // Changed to int id
        {
            var role = await _dbContext.LoadAsync<Role>(id);
            if (role == null)
                return NotFound("Role not found for deletion!");

            await _dbContext.DeleteAsync(role);
            return Ok("Role deleted!");
        }
    }
}
