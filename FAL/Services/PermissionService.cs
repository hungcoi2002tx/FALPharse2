﻿using Amazon.DynamoDBv2.DataModel;
using FAL.Models;
using FAL.Services.IServices;
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

        public bool HasPermission(ClaimsPrincipal user, string resource, string action)
        {
            // Lấy roleId từ custom claim "RoleId"
            var roleIdClaim = user.FindFirst("RoleId");

            if (roleIdClaim == null)
            {
                // Không tìm thấy RoleId trong token
                return false;
            }

            // Chuyển đổi RoleId từ claim thành int
            var roleId = int.Parse(roleIdClaim.Value);

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
