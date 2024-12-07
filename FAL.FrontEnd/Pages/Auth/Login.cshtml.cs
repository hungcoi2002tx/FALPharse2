using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using Amazon.Runtime;
using Share.Model;
using System.IdentityModel.Tokens.Jwt;
using FAL.Utils;
using FAL.FrontEnd.Service.IService;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using FAL.FrontEnd.Helper;

namespace FAL.FrontEnd.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;  
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string ErrorMessage { get; set; }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public LoginModel(IHttpClientFactory httpClientFactory, IAuthService authService, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FaceDetectionAPI");
                if(Username != null && Password != null)
                {
                    var token = await _authService.GetTokenAsync(Username, Password);
                    if (!token.IsNullOrEmpty())
                    {
                        // Lưu token vào session hoặc cookie
                        Account userInfo = JwtHelper.DecodeJwt(token);

                        // Lưu JWT token vào session
                        HttpContext.Session.SetString("JwtToken", token);

                        // Lưu thông tin người dùng vào session
                        HttpContext.Session.SetString("Username", userInfo.Username ?? string.Empty);
                        HttpContext.Session.SetInt32("RoleId", userInfo.RoleId);
                        if (userInfo != null)
                        {
                            // Lưu token trong cookie
                            Response.Cookies.Append("AccessToken", token, new CookieOptions
                            {
                                HttpOnly = false,          // FE có thể truy cập
                                Secure = true,             // Chỉ gửi qua HTTPS
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTime.UtcNow.AddMinutes(30) // JWT hết hạn sau 30 phút
                            });
                            if (userInfo.RoleId == 1)
                            {
                                return Redirect("/Admin/Add");
                            }
                            else if (userInfo.RoleId == 2)
                            {
                                return Redirect("/Dashboard/Main");
                            }
                        }
                        return Redirect("/Error");
                    }
                }
                ErrorMessage = "Invalid login";
                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Redirect("/Error");
            }
        }

        public IActionResult OnGet()
        {
            var username = HttpContext.Session.GetString("Username");
            var roleId = HttpContext.Session.GetInt32("RoleId");

            if (!string.IsNullOrEmpty(username) && roleId.HasValue)
            {
                return RedirectToPage("/Dashboard/Main");
            }

            return Page();
        }
    }
}
