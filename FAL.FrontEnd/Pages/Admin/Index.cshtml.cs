using FAL.FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace FAL.FrontEnd.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<AccountViewDto> Accounts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(jwtToken))
            {
                Response.Redirect("/auth/login");
                return;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await client.GetAsync("https://dev.demorecognition.click/api/Accounts");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Accounts = JsonSerializer.Deserialize<List<AccountViewDto>>(json);
                // Debug log kết quả Deserialize
                Console.WriteLine("Accounts Count: " + Accounts.Count);
            }
            else
            {
                Console.WriteLine("Error fetching accounts: " + response.StatusCode);
                ModelState.AddModelError(string.Empty, "Không thể tải danh sách tài khoản!");
            }
        }
        public async Task<IActionResult> OnPostAsync(string username)
        {
            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(jwtToken))
            {
                return RedirectToPage("/auth/login");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            // Lấy thông tin tài khoản để kiểm tra trạng thái hiện tại
            var response = await client.GetAsync($"https://dev.demorecognition.click/api/accounts/{username}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không thể lấy thông tin tài khoản!";
                return RedirectToPage();
            }

            var accountJson = await response.Content.ReadAsStringAsync();
            var account = JsonSerializer.Deserialize<AccountViewDto>(accountJson);

            if (account == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản!";
                return RedirectToPage();
            }

            // Cập nhật trạng thái
            account.Status = account.Status == "Active" ? "Deactive" : "Active";
            var jsonContent = JsonSerializer.Serialize(account);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Gửi yêu cầu cập nhật
            var updateResponse = await client.PutAsync($"https://dev.demorecognition.click/api/accounts/{username}", content);
            if (!updateResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật trạng thái!";
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Cập nhật trạng thái thành công!";
            return RedirectToPage();
        }


    }
}
