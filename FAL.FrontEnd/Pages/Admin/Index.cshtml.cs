﻿using FAL.FrontEnd.Models;
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

            var response = await client.GetAsync("https://dev.demorecognition.click/api/Accounts");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Accounts = JsonSerializer.Deserialize<List<AccountViewDto>>(json);
                // Debug log the result of Deserialize
                Console.WriteLine("Accounts Count: " + Accounts.Count);
            }
            else
            {
                Console.WriteLine("Error fetching accounts: " + response.StatusCode);
                ModelState.AddModelError(string.Empty, "Unable to load the account list!");
            }
        }
        public async Task<IActionResult> OnPostAsync(string username)
        {
            var client = _httpClientFactory.CreateClient();
            var jwtToken = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(jwtToken))
            {
                return RedirectToPage("/auth/login");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            // Lấy thông tin người dùng hiện tại từ JWT
            var currentUsername = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(currentUsername))
            {
                TempData["ErrorMessage"] = "Unable to identify the current user!";
                return RedirectToPage();
            }

            // Kiểm tra nếu người dùng đang cố gắng deactive chính mình
            if (string.Equals(username, currentUsername, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "You cannot deactivate your own account!";
                return RedirectToPage();
            }

            // Retrieve account information to check the current status
            var response = await client.GetAsync($"https://dev.demorecognition.click/api/accounts/{username}");
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

            // Update the status
            account.Status = account.Status == "Active" ? "Deactive" : "Active";
            var jsonContent = JsonSerializer.Serialize(account);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Send the update request
            var updateResponse = await client.PutAsync($"https://dev.demorecognition.click/api/accounts/{username}", content);
            if (!updateResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Error updating the status!";
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Status updated successfully!";
            return RedirectToPage();
        }

    }
}
