using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using FAL.Services.IServices;
using Microsoft.AspNetCore.Authorization;

namespace FAL.Authors
{
    public class CustomAuthorizationFilter : IAuthorizationFilter
    {
        private readonly IPermissionService _permissionService;

        public CustomAuthorizationFilter(IPermissionService permissionService)
        {
            _permissionService = permissionService;
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
                context.Result = new UnauthorizedResult();
                return;
            }

            // Lấy thông tin về phương thức HTTP (action) và endpoint (resource)
            var httpMethod = context.HttpContext.Request.Method;
            var resource = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()?.ControllerName; // Có thể sử dụng đường dẫn API hoặc tên endpoint

            // Lấy roleId từ JWT token hoặc ClaimsPrincipal
            var user = context.HttpContext.User;
            var roleIdClaim = user.FindFirst("RoleId");

            if (roleIdClaim == null)
            {
                // Không tìm thấy RoleId trong token
                context.Result = new ForbidResult();
                return;
            }

            var roleId = int.Parse(roleIdClaim.Value);

            // Kiểm tra quyền truy cập
            if (!_permissionService.HasPermission(user, resource, httpMethod))
            {
                context.Result = new ForbidResult(); // Trả về 403 nếu không có quyền
                return;
            }

            // Nếu tất cả các kiểm tra thành công, cho phép tiếp tục xử lý
        }
    }
}
