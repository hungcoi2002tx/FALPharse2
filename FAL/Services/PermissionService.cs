using Amazon.DynamoDBv2.DataModel;
using FAL.Models;
using FAL.Services.IServices;

namespace FAL.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly DynamoDBContext _dbContext;

        public PermissionService(DynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Role> GetRoleByUsername(string username)
        {
            // Lấy thông tin người dùng từ DynamoDB bằng username
            var user = await _dbContext.LoadAsync<User>(username); 

            if (user == null)
            {
                return null;
            }

            // Lấy role tương ứng từ DynamoDB
            return await _dbContext.LoadAsync<Role>(user.RoleId); 
        }


        public bool HasPermission(Role role, string apiEndpoint, string method)
        {
            var permission = role.Permissions.FirstOrDefault(p => p.Resource == apiEndpoint);
            return permission != null && permission.Actions.Contains(method);
        }
    }
}
