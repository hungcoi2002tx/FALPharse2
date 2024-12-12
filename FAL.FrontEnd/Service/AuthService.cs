using FAL.FrontEnd.Service.IService;
using Share.Model;
using System.Text.Json;
using System.Text;
using Share.Constant;
using FAL.FrontEnd.Models;
using FAL.Utils;
using System.Net;

namespace FAL.FrontEnd.Service
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetTokenAsync(string username, string password)
        {

            var client = _httpClientFactory.CreateClient("FaceDetectionAPI");
            var loginData = new { username, password };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(FEGlobalVarians.LOGIN_ENDPOINT, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(responseContent);

                if (document.RootElement.TryGetProperty("token", out JsonElement tokenElement) && tokenElement.ValueKind == JsonValueKind.String)
                {
                    return tokenElement.GetString();
                }
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(responseContent);

                // Handle different error cases based on the API response
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    // Invalid login information
                    if (document.RootElement.TryGetProperty("message", out JsonElement messageElement))
                    {
                        throw new InvalidOperationException($"Bad Request: {messageElement.GetString()}");
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Username does not exist or password incorrect
                    if (document.RootElement.TryGetProperty("message", out JsonElement messageElement))
                    {
                        throw new UnauthorizedAccessException($"Unauthorized: {messageElement.GetString()}");
                    }
                }
                else
                {
                    // Handle other potential statuses (e.g., internal server errors)
                    throw new Exception($"Unexpected error: {response.StatusCode}");
                }
            }

            return string.Empty;
        }
    }
}
