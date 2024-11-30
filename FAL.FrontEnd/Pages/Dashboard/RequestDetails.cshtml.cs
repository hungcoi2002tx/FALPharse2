using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Share.DTO;
namespace FAL.FrontEnd.Pages.Dashboard
{
    public class RequestDetailsModel : PageModel
    {
        public string RequestType { get; set; }
        public List<ClientRequest> FilteredRequests { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public bool HasMorePages { get; set; }

        public async Task<IActionResult> OnGetAsync(string requestType, string requests, DateTime? startDate, DateTime? endDate, int page = 1)
        {
            RequestType = requestType;

            // Deserialize the requests data from query string
            var allRequests = JsonSerializer.Deserialize<GroupedRequestData>(requests);

            // Apply filters
            StartDate = startDate;
            EndDate = endDate;
            var filtered = allRequests.Requests.AsQueryable();

            // Parse all CreateDate strings into DateTime objects first
            // Parse all CreateDate strings into DateTime objects first
            var parsedRequests = filtered.ToList()
                .Select(r => new
                {
                    Request = r,
                    ParsedCreateDate = DateTime.Parse(r.CreateDate) // Direct parsing
                })
                .ToList();

            // Apply date filters
            if (startDate.HasValue)
            {
                parsedRequests = parsedRequests
                    .Where(x => x.ParsedCreateDate >= startDate.Value)
                    .ToList();
            }

            if (endDate.HasValue)
            {
                parsedRequests = parsedRequests
                    .Where(x => x.ParsedCreateDate <= endDate.Value)
                    .ToList();
            }

            // Extract the filtered requests back into the original list
            filtered = parsedRequests.Select(x => x.Request).AsQueryable();


            // Apply pagination
            CurrentPage = page;
            var skip = (page - 1) * PageSize;
            FilteredRequests = filtered.Skip(skip).Take(PageSize).ToList();
            HasMorePages = filtered.Count() > skip + PageSize;

            return Page();
        }
    }
}
