using System.Diagnostics.Eventing.Reader;

namespace FAL.FrontEnd.Middleware
{
    public class SessionAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var username = context.Session.GetString("Username");
            
            if(string.IsNullOrEmpty(username) && 
                !context.Request.Path.StartsWithSegments("/Auth/Login") &&
                !context.Request.Path.StartsWithSegments("/Auth/Register")
                )
            {
                context.Response.Redirect("/Auth/Login");
                return;
            }

            await _next(context);
        }
    }

}
