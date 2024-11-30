using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Share.DTO;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FAL.FrontEnd.Pages.Dashboard
{
    public class UserDetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserDetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<TrainStatsDetailDTO> UserDetails { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId)
        {
            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(jwtToken))
            {
                Response.Redirect("/auth/login");
                return null;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await client.GetAsync($"https://dev.demorecognition.click/api/Result/TrainStats/Details/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                UserDetails = JsonSerializer.Deserialize<List<TrainStatsDetailDTO>>(responseContent, options);
            }
            else
            {
                UserDetails = null;
            }

            return Page();
        }
    }
}
