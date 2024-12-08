using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FAL.Dtos;
using FAL.FrontEnd.Models;

namespace FAL.FrontEnd.Pages.Admin
{
    public class UpdateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UpdateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public AccountViewDto User { get; set; } = new AccountViewDto();

        [TempData]
        public string Message { get; set; }

        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new UnauthorizedAccessException("Token không tồn tại. Vui lòng đăng nhập lại.");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            return client;
        }

        public async Task<IActionResult> OnGetAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                Message = "Username không hợp lệ.";
                return RedirectToPage("/Admin/Index");
            }

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync($"https://dev.demorecognition.click/api/accounts/{username}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    return RedirectToPage("/Auth/Login");
                }

                if (!response.IsSuccessStatusCode)
                {
                    Message = "Không tìm thấy user.";
                    return RedirectToPage("/Admin/Index");
                }

                var content = await response.Content.ReadAsStringAsync();
                User = JsonSerializer.Deserialize<AccountViewDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                Message = ex.Message;
                return RedirectToPage("/Auth/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Message = "Dữ liệu không hợp lệ.";
                return Page();
            }

            try
            {
                var client = GetAuthenticatedClient();
                var jsonContent = JsonSerializer.Serialize(User);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"https://dev.demorecognition.click/api/accounts/{User.Username}", content);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    return RedirectToPage("/Auth/Login");
                }

                if (response.IsSuccessStatusCode)
                {
                    Message = "Cập nhật thành công!";
                    return RedirectToPage("/Admin/Index");
                }

                Message = "Cập nhật thất bại.";
            }
            catch (UnauthorizedAccessException ex)
            {
                Message = ex.Message;
                return RedirectToPage("/Auth/Login");
            }

            return Page();
        }
    }
}
