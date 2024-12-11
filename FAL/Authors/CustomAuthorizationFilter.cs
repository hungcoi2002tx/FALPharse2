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
        private const string DEACTIVE = "DEACTIVE";
        private readonly IPermissionService _permissionService;
        private readonly IDynamoDBContext _dbContext;

        public CustomAuthorizationFilter(IPermissionService permissionService, IDynamoDBContext dbContext)
        {
            _permissionService = permissionService;
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
            if (!context.HttpContext.User.Identity.IsAuthenticated)
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

            if (string.Equals(account.Status, DEACTIVE, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new JsonResult(new { message = "Your account has not been approved" })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            // Check permissions
            if (!_permissionService.HasPermission(user, resource, httpMethod))
            {
                context.Result = new JsonResult(new { message = "You do not have permission to access this feature." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

        }
    }
}
