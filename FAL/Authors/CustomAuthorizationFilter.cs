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

            // Bỏ qua kiểm tra ủy quyền nếu endpoint có [AllowAnonymous]
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return;
            }

            // Kiểm tra xem người dùng đã được xác thực chưa
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new JsonResult(new { message = "Unauthorized: You must be logged in." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            // Lấy thông tin về phương thức HTTP (action) và endpoint (resource)
            var httpMethod = context.HttpContext.Request.Method;
            var resource = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()?.ControllerName;

            // Lấy thông tin người dùng
            var user = context.HttpContext.User;
            var username = user.Identity.Name; // Lấy tên người dùng từ token

            // Kiểm tra trạng thái tài khoản
            var account = _dbContext.LoadAsync<Account>(username).Result; // Tìm tài khoản từ DynamoDB
            if (account == null)
            {
                context.Result = new JsonResult(new { message = "Không tìm thấy tài khoản" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            if (string.Equals(account.Status, DEACTIVE, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new JsonResult(new { message = "Tài khoản của bạn chưa được phê duyệt" })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            // Kiểm tra quyền truy cập
            if (!_permissionService.HasPermission(user, resource, httpMethod))
            {
                context.Result = new JsonResult(new { message = "Bạn không có quyền truy cập chức năng này." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

        }
    }
}
