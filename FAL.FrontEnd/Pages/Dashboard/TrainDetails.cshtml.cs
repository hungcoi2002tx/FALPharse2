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

        public async Task<IActionResult> OnGetAsync(string userId, [FromQuery] int page = 1)
        {
            CurrentPage = page;
            ViewData["userId"] = userId;

            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(jwtToken))
            {
                Response.Redirect("/auth/login");
                return null;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            async Task<PaginatedTrainStatsDetailResponse> FetchPageDataAsync(int pageToFetch)
            {
                var response = await client.GetAsync($"https://dev.demorecognition.click/api/Result/TrainStats/Details/{userId}?page={pageToFetch}&pageSize={PageSize}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<PaginatedTrainStatsDetailResponse>(responseContent, options);
                }
                return null;
            }

            // Initial fetch
            var paginatedResponse = await FetchPageDataAsync(CurrentPage);

            // Retry logic if the page >= 2 and no data is returned
            if ((paginatedResponse == null || !paginatedResponse.Data.Any()) && CurrentPage > 1)
            {
                paginatedResponse = await FetchPageDataAsync(CurrentPage - 1);
                if (paginatedResponse != null && paginatedResponse.Data.Any())
                {
                    CurrentPage--; // Adjust current page after successful retry
                }
            }

            // Set data or fallback to empty
            if (paginatedResponse != null && paginatedResponse.Data != null)
            {
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
