using FAL.FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace FAL.FrontEnd.Pages.Auth
{

    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public string ErrorMessage { get; set; }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string SystemName { get; set; }

        [BindProperty]
        public string WebhookUrl { get; set; }

        [BindProperty]
        public string WebhookSecretKey { get; set; }

        public RegisterModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient("FaceDetectionAPI");

            // Tạo đối tượng đăng ký dựa trên thông tin người dùng nhập
            var registerData = new
            {
                Username,
                Password,
                Email,
                SystemName,
                WebhookUrl,
                WebhookSecretKey
            };

            // Tạo nội dung yêu cầu đăng ký
            var content = new StringContent(JsonSerializer.Serialize(registerData), Encoding.UTF8, "application/json");

            // Gửi yêu cầu đăng ký đến API
            var response = await client.PostAsync(FEGlobalVarians.REGISTER_ENDPOINT, content);

            if (response.IsSuccessStatusCode)
            {
                // Đăng ký thành công, chuyển hướng đến trang đăng nhập
                return RedirectToPage("/Auth/Login");
            }
            else
            {
                // Đăng ký thất bại, hiển thị thông báo lỗi từ API
                ErrorMessage = await response.Content.ReadAsStringAsync();
                return Page();
            }
        }
    }

}
