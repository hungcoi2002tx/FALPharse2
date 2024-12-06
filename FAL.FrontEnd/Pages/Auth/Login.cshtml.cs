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

                    //    var claims = new List<Claim>
                    //{
                    //    new Claim(ClaimTypes.Name, userInfo.Username ?? string.Empty),
                    //    new Claim(ClaimTypes.Role, userInfo.RoleId.ToString()),
                    //    new Claim("JwtToken", token) // Custom claim to store JWT token
                    //};

                    //    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    //    var authProperties = new AuthenticationProperties
                    //    {
                    //        IsPersistent = true,// Persistent login,
                    //        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                    //    };

                    //    await HttpContext.SignInAsync(
                    //        CookieAuthenticationDefaults.AuthenticationScheme,
                    //        new ClaimsPrincipal(claimsIdentity),
                    //        authProperties);

                        // redirect if admin or system
                        if (userInfo != null)
                        {
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
                // Người dùng đã đăng nhập
                return RedirectToPage("/Dashboard/Main");
            }

            // Người dùng chưa đăng nhập
            return Page();
        }

        public IActionResult OnGetLogout()
        {
            HttpContext.Session.Clear(); // Xóa toàn bộ dữ liệu trong session
            return RedirectToPage("/Login");
        }
    }
}
