using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Share.DTO;
using Share.Model;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FAL.FrontEnd.Pages.Dashboard
{
    public class RequestModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RequestModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public RequestStatsResponse RequestStats { get; set; }

        // Include all request types from the enum
        public List<string> AllRequestTypes { get; set; } = Enum.GetNames(typeof(RequestTypeEnum)).ToList();

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

            // Call the RequestStats API
            var response = await client.GetAsync("https://dev.demorecognition.click/api/Result/RequestStats");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                RequestStats = JsonSerializer.Deserialize<RequestStatsResponse>(responseContent, options);
            }
            else
            {
                RequestStats = null;
            }

            return Page();
        }
    }
}
