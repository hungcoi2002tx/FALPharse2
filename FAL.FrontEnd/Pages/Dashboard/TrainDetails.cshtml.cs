using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Share.DTO;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FAL.FrontEnd.Pages.Dashboard
{
    public class TrainDetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TrainDetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<TrainStatsDetailDTO> UserDetails { get; set; }
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public async Task<IActionResult> OnGetAsync(string userId, int page = 1)
        {
            CurrentPage = page;

            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(jwtToken))
            {
                Response.Redirect("/auth/login");
                return null;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await client.GetAsync($"https://dev.demorecognition.click/api/Result/TrainStats/Details/{userId}?page={CurrentPage}&pageSize={PageSize}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var paginatedResponse = JsonSerializer.Deserialize<PaginatedTrainStatsDetailResponse>(responseContent, options);
                UserDetails = paginatedResponse.Data;
                TotalRecords = paginatedResponse.TotalRecords;
            }
            else
            {
                UserDetails = new List<TrainStatsDetailDTO>();
                TotalRecords = 0;
            }

            return Page();
        }
    }
}
