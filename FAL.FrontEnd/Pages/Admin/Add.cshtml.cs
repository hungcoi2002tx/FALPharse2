using FAL.FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FAL.FrontEnd.Pages.Admin
{
    public class AddModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public AccountAddDto NewAccount { get; set; }

        public AddModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet()
        {
            // Logic when the page is loaded
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var jwtToken = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(jwtToken))
            {
                return RedirectToPage("/auth/login");
            }

            // Create HTTP client and set JWT
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            // Convert NewAccount data to JSON
            var json = JsonSerializer.Serialize(NewAccount);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Send POST request to the API
            var response = await client.PostAsync("https://dev.demorecognition.click/api/Accounts", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Account creation successful!";
                return RedirectToPage("/admin/index");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Unable to create account: {errorContent}";
                return Page();
            }
        }
    }
}
