using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using FAL.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using Share.Model;

namespace FAL.Authors
{
    public class CustomAuthorizationFilter : IAuthorizationFilter
    {
        private const string ACTIVE = "ACTIVE";
        private readonly IDynamoDBContext _dbContext;

        public CustomAuthorizationFilter(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var endpoint = context.HttpContext.GetEndpoint();

            // Skip authorization check if the endpoint has [AllowAnonymous]
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return;
            }

            // Check if the user is authenticated
            if (context.HttpContext.User.Identity == null || !context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new JsonResult(new { message = "Unauthorized: You must be logged in." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            // Retrieve HTTP method (action) and endpoint (resource)
            var httpMethod = context.HttpContext.Request.Method;
            var resource = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()?.ControllerName;

            // Retrieve user information
            var user = context.HttpContext.User;
            var username = user.Identity.Name; // Extract username from the token

            // Check the account status
            var account = _dbContext.LoadAsync<Account>(username).Result; // Find account in DynamoDB
            if (account == null)
            {
                context.Result = new JsonResult(new { message = "Account not found" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            if (!IsActive(account))
            {
                context.Result = new JsonResult(new { message = $"Your account has not been approved. Status: {account.Status}." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            // Check permissions
            if (!HasPermission(account, resource, httpMethod))
            {
                context.Result = new JsonResult(new { message = "You do not have permission to access this feature." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

        }

        private static bool IsActive(Account account)
        {
            return string.Equals(account.Status, ACTIVE, StringComparison.OrdinalIgnoreCase);
        }

        private bool HasPermission(Account account, string? resource, string? action)
        {
            if (resource == null || action == null)
            {
                return false;
            }
            // Lấy roleId từ custom claim "RoleId"

            // Tìm Role từ DynamoDB
            var role = _dbContext.LoadAsync<Role>(account.RoleId).Result;

            if (role == null) return false;

            // Kiểm tra xem role có quyền trên resource và action cụ thể
            var hasPermission = role.Permissions.Any(p =>
                p.Resource == resource && p.Actions.Contains(action));

            return hasPermission;
        }
    }
}
