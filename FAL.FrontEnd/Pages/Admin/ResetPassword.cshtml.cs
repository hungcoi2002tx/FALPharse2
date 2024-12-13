using FAL.FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace FAL.FrontEnd.Pages.Admin
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ResetPasswordModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet =true)]
        public string Username { get; set; }

        public string NewPassword { get; set; }

        [BindProperty]
        public string Message { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Username))
            {
                Message = "Username is required.";
                return Page();
            }

            try
            {
                // Send the API request to reset password
                var client = _httpClientFactory.CreateClient();
                var jwtToken = HttpContext.Session.GetString("JwtToken");

                // Kiểm tra JWT token
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return RedirectToPage("/auth/login");
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var request = new StringContent(JsonConvert.SerializeObject(Username), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(FEGlobalVarians.RESET_PASS_ENDPOINT, request);

                if (response.IsSuccessStatusCode)
                {
                    // If successful, get the new password from the response
                    var content = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(content);
                    NewPassword = responseObject.newPassword;
                    Message = responseObject.message;
                }
                else
                {
                    Message = "Failed to reset password. " + response.ReasonPhrase;
                }
            }
            catch (System.Exception ex)
            {
                Message = "An error occurred while resetting the password.";
            }

            return Page();
        }
    }
}
