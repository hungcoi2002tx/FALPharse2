using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace FAL.Authors
{
    public class CustomAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IPermissionService _permissionService;

        public CustomAuthorizationFilter(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var username = context.HttpContext.User.Identity.Name;
            var apiEndpoint = context.HttpContext.Request.Path;
            var method = context.HttpContext.Request.Method;

            // Kiểm tra nếu người dùng chưa đăng nhập
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Lấy role từ PermissionService
            var role = await _permissionService.GetRoleByUsername(username);
            if (role == null || !_permissionService.HasPermission(role, apiEndpoint, method))
            {
                context.Result = new ForbidResult("Bạn không có quyền thực hiện hành động này.");
            }
        }
    }
}
