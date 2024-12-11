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

        private HttpClient GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new UnauthorizedAccessException("Token does not exist. Please log in again.");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            return client;
        }

        public async Task<IActionResult> OnGetAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Invalid username.";
                return RedirectToPage("/Admin/Index");
            }
            // Lấy thông tin người dùng hiện tại từ session
            var currentUsername = HttpContext.Session.GetString("Username");

            // Kiểm tra nếu người dùng cố gắng edit chính mình
            if (string.Equals(username, currentUsername, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Settings/Info");
            }
            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync($"{FEGlobalVarians.ACCOUNTS_ENDPOINT}/{username}");
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] = "You do not have permission to access this feature. Please log in again.";
                    HttpContext.Session.Clear();
                    return RedirectToPage("/Auth/Login");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TempData["ErrorMessage"] = "Session expired. Please log in again.";
                    return RedirectToPage("/Auth/Login");
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "User not found.";
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
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("/Auth/Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("/Auth/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var client = GetAuthenticatedClient();
                var jsonContent = JsonSerializer.Serialize(User);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{FEGlobalVarians.ACCOUNTS_ENDPOINT}/{User.Username}", content);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TempData["ErrorMessage"] = "Session expired. Please log in again.";
                    return RedirectToPage("/Auth/Login");
                }

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Update successful!";
                    return RedirectToPage("/Admin/Index");
                }

                TempData["ErrorMessage"] = "Update failed.";
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage("/Auth/Login");
            }

            return Page();
        }
    }
}
