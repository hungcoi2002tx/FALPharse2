using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;

namespace FAL.FrontEnd.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public string ErrorMessage { get; set; }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient("FaceDetectionAPI");
            var loginData = new { Username, Password };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var token = JsonDocument.Parse(responseContent).RootElement.GetProperty("token").GetString();

                // Lưu token vào session hoặc cookie
                HttpContext.Session.SetString("JwtToken", token);
                return RedirectToPage("/Admin/Index"); // Chuyển hướng đến trang chính cho Admin/User
            }
            else
            {
                ErrorMessage = "Invalid login attempt.";
                return Page();
            }
        }
    }
}
