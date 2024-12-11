namespace FAL.FrontEnd.Middleware
{
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Dictionary<string, int> _rolePermissions;


        public RoleAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
            _rolePermissions = new Dictionary<string, int>
            {
                { "/Admin/Add", 1 },
                { "/Admin", 1 },
                { "/Dashboard", 2 }
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            var roleId = context.Session.GetInt32("RoleId");
            if(roleId == 1 && context.Request.Path.StartsWithSegments("/Dashboard"))
            {
                context.Response.Redirect("/accessdenied");
                return;
            }

            foreach (var permission in _rolePermissions)
            {
                if (path.StartsWith(permission.Key) && roleId != permission.Value)
                {
                    context.Response.Redirect("/accessdenied");
                    return;
                }
            }

            await _next(context);
        }
    }
}
