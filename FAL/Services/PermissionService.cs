using Amazon.DynamoDBv2.DataModel;
using FAL.Services.IServices;
using Share.Model;
using System.Security.Claims;

namespace FAL.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IDynamoDBContext _dbContext;

        public PermissionService(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool HasPermission(ClaimsPrincipal user, string? resource, string? action)
        {
            if (resource == null || action == null)
            {
                return false;
            }
            // Lấy roleId từ custom claim "RoleId"
            var roleIdClaim = user.FindFirst("RoleId");

            if (!int.TryParse(roleIdClaim.Value, out var roleId))
            {
                // Invalid RoleId value
                return false;
            }

            // Tìm Role từ DynamoDB
            var role = _dbContext.LoadAsync<Role>(roleId).Result;

            if (role == null) return false;

            // Kiểm tra xem role có quyền trên resource và action cụ thể
            var hasPermission = role.Permissions.Any(p =>
                p.Resource == resource && p.Actions.Contains(action));

            return hasPermission;
        }

    }

}
