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
        public string SearchUserId { get; set; }
        public int CurrentPage { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(string searchUserId = null, int page = 1)
        {
            SearchUserId = searchUserId;
            CurrentPage = page;

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

            // Build API query string
            var queryParams = $"?page={CurrentPage}&pageSize=10";
            if (!string.IsNullOrEmpty(SearchUserId))
            {
                queryParams += $"&searchUserId={SearchUserId}";
            }

            // Call the TrainStats API
            var response = await client.GetAsync($"https://dev.demorecognition.click/api/Result/TrainStats{queryParams}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                TrainStats = JsonSerializer.Deserialize<TrainStatsResponse>(responseContent, options);
            }
            else
            {
                TrainStats = null;
            }

            return Page();
        }
    }
}
