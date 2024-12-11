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
                throw new UnauthorizedAccessException("Token does not exist. Please log in again.");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            return client;
        }

        public async Task<IActionResult> OnGetAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                Message = "Invalid username.";
                return RedirectToPage("/Admin/Index");
            }

            try
            {
                var client = GetAuthenticatedClient();
                var response = await client.GetAsync($"https://dev.demorecognition.click/api/accounts/{username}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Message = "Session expired. Please log in again.";
                    return RedirectToPage("/Auth/Login");
                }

                if (!response.IsSuccessStatusCode)
                {
                    Message = "User not found.";
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
                Message = "Invalid data.";
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
                    Message = "Session expired. Please log in again.";
                    return RedirectToPage("/Auth/Login");
                }

                if (response.IsSuccessStatusCode)
                {
                    Message = "Update successful!";
                    return Page();
                    //return RedirectToPage("/Admin/Index");
                }

                Message = "Update failed.";
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
