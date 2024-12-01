using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using Share.DTO;
using Microsoft.AspNetCore.WebUtilities;

namespace FAL.FrontEnd.Pages.Dashboard
{
    public class RequestDetailsModel : PageModel
    {
        public string RequestType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public List<ClientRequest> RequestDetails { get; set; } = new();
        public int TotalRecords { get; set; }

        private readonly IHttpClientFactory _httpClientFactory;

        public RequestDetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(string requestType, DateTime? startDate, DateTime? endDate, int page = 1)
        {
            RequestType = requestType;
            StartDate = startDate;
            EndDate = endDate;
            CurrentPage = page;

            // Get the JWT token from session
            var jwtToken = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(jwtToken))
            {
                return RedirectToPage("/Auth/Login");
            }

            // Set up the HttpClient
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            // Construct the API query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "startDate", startDate?.ToString("yyyy-MM-dd") },
                { "endDate", endDate?.ToString("yyyy-MM-dd") },
                { "page", page.ToString() },
                { "pageSize", PageSize.ToString() }
            };

            var url = QueryHelpers.AddQueryString($"https://dev.demorecognition.click/api/Result/TrainStats/Details/{requestType}", queryParams);

            // Make the API call
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Deserialize the API response
                var paginatedResponse = JsonSerializer.Deserialize<GroupedRequestData>(responseContent, options);
                RequestDetails = paginatedResponse.Requests;
                TotalRecords = paginatedResponse.TotalRecords;
            }
            else
            {
                // Handle API error
                RequestDetails = new List<ClientRequest>();
                TotalRecords = 0;
                ModelState.AddModelError(string.Empty, "Unable to fetch data from the server.");
            }

            return Page();
        }
    }
}
