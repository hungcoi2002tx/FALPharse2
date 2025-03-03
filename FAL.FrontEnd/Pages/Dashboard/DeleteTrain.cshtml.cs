using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace FAL.FrontEnd.Pages.Dashboard
{
    public class DeleteTrainModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeleteTrainModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string faceId)
        {
            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(jwtToken))
            {
                Response.Redirect("/auth/login");
                return null;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            var response = await client.DeleteAsync ($"https://dev.demorecognition.click/api/Result/TrainStats/{userId}/{faceId}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Record deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete the record.";
            }

            // Redirect back to TrainDetails with the userId parameter
            var referer = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer); // Redirect to the previous page
            }

            // Fallback to TrainDetails if no referer is available
            return RedirectToPage("./TrainDetails", null, new { userId });
        }

    }
}
