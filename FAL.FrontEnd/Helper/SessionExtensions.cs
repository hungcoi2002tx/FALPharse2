namespace FAL.FrontEnd.Helper
{

    public static class SessionExtensions
    {
        private static IHttpContextAccessor _httpContextAccessor;

        // Hàm để thiết lập IHttpContextAccessor (gọi ở Startup hoặc Program.cs)
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static bool IsAuthenticated()
        {
            var context = _httpContextAccessor?.HttpContext;
            var username = context?.Session.GetString("Username");
            return !string.IsNullOrEmpty(username);
        }

        public static bool IsInRole(int roleId)
        {
            var context = _httpContextAccessor?.HttpContext;
            var role = context?.Session.GetInt32("RoleId");
            return role.HasValue && role.Value == roleId;
        }

        /// <summary>
        /// Cách dùng: Context.Session.GetUsername()
        /// Để tạm bên này vì cùng chung mục đích 
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static string GetUsername(this ISession session)
        {
            return session.GetString("Username") ?? "Guest";
        }
    }

}
