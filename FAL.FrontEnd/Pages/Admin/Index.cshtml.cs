using FAL.FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Security.Principal;
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

        public List<Account> Accounts { get; set; } = new();

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
                Accounts = JsonSerializer.Deserialize<List<Account>>(json);
                // Debug log kết quả Deserialize
                Console.WriteLine("Accounts Count: " + Accounts.Count);
            }
            else
            {
                Console.WriteLine("Error fetching accounts: " + response.StatusCode);
                ModelState.AddModelError(string.Empty, "Không thể tải danh sách tài khoản!");
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string username)
        {
            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(jwtToken))
            {
                return RedirectToPage("/auth/login");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            var response = await client.DeleteAsync($"https://dev.demorecognition.click/api/Accounts/{username}");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Không thể xóa tài khoản!");
            }

            return RedirectToPage();
        }
    }
}
