using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FAL.FrontEnd.Pages.Settings
{
    public class WebhookModel : PageModel
    {
        [BindProperty]
        public string WebhookUrl { get; set; }

        [BindProperty]
        public string WebhookSecretKey { get; set; }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        private readonly IHttpClientFactory _httpClientFactory;

        public WebhookModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Thông tin không hợp lệ.";
                return Page();
            }

            var httpClient = _httpClientFactory.CreateClient("FaceDetectionAPI");

            try
            {
                // Chuẩn bị dữ liệu gửi
                var webhookUpdateRequest = new
                {
                    WebhookUrl = this.WebhookUrl,
                    WebhookSecretKey = this.WebhookSecretKey
                };

                // Gửi request đến API
                var response = await httpClient.PutAsJsonAsync("api/users/webhook", webhookUpdateRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<dynamic>();
                    SuccessMessage = result?.Message ?? "Cập nhật thành công.";
                    return Page();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Lỗi: {error}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }

            return Page();
        }
    }
}
