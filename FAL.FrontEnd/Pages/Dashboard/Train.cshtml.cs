using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Share.DTO;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FAL.FrontEnd.Pages.Dashboard
{
    public class TrainModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TrainModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public TrainStatsResponse TrainStats { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            // Redirect to login if the token is missing
            if (string.IsNullOrEmpty(jwtToken))
            {
                Response.Redirect("/auth/login");
                return null; // Stop further execution
            }

            // Add JWT token to the Authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            // Call the TrainStats API
            var response = await client.GetAsync("https://dev.demorecognition.click/api/Result/TrainStats");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                TrainStats = JsonSerializer.Deserialize<TrainStatsResponse>(responseContent,options);
            }
            else
            {
                TrainStats = null;
            }

            return Page();
        }
    }
}
