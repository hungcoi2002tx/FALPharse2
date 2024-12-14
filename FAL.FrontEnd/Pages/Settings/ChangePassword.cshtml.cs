using FAL.FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Share.DTO;
using System.Net.Http.Headers;

namespace FAL.FrontEnd.Pages.Settings
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ChangePasswordModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string CurrentPassword { get; set; }
        [BindProperty]
        public string NewPassword { get; set; }
        [BindProperty]
        public string ConfirmNewPassword { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // Kiểm tra mật khẩu mới và xác nhận mật khẩu trùng khớp
            if (NewPassword != ConfirmNewPassword)
            {
                TempData["Message"] = "New passwords do not match!";
                return RedirectToPage();
            }

            var changePasswordRequest = new Models.ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword
            };

            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            // Kiểm tra JWT token
            if (string.IsNullOrEmpty(jwtToken))
            {
                return RedirectToPage("/auth/login");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            var response = await client.PutAsJsonAsync(FEGlobalVarians.CHANGE_PASS_ENDPOINT, changePasswordRequest);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                HttpContext.Session.Clear();
                return RedirectToPage("/Auth/Login");
            }
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Password has been changed successfully!";
                return RedirectToPage("/Index");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            TempData["Message"] = errorMessage;
            return RedirectToPage();
        }
    }
}
