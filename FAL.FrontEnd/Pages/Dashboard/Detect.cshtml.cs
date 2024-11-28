using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Share.DTO;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FAL.FrontEnd.Pages.Dashboard
{
    public class DetectModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DetectModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public DetectStatsResponse DetectStats { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            // Check if the token exists in the session
            if (string.IsNullOrEmpty(jwtToken))
            {
                Response.Redirect("/auth/login");
                return null; // Stop execution after redirect
            }

            // Add the token to the Authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            // Make the API request
            var response = await client.GetAsync("https://dev.demorecognition.click/api/Result/DetectStats");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                DetectStats = JsonSerializer.Deserialize<DetectStatsResponse>(responseContent);
            }
            else
            {
                DetectStats = null; // Handle errors appropriately
            }

            return Page();
        }
    }
}
