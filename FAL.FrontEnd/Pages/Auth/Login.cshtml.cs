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
                if (Username != null && Password != null)
                {
                    string token;
                    try
                    {
                        token = await _authService.GetTokenAsync(Username, Password);
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Handle bad request (invalid login info) error
                        ErrorMessage = ex.Message;
                        return Page();
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        // Handle unauthorized access (e.g., incorrect password or account issues)
                        ErrorMessage = ex.Message;
                        return Page();
                    }
                    catch (Exception ex)
                    {
                        // Generic error handling
                        ErrorMessage = "An error occurred while attempting to log in. Please try again.";
                        Console.WriteLine(ex);
                        return Page();

                    }

                    if (!string.IsNullOrEmpty(token))
                    {
                        // Decode JWT token and save user info
                        Account userInfo = JwtHelper.DecodeJwt(token);

                        // Save JWT token in session
                        HttpContext.Session.SetString("JwtToken", token);

                        // Save user info in session
                        HttpContext.Session.SetString("Username", userInfo.Username ?? string.Empty);
                        HttpContext.Session.SetInt32("RoleId", userInfo.RoleId);

                        if (userInfo != null)
                        {
                            // Save token in cookie
                            Response.Cookies.Append("AccessToken", token, new CookieOptions
                            {
                                HttpOnly = false,          // FE can access
                                Secure = true,             // Only sent over HTTPS
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTime.UtcNow.AddMinutes(30) // JWT expires after 30 minutes
                            });

                            // Redirect based on user role
                            if (userInfo.RoleId == 1)
                            {
                                return Redirect("/Admin/Index");
                            }
                            else if (userInfo.RoleId == 2)
                            {
                                return Redirect("/Dashboard/Main");
                            }
                        }
                        return Page();
                    }
                }
                ErrorMessage = "Invalid login";
                return Page();
            }
            catch (Exception ex)
            {
                // Catch all other exceptions
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
