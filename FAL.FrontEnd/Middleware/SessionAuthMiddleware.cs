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
            const string sessionStartKey = "SessionStartTime";
            var session = context.Session;

            // Lấy thời gian bắt đầu của session
            var sessionStartTime = session.GetString(sessionStartKey);

            if (string.IsNullOrEmpty(sessionStartTime))
            {
                // Nếu chưa có, tạo thời gian bắt đầu
                session.SetString(sessionStartKey, DateTime.UtcNow.ToString("o"));
            }
            else
            {
                // Kiểm tra thời gian hiện tại
                var startTime = DateTime.Parse(sessionStartTime);
                if (DateTime.Now - startTime > TimeSpan.FromMinutes(29)) // Time expired of jwt is 30 min
                {
                    // Hết hạn, xóa session
                    session.Clear();
                }
            }
            await _next(context);
        }
    }

}
