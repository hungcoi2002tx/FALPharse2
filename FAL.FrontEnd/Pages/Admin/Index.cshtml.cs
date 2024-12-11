using FAL.FrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace FAL.FrontEnd.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<AccountViewDto> Accounts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(jwtToken))
            {
                Response.Redirect("/auth/login");
                return;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await client.GetAsync(FEGlobalVarians.ACCOUNTS_ENDPOINT);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Accounts = JsonSerializer.Deserialize<List<AccountViewDto>>(json) ?? [];
                if (Accounts.Count == 0)
                {
                    TempData["ErrorMessage"] = "Unable to load the account list!";
                    return;

                }
                // Debug log the result of Deserialize
                Console.WriteLine("Accounts Count: " + Accounts.Count);
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    // Log out the user by clearing the session and redirect to login
                    HttpContext.Session.Clear();
                    TempData["Message"] = "You do not have permission to access this feature. Please log in again.";
                    Response.Redirect("/auth/login");
                    return;
                }
                Console.WriteLine("Error fetching accounts: " + response.StatusCode);
                TempData["ErrorMessage"] = "Unable to load the account list!";
            }
        }
        public async Task<IActionResult> OnPostAsync(string username)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var jwtToken = HttpContext.Session.GetString("JwtToken");

                // Kiểm tra JWT token
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return RedirectToPage("/auth/login");
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                // Lấy thông tin người dùng hiện tại từ session
                var currentUsername = HttpContext.Session.GetString("Username");
                if (string.IsNullOrEmpty(currentUsername))
                {
                    TempData["ErrorMessage"] = "Unable to identify the current user!";
                    return RedirectToPage();
                }

                // Kiểm tra nếu người dùng cố gắng deactivate chính mình
                if (string.Equals(username, currentUsername, StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "You cannot deactivate your own account!";
                    return RedirectToPage();
                }

                // Gửi yêu cầu để lấy thông tin tài khoản
                var response = await client.GetAsync($"{FEGlobalVarians.ACCOUNTS_ENDPOINT}/{username}");
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    HttpContext.Session.Clear();
                    return RedirectToPage("/Auth/Login");
                }
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Unable to fetch account information!";
                    return RedirectToPage();
                }

                var accountJson = await response.Content.ReadAsStringAsync();
                var account = JsonSerializer.Deserialize<AccountViewDto>(accountJson);
                if (account == null)
                {
                    TempData["ErrorMessage"] = "Account not found!";
                    return RedirectToPage();
                }

                // Chỉ cập nhật trạng thái của tài khoản
                var updatePayload = new { Status = account.Status == "Active" ? "Deactive" : "Active" };
                var jsonContent = JsonSerializer.Serialize(updatePayload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Gửi yêu cầu cập nhật trạng thái
                var updateResponse = await client.PutAsync($"{FEGlobalVarians.ACCOUNTS_ENDPOINT}/{username}", content);
                if (updateResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    TempData["Message"] = "You do not have permission to access this feature. Please log in again.";
                    HttpContext.Session.Clear();
                    return RedirectToPage("/Auth/Login");
                }
                if (!updateResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Error updating the status!";
                    return RedirectToPage();
                }

                TempData["SuccessMessage"] = "Status updated successfully!";
                return RedirectToPage();
            }
            catch (HttpRequestException)
            {
                // Xử lý lỗi kết nối API
                TempData["ErrorMessage"] = "Unable to connect to the server. Please try again later.";
                return RedirectToPage();
            }
            catch (Exception)
            {
                // Xử lý các lỗi bất ngờ
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return RedirectToPage();
            }
        }

    }
}
