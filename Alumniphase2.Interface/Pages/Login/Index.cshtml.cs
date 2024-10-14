using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Text;

namespace Alumniphase2.Interface.Pages.Login
{
    
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }
        private readonly HttpClient _httpClient;

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Create the request payload (body) with username and password
            var loginData = new
            {
                Username = this.Username,
                Password = this.Password
            };

            var jsonContent = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("http://fal-dev.eba-55qpmvbp.ap-southeast-1.elasticbeanstalk.com/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

                if (responseData != null && responseData.ContainsKey("token"))
                {
                    var jwtToken = responseData["token"];

                    Response.Cookies.Append("AuthToken", jwtToken, new CookieOptions
                    {
                        HttpOnly = true,   
                        Secure = true,     
                        SameSite = SameSiteMode.Strict 
                    });

                    return RedirectToPage("/DetectFace/Index");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
}
