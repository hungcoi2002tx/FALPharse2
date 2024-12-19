using FAL.FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace FAL.FrontEnd.Pages.Settings
{
    public class InfoModel : PageModel
    {
        [BindProperty]
        public string? UserName { get; set; }
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string SystemName { get; set; }

        [BindProperty]
        public string WebhookUrl { get; set; }

        [BindProperty]
        public string WebhookSecretKey { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        private readonly IHttpClientFactory _httpClientFactory;

        public InfoModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // OnGetAsync to fetch user information
        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(jwtToken))
            {
                Response.Redirect("/auth/login");
                return null;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            try
            {
                // Send GET request to fetch user information
                var response = await client.GetAsync(FEGlobalVarians.USERS_ME_ENDPOINT);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UserInfoViewDto>();

                    // Assign values from the API to properties
                    UserName = result?.UserName ?? "";
                    Email = result?.Email ?? "";
                    SystemName = result?.SystemName ?? "";
                    WebhookUrl = result?.WebhookUrl ?? "";
                    WebhookSecretKey = result?.WebhookSecretKey ?? "";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Error: {error}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            return Page();
        }

        // OnPostAsync to update user information
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Invalid information.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(jwtToken))
            {
                Response.Redirect("/auth/login");
                return null;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            try
            {
                // Prepare data to send
                var userUpdateRequest = new
                {
                    Email = this.Email,
                    WebhookUrl = this.WebhookUrl,
                    WebhookSecretKey = this.WebhookSecretKey
                };

                // Send PUT request to update user information
                var response = await client.PutAsJsonAsync(FEGlobalVarians.USERS_ENDPOINT, userUpdateRequest);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Update successful.";
                    return Page();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Error: {error}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }

            return Page();
        }
    }
}
